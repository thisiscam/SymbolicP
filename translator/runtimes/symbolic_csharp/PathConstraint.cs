using Microsoft.Z3;
using System;
using System.Diagnostics;
using System.Collections.Generic;

public interface IPathConstraint {

	bool Done { get; }

	bool CurrentBool(Solver solver, BoolExpr abstractVal);
	int CurrentInt(Solver solver, BitVecExpr abstractVal);
	
	bool NextBool(Solver solver, BoolExpr abstractVal);
	int NextInt(Solver solver, BitVecExpr abstractVal);
}


public struct BoolPathConstraint : IPathConstraint {
	bool explored;
	bool done;
	
	public BoolPathConstraint(Solver solver, BoolExpr abstractVal) {
		solver.Check ();
		var solverResult = solver.Model.Eval(abstractVal, true);
		switch(solverResult.BoolValue) {
			case Z3_lbool.Z3_L_TRUE: {
				explored = true;
				done = true;
				solver.Assert(abstractVal);
				break;
			}
			case Z3_lbool.Z3_L_FALSE: {
				explored = false;
				done = true;
				solver.Assert(SymbolicEngine.ctx.MkNot(abstractVal));
				break;
			}
			case Z3_lbool.Z3_L_UNDEF: {
				explored = true;
				done = false;
				solver.Assert(abstractVal);
				break;
			}
			default: {
				explored = true;
				done = false;
				throw new SystemException ("Unreachable");
			}
		}
		solver.Push();
	}

	public bool Done { get { return done; }}
	
	public bool CurrentBool(Solver solver, BoolExpr abstractVal) { 
		return explored;
	}

	public int CurrentInt(Solver solver, BitVecExpr abstractVal) { 
		throw new SystemException("Invalid get value from bool path constraint");
	}

	public bool NextBool(Solver solver, BoolExpr abstractVal) {
		solver.Push();
		this.explored = !this.explored;
		this.done = true;
		solver.Assert(this.explored ? abstractVal : SymbolicEngine.ctx.MkNot(abstractVal));
		return this.explored;
	}
		
	public int NextInt(Solver solver, BitVecExpr abstractVal) {
		throw new SystemException("Invalid get value from bool path constraint");
	}
}

public struct IntPathConstraint : IPathConstraint {
	System.Collections.Generic.List<BoolExpr> notEqs;
	BoolExpr currentEq;
	
	int currentVal;
	BitVecNum nextVal;
	
	public IntPathConstraint(Solver solver, BitVecExpr abstractVal) {
		notEqs = new System.Collections.Generic.List<BoolExpr> ();
		var status = solver.Check();
		Debug.Assert(status == Status.SATISFIABLE);
		var tmp = solver.Model.Eval (abstractVal, true);
		BitVecNum result = (BitVecNum)tmp;
		var eq = SymbolicEngine.ctx.MkEq(abstractVal, result);
		var not_eq = SymbolicEngine.ctx.MkNot(eq);
		currentEq = eq;
		currentVal = result.Int;
		solver.Push ();
		solver.Assert (not_eq);
		if(solver.Check () == Status.UNSATISFIABLE) {
			nextVal = null;
		} else {
			notEqs.Add(not_eq);
			nextVal = (BitVecNum)solver.Model.Evaluate(abstractVal);
		}
		solver.Pop ();
		solver.Assert(currentEq);
		solver.Push();
	}
	public bool Done { get { return nextVal == null; }}

	public bool CurrentBool(Solver solver, BoolExpr abstractVal) { 
		throw new SystemException("Invalid get value from int path constraint"); 
	}

	public int CurrentInt(Solver solver, BitVecExpr abstractVal) {
		return currentVal;
	}

	public bool NextBool(Solver solver, BoolExpr abstractVal) {
		throw new SystemException("Invalid get value from int path constraint");
	}

	public int NextInt(Solver solver, BitVecExpr abstractVal) {
		solver.Push();
		foreach(var constraint in notEqs) {
			solver.Assert(constraint);
		}
		currentEq = SymbolicEngine.ctx.MkEq(abstractVal, nextVal);
		currentVal = nextVal.Int;
		var not_eq = SymbolicEngine.ctx.MkNot(currentEq);
		solver.Assert (not_eq);
		if(solver.Check() == Status.UNSATISFIABLE) {
			nextVal = null;
		} else {
			notEqs.Add(not_eq);
			nextVal = (BitVecNum) solver.Model.Evaluate (abstractVal);
		}
		solver.Pop ();
		solver.Assert(currentEq);
		solver.Push ();
		return currentVal;
	}

}