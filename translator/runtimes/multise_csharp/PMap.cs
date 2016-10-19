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
        ValueSummary<SymbolicInteger> hash = k.InvokeMethod((_) => _.PTypeGetHashCode());
        ValueSummary<SymbolicInteger> idx = hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity);
        ValueSummary<MapEntry> firstEntry = this.data.GetIndex<PMap<K, V>.MapEntry>(idx);
        var vs_cond_27 = PathConstraint.BeginLoop();
        for (ValueSummary<MapEntry> iter = firstEntry; vs_cond_27.Loop(iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null))); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
        {
            ValueSummary<SymbolicBool> vs_lgc_tmp_0;
            var vs_cond_26 = ((new Func<ValueSummary<SymbolicBool>>(() =>
            {
                var vs_cond_49 = ((vs_lgc_tmp_0 = ValueSummary<SymbolicBool>.InitializeFrom(iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash)))).Cond();
                var vs_cond_ret_49 = new ValueSummary<SymbolicBool>();
                if (vs_cond_49.CondTrue())
                    vs_cond_ret_49.Merge(vs_lgc_tmp_0.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)));
                if (vs_cond_49.CondFalse())
                    vs_cond_ret_49.Merge(vs_lgc_tmp_0);
                vs_cond_49.MergeBranch();
                return vs_cond_ret_49;
            }

            )())).Cond();
            {
                if (vs_cond_26.CondTrue())
                {
                    throw new SystemException("Reinsertion of key" + k.ToString() + "into PMap");
                }
            }

            vs_cond_26.MergeBranch();
        }

        vs_cond_27.MergeBranch();
        this.ResizeIfNecessary();
        this.data.SetIndex<PMap<K, V>.MapEntry>(idx, new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(k, hash, v, firstEntry)));
        this._count.Increment();
    }

    private ValueSummary<V> Get(ValueSummary<K> k)
    {
        PathConstraint.PushFrame();
        var vs_ret_1 = new ValueSummary<V>();
        ValueSummary<SymbolicInteger> hash = k.InvokeMethod((_) => _.PTypeGetHashCode());
        ValueSummary<SymbolicInteger> idx = hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity);
        ValueSummary<MapEntry> firstEntry = this.data.GetIndex<PMap<K, V>.MapEntry>(idx);
        var vs_cond_29 = PathConstraint.BeginLoop();
        for (ValueSummary<MapEntry> iter = firstEntry; vs_cond_29.Loop(iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null))); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
        {
            ValueSummary<SymbolicBool> vs_lgc_tmp_1;
            var vs_cond_28 = ((new Func<ValueSummary<SymbolicBool>>(() =>
            {
                var vs_cond_50 = ((vs_lgc_tmp_1 = ValueSummary<SymbolicBool>.InitializeFrom(iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash)))).Cond();
                var vs_cond_ret_50 = new ValueSummary<SymbolicBool>();
                if (vs_cond_50.CondTrue())
                    vs_cond_ret_50.Merge(vs_lgc_tmp_1.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)));
                if (vs_cond_50.CondFalse())
                    vs_cond_ret_50.Merge(vs_lgc_tmp_1);
                vs_cond_50.MergeBranch();
                return vs_cond_ret_50;
            }

            )())).Cond();
            {
                if (vs_cond_28.CondTrue())
                {
                    PathConstraint.RecordReturnPath(vs_ret_1, iter.GetField<V>(_ => _.Value), vs_cond_28);
                }
            }

            vs_cond_28.MergeBranch(vs_cond_29);
        }

        if (vs_cond_29.MergeBranch())
        {
            throw new SystemException("Key does not exist in dictionary");
        }

        PathConstraint.PopFrame();
        return vs_ret_1;
    }

    private void Set(ValueSummary<K> k, ValueSummary<V> v)
    {
        PathConstraint.PushFrame();
        ValueSummary<SymbolicInteger> hash = k.InvokeMethod((_) => _.PTypeGetHashCode());
        ValueSummary<SymbolicInteger> idx = hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity);
        ValueSummary<MapEntry> firstEntry = this.data.GetIndex<PMap<K, V>.MapEntry>(idx);
        var vs_cond_31 = PathConstraint.BeginLoop();
        for (ValueSummary<MapEntry> iter = firstEntry; vs_cond_31.Loop(iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null))); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
        {
            ValueSummary<SymbolicBool> vs_lgc_tmp_2;
            var vs_cond_30 = ((new Func<ValueSummary<SymbolicBool>>(() =>
            {
                var vs_cond_51 = ((vs_lgc_tmp_2 = ValueSummary<SymbolicBool>.InitializeFrom(iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash)))).Cond();
                var vs_cond_ret_51 = new ValueSummary<SymbolicBool>();
                if (vs_cond_51.CondTrue())
                    vs_cond_ret_51.Merge(vs_lgc_tmp_2.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)));
                if (vs_cond_51.CondFalse())
                    vs_cond_ret_51.Merge(vs_lgc_tmp_2);
                vs_cond_51.MergeBranch();
                return vs_cond_ret_51;
            }

            )())).Cond();
            {
                if (vs_cond_30.CondTrue())
                {
                    iter.SetField<V>((_) => _.Value, v);
                    PathConstraint.RecordReturnPath(vs_cond_30);
                }
            }

            vs_cond_30.MergeBranch(vs_cond_31);
        }

        if (vs_cond_31.MergeBranch())
        {
            this.ResizeIfNecessary();
            this.data.SetIndex<PMap<K, V>.MapEntry>(idx, new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(k, hash, v, firstEntry)));
            this._count.Increment();
        }

        PathConstraint.PopFrame();
    }

    private void ResizeIfNecessary()
    {
        var vs_cond_34 = (this._count.InvokeBinary<int, int>((l, r) => l / r, this._capacity).InvokeBinary<double, bool>((l, r) => l > r, PMap<K, V>.LOAD_FACTOR)).Cond();
        {
            if (vs_cond_34.CondTrue())
            {
                ValueSummary<int> new_capacity = (this._capacity.Cast<double>(_ => (double)_).InvokeBinary<double, double>((l, r) => l * r, PMap<K, V>.RESIZE_RATIO)).Cast<int>(_ => (int)_);
                ValueSummary<ValueSummary<MapEntry>[]> new_data = ValueSummary<MapEntry>.NewVSArray(new_capacity);
                var vs_cond_33 = PathConstraint.BeginLoop();
                for (ValueSummary<int> i = 0; vs_cond_33.Loop(i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity)); i.Increment())
                {
                    var vs_cond_32 = PathConstraint.BeginLoop();
                    for (ValueSummary<MapEntry> iter = this.data.GetIndex<PMap<K, V>.MapEntry>(i); vs_cond_32.Loop(iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null))); iter.Assign<PMap<K, V>.MapEntry>(iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
                    {
                        ValueSummary<SymbolicInteger> idx = iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<int, SymbolicInteger>((l, r) => l % r, new_capacity);
                        new_data.SetIndex<PMap<K, V>.MapEntry>(idx, new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(iter.GetField<K>(_ => _.Key), iter.GetField<SymbolicInteger>(_ => _.Hash), iter.GetField<V>(_ => _.Value), new_data.GetIndex<PMap<K, V>.MapEntry>(idx))));
                    }

                    vs_cond_32.MergeBranch();
                }

                vs_cond_33.MergeBranch();
                this._capacity.Assign<int>(new_capacity);
                this.data.Assign(new_data);
            }
        }

        vs_cond_34.MergeBranch();
    }

    public void Remove(ValueSummary<K> k)
    {
        PathConstraint.PushFrame();
        ValueSummary<SymbolicBool> vs_lgc_tmp_3;
        ValueSummary<SymbolicInteger> hash = k.InvokeMethod((_) => _.PTypeGetHashCode());
        ValueSummary<SymbolicInteger> idx = hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity);
        ValueSummary<MapEntry> firstEntry = this.data.GetIndex<PMap<K, V>.MapEntry>(idx);
        var vs_cond_37 = ((new Func<ValueSummary<SymbolicBool>>(() =>
        {
            var vs_cond_52 = ((vs_lgc_tmp_3 = ValueSummary<SymbolicBool>.InitializeFrom(firstEntry.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash)))).Cond();
            var vs_cond_ret_52 = new ValueSummary<SymbolicBool>();
            if (vs_cond_52.CondTrue())
                vs_cond_ret_52.Merge(vs_lgc_tmp_3.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, firstEntry.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)));
            if (vs_cond_52.CondFalse())
                vs_cond_ret_52.Merge(vs_lgc_tmp_3);
            vs_cond_52.MergeBranch();
            return vs_cond_ret_52;
        }

        )())).Cond();
        {
            if (vs_cond_37.CondTrue())
            {
                this.data.SetIndex<PMap<K, V>.MapEntry>(idx, firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next));
                PathConstraint.RecordReturnPath(vs_cond_37);
            }

            if (vs_cond_37.CondFalse())
            {
                var vs_cond_36 = PathConstraint.BeginLoop();
                for (ValueSummary<MapEntry> iter = firstEntry; vs_cond_36.Loop(iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null))); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
                {
                    ValueSummary<SymbolicBool> vs_lgc_tmp_4;
                    var vs_cond_35 = ((new Func<ValueSummary<SymbolicBool>>(() =>
                    {
                        var vs_cond_53 = ((vs_lgc_tmp_4 = ValueSummary<SymbolicBool>.InitializeFrom(iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next).GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash)))).Cond();
                        var vs_cond_ret_53 = new ValueSummary<SymbolicBool>();
                        if (vs_cond_53.CondTrue())
                            vs_cond_ret_53.Merge(vs_lgc_tmp_4.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next).GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)));
                        if (vs_cond_53.CondFalse())
                            vs_cond_ret_53.Merge(vs_lgc_tmp_4);
                        vs_cond_53.MergeBranch();
                        return vs_cond_ret_53;
                    }

                    )())).Cond();
                    {
                        if (vs_cond_35.CondTrue())
                        {
                            iter.SetField<PMap<K, V>.MapEntry>((_) => _.Next, iter.GetField<PMap<K, V>.MapEntry>(_ => _.Next).GetField<PMap<K, V>.MapEntry>(_ => _.Next));
                            PathConstraint.RecordReturnPath(vs_cond_35);
                        }
                    }

                    vs_cond_35.MergeBranch(vs_cond_36);
                }

                vs_cond_36.MergeBranch(vs_cond_37);
            }
        }

        vs_cond_37.MergeBranch();
        PathConstraint.PopFrame();
    }

    public void Insert(ValueSummary<PTuple<K, V>> t)
    {
        this.Insert(t.GetField<K>(_ => _.Item1), t.GetField<V>(_ => _.Item2));
    }

    public ValueSummary<PBool> ContainsKey(ValueSummary<K> k)
    {
        PathConstraint.PushFrame();
        var vs_ret_6 = new ValueSummary<PBool>();
        ValueSummary<SymbolicInteger> hash = k.InvokeMethod((_) => _.PTypeGetHashCode());
        ValueSummary<SymbolicInteger> idx = hash.InvokeBinary<int, SymbolicInteger>((l, r) => l % r, this._capacity);
        ValueSummary<MapEntry> firstEntry = this.data.GetIndex<PMap<K, V>.MapEntry>(idx);
        var vs_cond_39 = PathConstraint.BeginLoop();
        for (ValueSummary<MapEntry> iter = firstEntry; vs_cond_39.Loop(iter.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null))); firstEntry.Assign<PMap<K, V>.MapEntry>(firstEntry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))
        {
            ValueSummary<SymbolicBool> vs_lgc_tmp_5;
            var vs_cond_38 = ((new Func<ValueSummary<SymbolicBool>>(() =>
            {
                var vs_cond_54 = ((vs_lgc_tmp_5 = ValueSummary<SymbolicBool>.InitializeFrom(iter.GetField<SymbolicInteger>(_ => _.Hash).InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l == r, hash)))).Cond();
                var vs_cond_ret_54 = new ValueSummary<SymbolicBool>();
                if (vs_cond_54.CondTrue())
                    vs_cond_ret_54.Merge(vs_lgc_tmp_5.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, iter.GetField<K>(_ => _.Key).InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), k)));
                if (vs_cond_54.CondFalse())
                    vs_cond_ret_54.Merge(vs_lgc_tmp_5);
                vs_cond_54.MergeBranch();
                return vs_cond_ret_54;
            }

            )())).Cond();
            {
                if (vs_cond_38.CondTrue())
                {
                    PathConstraint.RecordReturnPath(vs_ret_6, (PBool)true, vs_cond_38);
                }
            }

            vs_cond_38.MergeBranch(vs_cond_39);
        }

        if (vs_cond_39.MergeBranch())
        {
            PathConstraint.RecordReturnPath(vs_ret_6, (PBool)false);
        }

        PathConstraint.PopFrame();
        return vs_ret_6;
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
        var vs_cond_40 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = 0; vs_cond_40.Loop(i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity)); i.Increment())
        {
            ret.GetField<ValueSummary<PMap<K, V>.MapEntry>[]>(_ => _.data).SetIndex<PMap<K, V>.MapEntry>(i, this.CopyEntryChain(this.data.GetIndex<PMap<K, V>.MapEntry>(i)));
        }

        vs_cond_40.MergeBranch();
        return ret;
    }

    private ValueSummary<MapEntry> CopyEntryChain(ValueSummary<MapEntry> entry)
    {
        PathConstraint.PushFrame();
        var vs_ret_8 = new ValueSummary<MapEntry>();
        var vs_cond_41 = (entry.InvokeBinary<object, bool>((l, r) => l == r, new ValueSummary<object>(null))).Cond();
        {
            if (vs_cond_41.CondTrue())
            {
                PathConstraint.RecordReturnPath(vs_ret_8, new ValueSummary<PMap<K, V>.MapEntry>(null), vs_cond_41);
            }

            if (vs_cond_41.CondFalse())
            {
                PathConstraint.RecordReturnPath(vs_ret_8, new ValueSummary<PMap<K, V>.MapEntry>(new MapEntry(entry.GetField<K>(_ => _.Key), entry.GetField<SymbolicInteger>(_ => _.Hash), entry.GetField<V>(_ => _.Value).InvokeMethod((_) => _.DeepCopy()), this.CopyEntryChain(entry.GetField<PMap<K, V>.MapEntry>(_ => _.Next)))), vs_cond_41);
            }
        }

        vs_cond_41.MergeBranch();
        PathConstraint.PopFrame();
        return vs_ret_8;
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode()
    {
        ValueSummary<SymbolicInteger> ret = (SymbolicInteger)1;
        var vs_cond_43 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = 0; vs_cond_43.Loop(i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity)); i.Increment())
        {
            ValueSummary<MapEntry> entry = this.data.GetIndex<PMap<K, V>.MapEntry>(i);
            var vs_cond_42 = PathConstraint.BeginLoop();
            while (vs_cond_42.Loop(entry.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null))))
            {
                ret.Assign<SymbolicInteger>(entry.GetField<K>(_ => _.Key).InvokeMethod((_) => _.PTypeGetHashCode()).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, entry.GetField<V>(_ => _.Value).InvokeMethod((_) => _.PTypeGetHashCode())));
            }

            vs_cond_42.MergeBranch();
        }

        vs_cond_43.MergeBranch();
        return ret;
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PMap<K, V>> other)
    {
        PathConstraint.PushFrame();
        var vs_ret_10 = new ValueSummary<SymbolicBool>();
        var vs_cond_46 = (this.Count.InvokeBinary<int, bool>((l, r) => l != r, other.GetField<int>(_ => _.Count))).Cond();
        {
            if (vs_cond_46.CondTrue())
            {
                PathConstraint.RecordReturnPath(vs_ret_10, (SymbolicBool)false, vs_cond_46);
            }
        }

        if (vs_cond_46.MergeBranch())
        {
            var vs_cond_47 = PathConstraint.BeginLoop();
            for (ValueSummary<int> i = 0; vs_cond_47.Loop(i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity)); i.Increment())
            {
                ValueSummary<MapEntry> entry = this.data.GetIndex<PMap<K, V>.MapEntry>(i);
                var vs_cond_45 = PathConstraint.BeginLoop();
                while (vs_cond_45.Loop(entry.InvokeBinary<object, bool>((l, r) => l != r, new ValueSummary<object>(null))))
                {
                    ValueSummary<SymbolicBool> vs_lgc_tmp_6;
                    var vs_cond_44 = ((new Func<ValueSummary<SymbolicBool>>(() =>
                    {
                        var vs_cond_55 = ((vs_lgc_tmp_6 = ValueSummary<SymbolicBool>.InitializeFrom(other.InvokeMethod<K, V>((_, a0) => _.Get(a0), entry.GetField<K>(_ => _.Key)).Cast<object>(_ => (object)_).InvokeBinary<object, SymbolicBool>((l, r) => l != r, new ValueSummary<object>(null))))).Cond();
                        var vs_cond_ret_55 = new ValueSummary<SymbolicBool>();
                        if (vs_cond_55.CondTrue())
                            vs_cond_ret_55.Merge(vs_lgc_tmp_6);
                        if (vs_cond_55.CondFalse())
                            vs_cond_ret_55.Merge(vs_lgc_tmp_6.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l | r, entry.GetField<V>(_ => _.Value).InvokeMethod<V, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.InvokeMethod<K, V>((_, a0) => _[a0], entry.GetField<K>(_ => _.Key))).InvokeUnary<SymbolicBool>(_ => !_)));
                        vs_cond_55.MergeBranch();
                        return vs_cond_ret_55;
                    }

                    )())).Cond();
                    {
                        if (vs_cond_44.CondTrue())
                        {
                            PathConstraint.RecordReturnPath(vs_ret_10, (SymbolicBool)false, vs_cond_44);
                        }
                    }

                    vs_cond_44.MergeBranch(vs_cond_45);
                }

                vs_cond_45.MergeBranch(vs_cond_47);
            }

            if (vs_cond_47.MergeBranch())
            {
                PathConstraint.RecordReturnPath(vs_ret_10, (SymbolicBool)true);
            }
        }

        PathConstraint.PopFrame();
        return vs_ret_10;
    }
}