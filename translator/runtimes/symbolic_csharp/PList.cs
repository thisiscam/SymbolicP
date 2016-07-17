using System;
using System.Collections.Generic;

class PList<T> : List<T>, IPType<PList<T>> where T : IPType<T>{
	public void Insert(PTuple<PInteger, T> t)
    {
        this.Insert(t.Item1, t.Item2);
    }

	public PList<T> DeepCopy() {
		PList<T> r = new PList<T>();
		for(SymbolicInteger i=0; i < this.Count; i++) {
			r.Add(this[i].DeepCopy());
		}
		return r;
	}
	
	public SymbolicInteger PTypeGetHashCode()
    {
		SymbolicInteger ret = 1;
		for(int i=0; i < this.Count; i++) {
			ret = ret * 31 + this[i].PTypeGetHashCode();
        }
        return ret;
    }

	public SymbolicBool PTypeEquals(PList<T> other)
    {
		if(this.Count != other.Count) {
			return false;
		}
		for(int i=0; i < this.Count; i++) {
			if(!this[i].PTypeEquals(other[i])) {
				return false;
			}
		}
		return true;
    }
}