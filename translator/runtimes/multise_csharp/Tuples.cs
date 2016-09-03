using System;

public class PTuple<T1> : IPType<PTuple<T1>> where T1 : IPType<T1>
{
    public ValueSummary<T1> Item1;
    public PTuple(ValueSummary<T1> i1)
    {
        this.Item1 = i1;
    }

    public ValueSummary<PTuple<T1>> DeepCopy()
    {
        return new ValueSummary<PTuple<T1>>(new PTuple<T1>(Item1.InvokeMethod((_) => _.DeepCopy())));
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode()
    {
        return Item1.InvokeMethod((_) => _.PTypeGetHashCode());
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PTuple<T1>> other)
    {
        return Item1.InvokeMethod<T1, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T1>(_ => _.Item1));
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

    public ValueSummary<PTuple<T1, T2>> DeepCopy()
    {
        return new ValueSummary<PTuple<T1, T2>>(new PTuple<T1, T2>(Item1.InvokeMethod((_) => _.DeepCopy()), Item2.InvokeMethod((_) => _.DeepCopy())));
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode()
    {
        return Item1.InvokeMethod((_) => _.PTypeGetHashCode()).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item2.InvokeMethod((_) => _.PTypeGetHashCode()));
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PTuple<T1, T2>> other)
    {
        return Item1.InvokeMethod<T1, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T1>(_ => _.Item1)).AndAnd(Item2.InvokeMethod<T2, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T2>(_ => _.Item2)));
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

    public ValueSummary<PTuple<T1, T2, T3>> DeepCopy()
    {
        return new ValueSummary<PTuple<T1, T2, T3>>(new PTuple<T1, T2, T3>(Item1.InvokeMethod((_) => _.DeepCopy()), Item2.InvokeMethod((_) => _.DeepCopy()), Item3.InvokeMethod((_) => _.DeepCopy())));
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode()
    {
        return Item1.InvokeMethod((_) => _.PTypeGetHashCode()).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item2.InvokeMethod((_) => _.PTypeGetHashCode())).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item3.InvokeMethod((_) => _.PTypeGetHashCode()));
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PTuple<T1, T2, T3>> other)
    {
        return Item1.InvokeMethod<T1, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T1>(_ => _.Item1)).AndAnd(Item2.InvokeMethod<T2, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T2>(_ => _.Item2))).AndAnd(Item3.InvokeMethod<T3, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T3>(_ => _.Item3)));
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

    public ValueSummary<PTuple<T1, T2, T3, T4>> DeepCopy()
    {
        return new ValueSummary<PTuple<T1, T2, T3, T4>>(new PTuple<T1, T2, T3, T4>(Item1.InvokeMethod((_) => _.DeepCopy()), Item2.InvokeMethod((_) => _.DeepCopy()), Item3.InvokeMethod((_) => _.DeepCopy()), Item4.InvokeMethod((_) => _.DeepCopy())));
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode()
    {
        return Item1.InvokeMethod((_) => _.PTypeGetHashCode()).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item2.InvokeMethod((_) => _.PTypeGetHashCode())).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item3.InvokeMethod((_) => _.PTypeGetHashCode())).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item4.InvokeMethod((_) => _.PTypeGetHashCode()));
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PTuple<T1, T2, T3, T4>> other)
    {
        return Item1.InvokeMethod<T1, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T1>(_ => _.Item1)).AndAnd(Item2.InvokeMethod<T2, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T2>(_ => _.Item2))).AndAnd(Item3.InvokeMethod<T3, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T3>(_ => _.Item3))).AndAnd(Item4.InvokeMethod<T4, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T4>(_ => _.Item4)));
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

    public ValueSummary<PTuple<T1, T2, T3, T4, T5>> DeepCopy()
    {
        return new ValueSummary<PTuple<T1, T2, T3, T4, T5>>(new PTuple<T1, T2, T3, T4, T5>(Item1.InvokeMethod((_) => _.DeepCopy()), Item2.InvokeMethod((_) => _.DeepCopy()), Item3.InvokeMethod((_) => _.DeepCopy()), Item4.InvokeMethod((_) => _.DeepCopy()), Item5.InvokeMethod((_) => _.DeepCopy())));
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode()
    {
        return Item1.InvokeMethod((_) => _.PTypeGetHashCode()).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item2.InvokeMethod((_) => _.PTypeGetHashCode())).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item3.InvokeMethod((_) => _.PTypeGetHashCode())).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item4.InvokeMethod((_) => _.PTypeGetHashCode())).InvokeBinary<SymbolicInteger, SymbolicInteger>((l, r) => l ^ r, Item5.InvokeMethod((_) => _.PTypeGetHashCode()));
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PTuple<T1, T2, T3, T4, T5>> other)
    {
        return Item1.InvokeMethod<T1, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T1>(_ => _.Item1)).AndAnd(Item2.InvokeMethod<T2, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T2>(_ => _.Item2))).AndAnd(Item3.InvokeMethod<T3, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T3>(_ => _.Item3))).AndAnd(Item4.InvokeMethod<T4, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T4>(_ => _.Item4))).AndAnd(Item5.InvokeMethod<T5, SymbolicBool>((_, a0) => _.PTypeEquals(a0), this.Item5));
    }
}