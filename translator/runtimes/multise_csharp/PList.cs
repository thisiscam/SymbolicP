using System;
using System.Collections.Generic;

class PList<T> : List<T>, IPType<PList<T>> where T : IPType<T>
{
    public void Insert(ValueSummary<PList<T>> self, ValueSummary<PTuple<PInteger, T>> t)
    {
        self.InvokeMethod<int, T>((_, s, a0, a1) => _.Insert(s, a0, a1), t.GetField<PInteger>(_ => _.Item1), t.GetField<T>(_ => _.Item2));
    }

    public ValueSummary<PList<T>> DeepCopy(ValueSummary<PList<T>> self)
    {
        ValueSummary<PList<T>> ret = new ValueSummary<PList<T>>(new PList<T>());
        for (ValueSummary<SymbolicInteger> i = 0; i.InvokeBinary<SymbolicInteger, bool>((l, r) => l < r, self.GetField<int>(_ => _.Count)); i.InvokeMethod((_) => _++))
        {
            ret.InvokeMethod<T>((_, s, a0) => _.Add(s, a0), self.GetIndex((_, a0) => _[a0], i).InvokeDynamic((_, s) => _.DeepCopy(s)));
        }

        return ret;
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode(ValueSummary<PList<T>> self)
    {
        ValueSummary<SymbolicInteger> ret = 1;
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, self.GetField<int>(_ => _.Count)); i.InvokeMethod((_) => _++))
        {
            ret.Assign(ret.InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l * r, 31).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l + r, self.GetIndex((_, a0) => _[a0], i).InvokeDynamic((_, s) => _.PTypeGetHashCode(s))));
        }

        return ret;
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PList<T>> self, ValueSummary<PList<T>> other)
    {
        if (self.GetField<int>(_ => _.Count).InvokeBinary<int, bool>((l, r) => l != r, other.GetField<int>(_ => _.Count)))
        {
            return false;
        }

        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, self.GetField<int>(_ => _.Count)); i.InvokeMethod((_) => _++))
        {
            if (self.GetIndex((_, a0) => _[a0], i).InvokeDynamic<T>((_, s, a0) => _.PTypeEquals(s, a0), other.GetIndex((_, a0) => _[a0], i)).InvokeMethod((_) => !_))
            {
                return false;
            }
        }

        return true;
    }
}