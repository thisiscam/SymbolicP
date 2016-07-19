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

	public static bool operator true(PBool op)
	{
		return op.value;
	}

	public static bool operator false(PBool op)
	{
		return !op.value;
	}

	public SymbolicBool PTypeEquals(PBool other)
	{
		return this.value == other.value;
	}

	public SymbolicInteger PTypeGetHashCode()
	{
		if (this.value.IsAbstract()) {
			return new SymbolicInteger((BitVecExpr)SymbolicEngine.ctx.MkITE(this.value.AbstractValue, SymbolicEngine.ctx.MkBV(1, SymbolicInteger.INT_SIZE), SymbolicEngine.ctx.MkBV(0, SymbolicInteger.INT_SIZE)));
		} else {
			return new SymbolicInteger(this.value.ConcreteValue ? 1 : 0);
		}
	}

	public PBool DeepCopy() {
		return this;
	}
}