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
		var solverResult = solver.Check(abstractVal);
		switch(solverResult) {
			case Status.SATISFIABLE: {
				explored = true;
				done = true;
				break;
			}
			case Status.UNSATISFIABLE: {
				explored = false;
				done = true;
				break;
			}
			case Status.UNKNOWN: {
				explored = true;
				done = false;
				break;
			}
			default: {
				explored = true;
				done = false;
				break;
			}
		}
	}
	public bool Done { get { return done; }}
	
	public bool CurrentBool(Solver solver, BoolExpr abstractVal) { 
		solver.Push();
		if(explored) {
			solver.Assert(abstractVal);
		} else {
			solver.Assert(SymbolicEngine.ctx.MkNot(abstractVal));
		}
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
		BitVecNum result = (BitVecNum)solver.Model.Evaluate(abstractVal);
		var eq = SymbolicEngine.ctx.MkEq(abstractVal, result);
		var not_eq = SymbolicEngine.ctx.MkNot(eq);
		currentEq = eq;
		currentVal = result.Int;
		if(solver.Check(not_eq) == Status.UNSATISFIABLE) {
			nextVal = null;
		} else {
			notEqs.Add(not_eq);
			nextVal = (BitVecNum)solver.Model.Evaluate(abstractVal);
		}
	}
	public bool Done { get { return nextVal != null; }}

	public bool CurrentBool(Solver solver, BoolExpr abstractVal) { 
		throw new SystemException("Invalid get value from int path constraint"); 
	}

	public int CurrentInt(Solver solver, BitVecExpr abstractVal) {
		solver.Push();
		solver.Assert(currentEq);	
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
		if(solver.Check(not_eq) == Status.UNSATISFIABLE) {
			nextVal = null;
		} else {
			notEqs.Add(not_eq);
			nextVal = (BitVecNum)solver.Model.Evaluate(abstractVal);
		}
		solver.Assert(currentEq);
		return currentVal;
	}

}