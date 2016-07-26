using System;

public class PTuple<T1> : IPType<PTuple<T1>> where T1 : IPType<T1>
{
    public ValueSummary<T1> Item1;
    public PTuple(ValueSummary<T1> i1)
    {
        this.Item1 = i1;
    }

    public ValueSummary<PTuple<T1>> DeepCopy(ValueSummary<PTuple<T1>> self)
    {
        return new ValueSummary<PTuple<T1>>(new PTuple<T1>(Item1.InvokeDynamic((_, s) => _.DeepCopy(s))));
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode(ValueSummary<PTuple<T1>> self)
    {
        return Item1.InvokeDynamic((_, s) => _.PTypeGetHashCode(s));
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PTuple<T1>> self, ValueSummary<PTuple<T1>> other)
    {
        return Item1.InvokeDynamic<T1>((_, s, a0) => _.PTypeEquals(s, a0), other.GetField<T1>(_ => _.Item1));
    }
}

public class PTuple<T1, T2> : IPType<PTuple<T1, T2>> where T1 : IPType<T1> where T2 : IPType<T2>
{
    public ValueSummary<T1> Item1;
    public ValueSummary<T2> Item2;
    public PTuple(ValueSummary<T1> i1, ValueSummary<T2> i2)
    {
        this.Item1 = i1;
        this.Item2 = i2;
    }

    public ValueSummary<PTuple<T1, T2>> DeepCopy(ValueSummary<PTuple<T1, T2>> self)
    {
        return new ValueSummary<PTuple<T1, T2>>(new PTuple<T1, T2>(Item1.InvokeDynamic((_, s) => _.DeepCopy(s)), Item2.InvokeDynamic((_, s) => _.DeepCopy(s))));
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode(ValueSummary<PTuple<T1, T2>> self)
    {
        return Item1.InvokeDynamic((_, s) => _.PTypeGetHashCode(s)).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item2.InvokeDynamic((_, s) => _.PTypeGetHashCode(s)));
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PTuple<T1, T2>> self, ValueSummary<PTuple<T1, T2>> other)
    {
        return Item1.InvokeDynamic<T1>((_, s, a0) => _.PTypeEquals(s, a0), other.GetField<T1>(_ => _.Item1)).InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l && r, Item2.InvokeDynamic<T2>((_, s, a0) => _.PTypeEquals(s, a0), other.GetField<T2>(_ => _.Item2)));
    }
}

public class PTuple<T1, T2, T3> : IPType<PTuple<T1, T2, T3>> where T1 : IPType<T1> where T2 : IPType<T2> where T3 : IPType<T3>
{
    public ValueSummary<T1> Item1;
    public ValueSummary<T2> Item2;
    public ValueSummary<T3> Item3;
    public PTuple(ValueSummary<T1> i1, ValueSummary<T2> i2, ValueSummary<T3> i3)
    {
        this.Item1 = i1;
        this.Item2 = i2;
        this.Item3 = i3;
    }

    public ValueSummary<PTuple<T1, T2, T3>> DeepCopy(ValueSummary<PTuple<T1, T2, T3>> self)
    {
        return new ValueSummary<PTuple<T1, T2, T3>>(new PTuple<T1, T2, T3>(Item1.InvokeDynamic((_, s) => _.DeepCopy(s)), Item2.InvokeDynamic((_, s) => _.DeepCopy(s)), Item3.InvokeDynamic((_, s) => _.DeepCopy(s))));
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode(ValueSummary<PTuple<T1, T2, T3>> self)
    {
        return Item1.InvokeDynamic((_, s) => _.PTypeGetHashCode(s)).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item2.InvokeDynamic((_, s) => _.PTypeGetHashCode(s))).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item3.InvokeDynamic((_, s) => _.PTypeGetHashCode(s)));
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PTuple<T1, T2, T3>> self, ValueSummary<PTuple<T1, T2, T3>> other)
    {
        return Item1.InvokeDynamic<T1>((_, s, a0) => _.PTypeEquals(s, a0), other.GetField<T1>(_ => _.Item1)).InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l && r, Item2.InvokeDynamic<T2>((_, s, a0) => _.PTypeEquals(s, a0), other.GetField<T2>(_ => _.Item2))).InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l && r, Item3.InvokeDynamic<T3>((_, s, a0) => _.PTypeEquals(s, a0), other.GetField<T3>(_ => _.Item3)));
    }
}

public class PTuple<T1, T2, T3, T4> : IPType<PTuple<T1, T2, T3, T4>> where T1 : IPType<T1> where T2 : IPType<T2> where T3 : IPType<T3> where T4 : IPType<T4>
{
    public ValueSummary<T1> Item1;
    public ValueSummary<T2> Item2;
    public ValueSummary<T3> Item3;
    public ValueSummary<T4> Item4;
    public PTuple(ValueSummary<T1> i1, ValueSummary<T2> i2, ValueSummary<T3> i3, ValueSummary<T4> i4)
    {
        this.Item1 = i1;
        this.Item2 = i2;
        this.Item3 = i3;
        this.Item4 = i4;
    }

    public ValueSummary<PTuple<T1, T2, T3, T4>> DeepCopy(ValueSummary<PTuple<T1, T2, T3, T4>> self)
    {
        return new ValueSummary<PTuple<T1, T2, T3, T4>>(new PTuple<T1, T2, T3, T4>(Item1.InvokeDynamic((_, s) => _.DeepCopy(s)), Item2.InvokeDynamic((_, s) => _.DeepCopy(s)), Item3.InvokeDynamic((_, s) => _.DeepCopy(s)), Item4.InvokeDynamic((_, s) => _.DeepCopy(s))));
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode(ValueSummary<PTuple<T1, T2, T3, T4>> self)
    {
        return Item1.InvokeDynamic((_, s) => _.PTypeGetHashCode(s)).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item2.InvokeDynamic((_, s) => _.PTypeGetHashCode(s))).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item3.InvokeDynamic((_, s) => _.PTypeGetHashCode(s))).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item4.InvokeDynamic((_, s) => _.PTypeGetHashCode(s)));
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PTuple<T1, T2, T3, T4>> self, ValueSummary<PTuple<T1, T2, T3, T4>> other)
    {
        return Item1.InvokeDynamic<T1>((_, s, a0) => _.PTypeEquals(s, a0), other.GetField<T1>(_ => _.Item1)).InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l && r, Item2.InvokeDynamic<T2>((_, s, a0) => _.PTypeEquals(s, a0), other.GetField<T2>(_ => _.Item2))).InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l && r, Item3.InvokeDynamic<T3>((_, s, a0) => _.PTypeEquals(s, a0), other.GetField<T3>(_ => _.Item3))).InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l && r, Item4.InvokeDynamic<T4>((_, s, a0) => _.PTypeEquals(s, a0), other.GetField<T4>(_ => _.Item4)));
    }
}

public class PTuple<T1, T2, T3, T4, T5> : IPType<PTuple<T1, T2, T3, T4, T5>> where T1 : IPType<T1> where T2 : IPType<T2> where T3 : IPType<T3> where T4 : IPType<T4> where T5 : IPType<T5>
{
    public ValueSummary<T1> Item1;
    public ValueSummary<T2> Item2;
    public ValueSummary<T3> Item3;
    public ValueSummary<T4> Item4;
    public ValueSummary<T5> Item5;
    public PTuple(ValueSummary<T1> i1, ValueSummary<T2> i2, ValueSummary<T3> i3, ValueSummary<T4> i4, ValueSummary<T5> i5)
    {
        this.Item1 = i1;
        this.Item2 = i2;
        this.Item3 = i3;
        this.Item4 = i4;
        this.Item5 = i5;
    }

    public ValueSummary<PTuple<T1, T2, T3, T4, T5>> DeepCopy(ValueSummary<PTuple<T1, T2, T3, T4, T5>> self)
    {
        return new ValueSummary<PTuple<T1, T2, T3, T4, T5>>(new PTuple<T1, T2, T3, T4, T5>(Item1.InvokeDynamic((_, s) => _.DeepCopy(s)), Item2.InvokeDynamic((_, s) => _.DeepCopy(s)), Item3.InvokeDynamic((_, s) => _.DeepCopy(s)), Item4.InvokeDynamic((_, s) => _.DeepCopy(s)), Item5.InvokeDynamic((_, s) => _.DeepCopy(s))));
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode(ValueSummary<PTuple<T1, T2, T3, T4, T5>> self)
    {
        return Item1.InvokeDynamic((_, s) => _.PTypeGetHashCode(s)).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item2.InvokeDynamic((_, s) => _.PTypeGetHashCode(s))).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item3.InvokeDynamic((_, s) => _.PTypeGetHashCode(s))).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item4.InvokeDynamic((_, s) => _.PTypeGetHashCode(s))).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item5.InvokeDynamic((_, s) => _.PTypeGetHashCode(s)));
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PTuple<T1, T2, T3, T4, T5>> self, ValueSummary<PTuple<T1, T2, T3, T4, T5>> other)
    {
        return Item1.InvokeDynamic<T1>((_, s, a0) => _.PTypeEquals(s, a0), other.GetField<T1>(_ => _.Item1)).InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l && r, Item2.InvokeDynamic<T2>((_, s, a0) => _.PTypeEquals(s, a0), other.GetField<T2>(_ => _.Item2))).InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l && r, Item3.InvokeDynamic<T3>((_, s, a0) => _.PTypeEquals(s, a0), other.GetField<T3>(_ => _.Item3))).InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l && r, Item4.InvokeDynamic<T4>((_, s, a0) => _.PTypeEquals(s, a0), other.GetField<T4>(_ => _.Item4))).InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l && r, Item5.InvokeDynamic<T5>((_, s, a0) => _.PTypeEquals(s, a0), self.GetField<T5>(_ => _.Item5)));
    }
}