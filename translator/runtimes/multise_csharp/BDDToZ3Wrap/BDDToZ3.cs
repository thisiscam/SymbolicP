using System;
using Microsoft.Z3;
using BuDDySharp;
using System.Runtime.InteropServices;
using System.Reflection;

namespace BDDToZ3Wrap
{
	public static class Converter
	{
		private static Context ctx;

		public static void Init(Context ctx) {
			BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			var nCtx = (IntPtr)ctx.GetType().GetProperty("nCtx", bindFlags).GetValue(ctx, null);
			PInvoke.init_bdd_z3_wrap (nCtx);
			Converter.ctx = ctx;
		}

		public static BoolExpr ToZ3Expr(this bdd x) {
			return (BoolExpr)ctx.WrapAST (PInvoke.bdd_to_Z3_formula(bdd.getCPtr(x)));
		}

		public static bdd ToBDD(this BoolExpr expr) {
			return new bdd(PInvoke.Z3_formula_to_bdd(ctx.UnwrapAST (expr)), true);
		}
	}

	public static class PInvoke
	{
		const string Z3_DLL_NAME = "BDD_Z3_Wrap";

		[DllImport(Z3_DLL_NAME)]
		public extern static void init_bdd_z3_wrap(IntPtr ctx); 

		[DllImport(Z3_DLL_NAME)]
		public extern static IntPtr bdd_to_Z3_formula(HandleRef a0);

		[DllImport(Z3_DLL_NAME)]
		public extern static IntPtr Z3_formula_to_bdd (IntPtr ast);

		[DllImport(Z3_DLL_NAME)]
		public extern static void debug_print_used_bdd_vars();
	}
}

