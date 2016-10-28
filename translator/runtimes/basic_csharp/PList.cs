using System;
using System.Collections.Generic;

public class PList<T> : List<T>, IPType<PList<T>>, IEquatable<PList<T>> where T : IPType<T>{
	public void Insert(PTuple<PInteger, T> t)
    {
        this.Insert(t.Item1, t.Item2);
    }

	public PList<T> DeepCopy() {
		PList<T> ret = new PList<T>();
		for(int i=0; i < this.Count; i++) {
			ret.Add(this[i].DeepCopy());
		}
		return ret;
	}
	
	public override int GetHashCode()
    {
    	int ret = 1;
        for(int i=0; i < this.Count; i++) {
        	ret = ret * 31 + this[i].GetHashCode();
        }
        return ret;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        return Equals((PList<T>)obj);
    }

    public bool Equals(PList<T> other)
    {
    	if(this.Count != other.Count) {
    		return false;
    	}
    	for(int i=0; i < this.Count; i++) {
        	if(!this[i].Equals(other[i])) {
        		return false;
        	}
        }
        return true;
    }
}