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

	public bool ConcreteValue {
		get { 
			return concreteValue;
		}
	}

	public BoolExpr AbstractValue { 
		get { 
			return abstractValue;
		} 
	}

	public static implicit operator SymbolicBool(bool value) 
    { 
        return new SymbolicBool(value); 
    } 

    public static implicit operator bool(SymbolicBool op) 
    { 
		if (op.IsAbstract ()) {
			return SymbolicEngine.SE.NextBranch (op.abstractValue);
		} else {
			return op.ConcreteValue;
		}
    }

	public static SymbolicBool operator ==(SymbolicBool a, SymbolicBool b) 
    { 
    	if(a.IsAbstract()) {
    		if(b.IsAbstract()) {
    			return new SymbolicBool(SymbolicEngine.ctx.MkEq(a.abstractValue, b.abstractValue));
    		} else {
    			return new SymbolicBool(SymbolicEngine.ctx.MkEq(a.abstractValue, SymbolicEngine.ctx.MkBool(b.concreteValue)));
    		}
    	} else {
    		if(b.IsAbstract()) {
    			return new SymbolicBool(SymbolicEngine.ctx.MkEq(SymbolicEngine.ctx.MkBool(a.concreteValue), b.abstractValue));
			} else {
				return new SymbolicBool(a.concreteValue == b.concreteValue);
			}
    	}
    }

    public static SymbolicBool operator !=(SymbolicBool a, SymbolicBool b) 
    { 
		if(a.IsAbstract()) {
    		if(b.IsAbstract()) {
    			return new SymbolicBool(SymbolicEngine.ctx.MkNot(SymbolicEngine.ctx.MkEq(a.abstractValue, b.abstractValue)));
    		} else {
    			return new SymbolicBool(SymbolicEngine.ctx.MkEq(a.abstractValue, SymbolicEngine.ctx.MkBool(!b.concreteValue)));
    		}
    	} else {
    		if(b.IsAbstract()) {
    			return new SymbolicBool(SymbolicEngine.ctx.MkEq(SymbolicEngine.ctx.MkBool(!a.concreteValue), b.abstractValue));
			} else {
				return new SymbolicBool(a.concreteValue != b.concreteValue);
			}
    	}    
    }

	public static SymbolicBool operator &(SymbolicBool a, SymbolicBool b) 
	{ 
		if(a.IsAbstract()) {
			if(b.IsAbstract()) {
				return new SymbolicBool(SymbolicEngine.ctx.MkAnd(a.abstractValue, b.abstractValue));
			} else {
				return new SymbolicBool(SymbolicEngine.ctx.MkAnd(a.abstractValue, SymbolicEngine.ctx.MkBool(b.concreteValue)));
			}
		} else {
			if(b.IsAbstract()) {
				return new SymbolicBool(SymbolicEngine.ctx.MkAnd(SymbolicEngine.ctx.MkBool(a.concreteValue), b.abstractValue));
			} else {
				return new SymbolicBool(a.concreteValue & b.concreteValue);
			}
		}
	}

	public static SymbolicBool operator |(SymbolicBool a, SymbolicBool b) 
	{ 
		if(a.IsAbstract()) {
			if(b.IsAbstract()) {
				return new SymbolicBool(SymbolicEngine.ctx.MkOr(a.abstractValue, b.abstractValue));
			} else {
				return new SymbolicBool(SymbolicEngine.ctx.MkOr(a.abstractValue, SymbolicEngine.ctx.MkBool(b.concreteValue)));
			}
		} else {
			if(b.IsAbstract()) {
				return new SymbolicBool(SymbolicEngine.ctx.MkOr(SymbolicEngine.ctx.MkBool(a.concreteValue), b.abstractValue));
			} else {
				return new SymbolicBool(a.concreteValue | b.concreteValue);
			}
		}
	}

	public static SymbolicBool operator !(SymbolicBool a) 
	{ 
		if(a.IsAbstract()) {
			return new SymbolicBool(SymbolicEngine.ctx.MkNot(a.abstractValue));
		} else {
			return new SymbolicBool(!a.concreteValue);
		}    
	}
		
	public static bool operator true(SymbolicBool op)
	{
		if (op.IsAbstract ()) {
			return SymbolicEngine.SE.NextBranch (op.abstractValue);
		} else {
			return op.concreteValue;
		}
	}

	public static bool operator false(SymbolicBool op)
	{
		if (op.IsAbstract ()) {
			return SymbolicEngine.SE.NextBranch (SymbolicEngine.ctx.MkNot (op.abstractValue));
		} else {
			return !op.concreteValue;
		}
	}

	public override string ToString ()
	{
		return string.Format ("[SymbolicBool: ConcreteValue={0}, AbstractValue={1}]", ConcreteValue, AbstractValue);
	}
}