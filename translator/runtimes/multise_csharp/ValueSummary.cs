using System;

public class ValueSummary<T> {
	T value;

	#region constructor
	public ValueSummary(T t) {
		value = t;
	}

//	public ValueSummary(Func<T> f) {
//		value = f.Invoke ();
//	}
//
//	public ValueSummary(Func<ValueSummary<A1>, T> f, ValueSummary<A1> arg1) {
//		value = f.Invoke (arg1);
//	}
//
//	public ValueSummary(Func<ValueSummary<A1, A2>, T> f, ValueSummary<A1> arg1, ValueSummary<A2> arg2) {
//		value = f.Invoke (arg1, arg2);
//	}
//
//	public ValueSummary(Func<ValueSummary<A1, A2, A3>, T> f, ValueSummary<A1> arg1, ValueSummary<A2> arg2, ValueSummary<A3> arg3) {
//		value = f.Invoke (arg1, arg2, arg3);
//	}
//
//	public ValueSummary(Func<ValueSummary<A1, A2, A3, A4>, T> f, ValueSummary<A1> arg1, ValueSummary<A2> arg2, ValueSummary<A3> arg3, ValueSummary<A4> arg4) {
//		value = f.Invoke (arg1, arg2, arg3, arg4);
//	}

	public static implicit operator ValueSummary<T>(T t) 
	{ 
		return new ValueSummary (t);
	}
	
	#endregion

	public ValueSummary<R> GetField<R>(Func<T, ValueSummary<R>> f) {
		return f.Invoke (value);
	}

	public void Assign<T2>(ValueSummary<T2> other) where T2 : T {
		value = other.value;
	}

	public ValueSummary<R> Invoke<R>(params object[] args) {
		return (ValueSummary<R>)((this.value as Delegate).DynamicInvoke (args));
	}
		
	#region invokemethod
	public ValueSummary<R> InvokeMethod<R>(Func<T, ValueSummary<T>, ValueSummary<R>> f) {
		return f.Invoke(value, this);
	}

	public ValueSummary<R> InvokeMethod<A1, R>(Func<T, ValueSummary<T>, ValueSummary<A1>, ValueSummary<R>> f, ValueSummary<A1> arg1) {
		return f.Invoke(value, this, arg1);
	}

	public ValueSummary<R> InvokeMethod<A1, A2, R>(Func<T, ValueSummary<T>, ValueSummary<A1>, ValueSummary<A2>, 
														ValueSummary<R>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A1> arg2) {
		return f.Invoke(value, this, arg1, arg2);
	}

	public ValueSummary<R> InvokeMethod<A1, A2, A3, R>(Func<T, ValueSummary<T>, ValueSummary<A1>, ValueSummary<A1>, ValueSummary<A3>, ValueSummary<R>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A2> arg2, 
														ValueSummary<A3> arg3) 
	{
		return f.Invoke(value, this, arg1, arg2, arg3);
	}

	public ValueSummary<R> InvokeMethod<A1, A2, A3, A4, R>(Func<T, ValueSummary<T>, ValueSummary<A1>, ValueSummary<A1>, ValueSummary<A3>, ValueSummary<R>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A2> arg2, 
														ValueSummary<A3> arg3,
														ValueSummary<A4> arg4) 
	{
		return f.Invoke(value, this, arg1, arg2, arg3, arg4);
	}
	#endregion

	#region invokedynamic
	public ValueSummary<R> InvokeDynamic<R>(Func<T, ValueSummary<T>, ValueSummary<R>> f) {
		return f.Invoke(value, this);
	}

	public ValueSummary<R> InvokeDynamic<A1, R>(Func<T, ValueSummary<T>, ValueSummary<A1>, ValueSummary<R>> f, ValueSummary<A1> arg1) {
		return f.Invoke(value, this, arg1);
	}

	public ValueSummary<R> InvokeDynamic<A1, A2, R>(Func<T, ValueSummary<T>, ValueSummary<A1>, ValueSummary<A2>, 
		ValueSummary<R>> f, 
		ValueSummary<A1> arg1, 
		ValueSummary<A1> arg2) {
		return f.Invoke(value, this, arg1, arg2);
	}

	public ValueSummary<R> InvokeDynamic<A1, A2, A3, R>(Func<T, ValueSummary<T>, ValueSummary<A1>, ValueSummary<A1>, ValueSummary<A3>, ValueSummary<R>> f, 
		ValueSummary<A1> arg1, 
		ValueSummary<A2> arg2, 
		ValueSummary<A3> arg3) 
	{
		return f.Invoke(value, this, arg1, arg2, arg3);
	}

	public ValueSummary<R> InvokeDynamic<A1, A2, A3, A4, R>(Func<T, ValueSummary<T>, ValueSummary<A1>, ValueSummary<A1>, ValueSummary<A3>, ValueSummary<R>> f, 
		ValueSummary<A1> arg1, 
		ValueSummary<A2> arg2, 
		ValueSummary<A3> arg3,
		ValueSummary<A4> arg4) 
	{
		return f.Invoke(value, this, arg1, arg2, arg3, arg4);
	}
	#endregion

	public ValueSummary<R> InvokeBinary<T2, R>(Func<T, T2, R> f, ValueSummary<T2> other) {
		return new ValueSummary<R> (f.Invoke (value, other.value));
	}

	public ValueSummary<R> GetIndex<I, R>(Func<T, ValueSummary<I>, ValueSummary<R>> f, ValueSummary<I> i) {
		return f.Invoke(value, i);
	}

	public ValueSummary<R> GetIndex<I1, I2, R>(Func<T, ValueSummary<I1>, ValueSummary<I2>, ValueSummary<R>> f, ValueSummary<I1> i1, ValueSummary<I2> i2) {
		return f.Invoke(value, i1, i2);
	}
}