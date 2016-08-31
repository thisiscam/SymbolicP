#include <unordered_map>
#include <vector>
#include <functional>
#include "bdd.h"
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

static unordered_map<Z3_ast, bdd> Z3_formula_to_bdd_map;

#ifdef __cplusplus
extern "C" 
#endif

{

void init_bdd_z3_wrap(void* c)
{
	ctx = (Z3_context)c;
}

static Z3_ast _bdd_to_Z3_formula(bdd root)
{
	// TODO, optimize to make smt form more compact? e.g. using ITE, 
	// or switch over shape of root for possible simplier forms
	if(root == bddtrue) {
		return Z3_mk_true(ctx);
	} else if (root == bddfalse) {
		return Z3_mk_false(ctx);
	} else {
		Z3_ast t = bdd_vars_to_z3_formula[bdd_var(root)];

		Z3_ast left = _bdd_to_Z3_formula(bdd_low(root));
		Z3_inc_ref(ctx, left);
		Z3_ast right = _bdd_to_Z3_formula(bdd_high(root));
		Z3_inc_ref(ctx, right);

		Z3_ast const or_args_left[] = {t, left};
		Z3_ast const or_args_right[] = {t, right};

		Z3_ast t_or_left = Z3_mk_or(ctx, 2, or_args_left);
		Z3_inc_ref(ctx, t_or_left);
		Z3_dec_ref(ctx, left);
		Z3_ast t_or_right = Z3_mk_or(ctx, 2, or_args_right);
		Z3_inc_ref(ctx, t_or_right);
		Z3_dec_ref(ctx, right);

		Z3_ast const and_args[] = {t_or_left, t_or_right};

		Z3_ast ret = Z3_mk_and(ctx, 2, and_args);
		Z3_dec_ref(ctx, t_or_left);
		Z3_dec_ref(ctx, t_or_right);
		return ret;
	}
}

Z3_ast bdd_to_Z3_formula(bdd* r)
{
	Z3_ast ret = _bdd_to_Z3_formula(*r);
	Z3_inc_ref(ctx, ret);
	return ret;
}

bdd* Z3_formula_to_bdd(Z3_ast bool_exp)
{
	// TODO, decompose bool_exp into more atomic forms?
	switch(Z3_get_bool_value(ctx, bool_exp)) {
		case Z3_L_TRUE: {
			return new bdd(bddtrue);
		}
		case Z3_L_FALSE: {
			return new bdd(bddfalse);
		}
		default: {
			if(Z3_formula_to_bdd_map.count(bool_exp) > 0) {
				return new bdd(Z3_formula_to_bdd_map.at(bool_exp));
			} else {
				bdd new_var = bdd_ithvar(bdd_vars_to_z3_formula.size());
				Z3_inc_ref(ctx, bool_exp); // This will probably leak memory!
				bdd_vars_to_z3_formula.push_back(bool_exp);
				Z3_formula_to_bdd_map[bool_exp] = new_var;
				return new bdd(new_var);
			}
		}
	}
}

}