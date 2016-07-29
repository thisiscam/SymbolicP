using System;
using System.Collections.Generic;
using System.Diagnostics;

public class PMap<K, V> : IPType<PMap<K, V>> 
		where K : IPType<K> 
		where V : IPType<V>
{
	private class MapEntry {
		SymbolicInteger hash;
		K key;
		V value;
		MapEntry next;

		public K Key { get { return key; } }
		public SymbolicInteger Hash { get { return hash; } }
		public V Value { 
			get { 
				return value; 
			}
			set { 
				this.value = value;
			}
		}
		public MapEntry Next { 
			get { 
				return next; 
			} 
			set { 
				this.next = value;
			}
		}

		public MapEntry(K k, SymbolicInteger hash, V v, MapEntry next) {
			this.key = k;
			this.hash = hash;
			this.value = v;
			this.next = next;
		}
	}

	private int _capacity;
	private int _count;
	private MapEntry[] data;

	private static double LOAD_FACTOR = 0.7;
	private static double RESIZE_RATIO = 2.0;
	private static int DEFAULT_INITIAL_CAPACITY = 4;

	public PMap():this(PMap<K,V>.DEFAULT_INITIAL_CAPACITY) { }

	public PMap(int capacity) { 
		this._capacity = capacity;
		this._count = 0;
		this.data = new MapEntry[capacity];
	}
		
	public int Count { get { return _count; } }

	private void Insert(K k, V v) {
		SymbolicInteger hash = k.PTypeGetHashCode ();
		SymbolicInteger idx = hash % this._capacity;
		MapEntry firstEntry = this.data [idx];
		for (MapEntry iter = firstEntry; iter != null; firstEntry = firstEntry.Next) {
			if (iter.Hash == hash && iter.Key.PTypeEquals (k)) {
				throw new SystemException ("Reinsertion of key" + k.ToString() + "into PMap");
			}
		}

		this.ResizeIfNecessary ();

		this.data [idx] = new MapEntry (k, hash, v, firstEntry);
		this._count++;
	}

	private V Get(K k) {
		SymbolicInteger hash = k.PTypeGetHashCode ();
		SymbolicInteger idx = hash % this._capacity;
		MapEntry firstEntry = this.data [idx];
		for (MapEntry iter = firstEntry; iter != null; firstEntry = firstEntry.Next) {
			if (iter.Hash == hash && iter.Key.PTypeEquals (k)) {
				return iter.Value;
			}
		}
		throw new SystemException ("Key does not exist in dictionary");
	}

	private void Set(K k, V v) {
		SymbolicInteger hash = k.PTypeGetHashCode ();
		SymbolicInteger idx = hash % this._capacity;
		MapEntry firstEntry = this.data [idx];
		for (MapEntry iter = firstEntry; iter != null; firstEntry = firstEntry.Next) {
			if (iter.Hash == hash && iter.Key.PTypeEquals (k)) {
				iter.Value = v;
				return;
			}
		}

		this.ResizeIfNecessary ();

		this.data [idx] = new MapEntry (k, hash, v, firstEntry);
		this._count++;
	}

	private void ResizeIfNecessary () {
		if (this._count / this._capacity > PMap<K,V>.LOAD_FACTOR) {
			int new_capacity = (int)(this._capacity * PMap<K,V>.RESIZE_RATIO);
			MapEntry[] new_data = new MapEntry[new_capacity];
			for (int i = 0; i < this._capacity; i++) {
				for (MapEntry iter = this.data [i]; iter != null; iter = iter.Next) {
					SymbolicInteger idx = iter.Hash % new_capacity;
					new_data[idx] = new MapEntry(iter.Key, iter.Hash, iter.Value, new_data[idx]);
				}
			}
			this._capacity = new_capacity;
			this.data = new_data;
		}
	}

	public void Remove(K k) {
		SymbolicInteger hash = k.PTypeGetHashCode ();
		SymbolicInteger idx = hash % this._capacity;
		MapEntry firstEntry = this.data [idx];
		if (firstEntry.Hash == hash && firstEntry.Key.PTypeEquals (k)) {
			this.data [idx] = firstEntry.Next;
			return;
		} else {
			for (MapEntry iter = firstEntry; iter != null; firstEntry = firstEntry.Next) {
				if (iter.Next.Hash == hash && iter.Next.Key.PTypeEquals (k)) {
					iter.Next = iter.Next.Next;
					return;
				}
			}	
		}
	}

	public void Insert(PTuple<K, V> t)
    {
        this.Insert(t.Item1, t.Item2);
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
		PMap<K, V> ret = new PMap<K, V>(this._capacity);
		for (int i = 0; i < this._capacity; i++) {
			ret.data[i] = CopyEntryChain (this.data [i]);
		}
	    return ret;
	}

	private MapEntry CopyEntryChain(MapEntry entry) {
		if (entry == null) {
			return null;
		} else {
			return new MapEntry(entry.Key, entry.Hash, entry.Value.DeepCopy(), CopyEntryChain (entry.Next));
		}
	}

	public SymbolicInteger PTypeGetHashCode()
	{
		SymbolicInteger ret = 1;
		for (int i = 0; i < this._capacity; i++) {
			MapEntry entry = this.data[i];
			while(entry != null) {
				ret += entry.Key.PTypeGetHashCode() ^ entry.Value.PTypeGetHashCode();
			} 
		}
		return ret;
	}

	public SymbolicBool PTypeEquals(PMap<K, V> other)
    {
    	if(this.Count != other.Count) {
    		return false;
    	}
		for (int i = 0; i < this._capacity; i++) {
			MapEntry entry = this.data[i];
			while(entry != null) {
				if(other.Get(entry.Key) != null || !entry.Value.PTypeEquals(other[entry.Key])) {
					return false;
				}
			}
		}
        return true;
    }
}