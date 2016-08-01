using System;
using System.Collections.Generic;

class PList<T> : List<T>, IPType<PList<T>> where T : IPType<T> {
	public PInteger Count {
		get {
			return new PInteger(this._count);
		}
	}

	public void Insert(PTuple<PInteger, T> t)
    {
		this.Insert((SymbolicInteger)t.Item1, t.Item2);
    }

	public T this[PInteger index] { 
		get {
			return this.data [index];
		} 
		set {
			this.data [index] = value;
		}
	}

	public PList<T> DeepCopy() {
		PList<T> ret = new PList<T>();
		for(SymbolicInteger i=0; i < this._count; i++) {
			ret.Add(this[i].DeepCopy());
		}
		return ret;
	}
	
	public SymbolicInteger PTypeGetHashCode()
    {
		SymbolicInteger ret = 1;
		for(int i=0; i < this._count; i++) {
			ret = ret * 31 + this[i].PTypeGetHashCode();
        }
        return ret;
    }

	public SymbolicBool PTypeEquals(PList<T> other)
    {
		if(this._count != other._count) {
			return false;
		}
		for(int i=0; i < this._count; i++) {
			if(!this[i].PTypeEquals(other[i])) {
				return false;
			}
		}
		return true;
    }
}