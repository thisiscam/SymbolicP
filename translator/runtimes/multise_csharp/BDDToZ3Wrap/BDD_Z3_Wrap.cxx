#include <unordered_map>
#include <vector>
#include <functional>

/* Use the C version of bdd.h*/
#ifdef USE_SYLVAN
#include "sylvan.h"
#include "bdd_sylvan_z3_wrap_export.h"
#define WRAP_EXPORT BDD_SYLVAN_Z3_WRAP_EXPORT
#define bddtrue sylvan_true
#define bddfalse sylvan_false
#define bdd_low sylvan_low
#define bdd_high sylvan_high
#define bdd_ithvar sylvan_ithvar
#define bdd_nithvar sylvan_nithvar
#define bdd_var sylvan_var
#define bdd_addref sylvan_ref
#define bdd_delref sylvan_deref
#else
#define BUDDY_USE_C
extern "C" 
{
#include "bdd.h"
}
#include "bdd_buddy_z3_wrap_export.h"
#define WRAP_EXPORT BDD_BUDDY_Z3_WRAP_EXPORT
#endif

#include "z3.h"

using namespace std;

static Z3_context ctx;

static vector<Z3_ast> bdd_vars_to_z3_formula;

namespace std {
    template <>
        class hash<Z3_ast>{
        public :
            size_t operator()(const Z3_ast &ast ) const
            {
                return Z3_get_ast_id(ctx, ast);
            }
    };
    template <>
        class equal_to<Z3_ast>{
        public :
            bool operator()(const Z3_ast &a1, const Z3_ast &a2) const
            {
                return Z3_get_ast_id(ctx, a1) == Z3_get_ast_id(ctx, a2);
            }
    };
};

static unordered_map<Z3_ast, BDD> Z3_formula_to_bdd_map;

extern "C" 
{

WRAP_EXPORT void init_bdd_z3_wrap(void* c)
{
	ctx = (Z3_context)c;
}

static Z3_ast _bdd_to_Z3_formula(BDD root, unordered_map<BDD, Z3_ast> &visited, int* count)
{
	// TODO, optimize to make smt form more compact? e.g. using ITE, 
	// or switch over shape of root for possible simplier forms
	*count += 1;
	Z3_ast ret;
	if(visited.count(root) > 0) {
		ret = visited[root];
		Z3_inc_ref(ctx, ret);
		return ret;
	}
	if(root == bddtrue) {
		ret = Z3_mk_true(ctx);
		Z3_inc_ref(ctx, ret);
	} else if (root == bddfalse) {
		ret = Z3_mk_false(ctx);
		Z3_inc_ref(ctx, ret);
	} else {
		Z3_ast t = bdd_vars_to_z3_formula[bdd_var(root)];

		BDD low = bdd_addref(bdd_low(root));
		BDD high = bdd_addref(bdd_high(root));
		Z3_ast left = _bdd_to_Z3_formula(low, visited, count);
		Z3_ast right = _bdd_to_Z3_formula(high, visited, count);
		bdd_delref(low);
		bdd_delref(high);

		Z3_ast not_t = Z3_mk_not(ctx, t);
		Z3_inc_ref(ctx, not_t);
		Z3_ast const and_args_left[] = {not_t, left};
		Z3_ast const and_args_right[] = {t, right};

		Z3_ast t_and_not_left = Z3_mk_and(ctx, 2, and_args_left);
		Z3_inc_ref(ctx, t_and_not_left);
		Z3_dec_ref(ctx, left);
		Z3_dec_ref(ctx, not_t);
		Z3_ast t_and_right = Z3_mk_and(ctx, 2, and_args_right);
		Z3_inc_ref(ctx, t_and_right);
		Z3_dec_ref(ctx, right);

		Z3_ast const or_args[] = {t_and_not_left, t_and_right};

		ret = Z3_mk_or(ctx, 2, or_args);
		Z3_inc_ref(ctx, ret);
		Z3_dec_ref(ctx, t_and_not_left);
		Z3_dec_ref(ctx, t_and_right);
	}
	Z3_inc_ref(ctx, ret);
	bdd_addref(root);
	visited[root] = ret;
	return ret;
}

WRAP_EXPORT Z3_ast bdd_to_Z3_formula(BDD r)
{
	int i = 0;
	unordered_map<BDD, Z3_ast> visited;
	Z3_ast ret = _bdd_to_Z3_formula(r, visited, &i);
	for (unordered_map<BDD, Z3_ast>::iterator it = visited.begin(); it != visited.end(); ++it) {
		Z3_dec_ref(ctx, it->second);
		bdd_delref(it->first);
	}
	// printf("traversal count: %d, bddnodecount: %d\n", i, bdd_nodecount(*r));
	return ret;
}

WRAP_EXPORT BDD Z3_formula_to_bdd(Z3_ast bool_exp)
{
	// TODO, decompose bool_exp into more atomic forms?
	switch(Z3_get_bool_value(ctx, bool_exp)) {
		case Z3_L_TRUE: {
			return bddtrue;
		}
		case Z3_L_FALSE: {
			return bddfalse;
		}
		default: {
			if(Z3_formula_to_bdd_map.count(bool_exp) > 0) {
				return Z3_formula_to_bdd_map.at(bool_exp);
			} else {
				BDD new_var = bdd_ithvar(bdd_vars_to_z3_formula.size());
				Z3_inc_ref(ctx, bool_exp); // This will probably leak memory!
				bdd_vars_to_z3_formula.push_back(bool_exp);
				Z3_formula_to_bdd_map[bool_exp] = new_var;
				return new_var;
			}
		}
	}
}

WRAP_EXPORT Z3_ast get_ith_Z3_formula(int i)
{
	return bdd_vars_to_z3_formula[i];
}

WRAP_EXPORT int get_num_formulas()
{
	return bdd_vars_to_z3_formula.size();
}

WRAP_EXPORT void debug_print_used_bdd_vars()
{
	for(int i=0; i < bdd_vars_to_z3_formula.size(); i++)
	{
		printf("%d: %s\n", i, Z3_ast_to_string(ctx, bdd_vars_to_z3_formula[i]));
	}
}
WRAP_EXPORT BDD find_one_sat(BDD bdd)
{
	/* TODO: fix this function to take into account Z3 */
#ifdef USE_SYLVAN
	LACE_ME;
	BDDVAR vars[bdd_vars_to_z3_formula.size()];
	for(int i=0; i < bdd_vars_to_z3_formula.size(); i++) {
		vars[i] = i;
	}
	BDDSET varset = sylvan_set_fromarray(vars, bdd_vars_to_z3_formula.size());
	return bdd_addref(sylvan_sat_single(bdd, varset));
#else
	int* vars = new int[bdd_vars_to_z3_formula.size()];
	for(int i=0; i < bdd_vars_to_z3_formula.size(); i++) {
		vars[i] = i;
	}
	BDD varset = bdd_makeset(vars, bdd_vars_to_z3_formula.size());
	delete vars;
	return bdd_addref(bdd_satoneset(bdd, varset, bddtrue));
#endif
}

#ifdef USE_SYLVAN
WRAP_EXPORT void force_set_task_pc(BDD pc)
{
	WorkerP* __lace_worker = lace_get_worker();
	void** task_buf = (void**)__lace_worker->current_task->d;
	// printf("xxx worker %d: force set from %p to %lu\n", __lace_worker->worker, task_buf[5], pc);
	sylvan_ref(pc);
	task_buf[5] = (void*)pc;
}
WRAP_EXPORT void set_task_pc(BDD pc)
{
	WorkerP* __lace_worker = lace_get_worker();
	void** task_buf = (void**)__lace_worker->current_task->d;
	BDD old_pc = (BDD)task_buf[5];
	// printf("xxx worker %d: set from %lu to %lu\n", __lace_worker->worker, old_pc, pc);
	sylvan_ref(pc);
	sylvan_deref(old_pc);
	task_buf[5] = (void*)pc;
}
WRAP_EXPORT BDD get_task_pc()
{
	WorkerP* __lace_worker = lace_get_worker();
	void** task_buf = (void**)__lace_worker->current_task->d;
	BDD pc = (BDD)task_buf[5];
	// printf("xxx worker %d: get %lu\n", __lace_worker->worker, pc);
	return sylvan_ref(pc);
}
WRAP_EXPORT void task_pc_addaxiom(BDD bdd)
{
	LACE_ME;
	void** task_buf = (void**)__lace_worker->current_task->d;
	BDD old_pc = (BDD)task_buf[5];
	BDD new_pc = sylvan_ref(sylvan_and(old_pc, bdd));
	sylvan_deref(old_pc);
	// printf("xxx worker %d: and old %lu with %lu -> %lu\n", __lace_worker->worker, old_pc, bdd, new_pc);
	task_buf[5] = (void*)new_pc;
}
#endif
}