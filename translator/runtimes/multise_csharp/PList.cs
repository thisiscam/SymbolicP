using System;
using System.Collections.Generic;

class PList<T> : List<T>, IPType<PList<T>> where T : IPType<T>
{
    public ValueSummary<PInteger> Count
    {
        get
        {
            return new ValueSummary<PInteger>(new PInteger(this._count));
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
            return this.data.GetIndex<PInteger, T>((_, a0) => _[a0], index);
        }

        set
        {
            this.data.SetIndex<PInteger, T>((_, a0, r) => _[a0] = r, index, value);
        }
    }

    public ValueSummary<PList<T>> DeepCopy()
    {
        ValueSummary<PList<T>> ret = new ValueSummary<PList<T>>(new PList<T>());
        for (ValueSummary<SymbolicInteger> i = (SymbolicInteger)0; i.InvokeBinary<int, bool>((l, r) => l < r, this._count).Cond(); i.Increment())
        {
            ret.InvokeMethod<T>((_, a0) => _.Add(a0), this[i].InvokeMethod((_) => _.DeepCopy()));
        }

        return ret;
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode()
    {
        ValueSummary<SymbolicInteger> ret = (SymbolicInteger)1;
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
            if (this[i].InvokeMethod<T, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.InvokeMethod<int, T>((_, a0) => _[a0], i)).InvokeUnary<bool>(_ => !_).Cond())
            {
                return (SymbolicBool)false;
            }
        }

        return (SymbolicBool)true;
    }
}