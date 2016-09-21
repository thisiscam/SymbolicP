using System;
using System.Collections.Generic;
using System.Diagnostics;

public class PMap<K, V> : IPType<PMap<K, V>> where K : IPType<K> where V : IPType<V>
{
    private class MapEntry
    {
        ValueSummary<SymbolicInteger> hash = new ValueSummary<SymbolicInteger>(default (SymbolicInteger));
        ValueSummary<K> key = new ValueSummary<K>(default (K));
        ValueSummary<V> value = new ValueSummary<V>(default (V));
        ValueSummary<MapEntry> next = new ValueSummary<MapEntry>(default (MapEntry));
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

    private ValueSummary<int> _capacity = new ValueSummary<int>(default (int));
    private ValueSummary<int> _count = new ValueSummary<int>(default (int));
    private ValueSummary<ValueSummary<MapEntry>[]> data = new ValueSummary<ValueSummary<MapEntry>[]>(default (ValueSummary<MapEntry>[]));
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
        ValueSummary<SymbolicInteger> hash = ValueSummary<SymbolicInteger>.InitializeFrom(k.InvokeMethod((_) => _.PTypeGetHashCode()));
        ValueSummary<SymbolicInteger> idx = ValueSummary<SymbolicInteger>.InitializeFrom(hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity));
        ValueSummary<MapEntry> firstEntry = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(idx));
        for (ValueSummary<MapEntry> iter = ValueSummary<MapEntry>.InitializeFrom(firstEntry); iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Cond(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
        {
            ValueSummary<SymbolicBool> vs_lgc_tmp_0;
            if (((vs_lgc_tmp_0 = iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash)).Cond() ? vs_lgc_tmp_0.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)) : vs_lgc_tmp_0).Cond())
            {
                throw new SystemException("Reinsertion of key" + k.ToString() + "into PMap");
            }
        }

        this.ResizeIfNecessary();
        this.data.SetIndex<PMap<K, V>.MapEntry>(idx, new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(k, hash, v, firstEntry)));
        this._count.Increment();
    }

    private ValueSummary<V> Get(ValueSummary<K> k)
    {
        ValueSummary<SymbolicInteger> hash = ValueSummary<SymbolicInteger>.InitializeFrom(k.InvokeMethod((_) => _.PTypeGetHashCode()));
        ValueSummary<SymbolicInteger> idx = ValueSummary<SymbolicInteger>.InitializeFrom(hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity));
        ValueSummary<MapEntry> firstEntry = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(idx));
        for (ValueSummary<MapEntry> iter = ValueSummary<MapEntry>.InitializeFrom(firstEntry); iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Cond(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
        {
            ValueSummary<SymbolicBool> vs_lgc_tmp_1;
            if (((vs_lgc_tmp_1 = iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash)).Cond() ? vs_lgc_tmp_1.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)) : vs_lgc_tmp_1).Cond())
            {
                return iter.GetField<V>(_ => _.Value);
            }
        }

        throw new SystemException("Key does not exist in dictionary");
    }

    private void Set(ValueSummary<K> k, ValueSummary<V> v)
    {
        ValueSummary<SymbolicInteger> hash = ValueSummary<SymbolicInteger>.InitializeFrom(k.InvokeMethod((_) => _.PTypeGetHashCode()));
        ValueSummary<SymbolicInteger> idx = ValueSummary<SymbolicInteger>.InitializeFrom(hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity));
        ValueSummary<MapEntry> firstEntry = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(idx));
        for (ValueSummary<MapEntry> iter = ValueSummary<MapEntry>.InitializeFrom(firstEntry); iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Cond(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
        {
            ValueSummary<SymbolicBool> vs_lgc_tmp_2;
            if (((vs_lgc_tmp_2 = iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash)).Cond() ? vs_lgc_tmp_2.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)) : vs_lgc_tmp_2).Cond())
            {
                iter.SetField<V>((_) => _.Value, v);
                return;
            }
        }

        this.ResizeIfNecessary();
        this.data.SetIndex<PMap<K, V>.MapEntry>(idx, new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(k, hash, v, firstEntry)));
        this._count.Increment();
    }

    private void ResizeIfNecessary()
    {
        if (this._count.InvokeBinary<int, int>((l, r) => l / r, this._capacity).InvokeBinary<double, bool>((l, r) => l > r, PMap<K, V>.LOAD_FACTOR).Cond())
        {
            ValueSummary<int> new_capacity = ValueSummary<int>.InitializeFrom((this._capacity.Cast<double>(_ => (double)_).InvokeBinary<double, double>((l, r) => l * r, PMap<K, V>.RESIZE_RATIO)).Cast<int>(_ => (int)_));
            ValueSummary<ValueSummary<MapEntry>[]> new_data = ValueSummary<ValueSummary<MapEntry>[]>.InitializeFrom(ValueSummary<MapEntry>.NewVSArray(new_capacity));
            for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity).Cond(); i.Increment())
            {
                for (ValueSummary<MapEntry> iter = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(i)); iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Cond(); iter.Assign<PMap<K, V>.MapEntry>(iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
                {
                    ValueSummary<SymbolicInteger> idx = ValueSummary<SymbolicInteger>.InitializeFrom(iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<int, SymbolicInteger>((l, r) => l % r, new_capacity));
                    new_data.SetIndex<PMap<K, V>.MapEntry>(idx, new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(iter.GetField<K>(_ => _.Key), iter.GetField<SymbolicInteger>(_ => _.Hash), iter.GetField<V>(_ => _.Value), new_data.GetIndex<PMap<K, V>.MapEntry>(idx))));
                }
            }

            this._capacity = new_capacity;
            this.data = new_data;
        }
    }

    public void Remove(ValueSummary<K> k)
    {
        ValueSummary<SymbolicBool> vs_lgc_tmp_3;
        ValueSummary<SymbolicInteger> hash = ValueSummary<SymbolicInteger>.InitializeFrom(k.InvokeMethod((_) => _.PTypeGetHashCode()));
        ValueSummary<SymbolicInteger> idx = ValueSummary<SymbolicInteger>.InitializeFrom(hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity));
        ValueSummary<MapEntry> firstEntry = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(idx));
        if (((vs_lgc_tmp_3 = firstEntry.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash)).Cond() ? vs_lgc_tmp_3.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, firstEntry.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)) : vs_lgc_tmp_3).Cond())
        {
            this.data.SetIndex<PMap<K, V>.MapEntry>(idx, firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next));
            return;
        }
        else
        {
            for (ValueSummary<MapEntry> iter = ValueSummary<MapEntry>.InitializeFrom(firstEntry); iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Cond(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
            {
                ValueSummary<SymbolicBool> vs_lgc_tmp_4;
                if (((vs_lgc_tmp_4 = iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next).GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash)).Cond() ? vs_lgc_tmp_4.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next).GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)) : vs_lgc_tmp_4).Cond())
                {
                    iter.SetField<PMap<K, V>.MapEntry>((_) => _.Next, iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next).GetField<PMap<K, V>.MapEntry>(_ => _.Next));
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
        ValueSummary<SymbolicInteger> hash = ValueSummary<SymbolicInteger>.InitializeFrom(k.InvokeMethod((_) => _.PTypeGetHashCode()));
        ValueSummary<SymbolicInteger> idx = ValueSummary<SymbolicInteger>.InitializeFrom(hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity));
        ValueSummary<MapEntry> firstEntry = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(idx));
        for (ValueSummary<MapEntry> iter = ValueSummary<MapEntry>.InitializeFrom(firstEntry); iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Cond(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
        {
            ValueSummary<SymbolicBool> vs_lgc_tmp_5;
            if (((vs_lgc_tmp_5 = iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash)).Cond() ? vs_lgc_tmp_5.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)) : vs_lgc_tmp_5).Cond())
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
        ValueSummary<PMap<K, V>> ret = ValueSummary<PMap<K, V>>.InitializeFrom(new ValueSummary<PMap<K, V>>(new PMap<K, V>(this._capacity)));
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity).Cond(); i.Increment())
        {
            ret.GetField<ValueSummary<PMap<K, V>.MapEntry>[]>(_ => _.data).SetIndex<PMap<K, V>.MapEntry>(i, this.CopyEntryChain(this.data.GetIndex<PMap<K, V>.MapEntry>(i)));
        }

        return ret;
    }

    private ValueSummary<MapEntry> CopyEntryChain(ValueSummary<MapEntry> entry)
    {
        if (entry.InvokeBinary<object, bool>((l, r) => l == r, new ValueSummary<object>(null)).Cond())
        {
            return new ValueSummary<PMap<K, V>.MapEntry>(null);
        }
        else
        {
            return new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(entry.GetField<K>(_ => _.Key), entry.GetField<SymbolicInteger>(_ => _.Hash), entry.GetField<V>(_ => _.Value).InvokeMethod((_) => _.DeepCopy()), this.CopyEntryChain(entry.GetField<PMap<K, V>.MapEntry>(_ => _.Next))));
        }
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode()
    {
        ValueSummary<SymbolicInteger> ret = ValueSummary<SymbolicInteger>.InitializeFrom((SymbolicInteger)1);
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity).Cond(); i.Increment())
        {
            ValueSummary<MapEntry> entry = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(i));
            while (entry.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Cond())
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
            ValueSummary<MapEntry> entry = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(i));
            while (entry.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Cond())
            {
                ValueSummary<SymbolicBool> vs_lgc_tmp_6;
                if (((vs_lgc_tmp_6 = other.InvokeMethod<K, V>((_, a0) => _.Get(a0), entry.GetField<K>(_ => _.Key)).Cast<object>(_ => (object)_).InvokeBinary<object, SymbolicBool>((l, r) => l != r, new ValueSummary<object>(null))).Cond() ? vs_lgc_tmp_6 : vs_lgc_tmp_6.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l | r, entry.GetField<V>(_ => _.Value).InvokeMethod<V, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.InvokeMethod<K, V>((_, a0) => _[a0], entry.GetField<K>(_ => _.Key))).InvokeUnary<SymbolicBool>(_ => !_))).Cond())
                {
                    return (SymbolicBool)false;
                }
            }
        }

        return (SymbolicBool)true;
    }
}