using System;
using System.Collections.Generic;

class PList<T> : List<T>, IPType<PList<T>> where T : IPType<T> {
	public new PInteger Count {
		get {
			return (PInteger)this._count;
		}
	}

	public void Insert(PTuple<PInteger, T> t)
    {
		this.Insert((SymbolicInteger)t.Item1, t.Item2);
    }

    public void RemoveAt(PInteger idx)
    {
		this.RemoveAt((SymbolicInteger)idx);
    }

	public T this[PInteger index] { 
		get {
			if(index >= this.Count) {
				throw new IndexOutOfRangeException();
			}
			return this.data [index];
		} 
		set {
			if(index >= this.Count) {
				throw new IndexOutOfRangeException();
			}
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