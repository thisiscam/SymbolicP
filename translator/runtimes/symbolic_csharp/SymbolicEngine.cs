using Microsoft.Z3;
using System.Collections.Generic;
using System.Diagnostics;

public class SymbolicEngine {
	public static Context ctx = new Context();
	public static SymbolicEngine SE = new SymbolicEngine();


	private Solver solver;
	private System.Collections.Generic.List<IPathConstraint> pathConstraints = new System.Collections.Generic.List<IPathConstraint>();
	private int idx;


	public SymbolicEngine() {
		this.solver = ctx.MkSolver();
	}

	public bool Reset() {
		for(int i=pathConstraints.Count-1; i >=0; i--) {
			if(!pathConstraints[i].Done) {
				solver.Pop((uint)(pathConstraints.Count - i));
				pathConstraints.RemoveRange(i, pathConstraints.Count - i);
				return true;
			}
		}
		return false;
	}

	public bool NextBranch(BoolExpr abstractVal) {
		if (idx < pathConstraints.Count - 1) {
			/* Recovering program state */
		bool explored_branch = pathConstraints [idx].CurrentBool (solver, abstractVal);
			idx++;
			return explored_branch;
		} else if (idx >= pathConstraints.Count) {
			/* encountering new branch */
		var constraint = new BoolPathConstraint (solver, abstractVal);
			pathConstraints.Add (constraint);
			idx++;
		return constraint.CurrentBool (solver, abstractVal);
		} else {
			/* Forcing branch to other evaluation */
			Debug.Assert (pathConstraints [idx].Done == false);
			idx++;
		return pathConstraints [idx].NextBool (solver, abstractVal);
		}
	}

	public int NextIndex(BitVecExpr abstractVal) {
		if (idx < pathConstraints.Count - 1) {
			/* Recovering program state */
			int explored_idx = pathConstraints [idx].CurrentInt (solver, abstractVal);
			idx++;
			return explored_idx;
		} else if (idx >= pathConstraints.Count) {
			/* Encountering a new get index */
		var constraint = new IntPathConstraint (solver, abstractVal);
			pathConstraints.Add (constraint);
			idx++;
		return constraint.CurrentInt (solver, abstractVal);
		} else {
		int chosen = pathConstraints [idx].NextInt (solver, abstractVal);
			return chosen;
		}
	}
}