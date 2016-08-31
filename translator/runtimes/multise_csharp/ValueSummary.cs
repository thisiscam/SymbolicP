using System;
using Microsoft.Z3;
using BuDDySharp;
using SCG = System.Collections.Generic;
using System.Diagnostics;
using BDDToZ3Wrap;

internal struct GuardedValue<T> {
	public bdd bddForm;
	public T value;

	public GuardedValue(bdd bddForm, T value) {
		this.bddForm = bddForm;
		this.value = value;
	}
}

public class ValueSummary<T> {
	public static readonly ValueSummary<T> Null = new ValueSummary<T>();

	internal SCG.List<GuardedValue<T>> values = new SCG.List<GuardedValue<T>> ();

	internal T value;

	#region constructor
	private ValueSummary() { }

	public ValueSummary(T t) {
		value = t;
	}

	private ValueSummary(ValueSummary<T> t) {
		values = new SCG.List<GuardedValue<T>> ();
		foreach (var val in t.values) {
			values.Add (new GuardedValue<T> (val.bddForm, val.value));
		}
	}


	public static ValueSummary<ValueSummary<T>[]> NewVSArray(ValueSummary<int> size) 
	{
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

	#region vshelpers
	public void Merge(ValueSummary<T> other)
	{
		foreach (var v2 in other.values) {
			foreach(var v1 in this.values) {
				if (v1.Equals (v2)) {
					continue;
				}
			}
			this.values.Add (v2);
		}
	}

	public void Merge(ValueSummary<T> other, bdd pred)
	{
		foreach (var v2 in other.values) {
			foreach(var v1 in this.values) {
				if (v1.Equals (v2)) {
					continue;
				}
			}
			this.values.Add (new GuardedValue<T>(v2.bddForm.And(pred), v2.value));
		}
	}

	public void AddValue(bdd pred, T val)
	{
		foreach(var guardedValue in this.values) {
			if (guardedValue.value.Equals (val)) {
				guardedValue.bddForm = guardedValue.bddForm.Or (pred);
				return;
			}
		}
		this.values.Add (new GuardedValue<T> (pred, val));
	}

	public void Assign(ValueSummary<T> val, bdd pred)
	{
		foreach (var guardedVal in val.values) {
			var p = guardedVal.bddForm.And (pred);
			Assign (guardedVal.value, p);
		}
	}

	public void Assign(T val, bdd pred)
	{
		if (!pred.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
			var newVals = new SCG.List<GuardedValue<T>> ();
			foreach (var guardedThisVal in this.values) {
				guardedThisVal.bddForm = guardedThisVal.bddForm.And (pred.Not());
				if (!guardedThisVal.bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
					newVals.Add (guardedThisVal);
				}
			}
			this.values = newVals;
			AddValue (pred, val);
		}
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
	public ValueSummary<R> InvokeMethodHelper<R>(Func<T, ValueSummary<R>> f){
		var ret = new ValueSummary<R> ();
		foreach(var guardedTarget in this.values) {
			if (!PathConstraint.TryPushFrame (guardedTarget.bddForm)) {
				continue;
			}
			do {
				ret.Merge (f.Invoke (guardedTarget.value));
			} while(PathConstraint.BacktrackInvocation ());
			PathConstraint.PopFrame ();
		}
		return ret;
	}

	public ValueSummary<R> InvokeMethod<R>(Func<T, ValueSummary<R>> f) {
		return InvokeMethodHelper ((v) => f.Invoke (v));
	}

	public ValueSummary<R> InvokeMethod<A1, R>(Func<T, ValueSummary<A1>, ValueSummary<R>> f, ValueSummary<A1> arg1) {
		return InvokeMethodHelper ((v) => f.Invoke (v, arg1));
	}

	public ValueSummary<R> InvokeMethod<A1, A2, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>, 
														ValueSummary<R>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A2> arg2) {
		return InvokeMethodHelper ((v) => f.Invoke (v, arg1, arg2));
	}

	public ValueSummary<R> InvokeMethod<A1, A2, A3, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>, ValueSummary<R>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A2> arg2, 
														ValueSummary<A3> arg3) 
	{
		return InvokeMethodHelper ((v) => f.Invoke (v, arg1, arg2, arg3));
	}

	public ValueSummary<R> InvokeMethod<A1, A2, A3, A4, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>, ValueSummary<A4>, ValueSummary<R>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A2> arg2, 
														ValueSummary<A3> arg3,
														ValueSummary<A4> arg4) 
	{
		return f.Invoke(value, arg1, arg2, arg3, arg4);
	}

	public void InvokeMethodHelper(Action f){
		foreach(var guardedTarget in this.values) {
			if (!PathConstraint.TryPushFrame (guardedTarget.bddForm)) {
				continue;
			}
			do {
				f.Invoke (guardedTarget.value);
			} while(PathConstraint.BacktrackInvocation ());
			PathConstraint.PopFrame ();
		}
	}

	public void InvokeMethod(Action<T> f) {
		InvokeMethodHelper((v) => f.Invoke(v));
	}

	public void InvokeMethod<A1>(Action<T, ValueSummary<A1>> f, ValueSummary<A1> arg1) {
		InvokeMethodHelper((v) => f.Invoke(v, arg1));
	}

	public void InvokeMethod<A1, A2>(Action<T, ValueSummary<A1>, ValueSummary<A2>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A2> arg2) {
		InvokeMethodHelper((v) => f.Invoke(v, arg1, arg2));
	}

	public void InvokeMethod<A1, A2, A3>(Action<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A2> arg2, 
														ValueSummary<A3> arg3) 
	{
		InvokeMethodHelper((v) => f.Invoke(v, arg1, arg2, arg3));
	}

	public void InvokeMethod<A1, A2, A3, A4>(Action<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>, ValueSummary<A4>> f, 
														ValueSummary<A1> arg1, 
														ValueSummary<A2> arg2, 
														ValueSummary<A3> arg3,
														ValueSummary<A4> arg4) 
	{
		InvokeMethodHelper((v) => f.Invoke(v, arg1, arg2, arg3, arg4));
	}
	#endregion

	public ValueSummary<R> InvokeBinary<T2, R>(Func<T, T2, R> f, ValueSummary<T2> other) {
		var ret = new ValueSummary<R> ();
		foreach (var guardedVals in this.values) {
			foreach (var otherGuardedVals in other.values) {
				var bddForm = guardedVals.bddForm.And (otherGuardedVals.bddForm).And(PathConstraint.GetPC());
				if(!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
					ret.AddValue(bddForm, f.Invoke (guardedVals.value, otherGuardedVals.value));
				}
			}
		}
		return ret;
	}

	public ValueSummary<R> InvokeUnary<R>(Func<T, R> f) {
		var ret = new ValueSummary<R>();
		foreach (var guardedVals in this.values) {
			var bddForm = guardedVals.bddForm.And(PathConstraint.GetPC());
			if(!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
				ret.AddValue(bddForm, f.Invoke (this.value));
			}
		}
		return ret;
	}

	public ValueSummary<R> Cast<R>(Func<T, R> f) {
		return InvokeUnary<R> (f);
	}

	public override string ToString ()
	{
		return "[VS]{" + value.ToString() + "}";
	}
}

public static class ValueSummaryExt {
	public static ValueSummary<R> GetIndex<R>(this R[] array, ValueSummary<int> index) {
		var ret = new ValueSummary<R> ();
		foreach (var guardedIndex in index.values) {
			var bddForm = guardedIndex.bddForm.And (PathConstraint.GetPC ());
			if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
				ret.AddValue (bddForm, array [guardedIndex.value]);
			}
		}
		return ret;
	} 

	public static ValueSummary<R> GetIndex<R>(this R[] array, ValueSummary<SymbolicInteger> index) {
		var ret = new ValueSummary<R> ();
		foreach (var guardedIndex in index.values) {
			var bddForm = guardedIndex.bddForm.And (PathConstraint.GetPC ());
			if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
				if (guardedIndex.value.IsAbstract ()) {
					var allVals = PathConstraint.GetAllPossibleValues (guardedIndex.value.AbstractValue);
					foreach (var val in allVals) {
						ret.AddValue (bddForm, array[val]);
					}			
				} else {
					ret.AddValue (bddForm, array[guardedIndex.value.ConcreteValue]);
				}
			}
		}
		return ret;
	}

	public static ValueSummary<R> GetIndex<R>(this R[,] array, ValueSummary<int> i1, ValueSummary<int> i2) {
		var ret = new ValueSummary<R> ();
		foreach (var g2 in i2.values) {
			foreach (var g1 in i1.values) {
				var bddForm = g2.bddForm.And(g1.bddForm.And (PathConstraint.GetPC ()));
				if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
					ret.AddValue (bddForm, array [g1.value, g2.value]);
				}
			}
		}
		return ret;
	}

	public static ValueSummary<R> GetIndex<R>(this R[,] array, ValueSummary<int> i1, ValueSummary<PInteger> i2) {
		var ret = new ValueSummary<R> ();
		foreach (var g1 in i1.values) {
			foreach (var g2 in i2.values) {
				var bddForm = g1.bddForm.And (PathConstraint.GetPC ());
				if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
					if (g2.value.value.IsAbstract ()) {
						var allVals = PathConstraint.GetAllPossibleValues (g2.value.value.AbstractValue);
						foreach (var val in allVals) {
							ret.AddValue (bddForm, array[g1.value, val]);
						}	
					} else {
						ret.AddValue (bddForm, array [g1.value, g2.value.value.ConcreteValue]);
					}
				}
			}
		}
		return ret;
	}

	public static ValueSummary<R> GetIndex<R>(this ValueSummary<ValueSummary<R>[]> vs_array, ValueSummary<int> index) {
		var ret = new ValueSummary<R> ();
		foreach(var guardedIndex in index.values) {
			foreach(var guardedArray in vs_array.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And (PathConstraint.GetPC ()));
				if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
					ret.Merge(guardedArray.value [guardedIndex.value], bddForm);
				}
			}
		}
		return ret;
	}

	public static ValueSummary<R> GetIndex<R>(this ValueSummary<ValueSummary<R>[]> vs_array, ValueSummary<SymbolicInteger> index) {
		var ret = new ValueSummary<R> ();
		foreach(var guardedIndex in index.values) {
			foreach(var guardedArray in vs_array.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And (PathConstraint.GetPC ()));
				if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
					if (guardedIndex.value.IsAbstract ()) {
						foreach (Tuple<bdd, int> t in PathConstraint.GetAllPossibleValues(guardedIndex.value.AbstractValue)) {
							ret.Merge(guardedArray.value [t.Item2], bddForm.And(t.Item1));
						}
					} else {
						ret.Merge (guardedArray.value [guardedIndex.value.ConcreteValue], bddForm);
					}
				}
			}
		}
		return ret;
	}

	public static void SetIndex<R>(this ValueSummary<ValueSummary<R>[]> vs_array, ValueSummary<int> index, ValueSummary<R> val) {
		foreach(var guardedIndex in index.values) {
			foreach(var guardedArray in vs_array.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And (PathConstraint.GetPC ()));
				if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
					guardedArray.value [guardedIndex.value].Assign (val, bddForm);
				}
			}
		}
	}

	public static void SetIndex<R>(this ValueSummary<ValueSummary<R>[]> vs_array, ValueSummary<SymbolicInteger> index, ValueSummary<R> val) {
		foreach(var guardedIndex in index.values) {
			foreach(var guardedArray in vs_array.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And (PathConstraint.GetPC ()));
				if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
					if (guardedIndex.value.IsAbstract ()) {
						foreach (Tuple<bdd, int> t in PathConstraint.GetAllPossibleValues(guardedIndex.value.AbstractValue)) {
							guardedArray.value [t.Item2].Assign (val, bddForm.And(t.Item1));
						}
					} else {
						guardedArray.value [guardedIndex.value.ConcreteValue].Assign (val, bddForm);
					}
				}
			}
		}
	}

	public static ValueSummary<int> Increment(this ValueSummary<int> i) {
		var ret = new ValueSummary<int> (i);
		foreach (var val in i.values) {
			i.Assign (val.value + 1, val.bddForm.And (PathConstraint.GetPC ()));
		}
		return ret;
	} 

	public static ValueSummary<SymbolicInteger> Increment(this ValueSummary<SymbolicInteger> i) {
		var ret = new ValueSummary<SymbolicInteger> (i);
		foreach (var val in i.values) {
			i.Assign (val.value + 1, val.bddForm.And (PathConstraint.GetPC ()));
		}
		return ret;
	}

	public static ValueSummary<int> Decrement(this ValueSummary<int> i) {
		var ret = new ValueSummary<int> (i);
		foreach (var val in i.values) {
			i.Assign (val.value - 1, val.bddForm.And (PathConstraint.GetPC ()));
		}
		return ret;
	} 

	public static ValueSummary<SymbolicInteger> Decrement(this ValueSummary<SymbolicInteger> i) {
		var ret = new ValueSummary<SymbolicInteger> (i);
		foreach (var val in i.values) {
			i.Assign (val.value - 1, val.bddForm.And (PathConstraint.GetPC ()));
		}
		return ret;
	}

	private static bool CondConcreteHelper(bdd trueBDD, bdd falseBDD)
	{
		var trueFeasible = !trueBDD.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) && PathConstraint.SolveBooleanExpr (trueBDD.ToZ3Expr ()));
		var falseFeasible = !falseBDD.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) && PathConstraint.SolveBooleanExpr(falseBDD.ToZ3Expr()));
		if(trueFeasible) {
			if(falseFeasible) {
				
			} else {
				
			}
		} else {
			if(falseFeasible) {
				
			} else {
				
			}
		}
	}

	public static bool Cond(this ValueSummary<bool> b) {
		var pc = PathConstraint.GetPC ();
		Debug.Assert (b.values.Count <= 2);
		if (b.values.Count == 1) {
			PathConstraint.AddAxiom (b.values [0].bddForm);
			return b.values [0].value;
		} else {
			if (b.values [0].value) {
				CondConcreteHelper (b.values [0], b.values [1]);
			} else {
				CondConcreteHelper (b.values [1], b.values [0]);
			}
		}
	}
	public static bool Cond(this ValueSummary<SymbolicBool> b) {
		return b.value;
	}
	public static bool Cond(this ValueSummary<PBool> b) {
		return (SymbolicBool)b.value;
	}
}