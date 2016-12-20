using System;
using Microsoft.Z3;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security;

#if USE_SYLVAN
using bdd = SylvanSharp.bdd;
using BDDLIB = SylvanSharp.SylvanSharp;
using BDD = System.Int64;
#else
using bdd = BuDDySharp.bdd;
using BDDLIB = BuDDySharp.BuDDySharp;
using BDD = System.Int32;
#endif

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

#if USE_SYLVAN
			BDDLIB.sylvan_gc_hook_pregc(() => {
				GC.Collect();
				GC.WaitForPendingFinalizers();
				Console.WriteLine("Pre GC");
			});
			BDDLIB.sylvan_gc_hook_postgc(() => {
				Console.WriteLine("Post GC");
			});
#else
			unsafe {
				BDDLIB.gbc_hook((arg0, arg1) => {
					if(arg0 == 1) {
						GC.Collect();
						GC.WaitForPendingFinalizers();
						BDDLIB.default_gbchandler(0, arg1);
					} else if (arg0 == 0) {
						BDDLIB.default_gbchandler(0, arg1);
					}
				});
			}
#endif
		}
		
		public static Stopwatch Watch = new Stopwatch();
		public static BoolExpr ToZ3Expr(this bdd x) {
			Watch.Start();
			var y = PInvoke.bdd_to_Z3_formula(x.Id);
			Watch.Stop();
			var ret = (BoolExpr)ctx.WrapAST (y);
			return ret;
		}

		public static bdd ToBDD(this BoolExpr expr) {
			return new bdd(PInvoke.Z3_formula_to_bdd(ctx.UnwrapAST (expr)));
		}
		
		public static BoolExpr GetithZ3Expr(int i)
		{
			return (BoolExpr)ctx.WrapAST(PInvoke.get_ith_Z3_formula(i));
		}
		
		public static int get_num_used_formulas()
		{
			return PInvoke.get_num_formulas();
		}
		
		public static IEnumerable<BoolExpr> GetAllUsedFormulas()
		{
			var n = get_num_used_formulas();
			for(int i=0; i < n; i++)
			{
				yield return GetithZ3Expr(i);
			}
		}
		
		public static bdd OneSat(bdd aggregatePC)
		{
			return new bdd(PInvoke.find_one_sat(aggregatePC.Id), false);
		}
	}
	
	public static class PInvoke
	{

#if USE_SYLVAN 
		const string DLLNAME = "BDD_SYLVAN_Z3_WraP";
#else
		const string DLLNAME = "BDD_BUDDY_Z3_Wrap";
#endif

		[SuppressUnmanagedCodeSecurity]		
		[DllImport(DLLNAME)]
		public extern static void init_bdd_z3_wrap(IntPtr ctx); 

		[SuppressUnmanagedCodeSecurity]		
		[DllImport(DLLNAME)]
		public extern static IntPtr bdd_to_Z3_formula(BDD a0);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLLNAME)]
		public extern static BDD Z3_formula_to_bdd(IntPtr ast);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLLNAME)]
		public extern static IntPtr get_ith_Z3_formula(int i);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLLNAME)]
		public extern static int get_num_formulas();
	
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLLNAME)]
		public extern static void debug_print_used_bdd_vars();
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLLNAME)]
		public unsafe extern static void force_set_task_pc(BDD bdd);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLLNAME)]
		public unsafe extern static void set_task_pc(BDD bdd);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLLNAME)]
		public unsafe extern static BDD get_task_pc();

		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLLNAME)]
		public unsafe extern static void task_pc_addaxiom(BDD bdd);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLLNAME)]
		public unsafe extern static BDD find_one_sat(BDD bdd);
	}
}

