using System;
using System.Collections.Generic;

class PList<T> : List<T>, IPType<PList<T>> where T : IPType<T>
{
    public new ValueSummary<PInteger> Count
    {
        get
        {
            return this._count.Cast<PInteger>(_ => (PInteger)_);
        }
    }

    public void Insert(ValueSummary<PTuple<PInteger, T>> t)
    {
        this.Insert(t.GetField<PInteger>(_ => _.Item1).Cast<SymbolicInteger>(_ => (SymbolicInteger)_), t.GetField<T>(_ => _.Item2));
    }

    public void RemoveAt(ValueSummary<PInteger> idx)
    {
        this.RemoveAt(idx.Cast<SymbolicInteger>(_ => (SymbolicInteger)_));
    }

    public ValueSummary<T> this[ValueSummary<PInteger> index]
    {
        get
        {
            var vs_cond_12 = (index.InvokeBinary<PInteger, PBool>((l, r) => l >= r, this.Count)).Cond();
            {
                if (vs_cond_12.CondTrue())
                {
                    throw new IndexOutOfRangeException();
                }
            }

            vs_cond_12.MergeBranch();
            return this.data.GetIndex<T>(index);
        }

        set
        {
            var vs_cond_13 = (index.InvokeBinary<PInteger, PBool>((l, r) => l >= r, this.Count)).Cond();
            {
                if (vs_cond_13.CondTrue())
                {
                    throw new IndexOutOfRangeException();
                }
            }

            vs_cond_13.MergeBranch();
            this.data.SetIndex<T>(index, value);
        }
    }

    public ValueSummary<PList<T>> DeepCopy()
    {
        ValueSummary<PList<T>> ret = new ValueSummary<PList<T>>(new PList<T>());
        var vs_cond_14 = PathConstraint.BeginLoop();
        for (ValueSummary<SymbolicInteger> i = (SymbolicInteger)0; vs_cond_14.Loop(i.InvokeBinary<int, SymbolicBool>((l, r) => l < r, this._count)); i.Increment())
        {
            ret.InvokeMethod<T>((_, a0) => _.Add(a0), this[i].InvokeMethod((_) => _.DeepCopy()));
        }

        vs_cond_14.MergeBranch();
        return ret;
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PList<T>> other)
    {
        PathConstraint.PushFrame();
        var vs_ret_3 = new ValueSummary<SymbolicBool>();
        var vs_cond_16 = (this._count.InvokeBinary<int, bool>((l, r) => l != r, other.GetField<int>(_ => _._count))).Cond();
        {
            if (vs_cond_16.CondTrue())
            {
                PathConstraint.RecordReturnPath(vs_ret_3, (SymbolicBool)false, vs_cond_16);
            }
        }

        if (vs_cond_16.MergeBranch())
        {
            var vs_cond_17 = PathConstraint.BeginLoop();
            for (ValueSummary<int> i = 0; vs_cond_17.Loop(i.InvokeBinary<int, bool>((l, r) => l < r, this._count)); i.Increment())
            {
                var vs_cond_15 = (this[i].InvokeMethod<T, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.InvokeMethod<int, T>((_, a0) => _[a0], i)).InvokeUnary<SymbolicBool>(_ => !_)).Cond();
                {
                    if (vs_cond_15.CondTrue())
                    {
                        PathConstraint.RecordReturnPath(vs_ret_3, (SymbolicBool)false, vs_cond_15);
                    }
                }

                vs_cond_15.MergeBranch(vs_cond_17);
            }

            if (vs_cond_17.MergeBranch())
            {
                PathConstraint.RecordReturnPath(vs_ret_3, (SymbolicBool)true);
            }
        }

        PathConstraint.PopFrame();
        return vs_ret_3;
    }
}