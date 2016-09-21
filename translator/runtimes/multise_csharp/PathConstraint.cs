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
			False
		}

		public State state;

		public BranchPoint(State state) 
		{
			this.state = state;
		} 
	}

	public class Frame
	{
		public SCG.List<BranchPoint> BranchPoints = new SCG.List<BranchPoint> ();
		public int idx = -1;

		public bool is_recovering;

		public int pcs_idx;

		public Frame(int pcs_idx)
		{
			this.pcs_idx = pcs_idx;
		}
	}

	protected static Stack<Frame> frames = new Stack<Frame>();
	protected static SCG.List<bdd> pcs = new SCG.List<bdd> ();

	static PathConstraint()
	{
		ctx = new Context();
		solver = ctx.MkSolver();
		
		BuDDySharp.BuDDySharp.cpp_init (10000000, 10000000);
		BuDDySharp.BuDDySharp.setvarnum (10000);

		BDDToZ3Wrap.Converter.Init (ctx);

		pcs.Add (BuDDySharp.BuDDySharp.bddtrue);
		frames.Push (new Frame (0));
	}

	public static bdd GetPC()
	{
		return pcs [pcs.Count - 1];
	}

	public static void PushScope()
	{
		pcs.Add (pcs[pcs.Count - 1]);
	}

	public static void PopScope(int count = 1)
	{
		pcs.RemoveRange(pcs.Count - count, count);
	}

	public static bool TryAddAxiom(bdd bddForm)
	{
		bdd newPC = pcs[pcs.Count - 1].And (bddForm);
		if (newPC.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
			return false;
		} else {
			pcs [pcs.Count - 1] = newPC;
			return true;
		}
	}

	public static bool TryPushFrame(bdd bddForm)
	{
		bdd newPC = pcs[pcs.Count - 1].And (bddForm);
		if (newPC.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
			return false;
		} else {
			frames.Push (new Frame (pcs.Count));
			pcs.Add(newPC);
			return true;
		}
	}

	public static void AddAxiom(bdd bddForm)
	{
		pcs [pcs.Count - 1] = bddForm.And (pcs [pcs.Count - 1]);
	}
		
	public static void PopFrame()
	{
		var top = frames.Pop();
		pcs.RemoveRange (top.pcs_idx, pcs.Count - top.pcs_idx);
	}

	public static Frame GetCurrentFrame()
	{
		return frames.Peek ();
	}

	public static void NewBranchPoint(bdd branchBDD, BranchPoint.State kind) {
		PushScope ();
		AddAxiom (branchBDD);
		var frame = GetCurrentFrame ();
		frame.BranchPoints.Add (new BranchPoint(kind));
	}

	public static bool BacktrackInvocation()
	{
		var frame = GetCurrentFrame ();
		int i;
		for (i = frame.BranchPoints.Count - 1; i >= 0; i--) {
			if (frame.BranchPoints [i].state == BranchPoint.State.Both) {
				break;
			}
		}
		frame.BranchPoints.RemoveRange(i + 1, frame.BranchPoints.Count - i - 1);
		frame.idx = 0;
		return frame.BranchPoints.Count > 0;
	}

	public static bool IsRecovering()
	{
		return GetCurrentFrame().is_recovering;
	}

	public static bool TakeBranch()
	{
		var frame = GetCurrentFrame ();
		var branch_point = frame.BranchPoints [frame.idx];
		frame.idx++;
		switch (branch_point.state) {
		case BranchPoint.State.Both:
			{
				if (frame.BranchPoints.Count == frame.idx) {
					branch_point.state = BranchPoint.State.False;
					frame.is_recovering = false;
					return false;
				} else {
					return true;
				}
			}
		case BranchPoint.State.True:
			{
				return true;
			}
		case BranchPoint.State.False:
			{
				return false;
			}
		default:
			{
				throw new Exception ("Not reachable");
			}
		}
	}

	public static IEnumerable<Tuple<bdd, int>> GetAllPossibleValues(BitVecExpr abstractVal)
	{
		var constraint = GetPC().ToZ3Expr ();
		solver.Push ();
		solver.Assert (constraint);
		Status status = solver.Check ();
		while (status == Status.SATISFIABLE) {
			var pred = constraint.ToBDD ();
			var one_sln = ((BitVecNum)solver.Model.Eval (abstractVal, true));
			yield return new Tuple<bdd, int>(pred, one_sln.Int);
			constraint = ctx.MkEq(abstractVal, one_sln);
			solver.Assert (ctx.MkNot(constraint));
			status = solver.Check ();
		}
		if (status == Status.UNKNOWN) {
			Console.WriteLine ("Solver failure");
		}
		solver.Pop ();
	}

	public static bool SolveBooleanExpr(BoolExpr val)
	{
		var constraint = GetPC().ToZ3Expr();
		solver.Push();
		solver.Assert(constraint);
		solver.Assert(val);
		Status status = solver.Check();
		solver.Pop();
		switch (status) {
		case Status.SATISFIABLE:
			return true;
		case Status.UNSATISFIABLE:
			return false;
		case Status.UNKNOWN:
			{
				Console.WriteLine ("Solver failure");
				return true;
			}
		default:
			{
				throw new Exception ("Unreacable");
			}
		}
	}

	private static int solver_var_cnt = 0;
	public static ValueSummary<SymbolicInteger> NewSymbolicIntVar(string prefix, int ge, ValueSummary<int> lt) {
		var pc = GetPC ();
		var fresh_const = new SymbolicInteger((BitVecExpr)ctx.MkBVConst(String.Format("{0}_{1}", prefix, solver_var_cnt++), SymbolicInteger.INT_SIZE));
		bdd axiom = BuDDySharp.BuDDySharp.bddfalse;
		foreach (var guardedVal in lt.values) {
			axiom = axiom.Or (guardedVal.bddForm.And(pc).And ((fresh_const < guardedVal.value).AbstractValue.ToBDD ()));
		}
		AddAxiom ((ge <= fresh_const).AbstractValue.ToBDD ().And(axiom));
		return new ValueSummary<SymbolicInteger> (fresh_const);
	}

	public static ValueSummary<SymbolicBool> NewSymbolicBoolVar(string prefix) {
		var fresh_const = new SymbolicBool((BoolExpr)ctx.MkBoolConst(String.Format("{0}_{1}", prefix, solver_var_cnt++)));
		return new ValueSummary<SymbolicBool> (fresh_const);
	}
}