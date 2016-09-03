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

	public static SymbolicBool operator ==(SymbolicBool a, SymbolicBool b) 
    { 
    	if(a.IsAbstract()) {
    		if(b.IsAbstract()) {
    			return new SymbolicBool(PathConstraint.ctx.MkEq(a.abstractValue, b.abstractValue));
    		} else {
    			return new SymbolicBool(PathConstraint.ctx.MkEq(a.abstractValue, PathConstraint.ctx.MkBool(b.concreteValue)));
    		}
    	} else {
    		if(b.IsAbstract()) {
    			return new SymbolicBool(PathConstraint.ctx.MkEq(PathConstraint.ctx.MkBool(a.concreteValue), b.abstractValue));
			} else {
				return new SymbolicBool(a.concreteValue == b.concreteValue);
			}
    	}
    }

    public static SymbolicBool operator !=(SymbolicBool a, SymbolicBool b) 
    { 
		if(a.IsAbstract()) {
    		if(b.IsAbstract()) {
    			return new SymbolicBool(PathConstraint.ctx.MkNot(PathConstraint.ctx.MkEq(a.abstractValue, b.abstractValue)));
    		} else {
    			return new SymbolicBool(PathConstraint.ctx.MkEq(a.abstractValue, PathConstraint.ctx.MkBool(!b.concreteValue)));
    		}
    	} else {
    		if(b.IsAbstract()) {
    			return new SymbolicBool(PathConstraint.ctx.MkEq(PathConstraint.ctx.MkBool(!a.concreteValue), b.abstractValue));
			} else {
				return new SymbolicBool(a.concreteValue != b.concreteValue);
			}
    	}    
    }

	public static SymbolicBool operator &(SymbolicBool a, SymbolicBool b) 
	{ 
		if(a.IsAbstract()) {
			if(b.IsAbstract()) {
				return new SymbolicBool(PathConstraint.ctx.MkAnd(a.abstractValue, b.abstractValue));
			} else {
				return new SymbolicBool(PathConstraint.ctx.MkAnd(a.abstractValue, PathConstraint.ctx.MkBool(b.concreteValue)));
			}
		} else {
			if(b.IsAbstract()) {
				return new SymbolicBool(PathConstraint.ctx.MkAnd(PathConstraint.ctx.MkBool(a.concreteValue), b.abstractValue));
			} else {
				return new SymbolicBool(a.concreteValue & b.concreteValue);
			}
		}
	}

	public static SymbolicBool operator |(SymbolicBool a, SymbolicBool b) 
	{ 
		if(a.IsAbstract()) {
			if(b.IsAbstract()) {
				return new SymbolicBool(PathConstraint.ctx.MkAnd(a.abstractValue, b.abstractValue));
			} else {
				return new SymbolicBool(PathConstraint.ctx.MkAnd(a.abstractValue, PathConstraint.ctx.MkBool(b.concreteValue)));
			}
		} else {
			if(b.IsAbstract()) {
				return new SymbolicBool(PathConstraint.ctx.MkAnd(PathConstraint.ctx.MkBool(a.concreteValue), b.abstractValue));
			} else {
				return new SymbolicBool(a.concreteValue | b.concreteValue);
			}
		}
	}

	public static SymbolicBool operator !(SymbolicBool a) 
	{ 
		if(a.IsAbstract()) {
			return new SymbolicBool(PathConstraint.ctx.MkNot(a.abstractValue));
		} else {
			return new SymbolicBool(!a.concreteValue);
		}    
	}

	public override string ToString ()
	{
		return string.Format ("[SymbolicBool: ConcreteValue={0}, AbstractValue={1}]", ConcreteValue, AbstractValue);
	}
}