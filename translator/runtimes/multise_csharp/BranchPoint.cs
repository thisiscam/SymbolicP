using System;
using System.Diagnostics;

#if USE_SYLVAN
using bdd = SylvanSharp.bdd;
using BDDLIB = SylvanSharp.SylvanSharp;
#else
using bdd = BuDDySharp.bdd;
using BDDLIB = BuDDySharp.BuDDySharp;
#endif

public static partial class PathConstraint
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
		
		private bdd oldPC;

		public BranchPoint(bdd trueBDD, bdd falseBDD, State state, bdd oldPC)
		{
			this.state = state;
			this.trueBDD = trueBDD;
			this.falseBDD = falseBDD;
			this.returnPaths = bdd.bddfalse;
			this.trueBranchAllEscaped = false;
			this.falseBranchAllEscaped = false;
			this.oldPC = oldPC;
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
				RestorePC(this.oldPC);
				AddAxiom(this.falseBDD);
				return true;
			}
			else if (state == State.False) {
				AddAxiom(this.falseBDD);
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
			RestorePC(this.oldPC);
			AddAxiom(this.returnPaths.Not());
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
		bdd oldPC;
		
		public static LoopPoint NewLoopPoint(bdd pc)
		{
			var ret = new LoopPoint();
			ret.AllEscaped = true;
			ret.returnPaths = bdd.bddfalse;
			ret.oldPC = pc;
			ret.CurrentIterationAllEscaped = false;
			return ret;
		}
		
		public void RecordReturn(bool allReturn, bdd returnPaths)
		{
			this.CurrentIterationAllEscaped |= allReturn;
			this.returnPaths = this.returnPaths.Or(returnPaths);
		}

		public bool MergeBranch()
		{
			PathConstraint.RestorePC(oldPC);
			AddAxiom(this.returnPaths.Not());
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