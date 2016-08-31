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

	static PathConstraint()
	{
		ctx = new Context();
		solver = ctx.MkSolver();
	}

	public struct BranchPoint
	{
		public enum State 
		{
			Both,
			True,
			False
		}

		State state;

		public BranchPoint(State state) 
		{
			this.state = state;
		} 
	}

	public class Frame
	{
		public Stack<BranchPoint> BranchPoints = new Stack<BranchPoint> ();
		public int idx = -1;

		public int pcs_idx;

		public Frame(int pcs_idx)
		{
			this.pcs_idx = pcs_idx;
		}
	}

	protected static Stack<Frame> frames;
	protected static SCG.List<bdd> pcs;

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

	public static void NewBranchPoint(bdd trueBDD, bdd falseBDD) {
		AddAxiom (trueBDD);

	}

	public static bool BacktrackInvocation()
	{
		if (frames.Peek ().BranchPoints.Count == 0) {
			return false;
		} else {
			frames.Peek ().idx = 0;
			return true;
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
		}
	}
}