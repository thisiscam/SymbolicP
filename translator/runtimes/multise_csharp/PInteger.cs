using System;

public struct PInteger : IPType<PInteger> { 
	public SymbolicInteger value;

    public PInteger(int value) 
    { 
		this.value = new SymbolicInteger(value); 
    } 

    public static implicit operator PInteger(int value) 
    { 
        return new PInteger(value); 
    }

	public static implicit operator int(PInteger value) 
	{ 
		return value.value;
	}

    public static explicit operator PInteger(SymbolicInteger value) {
        return new PInteger(value);
    }

	public static explicit operator SymbolicInteger(PInteger value) {
		return value.value;
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

	public static PInteger operator ^(PInteger a, PInteger b) 
    { 
		return new PInteger(a.value ^ b.value); 
    }

	public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PInteger> other)
	{
		return this.value == other.value.value;
	}

	public ValueSummary<SymbolicInteger> PTypeGetHashCode()
	{
		return this.value;
	}

    public ValueSummary<PInteger> DeepCopy() {
    	return this;
    }

	public override string ToString ()
	{
		return value.ToString ();
	}
}