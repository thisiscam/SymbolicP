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
		idx = 0;
		intVarRecoveredIdx = 0;
		boolVarRecoveredIdx = 0;
	}

	public bool Reset() {
		idx = 0;
		intVarRecoveredIdx = 0;
		boolVarRecoveredIdx = 0;
		for(int i=pathConstraints.Count-1; i >=0; i--) {
			if(!pathConstraints[i].Done) {
				solver.Pop((uint)(pathConstraints.Count - i));
				pathConstraints.RemoveRange(i + 1, pathConstraints.Count - i - 1);
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
			idx++;
			return chosen;
		}
	}

	private System.Collections.Generic.List<SymbolicInteger> intVars = new System.Collections.Generic.List<SymbolicInteger>();
	private int intVarRecoveredIdx;
	public SymbolicInteger NewSymbolicIntVar(string prefix, SymbolicInteger ge, SymbolicInteger lt) {
		if (idx < pathConstraints.Count - 1) {
			var recovered = intVars [intVarRecoveredIdx];
			intVarRecoveredIdx++;
			return recovered;
		} else {
			if (intVarRecoveredIdx < this.intVars.Count - 1) {
				intVars.RemoveRange (intVarRecoveredIdx + 1, intVars.Count - intVarRecoveredIdx - 1);
			}
			var fresh_const = new SymbolicInteger((BitVecExpr)SymbolicEngine.ctx.MkFreshConst (prefix, SymbolicInteger.bitVecSort));
			solver.Assert((ge <= fresh_const).AbstractValue, (fresh_const < lt).AbstractValue);
			intVars.Add (fresh_const);
			intVarRecoveredIdx++;
			return fresh_const;
		}
	}

	private System.Collections.Generic.List<SymbolicBool> boolVars = new System.Collections.Generic.List<SymbolicBool>();
	private int boolVarRecoveredIdx;
	public SymbolicBool NewSymbolicBoolVar(string prefix) {
		if (idx < pathConstraints.Count - 1) {
			var recovered = boolVars [boolVarRecoveredIdx];
			boolVarRecoveredIdx++;
			return recovered;
		} else {
			if (boolVarRecoveredIdx < boolVars.Count - 1) {
				boolVars.RemoveRange (boolVarRecoveredIdx + 1, boolVars.Count - boolVarRecoveredIdx - 1);
			}
			var fresh_const = new SymbolicBool((BoolExpr)SymbolicEngine.ctx.MkFreshConst (prefix, SymbolicEngine.ctx.BoolSort));
			boolVars.Add (fresh_const);
			boolVarRecoveredIdx++;
			return fresh_const;
		}
	}
}