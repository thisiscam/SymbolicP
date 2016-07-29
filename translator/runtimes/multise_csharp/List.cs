public class List<T>
{
    const int INITIAL_CAPACITY = 4;
    internal ValueSummary<int> _count = 0;
    internal ValueSummary<int> _capacity = List<T>.INITIAL_CAPACITY;
    ValueSummary<ValueSummary<T>[]> data = new ValueSummary<T>[List<T>.INITIAL_CAPACITY];
    public void Add(ValueSummary<T> item)
    {
        if (this._count.InvokeBinary<int, bool>((l, r) => l >= r, this._capacity).Cond())
        {
            ValueSummary<int> new_capacity = this._capacity.InvokeBinary<int, int>((l, r) => l * r, 2);
            ValueSummary<ValueSummary<T>[]> new_data = new ValueSummary<T>[new_capacity];
            for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this._count).Cond(); i.Increment())
            {
                new_data.SetIndex<int, T>((_, a0, r) => _[a0] = r, i, this.data.GetIndex<int, T>((_, a0) => _[a0], i));
            }

            new_data.SetIndex<int, T>((_, a0, r) => _[a0] = r, this._count, item);
            this.data = new_data;
            this._capacity = new_capacity;
            this._count.Increment();
        }
        else
        {
            this.data.SetIndex<int, T>((_, a0, r) => _[a0] = r, this._count, item);
            this._count.Increment();
        }
    }

    public void Insert(ValueSummary<SymbolicInteger> idx, ValueSummary<T> item)
    {
        if (this._count.InvokeBinary<int, bool>((l, r) => l >= r, this._capacity).Cond())
        {
            ValueSummary<int> new_capacity = this._capacity.InvokeBinary<int, int>((l, r) => l * r, 2);
            ValueSummary<ValueSummary<T>[]> new_data = new ValueSummary<T>[new_capacity];
            ValueSummary<int> i = 0;
            for (; i.InvokeBinary<SymbolicInteger, bool>((l, r) => l < r, idx).Cond(); i.Increment())
            {
                new_data.SetIndex<int, T>((_, a0, r) => _[a0] = r, i, this.data.GetIndex<int, T>((_, a0) => _[a0], i));
            }

            new_data.SetIndex<int, T>((_, a0, r) => _[a0] = r, i, item);
            for (; i.InvokeBinary<int, bool>((l, r) => l < r, this._capacity).Cond(); i.Increment())
            {
                new_data.SetIndex<int, T>((_, a0, r) => _[a0] = r, i.InvokeBinary<int, int>((l, r) => l + r, 1), this.data.GetIndex<int, T>((_, a0) => _[a0], i));
            }

            this._capacity = new_capacity;
            this._count.Increment();
        }
        else
        {
            for (ValueSummary<int> i = this._count; i.InvokeBinary<SymbolicInteger, bool>((l, r) => l > r, idx).Cond(); i.Decrement())
            {
                this.data.SetIndex<int, T>((_, a0, r) => _[a0] = r, i, this.data.GetIndex<int, T>((_, a0) => _[a0], i.InvokeBinary<int, int>((l, r) => l - r, 1)));
            }

            this.data.SetIndex<SymbolicInteger, T>((_, a0, r) => _[a0] = r, idx, item);
            this._count.Increment();
        }
    }

    public void RemoveAt(ValueSummary<int> idx)
    {
        for (ValueSummary<int> i = idx.InvokeBinary<int, int>((l, r) => l + r, 1); i.InvokeBinary<int, bool>((l, r) => l < r, this._count).Cond(); i.Increment())
        {
            this.data.SetIndex<int, T>((_, a0, r) => _[a0] = r, i.InvokeBinary<int, int>((l, r) => l - r, 1), this.data.GetIndex<int, T>((_, a0) => _[a0], i));
        }

        this._count.Decrement();
    }

    public void RemoveRange(ValueSummary<int> start, ValueSummary<int> count)
    {
        for (ValueSummary<int> i = start.InvokeBinary<int, int>((l, r) => l + r, count); i.InvokeBinary<int, bool>((l, r) => l < r, this._count).Cond(); i.Increment())
        {
            this.data.SetIndex<int, T>((_, a0, r) => _[a0] = r, i.InvokeBinary<int, int>((l, r) => l - r, count), this.data.GetIndex<int, T>((_, a0) => _[a0], i));
        }

        this._count -= count;
    }

    public ValueSummary<T> this[ValueSummary<int> index]
    {
        get
        {
            return this.data.GetIndex<int, T>((_, a0) => _[a0], index);
        }

        set
        {
            this.data.SetIndex<int, T>((_, a0, r) => _[a0] = r, index, value);
        }
    }

    public ValueSummary<T> this[ValueSummary<SymbolicInteger> index]
    {
        get
        {
            return this.data.GetIndex<SymbolicInteger, T>((_, a0) => _[a0], index);
        }

        set
        {
            this.data.SetIndex<SymbolicInteger, T>((_, a0, r) => _[a0] = r, index, value);
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