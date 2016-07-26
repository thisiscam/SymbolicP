public class List<T>
{
    const int INITIAL_CAPACITY = 4;
    internal ValueSummary<int> _count = 0;
    internal ValueSummary<int> _capacity = List<T>.INITIAL_CAPACITY;
    ValueSummary<ValueSummary<T>[]> data = new ValueSummary<T>[List<T>.INITIAL_CAPACITY];
    public void Add(ValueSummary<List<T>> self, ValueSummary<T> item)
    {
        if (self.GetField<int>(_ => _._count).InvokeBinary<int, bool>((l, r) => l >= r, self.GetField<int>(_ => _._capacity)))
        {
            ValueSummary<int> new_capacity = self.GetField<int>(_ => _._capacity).InvokeBinary<int, int>((l, r) => l * r, 2);
            ValueSummary<ValueSummary<T>[]> new_data = new ValueSummary<T>[new_capacity];
            for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, self.GetField<int>(_ => _._count)); i.InvokeMethod((_) => _++))
            {
                new_data.GetIndex((_, a0) => _[a0], i).Assign(self.GetField<T[]>(_ => _.data).GetIndex((_, a0) => _[a0], i));
            }

            new_data.GetIndex((_, a0) => _[a0], this.GetField<int>(_ => _._count)).Assign(item);
            self.GetField<T[]>(_ => _.data).Assign(new_data);
            self.GetField<int>(_ => _._capacity).Assign(new_capacity);
            self.GetField<int>(_ => _._count).InvokeMethod((_) => _++);
        }
        else
        {
            self.GetField<T[]>(_ => _.data).GetIndex((_, a0) => _[a0], this.GetField<int>(_ => _._count)).Assign(item);
            self.GetField<int>(_ => _._count).InvokeMethod((_) => _++);
        }
    }

    public void Insert(ValueSummary<List<T>> self, ValueSummary<int> idx, ValueSummary<T> item)
    {
        if (self.GetField<int>(_ => _._count).InvokeBinary<int, bool>((l, r) => l >= r, self.GetField<int>(_ => _._capacity)))
        {
            ValueSummary<int> new_capacity = self.GetField<int>(_ => _._capacity).InvokeBinary<int, int>((l, r) => l * r, 2);
            ValueSummary<ValueSummary<T>[]> new_data = new ValueSummary<T>[new_capacity];
            ValueSummary<int> i;
            for (i.Assign(0); i.InvokeBinary<int, bool>((l, r) => l < r, idx); i.InvokeMethod((_) => _++))
            {
                new_data.GetIndex((_, a0) => _[a0], i).Assign(self.GetField<T[]>(_ => _.data).GetIndex((_, a0) => _[a0], i));
            }

            new_data.GetIndex((_, a0) => _[a0], i).Assign(item);
            for (; i.InvokeBinary<int, bool>((l, r) => l < r, self.GetField<int>(_ => _._capacity)); i.InvokeMethod((_) => _++))
            {
                new_data.GetIndex((_, a0) => _[a0], i.InvokeBinary<int, int>((l, r) => l + r, 1)).Assign(self.GetField<T[]>(_ => _.data).GetIndex((_, a0) => _[a0], i));
            }

            self.GetField<int>(_ => _._capacity).Assign(new_capacity);
            self.GetField<int>(_ => _._count).InvokeMethod((_) => _++);
        }
        else
        {
            for (ValueSummary<int> i = self.GetField<int>(_ => _._count); i.InvokeBinary<int, bool>((l, r) => l > r, idx); i.InvokeMethod((_) => _--))
            {
                self.GetField<T[]>(_ => _.data).GetIndex((_, a0) => _[a0], i).Assign(self.GetField<T[]>(_ => _.data).GetIndex((_, a0) => _[a0], i.InvokeBinary<int, int>((l, r) => l - r, 1)));
            }

            self.GetField<T[]>(_ => _.data).GetIndex((_, a0) => _[a0], idx).Assign(item);
            self.GetField<int>(_ => _._count).InvokeMethod((_) => _++);
        }
    }

    public void RemoveAt(ValueSummary<List<T>> self, ValueSummary<int> idx)
    {
        for (ValueSummary<int> i = idx.InvokeBinary<int, int>((l, r) => l + r, 1); i.InvokeBinary<int, bool>((l, r) => l < r, self.GetField<int>(_ => _._count)); i.InvokeMethod((_) => _++))
        {
            self.GetField<T[]>(_ => _.data).GetIndex((_, a0) => _[a0], i.InvokeBinary<int, int>((l, r) => l - r, 1)).Assign(self.GetField<T[]>(_ => _.data).GetIndex((_, a0) => _[a0], i));
        }

        self.GetField<int>(_ => _._count).InvokeMethod((_) => _--);
    }

    public void RemoveRange(ValueSummary<List<T>> self, ValueSummary<int> start, ValueSummary<int> count)
    {
        for (ValueSummary<int> i = start.InvokeBinary<int, int>((l, r) => l + r, count); i.InvokeBinary<int, bool>((l, r) => l < r, self.GetField<int>(_ => _._count)); i.InvokeMethod((_) => _++))
        {
            self.GetField<T[]>(_ => _.data).GetIndex((_, a0) => _[a0], i.InvokeBinary<int, int>((l, r) => l - r, count)).Assign(self.GetField<T[]>(_ => _.data).GetIndex((_, a0) => _[a0], i));
        }

        self.GetField<int>(_ => _._count).Assign(count);
    }

    public T this[ValueSummary<int> index]
    {
        get
        {
            return this.data.GetIndex((_, a0) => _[a0], index);
        }

        set
        {
            this.data.GetIndex((_, a0) => _[a0], index).Assign(value);
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