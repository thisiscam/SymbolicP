using Microsoft.Z3;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using SCG = System.Collections.Generic;
using BDDToZ3Wrap;
using System.Linq;
using System.Runtime.InteropServices;

#if USE_SYLVAN
using bdd = SylvanSharp.bdd;
using BDDLIB = SylvanSharp.SylvanSharp;
#else
using bdd = BuDDySharp.bdd;
using BDDLIB = BuDDySharp.BuDDySharp;
#endif

public static partial class PathConstraint
{
	public static Context ctx;
	public static Solver solver;

#if !USE_SYLVAN
	static bdd pc;
#endif

	static PathConstraint()
	{
		ctx = new Context();
		solver = ctx.MkSolver();
		var options = Program.options;

#if USE_SYLVAN
		SylvanSharp.Lace.Init(options.SylvanLaceNWorkers, options.SylvanLaceStack);
		BDDLIB.init(options.SylvanNumInitialNodesLg2, options.SylvanNumMaxNodesLg2, 
					options.SylvanNumInitialCachesizeLg2, options.SylvanNumMaxCachesizeLg2,
					options.SylvanGranularity
		);
		AppDomain.CurrentDomain.ProcessExit += (sender, e) => {
			BDDLIB.quit();
			SylvanSharp.Lace.Exit();
		};
#else
		BDDLIB.init(options.BDDNumInitialNodes, options.BDDNumInitialNodes / options.BDDCacheRatio);
		var x = BDDLIB.setcacheratio(options.BDDCacheRatio);
		x = BDDLIB.setvarnum(options.BDDNumVars);
		Console.WriteLine(BDDLIB.varnum());
		x = BDDLIB.setmaxincrease(options.BDDMaxIncrease);
		BDDLIB.reorder_verbose(2);
		BDDLIB.autoreorder(BDDLIB.BDD_REORDER_NONE);
#endif
		BDDToZ3Wrap.Converter.Init(ctx);
		
		SetPCTrue();

		InitSymVar();
	}
	
	private static void SetPCTrue()
	{
#if USE_SYLVAN
		var handle = GCHandle.Alloc(bdd.bddtrue);
		BDDToZ3Wrap.PInvoke.set_task_pc(GCHandle.ToIntPtr(handle));
#else
		pc = bdd.bddtrue;
#endif
	}
	
	static SCG.List<SymbolicBool> sym_bool_vars;
	static ValueSummary<int> solver_bool_var_cnt;
	
	private static bool recovering = false;
	public static void BeginRecover()
	{
		solver.Reset();
		SetPCTrue();
		decision_cnt = 0;
		solver_bool_var_cnt = 0;
		sym_bool_vars = new SCG.List<SymbolicBool>();
		recovering = true;
	}

	private static void InitSymVar()
	{
		sym_bool_vars = new SCG.List<SymbolicBool>();
		solver_bool_var_cnt = 0;
	}
	
	public static bdd GetPC()
	{
#if USE_SYLVAN
	 	return (bdd)GCHandle.FromIntPtr(BDDToZ3Wrap.PInvoke.get_task_pc()).Target;
#else
		return pc;
#endif
	}

	public static void AddAxiom(bdd bddForm)
	{
#if USE_SYLVAN
		var handle = GCHandle.FromIntPtr(BDDToZ3Wrap.PInvoke.get_task_pc());
		var newPC = ((bdd)handle.Target).And(bddForm);
		handle.Free();
		var newPCHandle = GCHandle.Alloc(newPC);
		BDDToZ3Wrap.PInvoke.set_task_pc(GCHandle.ToIntPtr(newPCHandle));
#else
		pc = bddForm.And(pc);
#endif
	}
	
	public static void RestorePC(bdd oldPC)
	{
#if USE_SYLVAN
		var handle = GCHandle.FromIntPtr(BDDToZ3Wrap.PInvoke.get_task_pc());
		handle.Free();
		var oldPCHandle = GCHandle.Alloc(oldPC, GCHandleType.Pinned);
		BDDToZ3Wrap.PInvoke.set_task_pc(oldPCHandle.AddrOfPinnedObject());
#else
		pc = oldPC;
#endif
	}
	
#region record_return_paths
	public static void RecordReturnPath<T>(ValueSummary<T> ret, ValueSummary<T> val, BranchPoint bp)
	{
		ret.Merge(val);
		RecordReturnPath(bp);
	}
	
	public static void RecordReturnPath(BranchPoint bp)
	{
		var pc = GetPC();
		bp.RecordReturn(true, pc);
	}
	
	public static void RecordReturnPath<T>(ValueSummary<T> ret, ValueSummary<T> val, LoopPoint bp)
	{
		ret.Merge(val);
		RecordReturnPath(bp);
	}
	
	public static void RecordReturnPath(LoopPoint bp)
	{
		var pc = GetPC();
		bp.RecordReturn(true, pc);
	}

	public static void RecordReturnPath<T>(ValueSummary<T> ret, ValueSummary<T> val)
	{
		ret.Merge(val);
	}
	
	public static void RecordReturnPath()
	{
		
	}
#endregion

	public static LoopPoint BeginLoop()
	{
		return LoopPoint.NewLoopPoint(GetPC());
	}

	public static Stopwatch SolverStopWatch = new Stopwatch();
	
	public static IEnumerable<Tuple<bdd, int>> GetAllPossibleValues(BitVecExpr abstractVal)
	{
		var constraint = GetPC().ToZ3Expr();
		SolverStopWatch.Start();
		solver.Push();
		solver.Assert(constraint);
		Status status = solver.Check();
		SolverStopWatch.Stop();
		var ret = new SCG.List<Tuple<bdd, int>>();
		while (status == Status.SATISFIABLE) {
			SolverStopWatch.Start();
			var one_sln = ((BitVecNum)solver.Model.Eval(abstractVal, true));
			SolverStopWatch.Stop();
			constraint = ctx.MkEq(abstractVal, one_sln);
			ret.Add(new Tuple<bdd, int>(constraint.ToBDD(), one_sln.Int));
			SolverStopWatch.Start();
			solver.Assert(ctx.MkNot(constraint));
			status = solver.Check();
			SolverStopWatch.Stop();
		}
		foreach(var t1 in ret)
		{
			var bddForm = t1.Item1;
			foreach(var t2 in ret)
			{
				if(t1 != t2) { bddForm = bddForm.And(t2.Item1.Not()); }
			}
			yield return new Tuple<bdd, int>(bddForm, t1.Item2);
		}
		if (status == Status.UNKNOWN) {
			Console.WriteLine("Solver failure");
		}
		solver.Pop();
	}

	public static bool SolveBooleanExpr(BoolExpr val)
	{
		var constraint = GetPC().ToZ3Expr();
		SolverStopWatch.Start();
		solver.Push();
		solver.Assert(ctx.MkAnd(constraint, val));
		Status status = solver.Check();
		solver.Pop();
		SolverStopWatch.Stop();
		switch (status) {
			case Status.SATISFIABLE: {
					return true;
				}
			case Status.UNSATISFIABLE: {
					return false;
				}
			case Status.UNKNOWN:
			default: {
					throw new Exception("Solver failure");
				}
		}
	}

	public static bool EvalPc()
	{
		return bdd.bddtrue.FormulaBDDSolverSAT();
	}

	public static ValueSummary<SymbolicBool> NewSymbolicBoolVar(string prefix)
	{
		return Allocate<SymbolicBool>((idx) => 
			{
				var sym_var_name = String.Format("{0}_{1}", prefix, idx);
				var fresh_const = new SymbolicBool(ctx.MkBoolConst(sym_var_name).ToBDD());
				return fresh_const;
			}, sym_bool_vars, solver_bool_var_cnt);
	}
	
	public static ValueSummary<T> Allocate<T>(Func<int, T> f, SCG.List<T> allAllocated, ValueSummary<int> counter)
    {
    	var ret = new ValueSummary<T>();
    	var pc = PathConstraint.GetPC();
    	foreach(var allocCnt in counter.values)
    	{
    		var bddForm = allocCnt.bddForm.And(pc);
    		if(bddForm.FormulaBDDSAT())
    		{
    			if(allocCnt.value >= allAllocated.Count)
    			{
    				allAllocated.Add(f.Invoke(allocCnt.value));
				}
				ret.AddValue(bddForm, allAllocated[allocCnt.value]);
			}
		}
		counter.Increment();
		return ret;
	}
	
	private static int decision_cnt = 0;
	private static bdd DecisionTreeFromIdx(bdd[] vars, int idx)
	{
		var ret = bdd.bddtrue;
		for(int i=0; i < vars.Length; i++) {
			if(idx % 2 == 0) {
				ret = ret.And(vars[i].Not());
			} else {
				ret = ret.And(vars[i]);
			}
			idx /= 2;
		}
		return ret;
	}
	
	private static bdd DecisionTreeFromIdx(bdd[] vars, int idx, int max_decisions)
	{
		if(idx == max_decisions - 1)
		{
			var ret = bdd.bddfalse;
			for(int i=idx; i < Math.Pow(2, vars.Length); i++)
			{
				ret = ret.Or(DecisionTreeFromIdx(vars, i));
			}
			return ret;
		}
		return DecisionTreeFromIdx(vars, idx);
	}
	
	private static SCG.List<int> num_decision_vars_history = new SCG.List<int>();
	public static ValueSummary<int> ChooseRandomIndex(ValueSummary<int> count)
	{
		var pc = GetPC();
		int num_decision_vars = -1;
		if(recovering) {
			num_decision_vars = num_decision_vars_history[decision_cnt];
		} else {
			var max_count = count.values.Max((GuardedValue<int> arg) => 
				{	
					if(!arg.bddForm.And(pc).EqualEqual(bdd.bddfalse)) {
						return arg.value;
					} else {
						return 0;
					}
				}
			);
			Debug.Assert(max_count >= 1);
			// Create BDD vars
			num_decision_vars = (int) Math.Ceiling(Math.Log(max_count, 2));
			num_decision_vars_history.Add(num_decision_vars);
		}
		var all_vars = new bdd[num_decision_vars];
		for(int i=0; i < num_decision_vars; i++)
		{
			var sym_var_name = String.Format("decision_{0}_{1}", decision_cnt, i);
			var fresh_const = new SymbolicBool(ctx.MkBoolConst(sym_var_name).ToBDD());
			all_vars[i] = fresh_const.AbstractValue;
		}
		decision_cnt++;
		ValueSummary<int> idx = new ValueSummary<int>();
		foreach(var guardedCnt in count.values)
		{
			var bddForm = guardedCnt.bddForm.And(pc);
			for(int i=0; i < guardedCnt.value; i++) {
				idx.AddValue(bddForm.And(DecisionTreeFromIdx(all_vars, i, guardedCnt.value)), i);
			}
		}
		return idx;
	}
	
	public static bdd ExtractOneCounterExampleFromAggregatePC(bdd aggregatePC)
	{
		solver.Push();
		solver.Assert(aggregatePC.ToZ3Expr());
		var ret = bdd.bddtrue;
		int i = 0;
		foreach(var b in BDDToZ3Wrap.Converter.GetAllUsedFormulas())
		{
			solver.Check();
			var v = (BoolExpr)solver.Model.Evaluate(b, true);
			solver.Assert(ctx.MkEq(v, b));
			if(v.BoolValue == Z3_lbool.Z3_L_TRUE) {
				ret = ret.And(bdd.ithvar(i));
			} else if(v.BoolValue == Z3_lbool.Z3_L_FALSE) {
				ret = ret.And(bdd.nithvar(i));
			} else {
				throw new Exception("Not reachable");
			}
			i++;
		}
		solver.Pop();
		return ret;
	}
	
	public static bool DebugProofEquivalence(BoolExpr e1, BoolExpr e2)
	{
		solver.Push();
		var eq = ctx.MkIff(e1, e2);
		solver.Assert(eq);
		var status = solver.Check();
		solver.Pop();
		switch (status) 
		{
			case Status.SATISFIABLE:
				return true;
			case Status.UNSATISFIABLE:
				return false;
			case Status.UNKNOWN:
				throw new Exception("error");
		}
		return false;
 	}
 	
#region BDD_Z3_Ext
	public static bool FormulaBDDSAT(this bdd x)
	{
		return !x.EqualEqual(bdd.bddfalse);
	}
	public static bool FormulaBDDSolverSAT(this bdd x)
	{
		return !x.EqualEqual(bdd.bddfalse) 
			&& (!x.not_pure_bool() || SolveBooleanExpr(x.ToZ3Expr()));
	}
	public static bool FormulaSolverSAT(this bdd x)
	{
		return SolveBooleanExpr(x.ToZ3Expr());
	}
#endregion	
}