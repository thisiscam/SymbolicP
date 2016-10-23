public class List<T>
{
    const int INITIAL_CAPACITY = 4;
    internal ValueSummary<int> _count = 0;
    internal ValueSummary<int> _capacity = List<T>.INITIAL_CAPACITY;
    protected ValueSummary<ValueSummary<T>[]> data = ValueSummary<T>.NewVSArray(List<T>.INITIAL_CAPACITY);
    public void Add(ValueSummary<T> item)
    {
        var vs_cond_1 = (this._count.InvokeBinary<int, bool>((l, r) => l >= r, this._capacity)).Cond();
        {
            if (vs_cond_1.CondTrue())
            {
                ValueSummary<int> new_capacity = this._capacity.InvokeBinary<int, int>((l, r) => l * r, 2);
                ValueSummary<ValueSummary<T>[]> new_data = ValueSummary<T>.NewVSArray(new_capacity);
                var vs_cond_0 = PathConstraint.BeginLoop();
                for (ValueSummary<int> i = 0; vs_cond_0.Loop(i.InvokeBinary<int, bool>((l, r) => l < r, this._count)); i.Increment())
                {
                    new_data.SetIndex<T>(i, this.data.GetIndex<T>(i));
                }

                vs_cond_0.MergeBranch();
                new_data.SetIndex<T>(this._count, item);
                this.data.Assign(new_data);
                this._capacity.Assign<int>(new_capacity);
                this._count.Increment();
            }

            if (vs_cond_1.CondFalse())
            {
                this.data.SetIndex<T>(this._count, item);
                this._count.Increment();
            }
        }

        vs_cond_1.MergeBranch();
    }

    public void Insert(ValueSummary<SymbolicInteger> idx, ValueSummary<T> item)
    {
        var vs_cond_5 = (this._count.InvokeBinary<int, bool>((l, r) => l >= r, this._capacity)).Cond();
        {
            if (vs_cond_5.CondTrue())
            {
                ValueSummary<int> new_capacity = this._capacity.InvokeBinary<int, int>((l, r) => l * r, 2);
                ValueSummary<ValueSummary<T>[]> new_data = ValueSummary<T>.NewVSArray(new_capacity);
                ValueSummary<int> i = 0;
                var vs_cond_2 = PathConstraint.BeginLoop();
                for (; vs_cond_2.Loop(i.InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l < r, idx)); i.Increment())
                {
                    new_data.SetIndex<T>(i, this.data.GetIndex<T>(i));
                }

                vs_cond_2.MergeBranch();
                new_data.SetIndex<T>(i, item);
                var vs_cond_3 = PathConstraint.BeginLoop();
                for (; vs_cond_3.Loop(i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity)); i.Increment())
                {
                    new_data.SetIndex<T>(i.InvokeBinary<int, int>((l, r) => l + r, 1), this.data.GetIndex<T>(i));
                }

                vs_cond_3.MergeBranch();
                this._capacity.Assign<int>(new_capacity);
                this._count.Increment();
            }

            if (vs_cond_5.CondFalse())
            {
                var vs_cond_4 = PathConstraint.BeginLoop();
                for (ValueSummary<int> i = ValueSummary<int>.InitializeFrom(this._count); vs_cond_4.Loop(i.InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l > r, idx)); i.Decrement())
                {
                    this.data.SetIndex<T>(i, this.data.GetIndex<T>(i.InvokeBinary<int, int>((l, r) => l - r, 1)));
                }

                vs_cond_4.MergeBranch();
                this.data.SetIndex<T>(idx, item);
                this._count.Increment();
            }
        }

        vs_cond_5.MergeBranch();
    }

    public void RemoveAt(ValueSummary<int> idx)
    {
        var vs_cond_6 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = idx.InvokeBinary<int, int>((l, r) => l + r, 1); vs_cond_6.Loop(i.InvokeBinary<int, bool>((l, r) => l < r, this._count)); i.Increment())
        {
            this.data.SetIndex<T>(i.InvokeBinary<int, int>((l, r) => l - r, 1), this.data.GetIndex<T>(i));
        }

        vs_cond_6.MergeBranch();
        this._count.Decrement();
    }

    public void RemoveRange(ValueSummary<int> start, ValueSummary<int> count)
    {
        var vs_cond_7 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = start.InvokeBinary<int, int>((l, r) => l + r, count); vs_cond_7.Loop(i.InvokeBinary<int, bool>((l, r) => l < r, this._count)); i.Increment())
        {
            this.data.SetIndex<T>(i.InvokeBinary<int, int>((l, r) => l - r, count), this.data.GetIndex<T>(i));
        }

        vs_cond_7.MergeBranch();
        this._count.Assign<int>(this._count.InvokeBinary<int, int>((l, r) => l - r, count));
    }

    public ValueSummary<T> this[ValueSummary<int> index]
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

    public ValueSummary<T> this[ValueSummary<SymbolicInteger> index]
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

    public ValueSummary<int> Count
    {
        get
        {
            return this._count;
        }
    }
}