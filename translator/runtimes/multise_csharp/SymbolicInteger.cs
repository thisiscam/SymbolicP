using System;
using Microsoft.Z3;
using System.Diagnostics;

public struct SymbolicInteger {
    public const int INT_SIZE = 32;
	public static readonly Sort bitVecSort = PathConstraint.ctx.MkBitVecSort(SymbolicInteger.INT_SIZE);

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

	public int ConcreteValue {
		get { 
			return concreteValue;
		}
	}

	public BitVecExpr AbstractValue { 
		get { 
			return abstractValue;
		} 
	}

	public static implicit operator SymbolicInteger(int value)
    { 
        return new SymbolicInteger(value);
    }

    public static SymbolicInteger operator +(SymbolicInteger a, SymbolicInteger b) 
    {
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(PathConstraint.ctx.MkBVAdd(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(PathConstraint.ctx.MkBVAdd(a.abstractValue, PathConstraint.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(PathConstraint.ctx.MkBVAdd(PathConstraint.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue + b.concreteValue); 
            }
        }
    }

    public static SymbolicInteger operator -(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(PathConstraint.ctx.MkBVSub(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(PathConstraint.ctx.MkBVSub(a.abstractValue, PathConstraint.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(PathConstraint.ctx.MkBVSub(PathConstraint.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue - b.concreteValue); 
            }
        }
    }

    public static SymbolicInteger operator /(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(PathConstraint.ctx.MkBVSDiv(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(PathConstraint.ctx.MkBVSDiv(a.abstractValue, PathConstraint.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(PathConstraint.ctx.MkBVSDiv(PathConstraint.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue / b.concreteValue); 
            }
        }
    }

    public static SymbolicInteger operator *(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(PathConstraint.ctx.MkBVMul(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(PathConstraint.ctx.MkBVMul(a.abstractValue, PathConstraint.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(PathConstraint.ctx.MkBVMul(PathConstraint.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue * b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator ==(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(PathConstraint.ctx.MkEq(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(PathConstraint.ctx.MkEq(a.abstractValue, PathConstraint.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(PathConstraint.ctx.MkEq(PathConstraint.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue == b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator !=(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(PathConstraint.ctx.MkNot(PathConstraint.ctx.MkEq(a.abstractValue, b.abstractValue)));
            } else {
                return new SymbolicBool(PathConstraint.ctx.MkNot(PathConstraint.ctx.MkEq(a.abstractValue, PathConstraint.ctx.MkBV(b.concreteValue, INT_SIZE))));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(PathConstraint.ctx.MkNot(PathConstraint.ctx.MkEq(PathConstraint.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue)));
            } else {
                return new SymbolicBool(a.concreteValue != b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator >(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(PathConstraint.ctx.MkBVSGT(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(PathConstraint.ctx.MkBVSGT(a.abstractValue, PathConstraint.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(PathConstraint.ctx.MkBVSGT(PathConstraint.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue > b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator >=(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(PathConstraint.ctx.MkBVSGE(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(PathConstraint.ctx.MkBVSGE(a.abstractValue, PathConstraint.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(PathConstraint.ctx.MkBVSGE(PathConstraint.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue >= b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator <(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(PathConstraint.ctx.MkBVSLT(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(PathConstraint.ctx.MkBVSLT(a.abstractValue, PathConstraint.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(PathConstraint.ctx.MkBVSLT(PathConstraint.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue < b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator <=(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(PathConstraint.ctx.MkBVSLE(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(PathConstraint.ctx.MkBVSLE(a.abstractValue, PathConstraint.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(PathConstraint.ctx.MkBVSLE(PathConstraint.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue <= b.concreteValue); 
            }
        }
    }

	public static SymbolicInteger operator ^(SymbolicInteger a, SymbolicInteger b) 
	{ 
		if(a.IsAbstract()) {
			if(b.IsAbstract()) {
				return new SymbolicInteger(PathConstraint.ctx.MkBVXOR(a.abstractValue, b.abstractValue));
			} else {
				return new SymbolicInteger(PathConstraint.ctx.MkBVXOR(a.abstractValue, PathConstraint.ctx.MkBV(b.concreteValue, INT_SIZE)));
			}
		} else {
			if(b.IsAbstract()) {
				return new SymbolicInteger(PathConstraint.ctx.MkBVXOR(PathConstraint.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
			} else {
				return new SymbolicInteger(a.concreteValue ^ b.concreteValue); 
			}
		}
	}

    public static SymbolicInteger operator %(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
				return new SymbolicInteger(PathConstraint.ctx.MkBVSMod(a.abstractValue, b.abstractValue));
            } else {
				return new SymbolicInteger(PathConstraint.ctx.MkBVSMod(a.abstractValue, PathConstraint.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
				return new SymbolicInteger(PathConstraint.ctx.MkBVSMod(PathConstraint.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue ^ b.concreteValue); 
            }
        }
    }

    public static SymbolicInteger operator +(SymbolicInteger a, int b) 
    {
        if(a.IsAbstract()) {
            return new SymbolicInteger(PathConstraint.ctx.MkBVAdd(a.abstractValue, PathConstraint.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicInteger(a.concreteValue + b); 
        }
    }

    public static SymbolicInteger operator -(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicInteger(PathConstraint.ctx.MkBVSub(a.abstractValue, PathConstraint.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicInteger(a.concreteValue - b); 
        }
    }

    public static SymbolicInteger operator /(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicInteger(PathConstraint.ctx.MkBVSDiv(a.abstractValue, PathConstraint.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicInteger(a.concreteValue / b); 
        }
    }

    public static SymbolicInteger operator *(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicInteger(PathConstraint.ctx.MkBVMul(a.abstractValue, PathConstraint.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicInteger(a.concreteValue * b); 
        }
    }

    public static SymbolicBool operator ==(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicBool(PathConstraint.ctx.MkEq(a.abstractValue, PathConstraint.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicBool(a.concreteValue == b); 
        }
    }

    public static SymbolicBool operator !=(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicBool(PathConstraint.ctx.MkNot(PathConstraint.ctx.MkEq(a.abstractValue, PathConstraint.ctx.MkBV(b, INT_SIZE))));
        } else {
            return new SymbolicBool(a.concreteValue != b); 
        }
    }

    public static SymbolicBool operator >(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicBool(PathConstraint.ctx.MkBVSGT(a.abstractValue, PathConstraint.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicBool(a.concreteValue > b); 
        }
    }

    public static SymbolicBool operator >=(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicBool(PathConstraint.ctx.MkBVSGE(a.abstractValue, PathConstraint.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicBool(a.concreteValue >= b); 
        }
    }

    public static SymbolicBool operator <(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicBool(PathConstraint.ctx.MkBVSLT(a.abstractValue, PathConstraint.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicBool(a.concreteValue < b); 
        }
    }

    public static SymbolicBool operator <=(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicBool(PathConstraint.ctx.MkBVSLE(a.abstractValue, PathConstraint.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicBool(a.concreteValue <= b); 
        }
    }

    public static SymbolicInteger operator ^(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicInteger(PathConstraint.ctx.MkBVXOR(a.abstractValue, PathConstraint.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicInteger(a.concreteValue ^ b); 
        }
    }

    public static SymbolicInteger operator %(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
			return new SymbolicInteger(PathConstraint.ctx.MkBVSMod(a.abstractValue, PathConstraint.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicInteger(a.concreteValue % b); 
        }
    }
    
    public static SymbolicInteger operator -(SymbolicInteger a) 
	{ 
		if(a.IsAbstract()) {
			return new SymbolicInteger(PathConstraint.ctx.MkBVNeg(a.abstractValue));
        } else {
            return new SymbolicInteger(-a.concreteValue); 
        }
	}

	public override string ToString ()
	{
		return string.Format ("[SymbolicInteger: ConcreteValue={0}, AbstractValue={1}]", ConcreteValue, AbstractValue);
	}
}