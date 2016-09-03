using System;
using Microsoft.Z3;

public struct PBool : IPType<PBool> {
	SymbolicBool value;

	public PBool(SymbolicBool value) {
		this.value = value;		
	}

	public static implicit operator PBool(bool value) 
	{ 
		return new PBool(new SymbolicBool(value)); 
	}


	public static implicit operator PBool(SymbolicBool value) 
    { 
        return new PBool(value); 
    } 

	public static implicit operator SymbolicBool(PBool b) 
    { 
        return b.value; 
    }

	public static PBool operator ==(PBool a, PBool b) 
    { 
        return new PBool(a.value == b.value); 
    }

    public static PBool operator !=(PBool a, PBool b) 
    { 
        return new PBool(a.value != b.value); 
    }

	public static PBool operator !(PBool a) 
	{ 
		return new PBool(!a.value); 
	}

	public static PBool operator &(PBool a, PBool b) 
	{ 
		return new PBool(a.value & b.value); 
	}

	public static PBool operator |(PBool a, PBool b) 
	{ 
		return new PBool(a.value | b.value); 
	}

	public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PBool> other)
	{
		return this.value == other.value.value;
	}

	public ValueSummary<SymbolicInteger> PTypeGetHashCode()
	{
		if (this.value.IsAbstract()) {
			return new SymbolicInteger((BitVecExpr)PathConstraint.ctx.MkITE(this.value.AbstractValue, PathConstraint.ctx.MkBV(1, SymbolicInteger.INT_SIZE), PathConstraint.ctx.MkBV(0, SymbolicInteger.INT_SIZE)));
		} else {
			return new SymbolicInteger(this.value.ConcreteValue ? 1 : 0);
		}
	}

	public ValueSummary<PBool> DeepCopy() {
		return this;
	}
}