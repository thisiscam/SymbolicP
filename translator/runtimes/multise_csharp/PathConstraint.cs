using Microsoft.Z3;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using BuDDySharp;
using SCG = System.Collections.Generic;
using BDDToZ3Wrap;


public partial class PathConstraint
{
	public static Context ctx;
	public static Solver solver;

	public class Frame
	{
		public int pcs_idx;

		public Frame(int pcs_idx)
		{
			this.pcs_idx = pcs_idx;
		}
	}

	protected static Stack<Frame> frames = new Stack<Frame>();
	protected static SCG.List<bdd> pcs = new SCG.List<bdd>();

	static PathConstraint()
	{
		ctx = new Context();
		solver = ctx.MkSolver();

		BuDDySharp.BuDDySharp.cpp_init(5000000, 5000000);
		BuDDySharp.BuDDySharp.setcacheratio(64);
		BuDDySharp.BuDDySharp.setvarnum(10000);

		BDDToZ3Wrap.Converter.Init(ctx);

		pcs.Add(BuDDySharp.BuDDySharp.bddtrue);
		frames.Push(new Frame(0));

		InitSymVar();
	}

	protected static SCG.List<SymbolicBool> sym_bool_vars;
	protected static ValueSummary<int> solver_bool_var_cnt;
	protected static SCG.List<SymbolicInteger> sym_int_vars;
	protected static ValueSummary<int> solver_int_var_cnt;

	private static void InitSymVar()
	{
		sym_bool_vars = new SCG.List<SymbolicBool>();
		solver_bool_var_cnt = 0;
		sym_int_vars = new SCG.List<SymbolicInteger>();
		solver_int_var_cnt = 0;
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
		frames.Push(new Frame(pcs.Count - 1));
	}

	public static void AddAxiom(bdd bddForm)
	{
		pcs[pcs.Count - 1] = bddForm.And(pcs[pcs.Count - 1]);
	}

	public static void PopFrame()
	{
		frames.Pop();
		PopScope();
	}

	public static Frame GetCurrentFrame()
	{
		return frames.Peek();
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
		return SolveBooleanExpr(ctx.MkTrue());
	}

	public static ValueSummary<SymbolicInteger> NewSymbolicIntVar(string prefix, int ge, ValueSummary<int> lt)
	{
		var ret = new ValueSummary<SymbolicInteger>();
		var pc = GetPC();
		foreach (var guardedCnt in solver_int_var_cnt.values) {
			var bddForm = pc.And(guardedCnt.bddForm);
			if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) && SolveBooleanExpr(bddForm.ToZ3Expr())) {
				var idx = guardedCnt.value;
				if (idx < sym_int_vars.Count) {
					ret.AddValue(bddForm, sym_int_vars[idx]);
				}
				else {
					var fresh_const = new SymbolicInteger((BitVecExpr)ctx.MkBVConst(String.Format("{0}_{1}", prefix, idx), SymbolicInteger.INT_SIZE));
					sym_int_vars.Add(fresh_const);
					solver.Assert((ge <= fresh_const).AbstractValue);
					var cnt_pred = BuDDySharp.BuDDySharp.bddfalse;
					foreach (var guardedVal in lt.values) {
						solver.Assert(ctx.MkImplies(guardedVal.bddForm.And(bddForm).ToZ3Expr(), (fresh_const < guardedVal.value).AbstractValue));
						cnt_pred = cnt_pred.Or(guardedVal.bddForm);
					}
					ret.AddValue(bddForm.And(cnt_pred), fresh_const);
				}
			}
		}
		solver_int_var_cnt.Increment();
		return ret;
	}

	public static ValueSummary<SymbolicBool> NewSymbolicBoolVar(string prefix)
	{
		var ret = new ValueSummary<SymbolicBool>();
		var pc = GetPC();
		foreach (var guardedCnt in solver_bool_var_cnt.values) {
			var bddForm = pc.And(guardedCnt.bddForm);
			if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
				var idx = guardedCnt.value;
				if (idx < sym_bool_vars.Count) {
					ret.AddValue(bddForm, sym_bool_vars[idx]);
				}
				else {
					var sym_var_name = String.Format("{0}_{1}", prefix, idx);
					var fresh_const = new SymbolicBool((BoolExpr)ctx.MkBoolConst(sym_var_name));
					sym_bool_vars.Add(fresh_const);
					ret.AddValue(bddForm, fresh_const);
				}
			}
		}
		solver_bool_var_cnt.Increment();
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
}