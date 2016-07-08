using System;

public struct PInteger : IPType<PInteger> { 
    SymbolicInteger value;

    public PInteger(SymbolicInteger value) 
    { 
        this.value = value; 
    } 

    public static implicit operator PInteger(SymbolicInteger value) 
    { 
        return new PInteger(value); 
    }

    public static PInteger operator +(PInteger a, PInteger b) 
    { 
        return new PInteger(a.value + b.value); 
    }

    public static PInteger operator -(PInteger a, PInteger b) 
    { 
        return new PInteger(a.value - b.value); 
    }

    public static PInteger operator /(PInteger a, PInteger b) 
    { 
        return new PInteger(a.value / b.value); 
    }

    public static PInteger operator *(PInteger a, PInteger b) 
    { 
        return new PInteger(a.value * b.value); 
    }

    public static PBool operator ==(PInteger a, PInteger b) 
    { 
        return new PBool(a.value == b.value); 
    }

    public static PBool operator !=(PInteger a, PInteger b) 
    { 
        return new PBool(a.value != b.value); 
    }

    public static PBool operator >(PInteger a, PInteger b) 
    { 
        return new PBool(a.value > b.value); 
    }

    public static PBool operator >=(PInteger a, PInteger b) 
    { 
        return new PBool(a.value >= b.value); 
    }

    public static PBool operator <(PInteger a, PInteger b) 
    { 
        return new PBool(a.value < b.value); 
    }

    public static PBool operator <=(PInteger a, PInteger b) 
    { 
        return new PBool(a.value <= b.value); 
    }

    public SymbolicBool Equals(PInteger other)
	{
	    return this.value == other.value;
	}

	public override SymbolicInteger GetHashCode()
	{
	    return this.value.GetHashCode();
	}

    public PInteger DeepCopy() {
    	return this;
    }
}