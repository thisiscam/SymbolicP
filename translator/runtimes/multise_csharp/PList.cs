using System;
using System.Collections.Generic;

class PList<T> : List<T>, IPType<PList<T>> where T : IPType<T>
{
    public ValueSummary<PInteger> Count
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

    public ValueSummary<T> this[ValueSummary<PInteger> index]
    {
        get
        {
            return this.data.GetIndex<T>(index);
        }

        set
        {
            this.data.SetIndex<T>(index, value);
        }
    }

    public ValueSummary<PList<T>> DeepCopy()
    {
        ValueSummary<PList<T>> ret = ValueSummary<PList<T>>.InitializeFrom(new ValueSummary<PList<T>>(new PList<T>()));
        for (ValueSummary<SymbolicInteger> i = ValueSummary<SymbolicInteger>.InitializeFrom((SymbolicInteger)0); i.InvokeBinary<int, SymbolicBool>((l, r) => l < r, this._count).Cond(); i.Increment())
        {
            ret.InvokeMethod<T>((_, a0) => _.Add(a0), this[i].InvokeMethod((_) => _.DeepCopy()));
        }

        return ret;
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode()
    {
        ValueSummary<SymbolicInteger> ret = ValueSummary<SymbolicInteger>.InitializeFrom((SymbolicInteger)1);
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._count).Cond(); i.Increment())
        {
            ret.Assign<SymbolicInteger>(ret.InvokeBinary<int, SymbolicInteger>((l, r) => l * r, 31).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l + r, this[i].InvokeMethod((_) => _.PTypeGetHashCode())));
        }

        return ret;
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PList<T>> other)
    {
        if (this._count.InvokeBinary<int, bool>((l, r) => l != r, other.GetField<int>(_ => _._count)).Cond())
        {
            return (SymbolicBool)false;
        }

        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._count).Cond(); i.Increment())
        {
            if (this[i].InvokeMethod<T, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.InvokeMethod<int, T>((_, a0) => _[a0], i)).InvokeUnary<SymbolicBool>(_ => !_).Cond())
            {
                return (SymbolicBool)false;
            }
        }

        return (SymbolicBool)true;
    }
}