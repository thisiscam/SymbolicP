using System;
using Microsoft.Z3;
using BuDDySharp;
using SCG = System.Collections.Generic;
using System.Diagnostics;
using BDDToZ3Wrap;
using System.Collections.Generic;
using System.Linq;

internal class GuardedValue<T>
{
	public bdd bddForm;
	public T value;

	public GuardedValue(bdd bddForm, T value)
	{
		this.bddForm = bddForm;
		this.value = value;
	}

	public GuardedValue(GuardedValue<T> other)
	{
		this.bddForm = other.bddForm;
		this.value = other.value;
	}
}

public class ValueSummary<T>
{

	internal SCG.List<GuardedValue<T>> values = new SCG.List<GuardedValue<T>>();

#if DEBUG_VS
	private static SCG.List<WeakReference<ValueSummary<T>>> ALL_VS = new SCG.List<WeakReference<ValueSummary<T>>>();
#endif

	#region constructor
	public ValueSummary()
	{
#if DEBUG_VS
		ALL_VS.Add (new WeakReference<ValueSummary<T>>(this));
#endif
	}

	public ValueSummary(T t) : this()
	{
		this.values.Add(new GuardedValue<T>(PathConstraint.GetPC(), t));
	}

	public static implicit operator T(ValueSummary<T> vs)
	{
		Debug.Assert(vs.values.Count == 1);
		return vs.values[0].value;
	}

	public static ValueSummary<T> InitializeFrom(ValueSummary<T> t)
	{
		var ret = new ValueSummary<T>();
		var pc = PathConstraint.GetPC();
		ret.values = new SCG.List<GuardedValue<T>>();
		foreach (var val in t.values) {
			var bddForm = val.bddForm.And(pc);
			if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
				ret.values.Add(new GuardedValue<T>(bddForm, val.value));
			}
		}
#if DEBUG
		ret.AssertPredExcusion();
#endif
		return ret;
	}

	public static ValueSummary<ValueSummary<T>[]> NewVSArray(ValueSummary<int> size)
	{
		var pc = PathConstraint.GetPC();
		var ret = new ValueSummary<ValueSummary<T>[]>();
		foreach (var guardedSize in size.values) {
			var bddForm = pc.And(guardedSize.bddForm);
			if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
				var array = new ValueSummary<T>[guardedSize.value];
				for (int i = 0; i < guardedSize.value; i++) {
					array[i] = new ValueSummary<T>(default(T));
				}
				ret.values.Add(new GuardedValue<ValueSummary<T>[]>(bddForm, array));
			}
		}
		return ret;
	}

	public static implicit operator ValueSummary<T>(T t)
	{
		return new ValueSummary<T>(t);
	}

	#endregion

	#region vshelpers
	public void Merge(ValueSummary<T> other)
	{
		Merge(other, PathConstraint.GetPC());
	}

	public void Merge(ValueSummary<T> other, bdd pred)
	{
		foreach (var v2 in other.values) {
			var bddForm = v2.bddForm.And(pred);
			if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
				AddValue(bddForm, v2.value);
			}
		}
	}

	public void AddValue(bdd pred, T val)
	{
		foreach (var guardedValue in this.values) {
			if (EqualityComparer<T>.Default.Equals(guardedValue.value, val)) {
				guardedValue.bddForm = guardedValue.bddForm.Or(pred);
#if DEBUG
				this.AssertPredExcusion();
#endif
				return;
			}
		}
		this.values.Add(new GuardedValue<T>(pred, val));
#if DEBUG
		this.AssertPredExcusion();
#endif
	}

	public void AddValue(T val)
	{
		AddValue(PathConstraint.GetPC(), val);
	}

	public void Update<T2>(ValueSummary<T2> val, bdd pred) where T2 : T
	{
		foreach (var guardedVal in val.values) {
			var p = guardedVal.bddForm.And(pred);
			Update(guardedVal.value, p);
		}
	}

	public void Update<T2>(T2 val, bdd pred) where T2 : T
	{
		if (!pred.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
			var newVals = new SCG.List<GuardedValue<T>>();
			var not_pred = pred.Not();
			foreach (var guardedThisVal in this.values) {
				var bddForm = guardedThisVal.bddForm.And(not_pred);
				if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
					newVals.Add(new GuardedValue<T>(bddForm, guardedThisVal.value));
				}
			}
			this.values = newVals;
			AddValue(pred, val);
		}
	}
	#endregion

	private void NullTargetCheck<TTarget>(Action<bdd, TTarget> f, TTarget target, bdd pred)
	{
		if (target != null) {
			f.Invoke(pred, target);
		}
		else {
			if (PathConstraint.SolveBooleanExpr(pred.ToZ3Expr())) {
				throw new Exception("Null target");
			}
		}
	}

	public ValueSummary<R> GetField<R>(Func<T, ValueSummary<R>> f)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var guardedVal in values) {
			var bddForm = guardedVal.bddForm.And(pc);
			if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
				NullTargetCheck((c, v) => ret.Merge(f.Invoke(v), c), guardedVal.value, bddForm);
			}
		}
		return ret;
	}

	public void SetField<R>(Func<T, ValueSummary<R>> f, ValueSummary<R> val)
	{
		var pc = PathConstraint.GetPC();
		foreach (var guardedVal in values) {
			var bddForm = guardedVal.bddForm.And(pc);
			if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
				NullTargetCheck((c, v) => f.Invoke(v).Update(val, c), guardedVal.value, bddForm);
			}
		}
	}

	public ValueSummary<R> GetConstField<R>(Func<T, R> f)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var guardedVal in values) {
			var bddForm = guardedVal.bddForm.And(pc);
			if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
				NullTargetCheck((c, v) => ret.AddValue(c, f.Invoke(v)), guardedVal.value, bddForm);
			}
		}
		return ret;
	}

	public void Assign<T2>(ValueSummary<T2> other) where T2 : T
	{
		Update(other, PathConstraint.GetPC());
	}

	public void Invoke(params object[] args)
	{
		InvokeMethodHelper(t => (t as Delegate).DynamicInvoke(args));
	}

	public ValueSummary<R> Invoke<R>(params object[] args)
	{
		return InvokeMethodHelper<R>(t => (R)((t as Delegate).DynamicInvoke(args)));
	}

	#region invokemethod
	public ValueSummary<R> InvokeMethodHelper<R>(Func<T, ValueSummary<R>> f)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var guardedTarget in this.values) {
			bdd newPC = pc.And (guardedTarget.bddForm);
			if (!newPC.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) && PathConstraint.SolveBooleanExpr(guardedTarget.bddForm.ToZ3Expr())) {
				NullTargetCheck((c, v) => 
				{
					PathConstraint.PushScope();
					PathConstraint.AddAxiom(c);
					ret.Merge(f.Invoke(v));
					PathConstraint.PopScope(); 
				},
				guardedTarget.value, guardedTarget.bddForm);
			}			
		}
		return ret;
	}

	public ValueSummary<R> InvokeMethod<R>(Func<T, ValueSummary<R>> f)
	{
		return InvokeMethodHelper((v) => f.Invoke(v));
	}

	public ValueSummary<R> InvokeMethod<A1, R>(Func<T, ValueSummary<A1>, ValueSummary<R>> f, ValueSummary<A1> arg1)
	{
		return InvokeMethodHelper((v) => f.Invoke(v, arg1));
	}

	public ValueSummary<R> InvokeMethod<A1, A2, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>,
														ValueSummary<R>> f,
														ValueSummary<A1> arg1,
														ValueSummary<A2> arg2)
	{
		return InvokeMethodHelper((v) => f.Invoke(v, arg1, arg2));
	}

	public ValueSummary<R> InvokeMethod<A1, A2, A3, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>, ValueSummary<R>> f,
														ValueSummary<A1> arg1,
														ValueSummary<A2> arg2,
														ValueSummary<A3> arg3)
	{
		return InvokeMethodHelper((v) => f.Invoke(v, arg1, arg2, arg3));
	}

	public ValueSummary<R> InvokeMethod<A1, A2, A3, A4, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>, ValueSummary<A4>, ValueSummary<R>> f,
														ValueSummary<A1> arg1,
														ValueSummary<A2> arg2,
														ValueSummary<A3> arg3,
														ValueSummary<A4> arg4)
	{
		return InvokeMethodHelper((v) => f.Invoke(v, arg1, arg2, arg3, arg4));
	}

	public void InvokeMethodHelper(Action<T> f)
	{
		var pc = PathConstraint.GetPC();
		foreach (var guardedTarget in this.values) {
			bdd newPC = pc.And (guardedTarget.bddForm);
			if (!newPC.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) && PathConstraint.SolveBooleanExpr(guardedTarget.bddForm.ToZ3Expr())) {
				NullTargetCheck((c, v) => 
				{
					PathConstraint.PushScope();
					PathConstraint.AddAxiom(c);
					f.Invoke(v);
					PathConstraint.PopScope(); 
				},
				guardedTarget.value, guardedTarget.bddForm);
			}			
		}
	}

	public void InvokeMethod(Action<T> f)
	{
		InvokeMethodHelper((v) => f.Invoke(v));
	}

	public void InvokeMethod<A1>(Action<T, ValueSummary<A1>> f, ValueSummary<A1> arg1)
	{
		InvokeMethodHelper((v) => f.Invoke(v, arg1));
	}

	public void InvokeMethod<A1, A2>(Action<T, ValueSummary<A1>, ValueSummary<A2>> f,
														ValueSummary<A1> arg1,
														ValueSummary<A2> arg2)
	{
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

	public ValueSummary<R> InvokeBinary<T2, R>(Func<T, T2, R> f, ValueSummary<T2> other)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var guardedVals in this.values) {
			foreach (var otherGuardedVals in other.values) {
				var bddForm = guardedVals.bddForm.And(otherGuardedVals.bddForm).And(pc);
				if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
					ret.AddValue(bddForm, f.Invoke(guardedVals.value, otherGuardedVals.value));
				}
			}
		}
#if DEBUG
		ret.AssertPredExcusion();
#endif
		return ret;
	}

	public ValueSummary<R> InvokeUnary<R>(Func<T, R> f)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();	
		foreach (var guardedVal in this.values) {
			var bddForm = guardedVal.bddForm.And(pc);
			if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
				ret.AddValue(bddForm, f.Invoke(guardedVal.value));
			}
		}
		return ret;
	}

	public ValueSummary<R> Cast<R>(Func<T, R> f)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var guardedVal in this.values) {
			var bddForm = guardedVal.bddForm.And(pc);
			if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
				NullTargetCheck((c, v) =>
				{
					try {
						ret.AddValue(c, f.Invoke(v));
					}
					catch (InvalidCastException e) {
						if (PathConstraint.SolveBooleanExpr(bddForm.ToZ3Expr())) {
							throw e;
						}
					}
				}, 
				guardedVal.value, bddForm);
			}
		}
		return ret;
	}

	public override string ToString()
	{
		return "[VS]{" + values.Select((gv) => String.Format("{0}", gv.value)).Aggregate("", (a, v) => a + v + ",") + "}";
	}
}

public static class ValueSummaryExt
{
	private static void ArrayIndexOutOfBoundCheck<R>(Action<bdd, R[], int> ifInBoundThenRun, R[] array, int index, bdd bddForm)
	{
		if (index >= 0 && index < array.Length) {
			ifInBoundThenRun.Invoke(bddForm, array, index);
		}
		else {
			if (PathConstraint.SolveBooleanExpr(bddForm.ToZ3Expr())) {
				BuDDySharp.BuDDySharp.printdot(bddForm);
				BDDToZ3Wrap.PInvoke.debug_print_used_bdd_vars();
				throw new Exception("Array index out of bound!");
			}
		}
	}

	private static void ArrayIndexOutOfBoundCheck<R>(Action<bdd, R[,], int, int> ifInBoundThenRun, R[,] array, int i1, int i2, bdd bddForm)
	{
		if (i1 >=0 && i1 < array.GetLength(0) && i2 >= 0 && i2 < array.GetLength(1)) {
			ifInBoundThenRun.Invoke(bddForm, array, i1, i2);
		}
		else {
			if (PathConstraint.SolveBooleanExpr(bddForm.ToZ3Expr())) {
				throw new Exception("Array index out of bound!");
			}
		}
	}

	public static ValueSummary<R> GetIndex<R>(this R[] array, ValueSummary<int> index)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var guardedIndex in index.values) {
			var bddForm = guardedIndex.bddForm.And(pc);
			if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
				ArrayIndexOutOfBoundCheck((c, a, i) => ret.AddValue(c, a[i]), array, guardedIndex.value, bddForm);
			}
		}
		return ret;
	}

	public static ValueSummary<R> GetIndex<R>(this R[] array, ValueSummary<SymbolicInteger> index)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var guardedIndex in index.values) {
			var bddForm = guardedIndex.bddForm.And(pc);
			if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
				if (guardedIndex.value.IsAbstract()) {
					var allVals = PathConstraint.GetAllPossibleValues(guardedIndex.value.AbstractValue);
					foreach (var val in allVals) {
						ArrayIndexOutOfBoundCheck((c, a, i) => ret.AddValue(c, a[i]), array, val.Item2, bddForm.And(val.Item1));
					}
				}
				else {
					ArrayIndexOutOfBoundCheck((c, a, i) => ret.AddValue(c, a[i]), array, guardedIndex.value.ConcreteValue, bddForm);
				}
			}
		}
		return ret;
	}

	public static ValueSummary<R> GetIndex<R>(this R[,] array, ValueSummary<int> idx1, ValueSummary<int> idx2)
	{
		var ret = new ValueSummary<R>();
		foreach (var g2 in idx2.values) {
			foreach (var g1 in idx1.values) {
				var bddForm = g2.bddForm.And(g1.bddForm.And(PathConstraint.GetPC()));
				if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
					ArrayIndexOutOfBoundCheck((c, a, i1, i2) => ret.AddValue(c, a[i1, i2]), array, g1.value, g2.value, bddForm);
				}
			}
		}
		return ret;
	}

	public static ValueSummary<R> GetIndex<R>(this R[,] array, ValueSummary<int> idx1, ValueSummary<PInteger> idx2)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var g1 in idx1.values) {
			foreach (var g2 in idx2.values) {
				var bddForm = g1.bddForm.And(g2.bddForm).And(pc);
				if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
					if (g2.value.value.IsAbstract()) {
						var allVals = PathConstraint.GetAllPossibleValues(g2.value.value.AbstractValue);
						foreach (var val in allVals) {
							ArrayIndexOutOfBoundCheck((c, a, i1, i2) => ret.AddValue(c, a[i1, i2]), array, g1.value, val.Item2, bddForm.And(val.Item1));
						}
					}
					else {
						ArrayIndexOutOfBoundCheck((c, a, i1, i2) => ret.AddValue(c, a[i1, i2]), array, g1.value, g2.value.value.ConcreteValue, bddForm);
					}
				}
			}
		}
		return ret;
	}

	public static ValueSummary<R> GetIndex<R>(this ValueSummary<ValueSummary<R>[]> vs_array, ValueSummary<int> index)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var guardedArray in vs_array.values) {
			foreach (var guardedIndex in index.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And(pc));
				if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
					ArrayIndexOutOfBoundCheck((c, a, i) => ret.Merge(a[i], c), guardedArray.value, guardedIndex.value, bddForm);
				}
			}
		}
		return ret;
	}

	public static ValueSummary<R> GetIndex<R>(this ValueSummary<ValueSummary<R>[]> vs_array, ValueSummary<SymbolicInteger> index)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var guardedArray in vs_array.values) {
			foreach (var guardedIndex in index.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And(pc));
				if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
					if (guardedIndex.value.IsAbstract()) {
						foreach (var t in PathConstraint.GetAllPossibleValues(guardedIndex.value.AbstractValue)) {
							ArrayIndexOutOfBoundCheck((c, a, i) => ret.Merge(a[i], c), guardedArray.value, t.Item2, bddForm.And(t.Item1));
						}
					}
					else {
						ArrayIndexOutOfBoundCheck((c, a, i) => ret.Merge(a[i], c), guardedArray.value, guardedIndex.value.ConcreteValue, bddForm);
					}
				}
			}
		}
		return ret;
	}

	public static ValueSummary<R> GetIndex<R>(this ValueSummary<ValueSummary<R>[]> vs_array, ValueSummary<PInteger> index)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var guardedIndex in index.values) {
			foreach (var guardedArray in vs_array.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And(pc));
				if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
					if (guardedIndex.value.value.IsAbstract()) {
						foreach (var t in PathConstraint.GetAllPossibleValues(guardedIndex.value.value.AbstractValue)) {
							ArrayIndexOutOfBoundCheck((c, a, i) => ret.Merge(a[i], c), guardedArray.value, t.Item2, bddForm.And(t.Item1));
						}
					}
					else {
						ArrayIndexOutOfBoundCheck((c, a, i) => ret.Merge(a[i], c), guardedArray.value, guardedIndex.value.value.ConcreteValue, bddForm);
					}
				}
			}
		}
		return ret;
	}

	public static void SetIndex<R>(this ValueSummary<ValueSummary<R>[]> vs_array, ValueSummary<int> index, ValueSummary<R> val)
	{
		var pc = PathConstraint.GetPC();
		foreach (var guardedIndex in index.values) {
			foreach (var guardedArray in vs_array.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And(pc));
				if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
					ArrayIndexOutOfBoundCheck((c, a, i) => a[i].Update(val, c), guardedArray.value, guardedIndex.value, bddForm);
				}
			}
		}
	}

	public static void SetIndex<R>(this ValueSummary<ValueSummary<R>[]> vs_array, ValueSummary<SymbolicInteger> index, ValueSummary<R> val)
	{
		var pc = PathConstraint.GetPC();
		foreach (var guardedIndex in index.values) {
			foreach (var guardedArray in vs_array.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And(pc));
				if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
					if (guardedIndex.value.IsAbstract()) {
						foreach (Tuple<bdd, int> t in PathConstraint.GetAllPossibleValues(guardedIndex.value.AbstractValue)) {
							ArrayIndexOutOfBoundCheck((c, a, i) => a[i].Update(val, c), guardedArray.value, t.Item2, bddForm.And(t.Item1));
						}
					}
					else {
							ArrayIndexOutOfBoundCheck((c, a, i) => a[i].Update(val, c), guardedArray.value, guardedIndex.value.ConcreteValue, bddForm);
					}
				}
			}
		}
	}

	public static void SetIndex<R>(this ValueSummary<ValueSummary<R>[]> vs_array, ValueSummary<PInteger> index, ValueSummary<R> val)
	{
		foreach (var guardedIndex in index.values) {
			foreach (var guardedArray in vs_array.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And(PathConstraint.GetPC()));
				if (!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
					if (guardedIndex.value.value.IsAbstract()) {
						foreach (Tuple<bdd, int> t in PathConstraint.GetAllPossibleValues(guardedIndex.value.value.AbstractValue)) {
							ArrayIndexOutOfBoundCheck((c, a, i) => a[i].Update(val, c), guardedArray.value, t.Item2, bddForm.And(t.Item1));
						}
					}
					else {
						ArrayIndexOutOfBoundCheck((c, a, i) => a[i].Update(val, c), guardedArray.value, guardedIndex.value.value.ConcreteValue, bddForm);
					}
				}
			}
		}
	}

	public static ValueSummary<int> Increment(this ValueSummary<int> i)
	{
		i.Update(i.InvokeUnary(arg => arg + 1), PathConstraint.GetPC());
		return ValueSummary<int>.InitializeFrom(i);
	}

	public static ValueSummary<SymbolicInteger> Increment(this ValueSummary<SymbolicInteger> i)
	{
		i.Update(i.InvokeUnary(arg => arg + 1), PathConstraint.GetPC());
		return ValueSummary<SymbolicInteger>.InitializeFrom(i);
	}

	public static ValueSummary<int> Decrement(this ValueSummary<int> i)
	{
		i.Update(i.InvokeUnary(arg => arg - 1), PathConstraint.GetPC());
		return ValueSummary<int>.InitializeFrom(i);
	}

	public static ValueSummary<SymbolicInteger> Decrement(this ValueSummary<SymbolicInteger> i)
	{
		i.Update(i.InvokeUnary(arg => arg - 1), PathConstraint.GetPC());
		return ValueSummary<SymbolicInteger>.InitializeFrom(i);
	}

	private static PathConstraint.BranchPoint CondConcreteHelper(bdd trueBDD, bdd falseBDD)
	{
		//var a0 = trueBDD.ToZ3Expr();
		//var a1 = falseBDD.ToZ3Expr();
		//PathConstraint.solver.Push();
		//PathConstraint.solver.Assert(PathConstraint.ctx.MkNot(PathConstraint.ctx.MkIff(a0, PathConstraint.ctx.MkNot(a1))));
		//var s = PathConstraint.solver.Check();	
		//PathConstraint.solver.Pop();
		//if (s != Status.UNSATISFIABLE) {
		//	Debugger.Break();
		//}
		var trueFeasible = !trueBDD.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) && PathConstraint.SolveBooleanExpr(trueBDD.ToZ3Expr());
		var falseFeasible = !falseBDD.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) && PathConstraint.SolveBooleanExpr(falseBDD.ToZ3Expr());
		if (trueFeasible) {
			if (falseFeasible) {
				return new PathConstraint.BranchPoint(trueBDD, falseBDD, PathConstraint.BranchPoint.State.Both);
			}
			else {
				return new PathConstraint.BranchPoint(trueBDD, null, PathConstraint.BranchPoint.State.True);
			}
		}
		else {
			if (falseFeasible) {
				return new PathConstraint.BranchPoint(null, falseBDD, PathConstraint.BranchPoint.State.False);
			}
			else {
				if (!PathConstraint.EvalPc())
				{
					return new PathConstraint.BranchPoint(null, null, PathConstraint.BranchPoint.State.None);	
				}
				else 
				{
					throw new Exception("Not reachable");
				}
			}
		}
	}

	public static PathConstraint.BranchPoint _Cond(this ValueSummary<bool> b)
	{
		var pc = PathConstraint.GetPC();
		if (b.values.Count > 2) { Debugger.Break();}
		if (b.values.Count == 1) {
			if (b.values[0].value) {
				//if (!pc.EqualEqual(b.values[0].bddForm)) { Debugger.Break();}
				return new PathConstraint.BranchPoint(b.values[0].bddForm.And(pc), null, PathConstraint.BranchPoint.State.True);
			}
			else {
				//if (!pc.EqualEqual(b.values[0].bddForm)) { Debugger.Break();}
				return new PathConstraint.BranchPoint(null, b.values[0].bddForm.And(pc), PathConstraint.BranchPoint.State.False);
			}
		}
		else if (b.values.Count == 2) {
			if (b.values[0].value) {
				return CondConcreteHelper(b.values[0].bddForm.And(pc), b.values[1].bddForm.And(pc));
			}
			else {
				return CondConcreteHelper(b.values[1].bddForm.And(pc), b.values[0].bddForm.And(pc));
			}
		} else {
				if (!PathConstraint.EvalPc())
				{
					return new PathConstraint.BranchPoint(null, null, PathConstraint.BranchPoint.State.None);	
				}
				else 
				{
					throw new Exception("Not reachable");
				}
		}
	}
	
	public static PathConstraint.BranchPoint Cond(this ValueSummary<bool> b) 
	{
		var ret = _Cond(b);
		PathConstraint.PushScope();
		PathConstraint.GetCurrentFrame().return_stack.Push(BuDDySharp.BuDDySharp.bddfalse);
		return ret;
 	}

	public static PathConstraint.BranchPoint _Cond(this ValueSummary<SymbolicBool> b)
	{
		var pc = PathConstraint.GetPC();
		bdd predTrue = BuDDySharp.BuDDySharp.bddfalse, predFalse = BuDDySharp.BuDDySharp.bddfalse;
		foreach (var guardedBooleanVal in b.values) {
			var guardedValuePcPred = guardedBooleanVal.bddForm.And(pc);
			if (!guardedValuePcPred.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) && PathConstraint.SolveBooleanExpr(guardedValuePcPred.ToZ3Expr())) {
				if (guardedBooleanVal.value.IsAbstract()) {
					var c = guardedBooleanVal.value.AbstractValue.ToBDD();
					if (c.EqualEqual(BuDDySharp.BuDDySharp.bddtrue)) {
						predTrue = predTrue.Or(guardedValuePcPred);
					}
					else if (c.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
						predFalse = predFalse.Or(guardedValuePcPred);
					}
					else {
						var tmp = guardedValuePcPred.And(c);
						var trueFeasible = !tmp.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) && PathConstraint.SolveBooleanExpr(tmp.ToZ3Expr());
						if (trueFeasible) {
							predTrue = predTrue.Or(tmp);
						}
						tmp = guardedValuePcPred.And(c.Not());
						var falseFeasible = !tmp.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) && PathConstraint.SolveBooleanExpr(tmp.ToZ3Expr());
						if (falseFeasible) {
							predFalse = predFalse.Or(tmp);
						}
					}
				}
				else {
					if (guardedBooleanVal.value.ConcreteValue) {
						predTrue = predTrue.Or(guardedValuePcPred);
					}
					else {
						predFalse = predFalse.Or(guardedValuePcPred);
					}
				}
			}
		}
		if (predTrue != BuDDySharp.BuDDySharp.bddfalse) {
			if (predFalse != BuDDySharp.BuDDySharp.bddfalse) {
				return new PathConstraint.BranchPoint(predTrue, predFalse, PathConstraint.BranchPoint.State.Both);
			}
			else {
				return new PathConstraint.BranchPoint(predTrue, null, PathConstraint.BranchPoint.State.True);
			}
		}
		else {
			if (predFalse != BuDDySharp.BuDDySharp.bddfalse) {
				return new PathConstraint.BranchPoint(null, predFalse, PathConstraint.BranchPoint.State.False);
			}
			else {
				if (!PathConstraint.EvalPc())
				{
					return new PathConstraint.BranchPoint(null, null, PathConstraint.BranchPoint.State.None);	
				}
				else 
				{
					throw new Exception("Not reachable");
				}
			}
		}
	}
	
	public static PathConstraint.BranchPoint Cond(this ValueSummary<SymbolicBool> b) 
	{
		var ret = _Cond(b);
		PathConstraint.PushScope();
		PathConstraint.GetCurrentFrame().return_stack.Push(BuDDySharp.BuDDySharp.bddfalse);
		return ret;
 	}
	
	public static PathConstraint.BranchPoint _Cond(this ValueSummary<PBool> b)
	{
		//TODO make this more efficient
		var tmp = new ValueSummary<SymbolicBool>();
		foreach (var v in b.values) {
			tmp.values.Add(new GuardedValue<SymbolicBool>(v.bddForm, (SymbolicBool)v.value));
		}
		return _Cond(tmp);
	}
	
	public static PathConstraint.BranchPoint Cond(this ValueSummary<PBool> b) 
	{
		var ret = _Cond(b);
		PathConstraint.PushScope();
		PathConstraint.GetCurrentFrame().return_stack.Push(BuDDySharp.BuDDySharp.bddfalse);
		return ret;
 	}

	public static bool LoopHelper(Func<PathConstraint.BranchPoint> f)
	{
		var not_return_paths = PathConstraint.GetCurrentFrame().return_stack.Peek().Not();
		PathConstraint.AddAxiom(not_return_paths);
		if (PathConstraint.EvalPc()) {
			var c = f.Invoke();
			switch (c.state) {
				case PathConstraint.BranchPoint.State.Both:
				case PathConstraint.BranchPoint.State.True: 
					{
						PathConstraint.AddAxiom(c.trueBDD);
						return true;
					}
				case PathConstraint.BranchPoint.State.None:
				case PathConstraint.BranchPoint.State.False: 
					{
						PathConstraint.MergeBranch();
						return false;
					}
				default: 
					{
						throw new Exception("Not reachable");
					}
			}
		}
		else 
		{
			PathConstraint.MergeBranch();
			return false;	
 		}
	}

	public static bool Loop(this ValueSummary<bool> b)
	{
		return LoopHelper(() => _Cond(b));
	}

	public static bool Loop(this ValueSummary<SymbolicBool> b)
	{
		return LoopHelper(() => _Cond(b));
	}
	
	public static bool Loop(this ValueSummary<PBool> b)
	{
		return LoopHelper(() => _Cond(b));
	}

	public static void RecordReturn<T>(this ValueSummary<T> x, ValueSummary<T> other)
	{
		PathConstraint.RecordReturnPath();
		x.Merge(other, PathConstraint.GetPC());
	}


#region DEBUG_VS
	public static void AssertPredExcusion<T>(this ValueSummary<T> x)
	{
		PathConstraint.solver.Push();
		var a = PathConstraint.ctx.MkTrue();
		foreach(var g1 in x.values) 
		{
			foreach (var g2 in x.values) 
			{
				if (g1 != g2) {
					a = PathConstraint.ctx.MkAnd(a, PathConstraint.ctx.MkImplies(g1.bddForm.ToZ3Expr(), g2.bddForm.Not().ToZ3Expr()));
				}
			}
		}
		PathConstraint.solver.Assert(PathConstraint.ctx.MkNot(a));
		var status = PathConstraint.solver.Check();
		PathConstraint.solver.Pop();
		if (status != Status.UNSATISFIABLE) 
		{
			throw new Exception("BDD exclusion failure");
		}
	} 
#endregion
}