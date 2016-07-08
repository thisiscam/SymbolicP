using System;
using Z3;

public struct SymbolicInteger : ISymbolicHashable<SymbolicInteger> {
    const int INT_SIZE = 32;

    int concreteValue;
    BitVecExpr abstractValue;

    public SymbolicInteger(int value)
    { 
        this.concreteValue = value; 
    } 

    public SymbolicInteger(BitVecExpr value) 
    { 
        this.abstractValue = value; 
    } 

    public static implicit operator SymbolicInteger(int value) 
    { 
        return new SymbolicInteger(value);
    }

    public static implicit operator int(SymbolicInteger integer) 
    { 
        if(abstractValue != null) {
            throw new SystemException("Cannot convert abstract value to int")
        }
        return integer.value;
    }

    public bool IsAbstract() {
        return abstractValue != null;
    }

    public static SymbolicInteger operator +(SymbolicInteger a, SymbolicInteger b) 
    {
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(MkBVAdd(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(MkBVAdd(a.abstractValue, MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(MkBVAdd(MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue + b.concreteValue); 
            }
        }
    }

    public static SymbolicInteger operator -(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(MkBVSub(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(MkBVSub(a.abstractValue, MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(MkBVSub(MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue - b.concreteValue); 
            }
        }
    }

    public static SymbolicInteger operator /(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(MkBVSDiv(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(MkBVSDiv(a.abstractValue, MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(MkBVSDiv(MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue / b.concreteValue); 
            }
        }
    }

    public static SymbolicInteger operator *(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(MkBVMul(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(MkBVMul(a.abstractValue, MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(MkBVMul(MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue * b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator ==(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(MkEq(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(MkEq(a.abstractValue, MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(MkEq(MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue == b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator !=(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(MkNot(MkEq(a.abstractValue, b.abstractValue)));
            } else {
                return new SymbolicBool(MkNot(MkEq(a.abstractValue, MkBV(b.concreteValue, INT_SIZE))));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(MkNot(MkEq(MkBV(a.concreteValue, INT_SIZE), b.abstractValue)));
            } else {
                return new SymbolicBool(a.concreteValue != b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator >(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(MkBVSGT(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(MkBVSGT(a.abstractValue, MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(MkBVSGT(MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue > b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator >=(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(MkBVGE(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(MkBVGE(a.abstractValue, MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(MkBVGE(MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue >= b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator <(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(MkBVLT(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(MkBVLT(a.abstractValue, MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(MkBVLT(MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue < b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator <=(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(MkBVLE(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(MkBVLE(a.abstractValue, MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(MkBVLE(MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue <= b.concreteValue); 
            }
        }
    }

    public SymbolicBool Equals(PInteger other)
    {
        return this.value == other.value;
    }

    public SymbolicInteger GetHashCode()
    {
        return this.value;
    }
}