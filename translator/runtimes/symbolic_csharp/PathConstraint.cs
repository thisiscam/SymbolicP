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
		var solverResult = solver.Model.Evaluate(abstractVal);
		solver.Push();
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
	System.Collections.Generic.List<int> possible_vals;
	int current_idx;
	public IntPathConstraint(Solver solver, BitVecExpr abstractVal) {
		solver.Push();
		possible_vals = new System.Collections.Generic.List<int>();
		solver.Push();
		Status status = solver.Check();
		while (status == Status.SATISFIABLE) {
			var one_sln = ((BitVecNum)solver.Model.Eval(abstractVal, true));
			possible_vals.Add(one_sln.Int);
			solver.Assert(SymbolicEngine.ctx.MkNot(SymbolicEngine.ctx.MkEq(abstractVal, one_sln)));
			status = solver.Check();
		}
		solver.Pop();
		current_idx = 0;
		solver.Assert(SymbolicEngine.ctx.MkEq(abstractVal, SymbolicEngine.ctx.MkBV(possible_vals[current_idx], SymbolicInteger.INT_SIZE)));
	}
	public bool Done { get { return current_idx == possible_vals.Count - 1; }}

	public bool CurrentBool(Solver solver, BoolExpr abstractVal) { 
		throw new SystemException("Invalid get value from int path constraint"); 
	}

	public int CurrentInt(Solver solver, BitVecExpr abstractVal) {
		return possible_vals[current_idx];
	}

	public bool NextBool(Solver solver, BoolExpr abstractVal) {
		throw new SystemException("Invalid get value from int path constraint");
	}

	public int NextInt(Solver solver, BitVecExpr abstractVal) {
		current_idx++;
		solver.Push();
		solver.Assert(SymbolicEngine.ctx.MkEq(abstractVal, SymbolicEngine.ctx.MkBV(possible_vals[current_idx], SymbolicInteger.INT_SIZE)));
		return possible_vals[current_idx];
	}

}