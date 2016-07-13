using System;

public struct PBool : IPType<PBool>, IEquatable<PBool> {
	SymbolicBool value;

	public PBool(SymbolicBool value) {
		this.value = value;		
	}

	public static implicit operator PBool(bool value) 
	{ 
		return new PBool(new SymbolicBool(value)); 
	}


	public static implicit operator PBool(SymbolicBool value) 
    { 
        return new PBool(value); 
    } 

	public static implicit operator SymbolicBool(PBool b) 
    { 
        return b.value; 
    }

	public static PBool operator ==(PBool a, PBool b) 
    { 
        return new PBool(a.value == b.value); 
    }

    public static PBool operator !=(PBool a, PBool b) 
    { 
        return new PBool(a.value != b.value); 
    }

	public static PBool operator !(PBool a) 
	{ 
		return new PBool(!a.value); 
	}

	public static PBool operator &(PBool a, PBool b) 
	{ 
		return new PBool(a.value & b.value); 
	}

	public static PBool operator |(PBool a, PBool b) 
	{ 
		return new PBool(a.value | b.value); 
	}

	public static bool operator true(PBool op)
	{
		return op.value;
	}

	public static bool operator false(PBool op)
	{
		return op.value;
	}

    public bool Equals(PBool other)
	{
	    return this.value == other.value;
	}
	public override bool Equals(object obj)
	{
	    return obj is PBool && this.value == ((PBool)obj).value;
	}
	public SymbolicInteger GetHashCode()
	{
	    return this.value.GetHashCode();
	}

	public PBool DeepCopy() {
		return this;
	}
}