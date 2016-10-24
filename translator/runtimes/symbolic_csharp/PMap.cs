using System;
using System.Collections.Generic;
using System.Diagnostics;


public class PMap<K, V> : IPType<PMap<K, V>> 
		where K : IPType<K> 
		where V : IPType<V>
{

	public PInteger Count { get { return data.Count; } }
	
	private PList<PTuple<K, V>> data = new PList<PTuple<K, V>>();

	private void Insert(K k, V v) {
		for (int i=0; i < data.Count; i++) {
			if (k.PTypeEquals (data[i].Item1)) {
				throw new SystemException ("Reinsertion of key" + k.ToString() + "into PMap");
			}
		}
		data.Add(new PTuple<K, V>(k, v));
	}

	private V Get(K k) {
		for (int i=0; i < data.Count; i++) {
			if (k.PTypeEquals (data[i].Item1)) {
				return data[i].Item2;
			}
		}
		throw new SystemException ("Key does not exist in dictionary");
	}

	private void Set(K k, V v) {
		for (int i=0; i < data.Count; i++) {
			if (k.PTypeEquals (data[i].Item1)) {
				data[i].Item2 = v;
				return;
			}
		}
		Insert(k, v);
	}

	public void Remove(K k) {
		for (int i=0; i < data.Count; i++) {
			if (k.PTypeEquals (data[i].Item1)) {
				this.data.RemoveAt(i);
				return;
			}
		}
	}

	public void Insert(PTuple<K, V> t)
    {
        this.Insert(t.Item1, t.Item2);
    }

    public PBool ContainsKey(K k) {
    	for (int i=0; i < data.Count; i++) {
			if (k.PTypeEquals (data[i].Item1)) {
				return true;
			}
		}
		return false;
    }

	public V this [K key] {
		get {
			return this.Get (key);
		}
		set {
			this.Set (key, value);
		}
	}

	public PMap<K, V> DeepCopy() {
		PMap<K, V> ret = new PMap<K, V>();
		for(int i=0; i < data.Count; i++)
		{
			ret.data.Add(data[i].DeepCopy());
		}
		return ret;
	}

	public SymbolicBool PTypeEquals(PMap<K, V> other)
    {
    	return this.data.PTypeEquals(other.data);
    }
}