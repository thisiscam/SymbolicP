public class List<T>
{
    const int INITIAL_CAPACITY = 4;
    internal ValueSummary<int> _count = 0;
    internal ValueSummary<int> _capacity = ValueSummary<int>.InitializeFrom(List<T>.INITIAL_CAPACITY);
    protected ValueSummary<ValueSummary<T>[]> data = ValueSummary<ValueSummary<T>[]>.InitializeFrom(ValueSummary<T>.NewVSArray(List<T>.INITIAL_CAPACITY));
    public void Add(ValueSummary<T> item)
    {
        {
            var vs_cond_18 = (this._count.InvokeBinary<int, bool>((l, r) => l >= r, this._capacity)).Cond();
            if (vs_cond_18.CondTrue())
            {
                ValueSummary<int> new_capacity = ValueSummary<int>.InitializeFrom(this._capacity.InvokeBinary<int, int>((l, r) => l * r, 2));
                ValueSummary<ValueSummary<T>[]> new_data = ValueSummary<ValueSummary<T>[]>.InitializeFrom(ValueSummary<T>.NewVSArray(new_capacity));
                {
                    PathConstraint.BeginLoop();
                    for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._count).Loop(); i.Increment())
                    {
                        new_data.SetIndex<T>(i, this.data.GetIndex<T>(i));
                    }
                }

                new_data.SetIndex<T>(this._count, item);
                this.data.Assign(new_data);
                this._capacity.Assign<int>(new_capacity);
                this._count.Increment();
            }

            if (vs_cond_18.CondFalse())
            {
                this.data.SetIndex<T>(this._count, item);
                this._count.Increment();
            }

            vs_cond_18.MergeBranch();
        }
    }

    public void Insert(ValueSummary<SymbolicInteger> idx, ValueSummary<T> item)
    {
        {
            var vs_cond_19 = (this._count.InvokeBinary<int, bool>((l, r) => l >= r, this._capacity)).Cond();
            if (vs_cond_19.CondTrue())
            {
                ValueSummary<int> new_capacity = ValueSummary<int>.InitializeFrom(this._capacity.InvokeBinary<int, int>((l, r) => l * r, 2));
                ValueSummary<ValueSummary<T>[]> new_data = ValueSummary<ValueSummary<T>[]>.InitializeFrom(ValueSummary<T>.NewVSArray(new_capacity));
                ValueSummary<int> i = 0;
                {
                    PathConstraint.BeginLoop();
                    for (; i.InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l < r, idx).Loop(); i.Increment())
                    {
                        new_data.SetIndex<T>(i, this.data.GetIndex<T>(i));
                    }
                }

                new_data.SetIndex<T>(i, item);
                {
                    PathConstraint.BeginLoop();
                    for (; i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity).Loop(); i.Increment())
                    {
                        new_data.SetIndex<T>(i.InvokeBinary<int, int>((l, r) => l + r, 1), this.data.GetIndex<T>(i));
                    }
                }

                this._capacity.Assign<int>(new_capacity);
                this._count.Increment();
            }

            if (vs_cond_19.CondFalse())
            {
                {
                    PathConstraint.BeginLoop();
                    for (ValueSummary<int> i = ValueSummary<int>.InitializeFrom(this._count); i.InvokeBinary<SymbolicInteger, SymbolicBool>((l, r) => l > r, idx).Loop(); i.Decrement())
                    {
                        this.data.SetIndex<T>(i, this.data.GetIndex<T>(i.InvokeBinary<int, int>((l, r) => l - r, 1)));
                    }
                }

                this.data.SetIndex<T>(idx, item);
                this._count.Increment();
            }

            vs_cond_19.MergeBranch();
        }
    }

    public void RemoveAt(ValueSummary<int> idx)
    {
        {
            PathConstraint.BeginLoop();
            for (ValueSummary<int> i = ValueSummary<int>.InitializeFrom(idx.InvokeBinary<int, int>((l, r) => l + r, 1)); i.InvokeBinary<int, bool>((l, r) => l < r, this._count).Loop(); i.Increment())
            {
                this.data.SetIndex<T>(i.InvokeBinary<int, int>((l, r) => l - r, 1), this.data.GetIndex<T>(i));
            }
        }

        this._count.Decrement();
    }

    public void RemoveRange(ValueSummary<int> start, ValueSummary<int> count)
    {
        {
            PathConstraint.BeginLoop();
            for (ValueSummary<int> i = ValueSummary<int>.InitializeFrom(start.InvokeBinary<int, int>((l, r) => l + r, count)); i.InvokeBinary<int, bool>((l, r) => l < r, this._count).Loop(); i.Increment())
            {
                this.data.SetIndex<T>(i.InvokeBinary<int, int>((l, r) => l - r, count), this.data.GetIndex<T>(i));
            }
        }

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