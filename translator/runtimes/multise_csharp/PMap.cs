using System;
using System.Collections.Generic;
using System.Diagnostics;

public class PMap<K, V> : IPType<PMap<K, V>> where K : IPType<K> where V : IPType<V>
{
    private class MapEntry<K, V>
    {
        ValueSummary<SymbolicInteger> hash;
        ValueSummary<K> key;
        ValueSummary<V> value;
        ValueSummary<MapEntry<K, V>> next;
        public ValueSummary<K> Key
        {
            get
            {
                return this.key;
            }
        }

        public ValueSummary<SymbolicInteger> Hash
        {
            get
            {
                return this.hash;
            }
        }

        public ValueSummary<V> Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.value.Assign(value);
            }
        }

        public ValueSummary<MapEntry<K, V>> Next
        {
            get
            {
                return this.next;
            }

            set
            {
                this.next.Assign(value);
            }
        }

        public MapEntry(ValueSummary<K> k, ValueSummary<SymbolicInteger> hash, ValueSummary<V> v, ValueSummary<MapEntry<K, V>> next)
        {
            this.key = k;
            this.hash = hash;
            this.value = v;
            this.next = next;
        }
    }

    private ValueSummary<int> _capacity;
    private ValueSummary<int> _count;
    private ValueSummary<ValueSummary<MapEntry<K, V>>[]> data;
    private static double LOAD_FACTOR = 0.7;
    private static double RESIZE_RATIO = 2.0;
    private static int DEFAULT_INITIAL_CAPACITY = 4;
    public PMap(): this (PMap<K, V>.DEFAULT_INITIAL_CAPACITY)
    {
    }

    public PMap(ValueSummary<int> capacity)
    {
        this._capacity = capacity;
        this._count = 0;
        this.data = new ValueSummary<MapEntry<K, V>>[capacity];
    }

    public ValueSummary<int> Count
    {
        get
        {
            return this._count;
        }
    }

    private void Insert(ValueSummary<PMap<K, V>> self, ValueSummary<K> k, ValueSummary<V> v)
    {
        ValueSummary<SymbolicInteger> hash = k.InvokeDynamic((_, s) => _.PTypeGetHashCode(s));
        ValueSummary<SymbolicInteger> idx = hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, self.GetField<int>(_ => _._capacity));
        ValueSummary<MapEntry<K, V>> firstEntry = self.GetField<PMap<K, V>.MapEntry<K, V>[]>(_ => _.data).GetIndex((_, a0) => _[a0], idx);
        for (ValueSummary<MapEntry<K, V>> iter = firstEntry; iter.InvokeBinary<object, bool>((l, r) => l != r, null); firstEntry.Assign(firstEntry.GetField<PMap<K, V>.MapEntry<K, V>>(_ => _.Next)))
        {
            if (iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash).InvokeBinary<SymbolicBool, bool>((l, r) => l && r, iter.GetField<K>(_ => _.Key).InvokeDynamic<K>((_, s, a0) => _.PTypeEquals(s, a0), k)))
            {
                throw new SystemException("Reinsertion of key" + k.ToString() + "into PMap");
            }
        }

        self.InvokeMethod((_, s) => _.ResizeIfNecessary(s));
        self.GetField<PMap<K, V>.MapEntry<K, V>[]>(_ => _.data).GetIndex((_, a0) => _[a0], idx).Assign(new ValueSummary<PMap<K, V>.MapEntry<K, V>>(new MapEntry<K, V>(k, hash, v, firstEntry)));
        self.GetField<int>(_ => _._count).InvokeMethod((_) => _++);
    }

    private ValueSummary<V> Get(ValueSummary<PMap<K, V>> self, ValueSummary<K> k)
    {
        ValueSummary<SymbolicInteger> hash = k.InvokeDynamic((_, s) => _.PTypeGetHashCode(s));
        ValueSummary<SymbolicInteger> idx = hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, self.GetField<int>(_ => _._capacity));
        ValueSummary<MapEntry<K, V>> firstEntry = self.GetField<PMap<K, V>.MapEntry<K, V>[]>(_ => _.data).GetIndex((_, a0) => _[a0], idx);
        for (ValueSummary<MapEntry<K, V>> iter = firstEntry; iter.InvokeBinary<object, bool>((l, r) => l != r, null); firstEntry.Assign(firstEntry.GetField<PMap<K, V>.MapEntry<K, V>>(_ => _.Next)))
        {
            if (iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash).InvokeBinary<SymbolicBool, bool>((l, r) => l && r, iter.GetField<K>(_ => _.Key).InvokeDynamic<K>((_, s, a0) => _.PTypeEquals(s, a0), k)))
            {
                return iter.GetField<V>(_ => _.Value);
            }
        }

        throw new SystemException("Key does not exist in dictionary");
    }

    private void Set(ValueSummary<PMap<K, V>> self, ValueSummary<K> k, ValueSummary<V> v)
    {
        ValueSummary<SymbolicInteger> hash = k.InvokeDynamic((_, s) => _.PTypeGetHashCode(s));
        ValueSummary<SymbolicInteger> idx = hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, self.GetField<int>(_ => _._capacity));
        ValueSummary<MapEntry<K, V>> firstEntry = self.GetField<PMap<K, V>.MapEntry<K, V>[]>(_ => _.data).GetIndex((_, a0) => _[a0], idx);
        for (ValueSummary<MapEntry<K, V>> iter = firstEntry; iter.InvokeBinary<object, bool>((l, r) => l != r, null); firstEntry.Assign(firstEntry.GetField<PMap<K, V>.MapEntry<K, V>>(_ => _.Next)))
        {
            if (iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash).InvokeBinary<SymbolicBool, bool>((l, r) => l && r, iter.GetField<K>(_ => _.Key).InvokeDynamic<K>((_, s, a0) => _.PTypeEquals(s, a0), k)))
            {
                iter.GetField<V>(_ => _.Value).Assign(v);
                return;
            }
        }

        self.InvokeMethod((_, s) => _.ResizeIfNecessary(s));
        self.GetField<PMap<K, V>.MapEntry<K, V>[]>(_ => _.data).GetIndex((_, a0) => _[a0], idx).Assign(new ValueSummary<PMap<K, V>.MapEntry<K, V>>(new MapEntry<K, V>(k, hash, v, firstEntry)));
        self.GetField<int>(_ => _._count).InvokeMethod((_) => _++);
    }

    private void ResizeIfNecessary(ValueSummary<PMap<K, V>> self)
    {
        if (self.GetField<int>(_ => _._count).InvokeBinary<int, double>((l, r) => l / r, self.GetField<int>(_ => _._capacity)).InvokeBinary<double, bool>((l, r) => l > r, PMap<K, V>.LOAD_FACTOR))
        {
            ValueSummary<int> new_capacity = (int)(self.GetField<int>(_ => _._capacity).InvokeBinary<double, double>((l, r) => l * r, PMap<K, V>.RESIZE_RATIO));
            ValueSummary<ValueSummary<MapEntry<K, V>>[]> new_data = new ValueSummary<MapEntry<K, V>>[new_capacity];
            for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, self.GetField<int>(_ => _._capacity)); i.InvokeMethod((_) => _++))
            {
                for (ValueSummary<MapEntry<K, V>> iter = self.GetField<PMap<K, V>.MapEntry<K, V>[]>(_ => _.data).GetIndex((_, a0) => _[a0], i); iter.InvokeBinary<object, bool>((l, r) => l != r, null); iter.Assign(iter.GetField<PMap<K, V>.MapEntry<K, V>>(_ => _.Next)))
                {
                    ValueSummary<SymbolicInteger> idx = iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<int, SymbolicInteger>((l, r) => l % r, new_capacity);
                    new_data.GetIndex((_, a0) => _[a0], idx).Assign(new ValueSummary<PMap<K, V>.MapEntry<K, V>>(new MapEntry<K, V>(iter.GetField<K>(_ => _.Key), iter.GetField<SymbolicInteger>(_ => _.Hash), iter.GetField<V>(_ => _.Value), new_data.GetIndex((_, a0) => _[a0], idx))));
                }
            }

            self.GetField<int>(_ => _._capacity).Assign(new_capacity);
            self.GetField<PMap<K, V>.MapEntry<K, V>[]>(_ => _.data).Assign(new_data);
        }
    }

    public void Remove(ValueSummary<PMap<K, V>> self, ValueSummary<K> k)
    {
        ValueSummary<SymbolicInteger> hash = k.InvokeDynamic((_, s) => _.PTypeGetHashCode(s));
        ValueSummary<SymbolicInteger> idx = hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, self.GetField<int>(_ => _._capacity));
        ValueSummary<MapEntry<K, V>> firstEntry = self.GetField<PMap<K, V>.MapEntry<K, V>[]>(_ => _.data).GetIndex((_, a0) => _[a0], idx);
        if (firstEntry.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash).InvokeBinary<SymbolicBool, bool>((l, r) => l && r, firstEntry.GetField<K>(_ => _.Key).InvokeDynamic<K>((_, s, a0) => _.PTypeEquals(s, a0), k)))
        {
            self.GetField<PMap<K, V>.MapEntry<K, V>[]>(_ => _.data).GetIndex((_, a0) => _[a0], idx).Assign(firstEntry.GetField<PMap<K, V>.MapEntry<K, V>>(_ => _.Next));
            return;
        }
        else
        {
            for (ValueSummary<MapEntry<K, V>> iter = firstEntry; iter.InvokeBinary<object, bool>((l, r) => l != r, null); firstEntry.Assign(firstEntry.GetField<PMap<K, V>.MapEntry<K, V>>(_ => _.Next)))
            {
                if (iter.GetField<PMap<K, V>.MapEntry<K, V>>(_ => _.Next).GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash).InvokeBinary<SymbolicBool, bool>((l, r) => l && r, iter.GetField<PMap<K, V>.MapEntry<K, V>>(_ => _.Next).GetField<K>(_ => _.Key).InvokeDynamic<K>((_, s, a0) => _.PTypeEquals(s, a0), k)))
                {
                    iter.GetField<PMap<K, V>.MapEntry<K, V>>(_ => _.Next).Assign(iter.GetField<PMap<K, V>.MapEntry<K, V>>(_ => _.Next).GetField<PMap<K, V>.MapEntry<K, V>>(_ => _.Next));
                    return;
                }
            }
        }
    }

    public void Insert(ValueSummary<PMap<K, V>> self, ValueSummary<PTuple<K, V>> t)
    {
        self.InvokeMethod<K, V>((_, s, a0, a1) => _.Insert(s, a0, a1), t.GetField<K>(_ => _.Item1), t.GetField<V>(_ => _.Item2));
    }

    public V this[ValueSummary<K> key]
    {
        get
        {
            return this.Get(key);
        }

        set
        {
            this.Set(key, value);
        }
    }

    public ValueSummary<PMap<K, V>> DeepCopy(ValueSummary<PMap<K, V>> self)
    {
        ValueSummary<PMap<K, V>> ret = new ValueSummary<PMap<K, V>>(new PMap<K, V>(self.GetField<int>(_ => _._capacity)));
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, self.GetField<int>(_ => _._capacity)); i.InvokeMethod((_) => _++))
        {
            ret.GetField<PMap<K, V>.MapEntry<K, V>[]>(_ => _.data).GetIndex((_, a0) => _[a0], i).Assign(self.InvokeMethod<PMap<K, V>.MapEntry<K, V>>((_, s, a0) => _.CopyEntryChain(s, a0), self.GetField<PMap<K, V>.MapEntry<K, V>[]>(_ => _.data).GetIndex((_, a0) => _[a0], i)));
        }

        return ret;
    }

    private ValueSummary<MapEntry<K, V>> CopyEntryChain(ValueSummary<PMap<K, V>> self, ValueSummary<MapEntry<K, V>> entry)
    {
        if (entry.InvokeBinary<object, bool>((l, r) => l == r, null))
        {
            return null;
        }
        else
        {
            return new ValueSummary<PMap<K, V>.MapEntry<K, V>>(new MapEntry<K, V>(entry.GetField<K>(_ => _.Key), entry.GetField<SymbolicInteger>(_ => _.Hash), entry.GetField<V>(_ => _.Value).InvokeDynamic((_, s) => _.DeepCopy(s)), CopyEntryChain.Invoke<PMap<K, V>.MapEntry<K, V>>(entry.GetField<PMap<K, V>.MapEntry<K, V>>(_ => _.Next))));
        }
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode(ValueSummary<PMap<K, V>> self)
    {
        ValueSummary<SymbolicInteger> ret = 1;
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, self.GetField<int>(_ => _._capacity)); i.InvokeMethod((_) => _++))
        {
            ValueSummary<var> entry = self.GetField<PMap<K, V>.MapEntry<K, V>[]>(_ => _.data).GetIndex((_, a0) => _[a0], i);
            while (entry.InvokeBinary<object, bool>((l, r) => l != r, null))
            {
                ret.Assign(entry.GetField<K>(_ => _.Key).GetHashCode().InvokeBinary<int, SymbolicInteger>((l, r) => l ^ r, entry.GetField<V>(_ => _.Value).GetHashCode()));
            }
        }

        return ret;
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PMap<K, V>> self, ValueSummary<PMap<K, V>> other)
    {
        if (self.GetField<int>(_ => _.Count).InvokeBinary<int, bool>((l, r) => l != r, other.GetField<int>(_ => _.Count)))
        {
            return false;
        }

        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, self.GetField<int>(_ => _._capacity)); i.InvokeMethod((_) => _++))
        {
            ValueSummary<var> entry = self.GetField<PMap<K, V>.MapEntry<K, V>[]>(_ => _.data).GetIndex((_, a0) => _[a0], i);
            while (entry.InvokeBinary<object, bool>((l, r) => l != r, null))
            {
                if (other.InvokeMethod<K>((_, s, a0) => _.Get(s, a0), entry.GetField<K>(_ => _.Key)).InvokeBinary<object, SymbolicBool>((l, r) => l != r, null).InvokeBinary<SymbolicBool, bool>((l, r) => l || r, entry.GetField<V>(_ => _.Value).InvokeDynamic<V>((_, s, a0) => _.PTypeEquals(s, a0), other.GetIndex((_, a0) => _[a0], entry.GetField<K>(_ => _.Key))).InvokeMethod((_) => !_)))
                {
                    return false;
                }
            }
        }

        return true;
    }
}