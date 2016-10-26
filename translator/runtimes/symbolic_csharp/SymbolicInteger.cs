using System;
using Microsoft.Z3;
using System.Diagnostics;

public struct SymbolicInteger {
    public const int INT_SIZE = 32;
	public static readonly Sort bitVecSort = SymbolicEngine.ctx.MkBitVecSort(SymbolicInteger.INT_SIZE);

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

    public static implicit operator int(SymbolicInteger integer) 
    { 
		if(integer.IsAbstract()) {
			return SymbolicEngine.SE.NextIndex(integer.abstractValue);
        } else {
            return integer.concreteValue;
        }
    }

    public static SymbolicInteger operator +(SymbolicInteger a, SymbolicInteger b) 
    {
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(SymbolicEngine.ctx.MkBVAdd(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(SymbolicEngine.ctx.MkBVAdd(a.abstractValue, SymbolicEngine.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(SymbolicEngine.ctx.MkBVAdd(SymbolicEngine.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue + b.concreteValue); 
            }
        }
    }

    public static SymbolicInteger operator -(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(SymbolicEngine.ctx.MkBVSub(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(SymbolicEngine.ctx.MkBVSub(a.abstractValue, SymbolicEngine.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(SymbolicEngine.ctx.MkBVSub(SymbolicEngine.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue - b.concreteValue); 
            }
        }
    }

    public static SymbolicInteger operator /(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(SymbolicEngine.ctx.MkBVSDiv(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(SymbolicEngine.ctx.MkBVSDiv(a.abstractValue, SymbolicEngine.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(SymbolicEngine.ctx.MkBVSDiv(SymbolicEngine.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue / b.concreteValue); 
            }
        }
    }

    public static SymbolicInteger operator *(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicInteger(SymbolicEngine.ctx.MkBVMul(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicInteger(SymbolicEngine.ctx.MkBVMul(a.abstractValue, SymbolicEngine.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicInteger(SymbolicEngine.ctx.MkBVMul(SymbolicEngine.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue * b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator ==(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(SymbolicEngine.ctx.MkEq(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(SymbolicEngine.ctx.MkEq(a.abstractValue, SymbolicEngine.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(SymbolicEngine.ctx.MkEq(SymbolicEngine.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue == b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator !=(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(SymbolicEngine.ctx.MkNot(SymbolicEngine.ctx.MkEq(a.abstractValue, b.abstractValue)));
            } else {
                return new SymbolicBool(SymbolicEngine.ctx.MkNot(SymbolicEngine.ctx.MkEq(a.abstractValue, SymbolicEngine.ctx.MkBV(b.concreteValue, INT_SIZE))));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(SymbolicEngine.ctx.MkNot(SymbolicEngine.ctx.MkEq(SymbolicEngine.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue)));
            } else {
                return new SymbolicBool(a.concreteValue != b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator >(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(SymbolicEngine.ctx.MkBVSGT(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(SymbolicEngine.ctx.MkBVSGT(a.abstractValue, SymbolicEngine.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(SymbolicEngine.ctx.MkBVSGT(SymbolicEngine.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue > b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator >=(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(SymbolicEngine.ctx.MkBVSGE(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(SymbolicEngine.ctx.MkBVSGE(a.abstractValue, SymbolicEngine.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(SymbolicEngine.ctx.MkBVSGE(SymbolicEngine.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue >= b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator <(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(SymbolicEngine.ctx.MkBVSLT(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(SymbolicEngine.ctx.MkBVSLT(a.abstractValue, SymbolicEngine.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(SymbolicEngine.ctx.MkBVSLT(SymbolicEngine.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue < b.concreteValue); 
            }
        }
    }

    public static SymbolicBool operator <=(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
                return new SymbolicBool(SymbolicEngine.ctx.MkBVSLE(a.abstractValue, b.abstractValue));
            } else {
                return new SymbolicBool(SymbolicEngine.ctx.MkBVSLE(a.abstractValue, SymbolicEngine.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
                return new SymbolicBool(SymbolicEngine.ctx.MkBVSLE(SymbolicEngine.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicBool(a.concreteValue <= b.concreteValue); 
            }
        }
    }

	public static SymbolicInteger operator ^(SymbolicInteger a, SymbolicInteger b) 
	{ 
		if(a.IsAbstract()) {
			if(b.IsAbstract()) {
				return new SymbolicInteger(SymbolicEngine.ctx.MkBVXOR(a.abstractValue, b.abstractValue));
			} else {
				return new SymbolicInteger(SymbolicEngine.ctx.MkBVXOR(a.abstractValue, SymbolicEngine.ctx.MkBV(b.concreteValue, INT_SIZE)));
			}
		} else {
			if(b.IsAbstract()) {
				return new SymbolicInteger(SymbolicEngine.ctx.MkBVXOR(SymbolicEngine.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
			} else {
				return new SymbolicInteger(a.concreteValue ^ b.concreteValue); 
			}
		}
	}

    public static SymbolicInteger operator %(SymbolicInteger a, SymbolicInteger b) 
    { 
        if(a.IsAbstract()) {
            if(b.IsAbstract()) {
				return new SymbolicInteger(SymbolicEngine.ctx.MkBVSMod(a.abstractValue, b.abstractValue));
            } else {
				return new SymbolicInteger(SymbolicEngine.ctx.MkBVSMod(a.abstractValue, SymbolicEngine.ctx.MkBV(b.concreteValue, INT_SIZE)));
            }
        } else {
            if(b.IsAbstract()) {
				return new SymbolicInteger(SymbolicEngine.ctx.MkBVSMod(SymbolicEngine.ctx.MkBV(a.concreteValue, INT_SIZE), b.abstractValue));
            } else {
                return new SymbolicInteger(a.concreteValue ^ b.concreteValue); 
            }
        }
    }

    public static SymbolicInteger operator +(SymbolicInteger a, int b) 
    {
        if(a.IsAbstract()) {
            return new SymbolicInteger(SymbolicEngine.ctx.MkBVAdd(a.abstractValue, SymbolicEngine.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicInteger(a.concreteValue + b); 
        }
    }

    public static SymbolicInteger operator -(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicInteger(SymbolicEngine.ctx.MkBVSub(a.abstractValue, SymbolicEngine.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicInteger(a.concreteValue - b); 
        }
    }

    public static SymbolicInteger operator /(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicInteger(SymbolicEngine.ctx.MkBVSDiv(a.abstractValue, SymbolicEngine.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicInteger(a.concreteValue / b); 
        }
    }

    public static SymbolicInteger operator *(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicInteger(SymbolicEngine.ctx.MkBVMul(a.abstractValue, SymbolicEngine.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicInteger(a.concreteValue * b); 
        }
    }

    public static SymbolicBool operator ==(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicBool(SymbolicEngine.ctx.MkEq(a.abstractValue, SymbolicEngine.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicBool(a.concreteValue == b); 
        }
    }

    public static SymbolicBool operator !=(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicBool(SymbolicEngine.ctx.MkNot(SymbolicEngine.ctx.MkEq(a.abstractValue, SymbolicEngine.ctx.MkBV(b, INT_SIZE))));
        } else {
            return new SymbolicBool(a.concreteValue != b); 
        }
    }

    public static SymbolicBool operator >(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicBool(SymbolicEngine.ctx.MkBVSGT(a.abstractValue, SymbolicEngine.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicBool(a.concreteValue > b); 
        }
    }

    public static SymbolicBool operator >=(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicBool(SymbolicEngine.ctx.MkBVSGE(a.abstractValue, SymbolicEngine.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicBool(a.concreteValue >= b); 
        }
    }

    public static SymbolicBool operator <(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicBool(SymbolicEngine.ctx.MkBVSLT(a.abstractValue, SymbolicEngine.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicBool(a.concreteValue < b); 
        }
    }

    public static SymbolicBool operator <=(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicBool(SymbolicEngine.ctx.MkBVSLE(a.abstractValue, SymbolicEngine.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicBool(a.concreteValue <= b); 
        }
    }

    public static SymbolicInteger operator ^(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicInteger(SymbolicEngine.ctx.MkBVXOR(a.abstractValue, SymbolicEngine.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicInteger(a.concreteValue ^ b); 
        }
    }

    public static SymbolicInteger operator %(SymbolicInteger a, int b) 
    { 
        if(a.IsAbstract()) {
			return new SymbolicInteger(SymbolicEngine.ctx.MkBVSMod(a.abstractValue, SymbolicEngine.ctx.MkBV(b, INT_SIZE)));
        } else {
            return new SymbolicInteger(a.concreteValue % b); 
        }
    }

    public static SymbolicInteger operator -(SymbolicInteger a) 
    { 
        if(a.IsAbstract()) {
            return new SymbolicInteger(SymbolicEngine.ctx.MkBVNeg(a.abstractValue));
        } else {
            return new SymbolicInteger(-a.concreteValue); 
        }
    }

	public override string ToString ()
	{
		return string.Format ("[SymbolicInteger: ConcreteValue={0}, AbstractValue={1}]", ConcreteValue, AbstractValue);
	}
}