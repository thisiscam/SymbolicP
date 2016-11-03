using Microsoft.Z3;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using BuDDySharp;
using SCG = System.Collections.Generic;
using BDDToZ3Wrap;
using System.Linq;

public static partial class PathConstraint
{
	public static Context ctx;
	public static Solver solver;

	static SCG.List<bdd> pcs = new SCG.List<bdd>();

	static PathConstraint()
	{
		ctx = new Context();
		solver = ctx.MkSolver();
		var options = Program.options;
		
		BuDDySharp.BuDDySharp.cpp_init(options.BDDNumInitialNodes, options.BDDNumInitialNodes);
		BuDDySharp.BuDDySharp.setcacheratio(options.BDDCacheRatio);
		BuDDySharp.BuDDySharp.setvarnum(options.BDDNumVars);
		BuDDySharp.BuDDySharp.setmaxincrease(options.BDDMaxIncrease);
		
		BDDToZ3Wrap.Converter.Init(ctx);

		pcs.Add(BuDDySharp.BuDDySharp.bddtrue);

		InitSymVar();
	}

	static SCG.List<SymbolicBool> sym_bool_vars;
	static ValueSummary<int> solver_bool_var_cnt;
	
	public static void Reset()
	{
		solver.Reset();
		pcs.Clear();
		pcs.Add(BuDDySharp.BuDDySharp.bddtrue);
		decision_cnt = 0;
		solver_bool_var_cnt = 0;
	}

	private static void InitSymVar()
	{
		sym_bool_vars = new SCG.List<SymbolicBool>();
		solver_bool_var_cnt = 0;
	}

	public static bdd GetPC()
	{
		return pcs[pcs.Count - 1];
	}

	public static void PushScope()
	{
		pcs.Add(pcs[pcs.Count - 1]);
	}

	public static void PopScope(int count = 1)
	{
		pcs.RemoveRange(pcs.Count - count, count);
	}

	public static void PushFrame()
	{
		PushScope();
	}

	public static void AddAxiom(bdd bddForm)
	{
		pcs[pcs.Count - 1] = bddForm.And(pcs[pcs.Count - 1]);
	}

	public static void PopFrame()
	{
		PopScope();
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
		PushScope();
		return LoopPoint.NewLoopPoint();
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
		return BuDDySharp.BuDDySharp.bddtrue.FormulaBDDSolverSAT();
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
		var ret = BuDDySharp.BuDDySharp.bddtrue;
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
			var ret = BuDDySharp.BuDDySharp.bddfalse;
			for(int i=idx; i < Math.Pow(2, vars.Length); i++)
			{
				ret = ret.Or(DecisionTreeFromIdx(vars, i));
			}
			return ret;
		}
		return DecisionTreeFromIdx(vars, idx);
	}
	
	public static ValueSummary<int> ChooseRandomIndex(ValueSummary<int> count)
	{
		var pc = GetPC();
		var max_count = count.values.Max((GuardedValue<int> arg) => 
			{	
				if(!arg.bddForm.And(pc).EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
					return arg.value;
				} else {
					return 0;
				}
			}
		);
		if(max_count < 1) {
			Debugger.Break();
		}
		// Create BDD vars
		var num_decision_vars = (int) Math.Ceiling(Math.Log(max_count, 2));
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
		var ret = BuDDySharp.BuDDySharp.bddtrue;
		int i = 0;
		foreach(var b in BDDToZ3Wrap.Converter.GetAllUsedFormulas())
		{
			solver.Check();
			var v = (BoolExpr)solver.Model.Evaluate(b, true);
			solver.Assert(ctx.MkEq(v, b));
			if(v.BoolValue == Z3_lbool.Z3_L_TRUE) {
				ret = ret.And(BuDDySharp.BuDDySharp.ithvar(i));
			} else if(v.BoolValue == Z3_lbool.Z3_L_FALSE) {
				ret = ret.And(BuDDySharp.BuDDySharp.nithvar(i));
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
		return !x.EqualEqual(BuDDySharp.BuDDySharp.bddfalse);
	}
	public static bool FormulaBDDSolverSAT(this bdd x)
	{
		return !x.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) 
			&& (!BuDDySharp.BuDDySharp.not_pure_bool(x) || SolveBooleanExpr(x.ToZ3Expr()));
	}
	public static bool FormulaSolverSAT(this bdd x)
	{
		return SolveBooleanExpr(x.ToZ3Expr());
	}
#endregion	
}