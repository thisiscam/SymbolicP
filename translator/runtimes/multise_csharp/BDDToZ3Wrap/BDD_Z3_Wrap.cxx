#include <unordered_map>
#include <vector>
#include <functional>

/* Use the C version of bdd.h*/
#ifdef USE_SYLVAN
#include "sylvan.h"
#define bddtrue sylvan_true
#define bddfalse sylvan_false
#define bdd_low sylvan_low
#define bdd_high sylvan_high
#define bdd_ithvar sylvan_ithvar
#define bdd_var sylvan_var
#else
#undef __cplusplus
extern "C" 
{
#include "bdd.h"
}
#define __cplusplus
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

void init_bdd_z3_wrap(void* c)
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

		Z3_ast left = _bdd_to_Z3_formula(bdd_low(root), visited, count);
		Z3_ast right = _bdd_to_Z3_formula(bdd_high(root), visited, count);

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
	visited[root] = ret;
	return ret;
}

Z3_ast bdd_to_Z3_formula(BDD r)
{
	int i = 0;
	unordered_map<BDD, Z3_ast> visited;
	Z3_ast ret = _bdd_to_Z3_formula(r, visited, &i);
	for (unordered_map<BDD, Z3_ast>::iterator it = visited.begin(); it != visited.end(); ++it) {
		Z3_dec_ref(ctx, it->second);
	}
	// printf("traversal count: %d, bddnodecount: %d\n", i, bdd_nodecount(*r));
	return ret;
}

BDD Z3_formula_to_bdd(Z3_ast bool_exp)
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

Z3_ast get_ith_Z3_formula(int i)
{
	return bdd_vars_to_z3_formula[i];
}

int get_num_formulas()
{
	return bdd_vars_to_z3_formula.size();
}

void debug_print_used_bdd_vars()
{
	for(int i=0; i < bdd_vars_to_z3_formula.size(); i++)
	{
		printf("%d: %s\n", i, Z3_ast_to_string(ctx, bdd_vars_to_z3_formula[i]));
	}
}

#ifdef USE_SYLVAN
void set_task_pc(void* pc)
{
	LACE_ME;
	void** task_buf = (void**)__lace_dq_head->d;
	task_buf[2] = pc;
}
void* get_task_pc()
{
	LACE_ME;
	void** task_buf = (void**)__lace_dq_head->d;
	return task_buf[2];
}
#endif
}