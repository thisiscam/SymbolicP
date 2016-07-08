using System;
using Microsoft.Z3;

public struct SymbolicBool {
	bool concreteValue;
	BoolExpr abstractValue;

	public SymbolicBool(bool value) {
		this.concreteValue = value;
		this.abstractValue = null;
	}

	public SymbolicBool(BoolExpr value) {
		this.concreteValue = false;
		this.abstractValue = value;
	}

	public bool IsAbstract() {
        return abstractValue != null;
    }

	public static implicit operator SymbolicBool(bool value) 
    { 
        return new SymbolicBool(value); 
    } 

    public static implicit operator bool(SymbolicBool b) 
    { 
    	if(!b.IsAbstract()) {
    		return b.concreteValue;
    	} else {
    		throw new SystemException("Cannot convert abstract bool");
    	}

    }

	public static SymbolicBool operator ==(SymbolicBool a, SymbolicBool b) 
    { 
    	if(a.IsAbstract()) {
    		if(b.IsAbstract()) {
    			return new SymbolicBool(Z3Wrapper.ctx.MkEq(a.abstractValue, b.abstractValue));
    		} else {
    			return new SymbolicBool(Z3Wrapper.ctx.MkEq(a.abstractValue, Z3Wrapper.ctx.MkBool(b.concreteValue)));
    		}
    	} else {
    		if(b.IsAbstract()) {
    			return new SymbolicBool(Z3Wrapper.ctx.MkEq(Z3Wrapper.ctx.MkBool(a.concreteValue), b.abstractValue));
			} else {
				return new SymbolicBool(a.concreteValue == b.concreteValue);
			}
    	}
    }

    public static SymbolicBool operator !=(SymbolicBool a, SymbolicBool b) 
    { 
		if(a.IsAbstract()) {
    		if(b.IsAbstract()) {
    			return new SymbolicBool(Z3Wrapper.ctx.MkNot(Z3Wrapper.ctx.MkEq(a.abstractValue, b.abstractValue)));
    		} else {
    			return new SymbolicBool(Z3Wrapper.ctx.MkEq(a.abstractValue, Z3Wrapper.ctx.MkBool(!b.concreteValue)));
    		}
    	} else {
    		if(b.IsAbstract()) {
    			return new SymbolicBool(Z3Wrapper.ctx.MkEq(Z3Wrapper.ctx.MkBool(!a.concreteValue), b.abstractValue));
			} else {
				return new SymbolicBool(a.concreteValue != b.concreteValue);
			}
    	}    
    }
		
	public static bool operator true(SymbolicBool op)
	{
		return op.concreteValue;
	}

	public static bool operator false(SymbolicBool op)
	{
		return op.concreteValue;
	}

    public SymbolicBool Equals(SymbolicBool other)
	{
	    return this == other;
	}

	public SymbolicInteger GetHashCode()
	{
		if (IsAbstract()) {
			return new SymbolicInteger((BitVecExpr)Z3Wrapper.ctx.MkITE(this.abstractValue, Z3Wrapper.ctx.MkBV(1, SymbolicInteger.INT_SIZE), Z3Wrapper.ctx.MkBV(0, SymbolicInteger.INT_SIZE)));
		} else {
			return new SymbolicInteger(this.concreteValue ? 1 : 0);
		}
	}
}