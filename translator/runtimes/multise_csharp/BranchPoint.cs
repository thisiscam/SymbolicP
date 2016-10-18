using System;
using BuDDySharp;

public partial class PathConstraint
{
	public class BranchPoint
	{
		public enum State
		{
			Both,
			BothWithTrueDone,
			True,
			False,
			None
		}

		public State state;

		public bdd trueBDD;
		public bdd falseBDD;

		private bool trueBranchAllEscaped;
		private bool falseBranchAllEscaped;
		private bdd returnPaths;

		public BranchPoint(bdd trueBDD, bdd falseBDD, State state)
		{
			this.state = state;
			this.trueBDD = trueBDD;
			this.falseBDD = falseBDD;
			this.returnPaths = BuDDySharp.BuDDySharp.bddfalse;
			this.trueBranchAllEscaped = false;
			this.falseBranchAllEscaped = false;
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
				this.state = State.BothWithTrueDone;
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


		bool AllEscaped
		{
			get { 
					return (this.state == State.BothWithTrueDone && this.trueBranchAllEscaped && this.falseBranchAllEscaped)
					|| (this.state == State.True && this.trueBranchAllEscaped)
					|| (this.state == State.False && this.falseBranchAllEscaped); 
			}
		}

		public void RecordReturn(bool allReturn, bdd returnPaths)
		{
			switch (this.state) {
				case State.Both:
				case State.True: {
						this.trueBranchAllEscaped |= allReturn;
						break;
					}
				case State.BothWithTrueDone:
				case State.False: {
						this.falseBranchAllEscaped |= allReturn;
						break;
					}
				default: {
						throw new Exception("Should not reach here");
					}
			}
			this.returnPaths = this.returnPaths.Or(returnPaths);
		}

		public bool MergeBranch()
		{
			PathConstraint.PopScope();
			PathConstraint.pcs[PathConstraint.pcs.Count - 1] = PathConstraint.pcs[PathConstraint.pcs.Count - 1].And(this.returnPaths.Not());
			return !AllEscaped;
		}

		public bool MergeBranch(BranchPoint into)
		{
			into.RecordReturn(this.AllEscaped, this.returnPaths);
			return MergeBranch();
		}

		public bool MergeBranch(LoopPoint into)
		{
			into.RecordReturn(this.AllEscaped, this.returnPaths);
			return MergeBranch();
		}
	}

	public class LoopPoint
	{
		bool AllEscaped;
		bool CurrentIterationAllEscaped;
		bdd returnPaths;
		
		public static LoopPoint NewLoopPoint()
		{
			var ret = new LoopPoint();
			ret.AllEscaped = true;
			ret.returnPaths = BuDDySharp.BuDDySharp.bddfalse;
			return ret;
		}
		
		public void RecordReturn(bool allReturn, bdd returnPaths)
		{
			this.CurrentIterationAllEscaped |= allReturn;
			this.returnPaths = this.returnPaths.Or(returnPaths);
		}

		public bool MergeBranch()
		{
			PathConstraint.PopScope();
			PathConstraint.pcs[PathConstraint.pcs.Count - 1] = PathConstraint.pcs[PathConstraint.pcs.Count - 1].And(this.returnPaths.Not());
			return !this.AllEscaped;
		}

		public bool MergeBranch(BranchPoint into)
		{
			into.RecordReturn(this.AllEscaped, this.returnPaths);
			return MergeBranch();
		}

		public bool MergeBranch(LoopPoint into)
		{
			into.RecordReturn(this.AllEscaped, this.returnPaths);
			return MergeBranch();
		}

		private bool LoopHelper(Func<PathConstraint.BranchPoint> f)
		{
			if (CurrentIterationAllEscaped) {
				return false;
			}
			else {
				var cond = f.Invoke();
				switch (cond.state) {
					case BranchPoint.State.Both: {
							this.AllEscaped = false;
							this.CurrentIterationAllEscaped = false;
							PathConstraint.AddAxiom(cond.trueBDD);
							return true;
						}
					case BranchPoint.State.True: {
							this.CurrentIterationAllEscaped = false;
							PathConstraint.AddAxiom(cond.trueBDD);
							return true;
						}
					case BranchPoint.State.False: {
							this.AllEscaped = false;
							return false;
						}
					default: {
							throw new Exception("Should not reach here");
						}
				}
			}
		}

		public bool Loop(ValueSummary<bool> b)
		{
			return LoopHelper(() => ValueSummaryExt._Cond(b));
		}

		public bool Loop(ValueSummary<SymbolicBool> b)
		{
			return LoopHelper(() => ValueSummaryExt._Cond(b));
		}

		public bool Loop(ValueSummary<PBool> b)
		{
			return LoopHelper(() => ValueSummaryExt._Cond(b));
		}
	}
}