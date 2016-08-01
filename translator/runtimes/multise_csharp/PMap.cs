using System;
using System.Collections.Generic;
using System.Diagnostics;

public class PMap<K, V> : IPType<PMap<K, V>> where K : IPType<K> where V : IPType<V>
{
    private class MapEntry
    {
        ValueSummary<SymbolicInteger> hash;
        ValueSummary<K> key;
        ValueSummary<V> value;
        ValueSummary<MapEntry> next;
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
                this.value = value;
            }
        }

        public ValueSummary<MapEntry> Next
        {
            get
            {
                return this.next;
            }

            set
            {
                this.next = value;
            }
        }

        public MapEntry(ValueSummary<K> k, ValueSummary<SymbolicInteger> hash, ValueSummary<V> v, ValueSummary<MapEntry> next)
        {
            this.key = k;
            this.hash = hash;
            this.value = v;
            this.next = next;
        }
    }

    private ValueSummary<int> _capacity;
    private ValueSummary<int> _count;
    private ValueSummary<ValueSummary<MapEntry>[]> data;
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
        this.data = ValueSummary<MapEntry>.NewVSArray(capacity);
    }

    public ValueSummary<int> Count
    {
        get
        {
            return this._count;
        }
    }

    private void Insert(ValueSummary<K> k, ValueSummary<V> v)
    {
        ValueSummary<SymbolicInteger> hash = k.InvokeMethod((_) => _.PTypeGetHashCode());
        ValueSummary<SymbolicInteger> idx = hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity);
        ValueSummary<MapEntry> firstEntry = this.data.GetIndex<SymbolicInteger, PMap<K, V>.MapEntry>((_, a0) => _[a0], idx);
        for (ValueSummary<MapEntry> iter = firstEntry; iter.InvokeBinary<object, bool>((l, r) => l != r, ValueSummary<object>.Null).Cond(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
        {
            if (iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash).InvokeBinary<SymbolicBool, bool>((l, r) => l && r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)).Cond())
            {
                throw new SystemException("Reinsertion of key" + k.ToString() + "into PMap");
            }
        }

        this.ResizeIfNecessary();
        this.data.SetIndex<SymbolicInteger, PMap<K, V>.MapEntry>((_, a0, r) => _[a0] = r, idx, new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(k, hash, v, firstEntry)));
        this._count.Increment();
    }

    private ValueSummary<V> Get(ValueSummary<K> k)
    {
        ValueSummary<SymbolicInteger> hash = k.InvokeMethod((_) => _.PTypeGetHashCode());
        ValueSummary<SymbolicInteger> idx = hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity);
        ValueSummary<MapEntry> firstEntry = this.data.GetIndex<SymbolicInteger, PMap<K, V>.MapEntry>((_, a0) => _[a0], idx);
        for (ValueSummary<MapEntry> iter = firstEntry; iter.InvokeBinary<object, bool>((l, r) => l != r, ValueSummary<object>.Null).Cond(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
        {
            if (iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash).InvokeBinary<SymbolicBool, bool>((l, r) => l && r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)).Cond())
            {
                return iter.GetField<V>(_ => _.Value);
            }
        }

        throw new SystemException("Key does not exist in dictionary");
    }

    private void Set(ValueSummary<K> k, ValueSummary<V> v)
    {
        ValueSummary<SymbolicInteger> hash = k.InvokeMethod((_) => _.PTypeGetHashCode());
        ValueSummary<SymbolicInteger> idx = hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity);
        ValueSummary<MapEntry> firstEntry = this.data.GetIndex<SymbolicInteger, PMap<K, V>.MapEntry>((_, a0) => _[a0], idx);
        for (ValueSummary<MapEntry> iter = firstEntry; iter.InvokeBinary<object, bool>((l, r) => l != r, ValueSummary<object>.Null).Cond(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
        {
            if (iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash).InvokeBinary<SymbolicBool, bool>((l, r) => l && r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)).Cond())
            {
                iter.SetField<V>((_, r) => _.Value = r, v);
                return;
            }
        }

        this.ResizeIfNecessary();
        this.data.SetIndex<SymbolicInteger, PMap<K, V>.MapEntry>((_, a0, r) => _[a0] = r, idx, new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(k, hash, v, firstEntry)));
        this._count.Increment();
    }

    private void ResizeIfNecessary()
    {
        if (this._count.InvokeBinary<int, double>((l, r) => l / r, this._capacity).InvokeBinary<double, bool>((l, r) => l > r, PMap<K, V>.LOAD_FACTOR).Cond())
        {
            ValueSummary<int> new_capacity = (this._capacity.Cast<double>(_ => (double)_).InvokeBinary<double, double>((l, r) => l * r, PMap<K, V>.RESIZE_RATIO)).Cast<int>(_ => (int)_);
            ValueSummary<ValueSummary<MapEntry>[]> new_data = ValueSummary<MapEntry>.NewVSArray(new_capacity);
            for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity).Cond(); i.Increment())
            {
                for (ValueSummary<MapEntry> iter = this.data.GetIndex<int, PMap<K, V>.MapEntry>((_, a0) => _[a0], i); iter.InvokeBinary<object, bool>((l, r) => l != r, ValueSummary<object>.Null).Cond(); iter.Assign<PMap<K, V>.MapEntry>(iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
                {
                    ValueSummary<SymbolicInteger> idx = iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<int, SymbolicInteger>((l, r) => l % r, new_capacity);
                    new_data.SetIndex<SymbolicInteger, PMap<K, V>.MapEntry>((_, a0, r) => _[a0] = r, idx, new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(iter.GetField<K>(_ => _.Key), iter.GetField<SymbolicInteger>(_ => _.Hash), iter.GetField<V>(_ => _.Value), new_data.GetIndex<SymbolicInteger, PMap<K, V>.MapEntry>((_, a0) => _[a0], idx))));
                }
            }

            this._capacity = new_capacity;
            this.data = new_data;
        }
    }

    public void Remove(ValueSummary<K> k)
    {
        ValueSummary<SymbolicInteger> hash = k.InvokeMethod((_) => _.PTypeGetHashCode());
        ValueSummary<SymbolicInteger> idx = hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity);
        ValueSummary<MapEntry> firstEntry = this.data.GetIndex<SymbolicInteger, PMap<K, V>.MapEntry>((_, a0) => _[a0], idx);
        if (firstEntry.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash).InvokeBinary<SymbolicBool, bool>((l, r) => l && r, firstEntry.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)).Cond())
        {
            this.data.SetIndex<SymbolicInteger, PMap<K, V>.MapEntry>((_, a0, r) => _[a0] = r, idx, firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next));
            return;
        }
        else
        {
            for (ValueSummary<MapEntry> iter = firstEntry; iter.InvokeBinary<object, bool>((l, r) => l != r, ValueSummary<object>.Null).Cond(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
            {
                if (iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next).GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash).InvokeBinary<SymbolicBool, bool>((l, r) => l && r, iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next).GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)).Cond())
                {
                    iter.SetField<PMap<K, V>.MapEntry>((_, r) => _.Next = r, iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next).GetField<PMap<K, V>.MapEntry>(_ => _.Next));
                    return;
                }
            }
        }
    }

    public void Insert(ValueSummary<PTuple<K, V>> t)
    {
        this.Insert(t.GetField<K>(_ => _.Item1), t.GetField<V>(_ => _.Item2));
    }

    public ValueSummary<PBool> ContainsKey(ValueSummary<K> k)
    {
        ValueSummary<SymbolicInteger> hash = k.InvokeMethod((_) => _.PTypeGetHashCode());
        ValueSummary<SymbolicInteger> idx = hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity);
        ValueSummary<MapEntry> firstEntry = this.data.GetIndex<SymbolicInteger, PMap<K, V>.MapEntry>((_, a0) => _[a0], idx);
        for (ValueSummary<MapEntry> iter = firstEntry; iter.InvokeBinary<object, bool>((l, r) => l != r, ValueSummary<object>.Null).Cond(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
        {
            if (iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash).InvokeBinary<SymbolicBool, bool>((l, r) => l && r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)).Cond())
            {
                return (PBool)true;
            }
        }

        return (PBool)false;
    }

    public ValueSummary<V> this[ValueSummary<K> key]
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

    public ValueSummary<PMap<K, V>> DeepCopy()
    {
        ValueSummary<PMap<K, V>> ret = new ValueSummary<PMap<K, V>>(new PMap<K, V>(this._capacity));
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity).Cond(); i.Increment())
        {
            ret.GetField<ValueSummary<PMap<K, V>.MapEntry>[]>(_ => _.data).SetIndex<int, PMap<K, V>.MapEntry>((_, a0, r) => _[a0] = r, i, this.CopyEntryChain(this.data.GetIndex<int, PMap<K, V>.MapEntry>((_, a0) => _[a0], i)));
        }

        return ret;
    }

    private ValueSummary<MapEntry> CopyEntryChain(ValueSummary<MapEntry> entry)
    {
        if (entry.InvokeBinary<object, bool>((l, r) => l == r, ValueSummary<object>.Null).Cond())
        {
            return ValueSummary<PMap<K, V>.MapEntry>.Null;
        }
        else
        {
            return new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(entry.GetField<K>(_ => _.Key), entry.GetField<SymbolicInteger>(_ => _.Hash), entry.GetField<V>(_ => _.Value).InvokeMethod((_) => _.DeepCopy()), this.CopyEntryChain(entry.GetField<PMap<K, V>.MapEntry>(_ => _.Next))));
        }
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode()
    {
        ValueSummary<SymbolicInteger> ret = (SymbolicInteger)1;
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity).Cond(); i.Increment())
        {
            ValueSummary<MapEntry> entry = this.data.GetIndex<int, PMap<K, V>.MapEntry>((_, a0) => _[a0], i);
            while (entry.InvokeBinary<object, bool>((l, r) => l != r, ValueSummary<object>.Null).Cond())
            {
                ret.Assign<SymbolicInteger>(entry.GetField<K>(_ => _.Key).InvokeMethod((_) => _.PTypeGetHashCode()).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, entry.GetField<V>(_ => _.Value).InvokeMethod((_) => _.PTypeGetHashCode())));
            }
        }

        return ret;
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PMap<K, V>> other)
    {
        if (this.Count.InvokeBinary<int, bool>((l, r) => l != r, other.GetField<int>(_ => _.Count)).Cond())
        {
            return (SymbolicBool)false;
        }

        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity).Cond(); i.Increment())
        {
            ValueSummary<MapEntry> entry = this.data.GetIndex<int, PMap<K, V>.MapEntry>((_, a0) => _[a0], i);
            while (entry.InvokeBinary<object, bool>((l, r) => l != r, ValueSummary<object>.Null).Cond())
            {
                if (other.InvokeMethod<K, V>((_, a0) => _.Get(a0), entry.GetField<K>(_ => _.Key)).Cast<object>(_ => (object)_).InvokeBinary<object, SymbolicBool>((l, r) => l != r, ValueSummary<object>.Null).InvokeBinary<SymbolicBool, bool>((l, r) => l || r, entry.GetField<V>(_ => _.Value).InvokeMethod<V, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.InvokeMethod<K, V>((_, a0) => _[a0], entry.GetField<K>(_ => _.Key))).InvokeUnary<SymbolicBool>(_ => !_)).Cond())
                {
                    return (SymbolicBool)false;
                }
            }
        }

        return (SymbolicBool)true;
    }
}