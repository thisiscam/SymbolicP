using System;
using Microsoft.Z3;

public struct SymbolicInteger {
    public const int INT_SIZE = 32;

    int concreteValue;
    BitVecExpr abstractValue;

    public SymbolicInteger(int value)
    { 
        this.concreteValue = value;
		this.abstractValue = null;
    } 

    public SymbolicInteger(BitVecExpr value) 
    { 
		this.concreteValue = 0;
        this.abstractValue = value; 
    } 

	public bool IsAbstract() {
		return this.abstractValue != null;
	}

    public static implicit operator SymbolicInteger(int value)
    { 
        return new SymbolicInteger(value);
    }

    public static implicit operator int(SymbolicInteger integer) 
    { 
		if(integer.IsAbstract()) {
			throw new SystemException ("Cannot convert abstract value to int");
        }
		return integer.concreteValue;
    }

    public static SymbolicInteger operator +(SymbolicInteger a, SymbolicInteger b) 
    {
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(Z3Wrapper.ctx.MkBVAdd(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(Z3Wrapper.ctx.MkBVAdd(a.abstractValue, Z3Wrapper.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(Z3Wrapper.ctx.MkBVAdd(Z3Wrapper.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue + b.concreteValue); 
            }
        }
    }

    public static SymbolicInteger operator -(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(Z3Wrapper.ctx.MkBVSub(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(Z3Wrapper.ctx.MkBVSub(a.abstractValue, Z3Wrapper.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(Z3Wrapper.ctx.MkBVSub(Z3Wrapper.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue - b.concreteValue); 
            }
        }
    }

    public static SymbolicInteger operator /(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(Z3Wrapper.ctx.MkBVSDiv(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(Z3Wrapper.ctx.MkBVSDiv(a.abstractValue, Z3Wrapper.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(Z3Wrapper.ctx.MkBVSDiv(Z3Wrapper.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue / b.concreteValue); 
            }
        }
    }

    public static SymbolicInteger operator *(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(Z3Wrapper.ctx.MkBVMul(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(Z3Wrapper.ctx.MkBVMul(a.abstractValue, Z3Wrapper.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(Z3Wrapper.ctx.MkBVMul(Z3Wrapper.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue * b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator ==(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(Z3Wrapper.ctx.MkEq(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(Z3Wrapper.ctx.MkEq(a.abstractValue, Z3Wrapper.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(Z3Wrapper.ctx.MkEq(Z3Wrapper.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue == b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator !=(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(Z3Wrapper.ctx.MkNot(Z3Wrapper.ctx.MkEq(a.abstractValue, b.abstractValue)));
            } else {
                return new SymbolicBool(Z3Wrapper.ctx.MkNot(Z3Wrapper.ctx.MkEq(a.abstractValue, Z3Wrapper.ctx.MkBV(b.concreteValue, INT_SIZE))));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(Z3Wrapper.ctx.MkNot(Z3Wrapper.ctx.MkEq(Z3Wrapper.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue)));
            } else {
                return new SymbolicBool(a.concreteValue != b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator >(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(Z3Wrapper.ctx.MkBVSGT(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(Z3Wrapper.ctx.MkBVSGT(a.abstractValue, Z3Wrapper.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(Z3Wrapper.ctx.MkBVSGT(Z3Wrapper.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue > b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator >=(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(Z3Wrapper.ctx.MkBVSGE(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(Z3Wrapper.ctx.MkBVSGE(a.abstractValue, Z3Wrapper.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(Z3Wrapper.ctx.MkBVSGE(Z3Wrapper.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue >= b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator <(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(Z3Wrapper.ctx.MkBVSLT(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(Z3Wrapper.ctx.MkBVSLT(a.abstractValue, Z3Wrapper.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(Z3Wrapper.ctx.MkBVSLT(Z3Wrapper.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue < b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator <=(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(Z3Wrapper.ctx.MkBVSLE(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(Z3Wrapper.ctx.MkBVSLE(a.abstractValue, Z3Wrapper.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(Z3Wrapper.ctx.MkBVSLE(Z3Wrapper.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue <= b.concreteValue); 
            }
        }
    }

	public SymbolicBool Equals(SymbolicInteger other)
    {
        return this == other;
    }

    public SymbolicInteger GetHashCode()
    {
		return this;
    }
}