using System;

public class ValueSummary<T> {
	public static readonly ValueSummary<T> Null = new ValueSummary<T>();

	internal T value;

	#region constructor
	private ValueSummary() { }

	public ValueSummary(T t) {
		value = t;
	}

	public static ValueSummary<ValueSummary<T>[]> NewVSArray(ValueSummary<int> size) {
		ValueSummary<T>[] array = new ValueSummary<T>[size.value];
		for (int i = 0; i < size.value; i++) {
			array [i] = ValueSummary<T>.Null;
		}
		return new ValueSummary<ValueSummary<T>[]>(array);
	}

	public static implicit operator ValueSummary<T>(T t) 
	{ 
		return new ValueSummary<T> (t);
	}

	public static implicit operator T(ValueSummary<T> vs) 
	{
		return vs.value;	
	}

	#endregion

	public ValueSummary<R> GetField<R>(Func<T, ValueSummary<R>> f) {
		return f.Invoke (value);
	}

	public void SetField<R>(Action<T, ValueSummary<R>> f, ValueSummary<R> v) {
		f.Invoke (value, v);
	}

	public ValueSummary<R> GetConstField<R>(Func<T, R> f) {
		return new ValueSummary<R>(f.Invoke (value));
	}

	public void Assign<T2>(ValueSummary<T2> other) where T2 : T {
		value = other.value;
	}

	public void Invoke(params object[] args) {
		(this.value as Delegate).DynamicInvoke (args);
	}

	public ValueSummary<R> Invoke<R>(params object[] args) {
		return (ValueSummary<R>)((this.value as Delegate).DynamicInvoke (args));
	}
		
	#region invokemethod
	public ValueSummary<R> InvokeMethod<R>(Func<T, ValueSummary<R>> f) {
		return f.Invoke(value);
	}

	public ValueSummary<R> InvokeMethod<A1, R>(Func<T, ValueSummary<A1>, ValueSummary<R>> f, ValueSummary<A1> arg1) {
		return f.Invoke(value, arg1);
	}

	public ValueSummary<R> InvokeMethod<A1, A2, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>, 
														ValueSummary<R>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A2> arg2) {
		return f.Invoke(value, arg1, arg2);
	}

	public ValueSummary<R> InvokeMethod<A1, A2, A3, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>, ValueSummary<R>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A2> arg2, 
														ValueSummary<A3> arg3) 
	{
		return f.Invoke(value, arg1, arg2, arg3);
	}

	public ValueSummary<R> InvokeMethod<A1, A2, A3, A4, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>, ValueSummary<A4>, ValueSummary<R>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A2> arg2, 
														ValueSummary<A3> arg3,
														ValueSummary<A4> arg4) 
	{
		return f.Invoke(value, arg1, arg2, arg3, arg4);
	}

	public void InvokeMethod(Action<T> f) {
		f.Invoke(value);
	}

	public void InvokeMethod<A1>(Action<T, ValueSummary<A1>> f, ValueSummary<A1> arg1) {
		f.Invoke(value, arg1);
	}

	public void InvokeMethod<A1, A2>(Action<T, ValueSummary<A1>, ValueSummary<A2>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A2> arg2) {
		f.Invoke(value, arg1, arg2);
	}

	public void InvokeMethod<A1, A2, A3>(Action<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A2> arg2, 
														ValueSummary<A3> arg3) 
	{
		f.Invoke(value, arg1, arg2, arg3);
	}

	public void InvokeMethod<A1, A2, A3, A4>(Action<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>, ValueSummary<A4>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A2> arg2, 
														ValueSummary<A3> arg3,
														ValueSummary<A4> arg4) 
	{
		f.Invoke(value, arg1, arg2, arg3, arg4);
	}
	#endregion

	#region invokedynamic
	public ValueSummary<R> InvokeDynamic<R>(Func<T, ValueSummary<R>> f) {
		return f.Invoke(value);
	}

	public ValueSummary<R> InvokeDynamic<A1, R>(Func<T, ValueSummary<A1>, ValueSummary<R>> f, ValueSummary<A1> arg1) {
		return f.Invoke(value, arg1);
	}

	public ValueSummary<R> InvokeDynamic<A1, A2, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>, 
		ValueSummary<R>> f, 
		ValueSummary<A1> arg1, 
		ValueSummary<A2> arg2) {
		return f.Invoke(value, arg1, arg2);
	}

	public ValueSummary<R> InvokeDynamic<A1, A2, A3, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>, ValueSummary<R>> f, 
		ValueSummary<A1> arg1, 
		ValueSummary<A2> arg2, 
		ValueSummary<A3> arg3) 
	{
		return f.Invoke(value, arg1, arg2, arg3);
	}

	public ValueSummary<R> InvokeDynamic<A1, A2, A3, A4, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>, ValueSummary<A4>, ValueSummary<R>> f, 
		ValueSummary<A1> arg1, 
		ValueSummary<A2> arg2, 
		ValueSummary<A3> arg3,
		ValueSummary<A4> arg4) 
	{
		return f.Invoke(value, arg1, arg2, arg3, arg4);
	}
	#endregion

	public ValueSummary<R> InvokeBinary<T2, R>(Func<T, T2, R> f, ValueSummary<T2> other) {
		return new ValueSummary<R> (f.Invoke (value, other.value));
	}

	public ValueSummary<R> InvokeUnary<R>(Func<T, R> f) {
		return new ValueSummary<R> (f.Invoke (value));
	}

	public ValueSummary<R> GetIndex<I, R>(Func<T, I, ValueSummary<R>> f, ValueSummary<I> i) {
		return f.Invoke(value, i.value);
	}

	public ValueSummary<R> GetIndex<I1, I2, R>(Func<T, I1, I2, ValueSummary<R>> f, ValueSummary<I1> i1, ValueSummary<I2> i2) {
		return f.Invoke(value, i1.value, i2.value);
	}

	public void SetIndex<I, R>(Action<T, I, ValueSummary<R>> f, ValueSummary<I> i, ValueSummary<R> v) {
		f.Invoke(value, i.value, v);
	}

	public void SetIndex<I1, I2, R>(Action<T, I1, I2, ValueSummary<R>> f, ValueSummary<I1> i1, ValueSummary<I2> i2, ValueSummary<R> v) {
		f.Invoke(value, i1.value, i2.value, v);
	}

	public ValueSummary<R> Cast<R>(Func<T, R> f) {
		return new ValueSummary<R> (f.Invoke (value));
	}

	public override string ToString ()
	{
		return "[VS]{" + value.ToString() + "}";
	}
}

public static class ValueSummaryExt {
	public static ValueSummary<R> GetIndex<R>(this R[] array, ValueSummary<int> index) {
		return array [index.value];
	} 

	public static ValueSummary<R> GetIndex<R>(this R[] array, ValueSummary<SymbolicInteger> index) {
		return array [index.value];
	}

	public static ValueSummary<R> GetIndex<R>(this R[,] array, ValueSummary<int> i1, ValueSummary<int> i2) {
		return array [i1.value, i2.value];
	} 

	public static ValueSummary<R> GetIndex<R>(this R[,] array, ValueSummary<int> i1, ValueSummary<PInteger> i2) {
		return array [i1.value, i2.value];
	}

	public static ValueSummary<int> Increment(this ValueSummary<int> i) {
		i.value++;
		return i.value;
	} 

	public static ValueSummary<SymbolicInteger> Increment(this ValueSummary<SymbolicInteger> i) {
		i.value++;
		return i.value;
	}

	public static ValueSummary<int> Decrement(this ValueSummary<int> i) {
		i.value--;
		return i.value;
	} 

	public static ValueSummary<SymbolicInteger> Decrement(this ValueSummary<SymbolicInteger> i) {
		i.value--;
		return i.value;
	}

	public static bool Cond(this ValueSummary<bool> b) {
		return b.value;
	}
	public static bool Cond(this ValueSummary<SymbolicBool> b) {
		return b.value;
	}
	public static bool Cond(this ValueSummary<PBool> b) {
		return (SymbolicBool)b.value;
	}

	public static int Switch(this ValueSummary<PInteger> vs) {
		return vs.value.value;
	}
}