using Microsoft.Z3;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using BuDDySharp;
using SCG = System.Collections.Generic;
using BDDToZ3Wrap;



public class PathConstraint
{
	public static Context ctx;
	public static Solver solver;

	public struct BranchPoint
	{
		public enum State
		{
			Both,
			True,
			False,
			None
		}

		public State state;

		public bdd trueBDD;
		public bdd falseBDD;

		public BranchPoint(bdd trueBDD, bdd falseBDD, State state)
		{
			this.state = state;
			this.trueBDD = trueBDD;
			this.falseBDD = falseBDD;
		}

		public bool CondTrue()
		{
			var takeBranch = state == State.Both || state == State.True;
			if (takeBranch) {
				PathConstraint.AddAxiom(this.trueBDD);
			}
			return takeBranch;
		}

		public bool CondFalse()
		{
			if (state == State.Both) {
				PathConstraint.pcs[PathConstraint.pcs.Count - 1] = PathConstraint.pcs[PathConstraint.pcs.Count - 2].And(this.falseBDD);
				return true;
			}
			else if (state == State.False) {
				PathConstraint.AddAxiom(this.falseBDD);
				return true;
			}
			else {
				return false;
			}
		}

		public void MergeBranch()
		{
			PathConstraint.MergeBranch();
		}
	}

	public class Frame
	{
		public int pcs_idx;

		public SCG.Stack<bdd> return_stack = new SCG.Stack<bdd>();

		public Frame(int pcs_idx)
		{
			this.pcs_idx = pcs_idx;
			return_stack.Push(BuDDySharp.BuDDySharp.bddfalse);
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

	//public static bool TryAddAxiom(bdd bddForm)
	//{
	//	bdd newPC = pcs[pcs.Count - 1].And (bddForm);
	//	if (newPC.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
	//		return false;
	//	} else {
	//		pcs [pcs.Count - 1] = newPC;
	//		return true;
	//	}
	//}

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
		var top = frames.Pop();
		PopScope();
	}

	public static Frame GetCurrentFrame()
	{
		return frames.Peek();
	}

	public static void RecordReturnPath()
	{
		var pc = GetPC();
		var return_stack = GetCurrentFrame().return_stack;
		var top = return_stack.Pop();
		return_stack.Push(top.Or(pc));
	}

	public static void BeginLoop()
	{
		PushScope();
		PathConstraint.GetCurrentFrame().return_stack.Push(BuDDySharp.BuDDySharp.bddfalse);
	}

	public static void MergeBranch()
	{
		var return_stack = PathConstraint.GetCurrentFrame().return_stack;
		var return_paths = return_stack.Pop();
		var top = return_stack.Pop();
		return_stack.Push(top.Or(return_paths));
		PathConstraint.PopScope();
		PathConstraint.pcs[PathConstraint.pcs.Count - 1] = PathConstraint.pcs[PathConstraint.pcs.Count - 1].And(return_paths.Not());
	}

	public static bool MergedPcFeasible()
	{
		var bdd_f = !GetPC().EqualEqual(BuDDySharp.BuDDySharp.bddfalse);
		var z3_f = EvalPc();
		return z3_f;
	}

	public static IEnumerable<Tuple<bdd, int>> GetAllPossibleValues(BitVecExpr abstractVal)
	{
		var constraint = GetPC().ToZ3Expr();
		solver.Push();
		solver.Assert(constraint);
		Status status = solver.Check();
		while (status == Status.SATISFIABLE) {
			var one_sln = ((BitVecNum)solver.Model.Eval(abstractVal, true));
			constraint = ctx.MkEq(abstractVal, one_sln);
			yield return new Tuple<bdd, int>(constraint.ToBDD(), one_sln.Int);
			solver.Assert(ctx.MkNot(constraint));
			status = solver.Check();
		}
		if (status == Status.UNKNOWN) {
			Console.WriteLine("Solver failure");
		}
		solver.Pop();
	}

	public static bool SolveBooleanExpr(BoolExpr val)
	{
		var constraint = GetPC().ToZ3Expr();
		solver.Push();
		solver.Assert(ctx.MkAnd(constraint, val));
		Status status = solver.Check();
		solver.Pop();
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