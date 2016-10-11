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
                this.value.Assign<V>(value);
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
                this.next.Assign<PMap<K, V>.MapEntry>(value);
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
        PathConstraint.PushFrame();
        ValueSummary<SymbolicInteger> hash = ValueSummary<SymbolicInteger>.InitializeFrom(k.InvokeMethod((_) => _.PTypeGetHashCode()));
        ValueSummary<SymbolicInteger> idx = ValueSummary<SymbolicInteger>.InitializeFrom(hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity));
        ValueSummary<MapEntry> firstEntry = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(idx));
        {
            PathConstraint.BeginLoop();
            for (ValueSummary<MapEntry> iter = ValueSummary<MapEntry>.InitializeFrom(firstEntry); iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Loop(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
            {
                ValueSummary<SymbolicBool> vs_lgc_tmp_0;
                {
                    var vs_cond_32 = ((new Func<ValueSummary<SymbolicBool>>(() =>
                    {
                        var vs_cond_1 = ((vs_lgc_tmp_0 = iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash))).Cond();
                        var vs_cond_ret_1 = new ValueSummary<SymbolicBool>();
                        if (vs_cond_1.CondTrue())
                            vs_cond_ret_1.Merge(vs_lgc_tmp_0.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)));
                        if (vs_cond_1.CondFalse())
                            vs_cond_ret_1.Merge(vs_lgc_tmp_0);
                        vs_cond_1.MergeBranch();
                        return vs_cond_ret_1;
                    }

                    )())).Cond();
                    if (vs_cond_32.CondTrue())
                    {
                        throw new SystemException("Reinsertion of key" + k.ToString() + "into PMap");
                    }

                    vs_cond_32.MergeBranch();
                }
            }
        }

        this.ResizeIfNecessary();
        this.data.SetIndex<PMap<K, V>.MapEntry>(idx, new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(k, hash, v, firstEntry)));
        this._count.Increment();
        PathConstraint.PopFrame();
    }

    private ValueSummary<V> Get(ValueSummary<K> k)
    {
        PathConstraint.PushFrame();
        var vs_ret_8 = new ValueSummary<V>();
        ValueSummary<SymbolicInteger> hash = ValueSummary<SymbolicInteger>.InitializeFrom(k.InvokeMethod((_) => _.PTypeGetHashCode()));
        ValueSummary<SymbolicInteger> idx = ValueSummary<SymbolicInteger>.InitializeFrom(hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity));
        ValueSummary<MapEntry> firstEntry = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(idx));
        {
            PathConstraint.BeginLoop();
            for (ValueSummary<MapEntry> iter = ValueSummary<MapEntry>.InitializeFrom(firstEntry); iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Loop(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
            {
                ValueSummary<SymbolicBool> vs_lgc_tmp_1;
                {
                    var vs_cond_33 = ((new Func<ValueSummary<SymbolicBool>>(() =>
                    {
                        var vs_cond_2 = ((vs_lgc_tmp_1 = iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash))).Cond();
                        var vs_cond_ret_2 = new ValueSummary<SymbolicBool>();
                        if (vs_cond_2.CondTrue())
                            vs_cond_ret_2.Merge(vs_lgc_tmp_1.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)));
                        if (vs_cond_2.CondFalse())
                            vs_cond_ret_2.Merge(vs_lgc_tmp_1);
                        vs_cond_2.MergeBranch();
                        return vs_cond_ret_2;
                    }

                    )())).Cond();
                    if (vs_cond_33.CondTrue())
                    {
                        vs_ret_8.RecordReturn(iter.GetField<V>(_ => _.Value));
                    }

                    vs_cond_33.MergeBranch();
                }
            }
        }

        if (PathConstraint.MergedPcFeasible())
        {
            throw new SystemException("Key does not exist in dictionary");
        }

        PathConstraint.PopFrame();
        return vs_ret_8;
    }

    private void Set(ValueSummary<K> k, ValueSummary<V> v)
    {
        PathConstraint.PushFrame();
        ValueSummary<SymbolicInteger> hash = ValueSummary<SymbolicInteger>.InitializeFrom(k.InvokeMethod((_) => _.PTypeGetHashCode()));
        ValueSummary<SymbolicInteger> idx = ValueSummary<SymbolicInteger>.InitializeFrom(hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity));
        ValueSummary<MapEntry> firstEntry = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(idx));
        {
            PathConstraint.BeginLoop();
            for (ValueSummary<MapEntry> iter = ValueSummary<MapEntry>.InitializeFrom(firstEntry); iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Loop(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
            {
                ValueSummary<SymbolicBool> vs_lgc_tmp_2;
                {
                    var vs_cond_34 = ((new Func<ValueSummary<SymbolicBool>>(() =>
                    {
                        var vs_cond_3 = ((vs_lgc_tmp_2 = iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash))).Cond();
                        var vs_cond_ret_3 = new ValueSummary<SymbolicBool>();
                        if (vs_cond_3.CondTrue())
                            vs_cond_ret_3.Merge(vs_lgc_tmp_2.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)));
                        if (vs_cond_3.CondFalse())
                            vs_cond_ret_3.Merge(vs_lgc_tmp_2);
                        vs_cond_3.MergeBranch();
                        return vs_cond_ret_3;
                    }

                    )())).Cond();
                    if (vs_cond_34.CondTrue())
                    {
                        iter.SetField<V>((_) => _.Value, v);
                        PathConstraint.RecordReturnPath();
                    }

                    vs_cond_34.MergeBranch();
                }
            }
        }

        if (PathConstraint.MergedPcFeasible())
        {
            this.ResizeIfNecessary();
            this.data.SetIndex<PMap<K, V>.MapEntry>(idx, new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(k, hash, v, firstEntry)));
            this._count.Increment();
        }

        PathConstraint.PopFrame();
    }

    private void ResizeIfNecessary()
    {
        {
            var vs_cond_35 = (this._count.InvokeBinary<int, int>((l, r) => l / r, this._capacity).InvokeBinary<double, bool>((l, r) => l > r, PMap<K, V>.LOAD_FACTOR)).Cond();
            if (vs_cond_35.CondTrue())
            {
                ValueSummary<int> new_capacity = ValueSummary<int>.InitializeFrom((this._capacity.Cast<double>(_ => (double)_).InvokeBinary<double, double>((l, r) => l * r, PMap<K, V>.RESIZE_RATIO)).Cast<int>(_ => (int)_));
                ValueSummary<ValueSummary<MapEntry>[]> new_data = ValueSummary<ValueSummary<MapEntry>[]>.InitializeFrom(ValueSummary<MapEntry>.NewVSArray(new_capacity));
                {
                    PathConstraint.BeginLoop();
                    for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity).Loop(); i.Increment())
                    {
                        {
                            PathConstraint.BeginLoop();
                            for (ValueSummary<MapEntry> iter = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(i)); iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Loop(); iter.Assign<PMap<K, V>.MapEntry>(iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
                            {
                                ValueSummary<SymbolicInteger> idx = ValueSummary<SymbolicInteger>.InitializeFrom(iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<int, SymbolicInteger>((l, r) => l % r, new_capacity));
                                new_data.SetIndex<PMap<K, V>.MapEntry>(idx, new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(iter.GetField<K>(_ => _.Key), iter.GetField<SymbolicInteger>(_ => _.Hash), iter.GetField<V>(_ => _.Value), new_data.GetIndex<PMap<K, V>.MapEntry>(idx))));
                            }
                        }
                    }
                }

                this._capacity.Assign<int>(new_capacity);
                this.data.Assign(new_data);
            }

            vs_cond_35.MergeBranch();
        }
    }

    public void Remove(ValueSummary<K> k)
    {
        PathConstraint.PushFrame();
        ValueSummary<SymbolicBool> vs_lgc_tmp_3;
        ValueSummary<SymbolicInteger> hash = ValueSummary<SymbolicInteger>.InitializeFrom(k.InvokeMethod((_) => _.PTypeGetHashCode()));
        ValueSummary<SymbolicInteger> idx = ValueSummary<SymbolicInteger>.InitializeFrom(hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity));
        ValueSummary<MapEntry> firstEntry = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(idx));
        {
            var vs_cond_36 = ((new Func<ValueSummary<SymbolicBool>>(() =>
            {
                var vs_cond_4 = ((vs_lgc_tmp_3 = firstEntry.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash))).Cond();
                var vs_cond_ret_4 = new ValueSummary<SymbolicBool>();
                if (vs_cond_4.CondTrue())
                    vs_cond_ret_4.Merge(vs_lgc_tmp_3.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, firstEntry.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)));
                if (vs_cond_4.CondFalse())
                    vs_cond_ret_4.Merge(vs_lgc_tmp_3);
                vs_cond_4.MergeBranch();
                return vs_cond_ret_4;
            }

            )())).Cond();
            if (vs_cond_36.CondTrue())
            {
                this.data.SetIndex<PMap<K, V>.MapEntry>(idx, firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next));
                PathConstraint.RecordReturnPath();
            }

            if (vs_cond_36.CondFalse())
            {
                {
                    PathConstraint.BeginLoop();
                    for (ValueSummary<MapEntry> iter = ValueSummary<MapEntry>.InitializeFrom(firstEntry); iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Loop(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
                    {
                        ValueSummary<SymbolicBool> vs_lgc_tmp_4;
                        {
                            var vs_cond_37 = ((new Func<ValueSummary<SymbolicBool>>(() =>
                            {
                                var vs_cond_5 = ((vs_lgc_tmp_4 = iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next).GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash))).Cond();
                                var vs_cond_ret_5 = new ValueSummary<SymbolicBool>();
                                if (vs_cond_5.CondTrue())
                                    vs_cond_ret_5.Merge(vs_lgc_tmp_4.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next).GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)));
                                if (vs_cond_5.CondFalse())
                                    vs_cond_ret_5.Merge(vs_lgc_tmp_4);
                                vs_cond_5.MergeBranch();
                                return vs_cond_ret_5;
                            }

                            )())).Cond();
                            if (vs_cond_37.CondTrue())
                            {
                                iter.SetField<PMap<K, V>.MapEntry>((_) => _.Next, iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next).GetField<PMap<K, V>.MapEntry>(_ => _.Next));
                                PathConstraint.RecordReturnPath();
                            }

                            vs_cond_37.MergeBranch();
                        }
                    }
                }
            }

            vs_cond_36.MergeBranch();
        }

        PathConstraint.PopFrame();
    }

    public void Insert(ValueSummary<PTuple<K, V>> t)
    {
        this.Insert(t.GetField<K>(_ => _.Item1), t.GetField<V>(_ => _.Item2));
    }

    public ValueSummary<PBool> ContainsKey(ValueSummary<K> k)
    {
        PathConstraint.PushFrame();
        var vs_ret_13 = new ValueSummary<PBool>();
        ValueSummary<SymbolicInteger> hash = ValueSummary<SymbolicInteger>.InitializeFrom(k.InvokeMethod((_) => _.PTypeGetHashCode()));
        ValueSummary<SymbolicInteger> idx = ValueSummary<SymbolicInteger>.InitializeFrom(hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity));
        ValueSummary<MapEntry> firstEntry = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(idx));
        {
            PathConstraint.BeginLoop();
            for (ValueSummary<MapEntry> iter = ValueSummary<MapEntry>.InitializeFrom(firstEntry); iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Loop(); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
            {
                ValueSummary<SymbolicBool> vs_lgc_tmp_5;
                {
                    var vs_cond_38 = ((new Func<ValueSummary<SymbolicBool>>(() =>
                    {
                        var vs_cond_6 = ((vs_lgc_tmp_5 = iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash))).Cond();
                        var vs_cond_ret_6 = new ValueSummary<SymbolicBool>();
                        if (vs_cond_6.CondTrue())
                            vs_cond_ret_6.Merge(vs_lgc_tmp_5.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)));
                        if (vs_cond_6.CondFalse())
                            vs_cond_ret_6.Merge(vs_lgc_tmp_5);
                        vs_cond_6.MergeBranch();
                        return vs_cond_ret_6;
                    }

                    )())).Cond();
                    if (vs_cond_38.CondTrue())
                    {
                        vs_ret_13.RecordReturn((PBool)true);
                    }

                    vs_cond_38.MergeBranch();
                }
            }
        }

        if (PathConstraint.MergedPcFeasible())
        {
            vs_ret_13.RecordReturn((PBool)false);
        }

        PathConstraint.PopFrame();
        return vs_ret_13;
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
        {
            PathConstraint.BeginLoop();
            for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity).Loop(); i.Increment())
            {
                ret.GetField<ValueSummary<PMap<K, V>.MapEntry>[]>(_ => _.data).SetIndex<PMap<K, V>.MapEntry>(i, this.CopyEntryChain(this.data.GetIndex<PMap<K, V>.MapEntry>(i)));
            }
        }

        return ret;
    }

    private ValueSummary<MapEntry> CopyEntryChain(ValueSummary<MapEntry> entry)
    {
        PathConstraint.PushFrame();
        var vs_ret_17 = new ValueSummary<MapEntry>();
        {
            var vs_cond_39 = (entry.InvokeBinary<object, bool>((l, r) => l == r, new ValueSummary<object>(null))).Cond();
            if (vs_cond_39.CondTrue())
            {
                vs_ret_17.RecordReturn(new ValueSummary<PMap<K, V>.MapEntry>(null));
            }

            if (vs_cond_39.CondFalse())
            {
                vs_ret_17.RecordReturn(new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(entry.GetField<K>(_ => _.Key), entry.GetField<SymbolicInteger>(_ => _.Hash), entry.GetField<V>(_ => _.Value).InvokeMethod((_) => _.DeepCopy()), this.CopyEntryChain(entry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))));
            }

            vs_cond_39.MergeBranch();
        }

        PathConstraint.PopFrame();
        return vs_ret_17;
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode()
    {
        ValueSummary<SymbolicInteger> ret = ValueSummary<SymbolicInteger>.InitializeFrom((SymbolicInteger)1);
        {
            PathConstraint.BeginLoop();
            for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity).Loop(); i.Increment())
            {
                ValueSummary<MapEntry> entry = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(i));
                {
                    PathConstraint.BeginLoop();
                    while (entry.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Loop())
                    {
                        ret.Assign<SymbolicInteger>(entry.GetField<K>(_ => _.Key).InvokeMethod((_) => _.PTypeGetHashCode()).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, entry.GetField<V>(_ => _.Value).InvokeMethod((_) => _.PTypeGetHashCode())));
                    }
                }
            }
        }

        return ret;
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PMap<K, V>> other)
    {
        PathConstraint.PushFrame();
        var vs_ret_19 = new ValueSummary<SymbolicBool>();
        {
            var vs_cond_40 = (this.Count.InvokeBinary<int, bool>((l, r) => l != r, other.GetField<int>(_ => _.Count))).Cond();
            if (vs_cond_40.CondTrue())
            {
                vs_ret_19.RecordReturn((SymbolicBool)false);
            }

            vs_cond_40.MergeBranch();
        }

        if (PathConstraint.MergedPcFeasible())
        {
            {
                PathConstraint.BeginLoop();
                for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity).Loop(); i.Increment())
                {
                    ValueSummary<MapEntry> entry = ValueSummary<MapEntry>.InitializeFrom(this.data.GetIndex<PMap<K, V>.MapEntry>(i));
                    {
                        PathConstraint.BeginLoop();
                        while (entry.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null)).Loop())
                        {
                            ValueSummary<SymbolicBool> vs_lgc_tmp_6;
                            {
                                var vs_cond_41 = ((new Func<ValueSummary<SymbolicBool>>(() =>
                                {
                                    var vs_cond_7 = ((vs_lgc_tmp_6 = other.InvokeMethod<K, V>((_, a0) => _.Get(a0), entry.GetField<K>(_ => _.Key)).Cast<object>(_ => (object)_).InvokeBinary<object, SymbolicBool>((l, r) => l != r, new ValueSummary<object>(null)))).Cond();
                                    var vs_cond_ret_7 = new ValueSummary<SymbolicBool>();
                                    if (vs_cond_7.CondTrue())
                                        vs_cond_ret_7.Merge(vs_lgc_tmp_6);
                                    if (vs_cond_7.CondFalse())
                                        vs_cond_ret_7.Merge(vs_lgc_tmp_6.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l | r, entry.GetField<V>(_ => _.Value).InvokeMethod<V, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.InvokeMethod<K, V>((_, a0) => _[a0], entry.GetField<K>(_ => _.Key))).InvokeUnary<SymbolicBool>(_ => !_)));
                                    vs_cond_7.MergeBranch();
                                    return vs_cond_ret_7;
                                }

                                )())).Cond();
                                if (vs_cond_41.CondTrue())
                                {
                                    vs_ret_19.RecordReturn((SymbolicBool)false);
                                }

                                vs_cond_41.MergeBranch();
                            }
                        }
                    }
                }
            }

            if (PathConstraint.MergedPcFeasible())
            {
                vs_ret_19.RecordReturn((SymbolicBool)true);
            }
        }

        PathConstraint.PopFrame();
        return vs_ret_19;
    }
}