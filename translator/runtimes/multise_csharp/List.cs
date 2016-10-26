using System;

public class List<T>
{
    internal ValueSummary<int> _count = 0;
    protected ValueSummary<DefaultArray<ValueSummary<T>>> data = new ValueSummary<DefaultArray<ValueSummary<T>>>(new DefaultArray<ValueSummary<T>>(() => default (T)));
    public void Add(ValueSummary<T> item)
    {
        this.data.SetIndex<T>(this._count, item);
        this._count.Increment();
    }

    public void Insert(ValueSummary<SymbolicInteger> idx, ValueSummary<T> item)
    {
        var vs_cond_0 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = ValueSummary<int>.InitializeFrom(this._count); vs_cond_0.Loop(i.InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l > r, idx)); i.Decrement())
        {
            this.data.SetIndex<T>(i, this.data.GetIndex<T>(i.InvokeBinary<int, int>((l, r) => l - r, 1)));
        }

        vs_cond_0.MergeBranch();
        this.data.SetIndex<T>(idx, item);
        this._count.Increment();
    }

    public void RemoveAt(ValueSummary<SymbolicInteger> idx)
    {
        var vs_cond_1 = PathConstraint.BeginLoop();
        for (ValueSummary<SymbolicInteger> i = idx.InvokeBinary<int, SymbolicInteger>((l, r) => l + r, 1); vs_cond_1.Loop(i.InvokeBinary<int, SymbolicBool>((l, r) => l < r, this._count)); i.Increment())
        {
            this.data.SetIndex<T>(i.InvokeBinary<int, SymbolicInteger>((l, r) => l - r, 1), this.data.GetIndex<T>(i));
        }

        vs_cond_1.MergeBranch();
        this._count.Decrement();
    }

    public void RemoveRange(ValueSummary<int> start, ValueSummary<int> count)
    {
        var vs_cond_2 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = start.InvokeBinary<int, int>((l, r) => l + r, count); vs_cond_2.Loop(i.InvokeBinary<int, bool>((l, r) => l < r, this._count)); i.Increment())
        {
            this.data.SetIndex<T>(i.InvokeBinary<int, int>((l, r) => l - r, count), this.data.GetIndex<T>(i));
        }

        vs_cond_2.MergeBranch();
        this._count.Assign<int>(this._count.InvokeBinary<int, int>((l, r) => l - r, count));
    }

    public ValueSummary<T> this[ValueSummary<int> index]
    {
        get
        {
            var vs_cond_3 = (index.InvokeBinary<int, bool>((l, r) => l >= r, this._count)).Cond();
            {
                if (vs_cond_3.CondTrue())
                {
                    throw new IndexOutOfRangeException();
                }
            }

            vs_cond_3.MergeBranch();
            return this.data.GetIndex<T>(index);
        }

        set
        {
            var vs_cond_4 = (index.InvokeBinary<int, bool>((l, r) => l >= r, this._count)).Cond();
            {
                if (vs_cond_4.CondTrue())
                {
                    throw new IndexOutOfRangeException();
                }
            }

            vs_cond_4.MergeBranch();
            this.data.SetIndex<T>(index, value);
        }
    }

    public ValueSummary<T> this[ValueSummary<SymbolicInteger> index]
    {
        get
        {
            var vs_cond_5 = (index.InvokeBinary<int, SymbolicBool>((l, r) => l >= r, this._count)).Cond();
            {
                if (vs_cond_5.CondTrue())
                {
                    throw new IndexOutOfRangeException();
                }
            }

            vs_cond_5.MergeBranch();
            return this.data.GetIndex<T>(index);
        }

        set
        {
            var vs_cond_6 = (index.InvokeBinary<int, SymbolicBool>((l, r) => l >= r, this._count)).Cond();
            {
                if (vs_cond_6.CondTrue())
                {
                    throw new IndexOutOfRangeException();
                }
            }

            vs_cond_6.MergeBranch();
            this.data.SetIndex<T>(index, value);
        }
    }

    public ValueSummary<int> Count
    {
        get
        {
            return this._count;
        }
    }
}