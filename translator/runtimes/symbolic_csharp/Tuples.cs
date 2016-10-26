using System;

public class PTuple<T1> : IPType<PTuple<T1>>
		where T1 : IPType<T1> {
	public T1 Item1;
	public PTuple(T1 i1) {
		Item1 = i1;
	}
	public PTuple<T1> DeepCopy() {
		return new PTuple<T1>(Item1.DeepCopy());
	}

    public SymbolicBool PTypeEquals(PTuple<T1> other)
    {
        return Item1.PTypeEquals(other.Item1);
    }
}

public class PTuple<T1, T2> : IPType<PTuple<T1, T2>>
		where T1 : IPType<T1>
		where T2 : IPType<T2> 
{
	public T1 Item1;
	public T2 Item2;
	public PTuple(T1 i1, T2 i2) {
		Item1 = i1;
		Item2 = i2;
	}
	public PTuple<T1, T2> DeepCopy() {
		return new PTuple<T1, T2>(
						Item1.DeepCopy(),
						Item2.DeepCopy());
	}

    public SymbolicBool PTypeEquals(PTuple<T1, T2> other)
    {
        return Item1.PTypeEquals(other.Item1) && Item2.PTypeEquals(other.Item2);
    }
}

public class PTuple<T1, T2, T3> : IPType<PTuple<T1, T2, T3>>
		where T1 : IPType<T1>
		where T2 : IPType<T2>
		where T3 : IPType<T3> 
{
	public T1 Item1;
	public T2 Item2;
	public T3 Item3;
	public PTuple(T1 i1, T2 i2, T3 i3) {
		Item1 = i1;
		Item2 = i2;
		Item3 = i3;
	}
	public PTuple<T1, T2, T3> DeepCopy() {
		return new PTuple<T1, T2, T3>(
								Item1.DeepCopy(),
								Item2.DeepCopy(),
								Item3.DeepCopy());
	}

    public SymbolicBool PTypeEquals(PTuple<T1, T2, T3> other)
    {
        return Item1.PTypeEquals(other.Item1) && Item2.PTypeEquals(other.Item2) && Item3.PTypeEquals(other.Item3);
    }
}

public class PTuple<T1, T2, T3, T4> : IPType<PTuple<T1, T2, T3, T4>>
		where T1 : IPType<T1>
		where T2 : IPType<T2>
		where T3 : IPType<T3>
		where T4 : IPType<T4> 
{
	public T1 Item1;
	public T2 Item2;
	public T3 Item3;
	public T4 Item4;
	public PTuple(T1 i1, T2 i2, T3 i3, T4 i4) {
		Item1 = i1;
		Item2 = i2;
		Item3 = i3;
		Item4 = i4;
	}
	public PTuple<T1, T2, T3, T4> DeepCopy() {
		return new PTuple<T1, T2, T3, T4>(
									Item1.DeepCopy(),
									Item2.DeepCopy(),
									Item3.DeepCopy(),
									Item4.DeepCopy());
	}

    public SymbolicBool PTypeEquals(PTuple<T1, T2, T3, T4> other)
    {
        return Item1.PTypeEquals(other.Item1) && Item2.PTypeEquals(other.Item2) && Item3.PTypeEquals(other.Item3) && Item4.PTypeEquals(other.Item4);
    }
}

public class PTuple<T1, T2, T3, T4, T5> : IPType<PTuple<T1, T2, T3, T4, T5>>
		where T1 : IPType<T1>
		where T2 : IPType<T2>
		where T3 : IPType<T3>
		where T4 : IPType<T4>
		where T5 : IPType<T5> 
{
	public T1 Item1;
	public T2 Item2;
	public T3 Item3;
	public T4 Item4;
	public T5 Item5;
	public PTuple(T1 i1, T2 i2, T3 i3, T4 i4, T5 i5) {
		Item1 = i1;
		Item2 = i2;
		Item3 = i3;
		Item4 = i4;
		Item5 = i5;
	}
	public PTuple<T1, T2, T3, T4, T5> DeepCopy() {
		return new PTuple<T1, T2, T3, T4, T5>(
									Item1.DeepCopy(),
									Item2.DeepCopy(),
									Item3.DeepCopy(),
									Item4.DeepCopy(),
									Item5.DeepCopy());
	}

    public SymbolicBool PTypeEquals(PTuple<T1, T2, T3, T4, T5> other)
    {
        return Item1.PTypeEquals(other.Item1) && Item2.PTypeEquals(other.Item2) && Item3.PTypeEquals(other.Item3) && Item4.PTypeEquals(other.Item4) && Item5.PTypeEquals(Item5);
    }
}

