using System;
using System.Collections.Generic;

public class PMap<K, V> : Dictionary<K, V>, IEquatable<PMap<K, V>>, IPType<PMap<K, V>> 
		where K : IPType<K> 
		where V : IPType<V>
{
	public PMap() : base() { }
	public PMap(int capacity) : base(capacity) { }
	public PMap(int capacity, IEqualityComparer<K> comparer) : base(capacity, comparer) { }


	public void Insert(PTuple<K, V> t)
    {
        this.Add(t.Item1, t.Item2);
    }

	public PMap<K, V> DeepCopy() {
		PMap<K, V> ret = new PMap<K, V>(this.Count, this.Comparer);
	    foreach (KeyValuePair<K, V> entry in this)
	    {
	        ret.Add(entry.Key, entry.Value.DeepCopy());
	    }
	    return ret;
	}

	public override int GetHashCode()
    {
    	int ret = 1;
        foreach (KeyValuePair<K, V> entry in this)
	    {
	        ret = ret * 31 + (entry.Key.GetHashCode() ^ entry.Value.GetHashCode());
	    }
        return ret;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        return Equals((PMap<K, V>)obj);
    }

    public bool Equals(PMap<K, V> other)
    {
    	if(this.Count != other.Count) {
    		return false;
    	}
        foreach (KeyValuePair<K, V> entry in this)
	    {
	     	if(!ContainsKey(entry.Key) || !entry.Value.Equals(other[entry.Key])) {
	     		return false;
	     	}   
	    }
        return true;
    }
}