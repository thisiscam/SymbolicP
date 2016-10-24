using System;
using System.Collections.Generic;
using System.Diagnostics;

public class PMap<K, V> : IPType<PMap<K, V>> where K : IPType<K> where V : IPType<V>
{
    public ValueSummary<PInteger> Count
    {
        get
        {
            return data.GetField<PInteger>(_ => _.Count);
        }
    }

    private ValueSummary<PList<PTuple<K, V>>> data = new ValueSummary<PList<PTuple<K, V>>>(new PList<PTuple<K, V>>());
    private void Insert(ValueSummary<K> k, ValueSummary<V> v)
    {
        var vs_cond_27 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = 0; vs_cond_27.Loop(i.InvokeBinary<PInteger, PBool>((l, r) => l < r, data.GetField<PInteger>(_ => _.Count))); i.Increment())
        {
            var vs_cond_26 = (k.InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), this.data.InvokeMethod<int, PTuple<K, V>>((_, a0) => _[a0], i).GetField<K>(_ => _.Item1))).Cond();
            {
                if (vs_cond_26.CondTrue())
                {
                    throw new SystemException("Reinsertion of key" + k.ToString() + "into PMap");
                }
            }

            vs_cond_26.MergeBranch();
        }

        vs_cond_27.MergeBranch();
        data.InvokeMethod<PTuple<K, V>>((_, a0) => _.Add(a0), new ValueSummary<PTuple<K, V>>(new PTuple<K, V>(k, v)));
    }

    private ValueSummary<V> Get(ValueSummary<K> k)
    {
        PathConstraint.PushFrame();
        var vs_ret_1 = new ValueSummary<V>();
        var vs_cond_29 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = 0; vs_cond_29.Loop(i.InvokeBinary<PInteger, PBool>((l, r) => l < r, data.GetField<PInteger>(_ => _.Count))); i.Increment())
        {
            var vs_cond_28 = (k.InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), this.data.InvokeMethod<int, PTuple<K, V>>((_, a0) => _[a0], i).GetField<K>(_ => _.Item1))).Cond();
            {
                if (vs_cond_28.CondTrue())
                {
                    PathConstraint.RecordReturnPath(vs_ret_1, this.data.InvokeMethod<int, PTuple<K, V>>((_, a0) => _[a0], i).GetField<V>(_ => _.Item2), vs_cond_28);
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
        var vs_cond_31 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = 0; vs_cond_31.Loop(i.InvokeBinary<PInteger, PBool>((l, r) => l < r, data.GetField<PInteger>(_ => _.Count))); i.Increment())
        {
            var vs_cond_30 = (k.InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), this.data.InvokeMethod<int, PTuple<K, V>>((_, a0) => _[a0], i).GetField<K>(_ => _.Item1))).Cond();
            {
                if (vs_cond_30.CondTrue())
                {
                    this.data.InvokeMethod<int, PTuple<K, V>>((_, a0) => _[a0], i).SetField<V>((_) => _.Item2, v);
                    PathConstraint.RecordReturnPath(vs_cond_30);
                }
            }

            vs_cond_30.MergeBranch(vs_cond_31);
        }

        if (vs_cond_31.MergeBranch())
        {
            this.Insert(k, v);
        }

        PathConstraint.PopFrame();
    }

    public void Remove(ValueSummary<K> k)
    {
        PathConstraint.PushFrame();
        var vs_cond_33 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = 0; vs_cond_33.Loop(i.InvokeBinary<PInteger, PBool>((l, r) => l < r, data.GetField<PInteger>(_ => _.Count))); i.Increment())
        {
            var vs_cond_32 = (k.InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), this.data.InvokeMethod<int, PTuple<K, V>>((_, a0) => _[a0], i).GetField<K>(_ => _.Item1))).Cond();
            {
                if (vs_cond_32.CondTrue())
                {
                    this.data.InvokeMethod<PInteger>((_, a0) => _.RemoveAt(a0), i.Cast<PInteger>(_ => (PInteger)_));
                    PathConstraint.RecordReturnPath(vs_cond_32);
                }
            }

            vs_cond_32.MergeBranch(vs_cond_33);
        }

        vs_cond_33.MergeBranch();
        PathConstraint.PopFrame();
    }

    public void Insert(ValueSummary<PTuple<K, V>> t)
    {
        this.Insert(t.GetField<K>(_ => _.Item1), t.GetField<V>(_ => _.Item2));
    }

    public ValueSummary<PBool> ContainsKey(ValueSummary<K> k)
    {
        PathConstraint.PushFrame();
        var vs_ret_5 = new ValueSummary<PBool>();
        var vs_cond_35 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = 0; vs_cond_35.Loop(i.InvokeBinary<PInteger, PBool>((l, r) => l < r, data.GetField<PInteger>(_ => _.Count))); i.Increment())
        {
            var vs_cond_34 = (k.InvokeMethod<K, SymbolicBool>((_, a0) => _.PTypeEquals(a0), this.data.InvokeMethod<int, PTuple<K, V>>((_, a0) => _[a0], i).GetField<K>(_ => _.Item1))).Cond();
            {
                if (vs_cond_34.CondTrue())
                {
                    PathConstraint.RecordReturnPath(vs_ret_5, (PBool)true, vs_cond_34);
                }
            }

            vs_cond_34.MergeBranch(vs_cond_35);
        }

        if (vs_cond_35.MergeBranch())
        {
            PathConstraint.RecordReturnPath(vs_ret_5, (PBool)false);
        }

        PathConstraint.PopFrame();
        return vs_ret_5;
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
        ValueSummary<PMap<K, V>> ret = new ValueSummary<PMap<K, V>>(new PMap<K, V>());
        var vs_cond_36 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = 0; vs_cond_36.Loop(i.InvokeBinary<PInteger, PBool>((l, r) => l < r, data.GetField<PInteger>(_ => _.Count))); i.Increment())
        {
            ret.GetField<PList<PTuple<K, V>>>(_ => _.data).InvokeMethod<PTuple<K, V>>((_, a0) => _.Add(a0), this.data.InvokeMethod<int, PTuple<K, V>>((_, a0) => _[a0], i).InvokeMethod((_) => _.DeepCopy()));
        }

        vs_cond_36.MergeBranch();
        return ret;
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PMap<K, V>> other)
    {
        return this.data.InvokeMethod<PList<PTuple<K, V>>, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<PList<PTuple<K, V>>>(_ => _.data));
    }
}