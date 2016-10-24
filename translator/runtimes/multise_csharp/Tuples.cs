using System;

public class PTuple<T1> : IPType<PTuple<T1>> where T1 : IPType<T1>
{
    public ValueSummary<T1> Item1 = new ValueSummary<T1>(default (T1));
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
    public ValueSummary<T1> Item1 = new ValueSummary<T1>(default (T1));
    public ValueSummary<T2> Item2 = new ValueSummary<T2>(default (T2));
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
        ValueSummary<SymbolicBool> vs_lgc_tmp_0;
        return (new Func<ValueSummary<SymbolicBool>>(() =>
        {
            var vs_cond_58 = ((vs_lgc_tmp_0 = ValueSummary<SymbolicBool>.InitializeFrom(Item1.InvokeMethod<T1, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T1>(_ => _.Item1))))).Cond();
            var vs_cond_ret_58 = new ValueSummary<SymbolicBool>();
            if (vs_cond_58.CondTrue())
                vs_cond_ret_58.Merge(vs_lgc_tmp_0.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, Item2.InvokeMethod<T2, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T2>(_ => _.Item2))));
            if (vs_cond_58.CondFalse())
                vs_cond_ret_58.Merge(vs_lgc_tmp_0);
            vs_cond_58.MergeBranch();
            return vs_cond_ret_58;
        }

        )());
    }
}

public class PTuple<T1, T2, T3> : IPType<PTuple<T1, T2, T3>> where T1 : IPType<T1> where T2 : IPType<T2> where T3 : IPType<T3>
{
    public ValueSummary<T1> Item1 = new ValueSummary<T1>(default (T1));
    public ValueSummary<T2> Item2 = new ValueSummary<T2>(default (T2));
    public ValueSummary<T3> Item3 = new ValueSummary<T3>(default (T3));
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
        ValueSummary<SymbolicBool> vs_lgc_tmp_2;
        ValueSummary<SymbolicBool> vs_lgc_tmp_1;
        return (new Func<ValueSummary<SymbolicBool>>(() =>
        {
            var vs_cond_59 = ((vs_lgc_tmp_1 = ValueSummary<SymbolicBool>.InitializeFrom((new Func<ValueSummary<SymbolicBool>>(() =>
            {
                var vs_cond_60 = ((vs_lgc_tmp_2 = ValueSummary<SymbolicBool>.InitializeFrom(Item1.InvokeMethod<T1, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T1>(_ => _.Item1))))).Cond();
                var vs_cond_ret_60 = new ValueSummary<SymbolicBool>();
                if (vs_cond_60.CondTrue())
                    vs_cond_ret_60.Merge(vs_lgc_tmp_2.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, Item2.InvokeMethod<T2, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T2>(_ => _.Item2))));
                if (vs_cond_60.CondFalse())
                    vs_cond_ret_60.Merge(vs_lgc_tmp_2);
                vs_cond_60.MergeBranch();
                return vs_cond_ret_60;
            }

            )())))).Cond();
            var vs_cond_ret_59 = new ValueSummary<SymbolicBool>();
            if (vs_cond_59.CondTrue())
                vs_cond_ret_59.Merge(vs_lgc_tmp_1.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, Item3.InvokeMethod<T3, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T3>(_ => _.Item3))));
            if (vs_cond_59.CondFalse())
                vs_cond_ret_59.Merge(vs_lgc_tmp_1);
            vs_cond_59.MergeBranch();
            return vs_cond_ret_59;
        }

        )());
    }
}

public class PTuple<T1, T2, T3, T4> : IPType<PTuple<T1, T2, T3, T4>> where T1 : IPType<T1> where T2 : IPType<T2> where T3 : IPType<T3> where T4 : IPType<T4>
{
    public ValueSummary<T1> Item1 = new ValueSummary<T1>(default (T1));
    public ValueSummary<T2> Item2 = new ValueSummary<T2>(default (T2));
    public ValueSummary<T3> Item3 = new ValueSummary<T3>(default (T3));
    public ValueSummary<T4> Item4 = new ValueSummary<T4>(default (T4));
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
        ValueSummary<SymbolicBool> vs_lgc_tmp_5;
        ValueSummary<SymbolicBool> vs_lgc_tmp_4;
        ValueSummary<SymbolicBool> vs_lgc_tmp_3;
        return (new Func<ValueSummary<SymbolicBool>>(() =>
        {
            var vs_cond_61 = ((vs_lgc_tmp_3 = ValueSummary<SymbolicBool>.InitializeFrom((new Func<ValueSummary<SymbolicBool>>(() =>
            {
                var vs_cond_62 = ((vs_lgc_tmp_4 = ValueSummary<SymbolicBool>.InitializeFrom((new Func<ValueSummary<SymbolicBool>>(() =>
                {
                    var vs_cond_63 = ((vs_lgc_tmp_5 = ValueSummary<SymbolicBool>.InitializeFrom(Item1.InvokeMethod<T1, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T1>(_ => _.Item1))))).Cond();
                    var vs_cond_ret_63 = new ValueSummary<SymbolicBool>();
                    if (vs_cond_63.CondTrue())
                        vs_cond_ret_63.Merge(vs_lgc_tmp_5.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, Item2.InvokeMethod<T2, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T2>(_ => _.Item2))));
                    if (vs_cond_63.CondFalse())
                        vs_cond_ret_63.Merge(vs_lgc_tmp_5);
                    vs_cond_63.MergeBranch();
                    return vs_cond_ret_63;
                }

                )())))).Cond();
                var vs_cond_ret_62 = new ValueSummary<SymbolicBool>();
                if (vs_cond_62.CondTrue())
                    vs_cond_ret_62.Merge(vs_lgc_tmp_4.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, Item3.InvokeMethod<T3, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T3>(_ => _.Item3))));
                if (vs_cond_62.CondFalse())
                    vs_cond_ret_62.Merge(vs_lgc_tmp_4);
                vs_cond_62.MergeBranch();
                return vs_cond_ret_62;
            }

            )())))).Cond();
            var vs_cond_ret_61 = new ValueSummary<SymbolicBool>();
            if (vs_cond_61.CondTrue())
                vs_cond_ret_61.Merge(vs_lgc_tmp_3.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, Item4.InvokeMethod<T4, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T4>(_ => _.Item4))));
            if (vs_cond_61.CondFalse())
                vs_cond_ret_61.Merge(vs_lgc_tmp_3);
            vs_cond_61.MergeBranch();
            return vs_cond_ret_61;
        }

        )());
    }
}

public class PTuple<T1, T2, T3, T4, T5> : IPType<PTuple<T1, T2, T3, T4, T5>> where T1 : IPType<T1> where T2 : IPType<T2> where T3 : IPType<T3> where T4 : IPType<T4> where T5 : IPType<T5>
{
    public ValueSummary<T1> Item1 = new ValueSummary<T1>(default (T1));
    public ValueSummary<T2> Item2 = new ValueSummary<T2>(default (T2));
    public ValueSummary<T3> Item3 = new ValueSummary<T3>(default (T3));
    public ValueSummary<T4> Item4 = new ValueSummary<T4>(default (T4));
    public ValueSummary<T5> Item5 = new ValueSummary<T5>(default (T5));
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
        ValueSummary<SymbolicBool> vs_lgc_tmp_9;
        ValueSummary<SymbolicBool> vs_lgc_tmp_8;
        ValueSummary<SymbolicBool> vs_lgc_tmp_7;
        ValueSummary<SymbolicBool> vs_lgc_tmp_6;
        return (new Func<ValueSummary<SymbolicBool>>(() =>
        {
            var vs_cond_64 = ((vs_lgc_tmp_6 = ValueSummary<SymbolicBool>.InitializeFrom((new Func<ValueSummary<SymbolicBool>>(() =>
            {
                var vs_cond_65 = ((vs_lgc_tmp_7 = ValueSummary<SymbolicBool>.InitializeFrom((new Func<ValueSummary<SymbolicBool>>(() =>
                {
                    var vs_cond_66 = ((vs_lgc_tmp_8 = ValueSummary<SymbolicBool>.InitializeFrom((new Func<ValueSummary<SymbolicBool>>(() =>
                    {
                        var vs_cond_67 = ((vs_lgc_tmp_9 = ValueSummary<SymbolicBool>.InitializeFrom(Item1.InvokeMethod<T1, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T1>(_ => _.Item1))))).Cond();
                        var vs_cond_ret_67 = new ValueSummary<SymbolicBool>();
                        if (vs_cond_67.CondTrue())
                            vs_cond_ret_67.Merge(vs_lgc_tmp_9.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, Item2.InvokeMethod<T2, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T2>(_ => _.Item2))));
                        if (vs_cond_67.CondFalse())
                            vs_cond_ret_67.Merge(vs_lgc_tmp_9);
                        vs_cond_67.MergeBranch();
                        return vs_cond_ret_67;
                    }

                    )())))).Cond();
                    var vs_cond_ret_66 = new ValueSummary<SymbolicBool>();
                    if (vs_cond_66.CondTrue())
                        vs_cond_ret_66.Merge(vs_lgc_tmp_8.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, Item3.InvokeMethod<T3, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T3>(_ => _.Item3))));
                    if (vs_cond_66.CondFalse())
                        vs_cond_ret_66.Merge(vs_lgc_tmp_8);
                    vs_cond_66.MergeBranch();
                    return vs_cond_ret_66;
                }

                )())))).Cond();
                var vs_cond_ret_65 = new ValueSummary<SymbolicBool>();
                if (vs_cond_65.CondTrue())
                    vs_cond_ret_65.Merge(vs_lgc_tmp_7.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, Item4.InvokeMethod<T4, SymbolicBool>((_, a0) => _.PTypeEquals(a0), other.GetField<T4>(_ => _.Item4))));
                if (vs_cond_65.CondFalse())
                    vs_cond_ret_65.Merge(vs_lgc_tmp_7);
                vs_cond_65.MergeBranch();
                return vs_cond_ret_65;
            }

            )())))).Cond();
            var vs_cond_ret_64 = new ValueSummary<SymbolicBool>();
            if (vs_cond_64.CondTrue())
                vs_cond_ret_64.Merge(vs_lgc_tmp_6.InvokeBinary<SymbolicBool, SymbolicBool>((l, r) => l & r, Item5.InvokeMethod<T5, SymbolicBool>((_, a0) => _.PTypeEquals(a0), this.Item5)));
            if (vs_cond_64.CondFalse())
                vs_cond_ret_64.Merge(vs_lgc_tmp_6);
            vs_cond_64.MergeBranch();
            return vs_cond_ret_64;
        }

        )());
    }
}