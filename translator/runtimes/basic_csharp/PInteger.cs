using System;

public struct PInteger : IPType<PInteger>, IEquatable<PInteger> { 
    int value;

    public PInteger(int value) 
    { 
        this.value = value; 
    } 

    public static implicit operator PInteger(int value) 
    { 
        return new PInteger(value); 
    }

	public static implicit operator int(PInteger value) 
	{ 
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

    public static PInteger operator -(PInteger a) 
    { 
        return new PInteger(-a.value);
    }

    public bool Equals(PInteger other)
	{
	    return this.value == other.value;
	}
	public override bool Equals(object obj)
	{
	    return obj is PInteger && this.value == ((PInteger)obj).value;
	}
	public override int GetHashCode()
	{
	    return this.value.GetHashCode();
	}

    public PInteger DeepCopy() {
    	return this;
    }

	public override string ToString ()
	{
		return value.ToString ();
	}
}