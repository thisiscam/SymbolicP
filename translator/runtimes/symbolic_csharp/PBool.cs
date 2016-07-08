using System;

public struct PBool : IPType<PBool> {
	SymbolicBool value;

	public PBool(SymbolicBool value) {
		this.value = value;		
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

    public SymbolicBool Equals(PBool other)
	{
	    return this.value == other.value;
	}

	public override SymbolicInteger GetHashCode()
	{
	    return this.value.GetHashCode();
	}

	public PBool DeepCopy() {
		return this;
	}
}