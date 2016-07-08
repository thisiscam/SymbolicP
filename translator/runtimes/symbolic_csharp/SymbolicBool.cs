using System;

public struct SymbolicBool : ISymbolicHashable<SymbolicBool> {
	bool concreteValue;
	BoolExpr abstractValue;

	public SymbolicBool(bool value) {
		this.concreteValue = value;		
	}

	public SymbolicBool(BoolExpr value) {
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
    			return new SymbolicBool(ctx.MkEq(a.abstractValue, b.abstractValue));
    		} else {
    			return new SymbolicBool(ctx.MkEq(a.abstractValue, ctx.MkBool(b.concreteValue)));
    		}
    	} else {
    		if(b.IsAbstract()) {
    			return new SymbolicBool(ctx.MkEq(ctx.MkBool(a.concreteValue), b.abstractValue));
			} else {
				return new SymbolicBool(a.concreteValue == b.concreteValue);
			}
    	}
    }

    public static SymbolicBool operator !=(SymbolicBool a, SymbolicBool b) 
    { 
		if(a.IsAbstract()) {
    		if(b.IsAbstract()) {
    			Context ctx = a.abstractValue.;
    			return new SymbolicBool(ctx.MkNot(ctx.MkEq(a.abstractValue, b.abstractValue)));
    		} else {
    			return new SymbolicBool(ctx.MkEq(a.abstractValue, ctx.MkBool(!b.concreteValue)));
    		}
    	} else {
    		if(b.IsAbstract()) {
    			return new SymbolicBool(ctx.MkEq(ctx.MkBool(!a.concreteValue), b.abstractValue));
			} else {
				return new SymbolicBool(a.concreteValue != b.concreteValue);
			}
    	}    
    }

    public SymbolicBool Equals(SymbolicBool other)
	{
	    return this == other;
	}

	public override SymbolicInteger GetHashCode()
	{
		if (IsAbstract()) {
	    	return new SymbolicInteger((IntExpr)ctx.MkITE(this.abstractValue, ctx.MkBV(1, SymbolicInteger.INT_SIZE), ctx.MkBV(0, SymbolicInteger.INT_SIZE)));
		} else {
			return new SymbolicInteger(this.concreteValue ? 1 : 0);
		}
	}
}