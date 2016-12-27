using System;
using Microsoft.Z3;
using SCG = System.Collections.Generic;
using System.Diagnostics;
using BDDToZ3Wrap;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if USE_SYLVAN
using bdd = SylvanSharp.bdd;
using BDDLIB = SylvanSharp.SylvanSharp;
#else
using bdd = BuDDySharp.bdd;
using BDDLIB = BuDDySharp.BuDDySharp;
#endif

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

public partial class ValueSummary<T>
{

	internal SCG.List<GuardedValue<T>> values = new SCG.List<GuardedValue<T>>();

#if false
	private static SCG.List<WeakReference<ValueSummary<T>>> ALL_VS = new SCG.List<WeakReference<ValueSummary<T>>>();
#endif

	#region constructor
	public ValueSummary()
	{
#if false
		ALL_VS.Add (new WeakReference<ValueSummary<T>>(this));
#endif
	}

	public ValueSummary(T t) : this()
	{
		this.values.Add(new GuardedValue<T>(PathConstraint.GetPC(), t));
	}
	
	public static ValueSummary<T> InitWithTrue(T t)
	{
		var ret = new ValueSummary<T>();
		ret.values.Add(new GuardedValue<T>(bdd.bddtrue, t));
		return ret;
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
			if (bddForm.FormulaBDDSAT()) {
				ret.values.Add(new GuardedValue<T>(bddForm, val.value));
			}
		}
#if DEBUG_VS
		ret.AssertPredExclusion();
#endif
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
			AddValue(bddForm, v2.value);
		}
#if MERGE_PVAL
		MergeMax();
#endif
	}
	
	public void AddValue(bdd pred, T val)
	{
		if(pred.FormulaBDDSAT()) {
#if !NO_MERGE_VS
			foreach (var guardedValue in this.values) {
				if (EqualityComparer<T>.Default.Equals(guardedValue.value, val)) {
					guardedValue.bddForm = guardedValue.bddForm.Or(pred);
					return;
				}
			}
#endif
			this.values.Add(new GuardedValue<T>(pred, val));
		}
#if DEBUG_VS
		this.AssertPredExclusion();
#endif
		//MergeMax();
	}

	public void AddValue(T val)
	{
		AddValue(PathConstraint.GetPC(), val);
	}
	
		
	public ValueSummary<T> LimitCombine(bdd limit0, ValueSummary<T> a1, bdd limit1)
	{
		var ret = new ValueSummary<T>();
		foreach(var v in this.values)
		{
			var bddForm = v.bddForm.And(limit0);
			if(bddForm.FormulaBDDSAT()) {
				ret.values.Add(new GuardedValue<T>(bddForm, v.value));
			}
		}
		foreach(var v in a1.values)
		{
			var bddForm = v.bddForm.And(limit1);
			if(bddForm.FormulaBDDSAT()) {
				ret.values.Add(new GuardedValue<T>(bddForm, v.value));
			}
		}
		ret.MergeMax();
		return ret;
	}
	
	private static void DeepMergePVal(GuardedValue<T> into, GuardedValue<T> from)
	{
		var newVal = (T) into.value.GetType().GetMethod("MemberwiseClone", BindingFlags.NonPublic|BindingFlags.Instance).Invoke(into.value, null);
		foreach(FieldInfo prop in from.value.GetType().GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance))
		{
			var intoFieldVal = prop.GetValue(into.value);
		    var fromFieldVal = prop.GetValue(from.value);
		    var limitCombine = intoFieldVal.GetType().GetMethod("LimitCombine");
	    	var combined = limitCombine.Invoke(intoFieldVal, new object[]{into.bddForm, fromFieldVal, from.bddForm} );
	    	prop.SetValue(newVal, combined);
		}
		into.bddForm = into.bddForm.Or(from.bddForm);
		into.value = newVal;
	}
	
	public void MergeMax()
	{
		if(typeof(IPType).IsAssignableFrom(typeof(T)) 
			|| typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition().IsEquivalentTo(typeof(DefaultArray<>))
			|| typeof(T).IsEquivalentTo(typeof(SendQueueItem))
			|| typeof(T).IsEquivalentTo(typeof(Scheduler.SchedulerChoice))) {
			var newVals = new SCG.List<GuardedValue<T>>();
			foreach(var sameTypeGroup in this.values.GroupBy((arg) => arg.value == null ? null : arg.value.GetType()))
			{
				if(sameTypeGroup.Key != null) {
					if((sameTypeGroup.Key.Name.StartsWith("PList") || sameTypeGroup.Key.Name.StartsWith("PTuple") || sameTypeGroup.Key.Name.StartsWith("PMap") || sameTypeGroup.Key.Name.StartsWith("SendQueueItem") || sameTypeGroup.Key.Name.StartsWith("SchedulerChoice")))
					{
						var enumerator = sameTypeGroup.GetEnumerator();
						enumerator.MoveNext();
						var merged = enumerator.Current;
						while(enumerator.MoveNext()) 
						{
							var curr = enumerator.Current;
							DeepMergePVal(merged, curr);
						}
						newVals.Add(merged);
						continue;
					} else if(sameTypeGroup.Key.Name.StartsWith("DefaultArray")) {
	 					dynamic newArray = sameTypeGroup.First().value.GetType().GetMethod("NewEmpty", BindingFlags.Public|BindingFlags.Instance).Invoke(sameTypeGroup.First().value, null);
						bdd limit = bdd.bddfalse;
						foreach(GuardedValue<T> guardedVal in sameTypeGroup)
	 					{
	 						dynamic innerdata = guardedVal.value.GetType().GetField("data", BindingFlags.NonPublic|BindingFlags.Instance).GetValue(guardedVal.value);
	 						int counter = 0;
	 						foreach(dynamic x in innerdata)
	 						{
	 							newArray[counter] = newArray[counter].LimitCombine(limit, x, guardedVal.bddForm);
	 							counter++;
							}
							limit = limit.Or(guardedVal.bddForm);
						}
						newVals.Add(new GuardedValue<T>(limit, newArray));
						continue;
					} else {
						newVals.AddRange(sameTypeGroup.GroupBy((arg) => arg.value).Select((arg) => {
							var pred = bdd.bddfalse;
							foreach(var guardedVal in arg)
							{
								pred = pred.Or(guardedVal.bddForm);
							}
							return new GuardedValue<T>(pred, arg.Key);
						}));
					}
				} else {
					var pred = bdd.bddfalse;
					foreach(var guardedVal in sameTypeGroup)
					{
						pred = pred.Or(guardedVal.bddForm);
					}
					newVals.Add(new GuardedValue<T>(pred, sameTypeGroup.First().value));
				}
			}
			this.values = newVals;
		} else {
			this.values = new SCG.List<GuardedValue<T>>(
				this.values.GroupBy((arg) => arg.value).Select((arg) => {
					var pred = bdd.bddfalse;
					foreach(var guardedVal in arg)
					{
						pred = pred.Or(guardedVal.bddForm);
					}
					return new GuardedValue<T>(pred, arg.Key);
				})
			);
		}
	}
	
	public void Update<T2>(ValueSummary<T2> val, bdd pred) where T2 : T
	{
		foreach (var guardedVal in val.values) {
			var p = guardedVal.bddForm.And(pred);
			Update(guardedVal.value, p);
		}
#if MERGE_PVAL
		MergeMax();
#endif
#if DEBUG_VS
		this.AssertPredExclusion();
#endif
	}

	public void Update<T2>(T2 val, bdd pred) where T2 : T
	{
		if (pred.FormulaBDDSAT()) {
			var newVals = new SCG.List<GuardedValue<T>>();
			var not_pred = pred.Not();
			foreach (var guardedThisVal in this.values) {
				var bddForm = guardedThisVal.bddForm.And(not_pred);
				if (bddForm.FormulaBDDSAT()) {
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
			if (pred.FormulaBDDSolverSAT()) {
				BDDToZ3Wrap.PInvoke.debug_print_used_bdd_vars();
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
			if (bddForm.FormulaBDDSAT()) {
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
			if (bddForm.FormulaBDDSAT()) {
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
			if (bddForm.FormulaBDDSAT()) {
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
		InvokeMethodHelper(t => {
				try {
					(t as Delegate).DynamicInvoke(args);
				} catch (TargetInvocationException ex) {
					throw ex.InnerException;
				}
			}
		);
	}

	public ValueSummary<R> Invoke<R>(params object[] args)
	{
		return InvokeMethodHelper<R>(t => {
				try {
					return (R)((t as Delegate).DynamicInvoke(args));
				} catch (TargetInvocationException ex) {
					throw ex.InnerException;
				}
			}
		);
	}

	#region invokemethod
	public ValueSummary<R> InvokeMethodHelper<R>(Func<T, ValueSummary<R>> f)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var guardedTarget in this.values) {
			bdd newPC = pc.And(guardedTarget.bddForm);
			if (newPC.FormulaBDDSolverSAT()) {
				NullTargetCheck((c, v) => 
				{
					PathConstraint.AddAxiom(c);
					ret.Merge(f.Invoke(v));
					PathConstraint.RestorePC(pc); 
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
			bdd newPC = pc.And(guardedTarget.bddForm);
			if (newPC.FormulaBDDSolverSAT()) {
				NullTargetCheck((c, v) => 
				{
					PathConstraint.AddAxiom(c);
					f.Invoke(v);
					PathConstraint.RestorePC(pc); 
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
				if (bddForm.FormulaBDDSAT()) {
					try {
						ret.AddValue(bddForm, f.Invoke(guardedVals.value, otherGuardedVals.value));
					} catch(DivideByZeroException) {
						if(bddForm.FormulaBDDSolverSAT()) {
							throw new Exception("Divide by zero");
						}
					}
				}
			}
		}
#if DEBUG_VS
		ret.AssertPredExclusion();
#endif
		return ret;
	}

	public ValueSummary<R> InvokeUnary<R>(Func<T, R> f)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();	
		foreach (var guardedVal in this.values) {
			var bddForm = guardedVal.bddForm.And(pc);
			if (bddForm.FormulaBDDSAT()) {
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
			try {
				ret.AddValue(bddForm, f.Invoke(guardedVal.value));
			}
			catch (InvalidCastException e) {
				if (bddForm.FormulaBDDSolverSAT()) {
					throw e;
				}
			} catch(NullReferenceException e) {
				if (bddForm.FormulaBDDSolverSAT()) {
					throw e;
				}
			}
		}
		return ret;
	}

	public override string ToString()
	{
		return "[VS]{" + values.Select((gv) => {
				if(gv.value is Delegate) {
					return (gv.value as Delegate).Method.Name;
				} else {
					return String.Format("{0}", gv.value);
				}
			}).Aggregate("", (a, v) => a + v + ",")
			+ "}";
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
			if (bddForm.FormulaBDDSolverSAT()) {
				bddForm.PrintDot();
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
			if (bddForm.FormulaBDDSolverSAT()) {
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
			if (bddForm.FormulaBDDSAT()) {
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
			if (bddForm.FormulaBDDSAT()) {
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
		var pc = PathConstraint.GetPC();
		foreach (var g2 in idx2.values) {
			foreach (var g1 in idx1.values) {
				var bddForm = g2.bddForm.And(g1.bddForm.And(pc));
				if (bddForm.FormulaBDDSAT()) {
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
				if (bddForm.FormulaBDDSAT()) {
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

	public static ValueSummary<R> GetIndex<R>(this ValueSummary<DefaultArray<ValueSummary<R>>> vs_array, ValueSummary<int> index)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var guardedArray in vs_array.values) {
			foreach (var guardedIndex in index.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And(pc));
				if (bddForm.FormulaBDDSAT()) {
					ret.Merge(guardedArray.value[guardedIndex.value], bddForm);
				}
			}
		}
		return ret;
	}

	public static ValueSummary<R> GetIndex<R>(this ValueSummary<DefaultArray<ValueSummary<R>>> vs_array, ValueSummary<SymbolicInteger> index)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var guardedArray in vs_array.values) {
			foreach (var guardedIndex in index.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And(pc));
				if (bddForm.FormulaBDDSAT()) {
					if (guardedIndex.value.IsAbstract()) {
						foreach (var t in PathConstraint.GetAllPossibleValues(guardedIndex.value.AbstractValue)) {
							ret.Merge(guardedArray.value[t.Item2], bddForm.And(t.Item1));
						}
					}
					else {
						ret.Merge(guardedArray.value[guardedIndex.value.ConcreteValue], bddForm);
					}
				}
			}
		}
		return ret;
	}

	public static ValueSummary<R> GetIndex<R>(this ValueSummary<DefaultArray<ValueSummary<R>>> vs_array, ValueSummary<PInteger> index)
	{
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		foreach (var guardedIndex in index.values) {
			foreach (var guardedArray in vs_array.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And(pc));
				if (bddForm.FormulaBDDSAT()) {
					if (guardedIndex.value.value.IsAbstract()) {
						foreach (var t in PathConstraint.GetAllPossibleValues(guardedIndex.value.value.AbstractValue)) {
							ret.Merge(guardedArray.value[t.Item2], bddForm.And(t.Item1));
						}
					}
					else {
						ret.Merge(guardedArray.value[guardedIndex.value.value.ConcreteValue], bddForm);
					}
				}
			}
		}
		return ret;
	}

	public static void SetIndex<R>(this ValueSummary<DefaultArray<ValueSummary<R>>> vs_array, ValueSummary<int> index, ValueSummary<R> val)
	{
		var pc = PathConstraint.GetPC();
		foreach (var guardedIndex in index.values) {
			foreach (var guardedArray in vs_array.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And(pc));
				if (bddForm.FormulaBDDSAT()) {
					guardedArray.value[guardedIndex.value].Update(val, bddForm);
				}
			}
		}
	}

	public static void SetIndex<R>(this ValueSummary<DefaultArray<ValueSummary<R>>> vs_array, ValueSummary<SymbolicInteger> index, ValueSummary<R> val)
	{
		var pc = PathConstraint.GetPC();
		foreach (var guardedIndex in index.values) {
			foreach (var guardedArray in vs_array.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And(pc));
				if (bddForm.FormulaBDDSAT()) {
					if (guardedIndex.value.IsAbstract()) {
						foreach (Tuple<bdd, int> t in PathConstraint.GetAllPossibleValues(guardedIndex.value.AbstractValue)) {
							guardedArray.value[t.Item2].Update(val, bddForm.And(t.Item1));
						}
					}
					else {
							guardedArray.value[guardedIndex.value.ConcreteValue].Update(val, bddForm);
					}
				}
			}
		}
	}

	public static void SetIndex<R>(this ValueSummary<DefaultArray<ValueSummary<R>>> vs_array, ValueSummary<PInteger> index, ValueSummary<R> val)
	{
		foreach (var guardedIndex in index.values) {
			foreach (var guardedArray in vs_array.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And(PathConstraint.GetPC()));
				if (bddForm.FormulaBDDSAT()) {
					if (guardedIndex.value.value.IsAbstract()) {
						foreach (Tuple<bdd, int> t in PathConstraint.GetAllPossibleValues(guardedIndex.value.value.AbstractValue)) {
							guardedArray.value[t.Item2].Update(val, bddForm.And(t.Item1));
						}
					}
					else {
						guardedArray.value[guardedIndex.value.value.ConcreteValue].Update(val, bddForm);
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
		var pc = PathConstraint.GetPC();
		var trueFeasible = trueBDD.FormulaBDDSolverSAT();
		var falseFeasible = falseBDD.FormulaBDDSolverSAT();
#if DEBUG_VS
		if (trueFeasible && falseFeasible) {
			var a0 = trueBDD.ToZ3Expr();
			var a1 = falseBDD.ToZ3Expr();
			PathConstraint.solver.Push();
			PathConstraint.solver.Assert(PathConstraint.ctx.MkAnd(a0, a1));
			var s = PathConstraint.solver.Check();
			PathConstraint.solver.Pop();
			if (s != Status.UNSATISFIABLE) {
				trueBDD.PrintDot();
				falseBDD.PrintDot();
				BDDToZ3Wrap.PInvoke.debug_print_used_bdd_vars();
				Console.WriteLine(a0);
				Debugger.Break();
			}
		}
#endif
		if (trueFeasible) {
			if (falseFeasible) {
				return new PathConstraint.BranchPoint(trueBDD, falseBDD, PathConstraint.BranchPoint.State.Both, pc);
			}
			else {
				return new PathConstraint.BranchPoint(trueBDD, null, PathConstraint.BranchPoint.State.True, pc);
			}
		}
		else {
			if (falseFeasible) {
				return new PathConstraint.BranchPoint(null, falseBDD, PathConstraint.BranchPoint.State.False, pc);
			}
			else {
				if (!PathConstraint.EvalPc())
				{
					return new PathConstraint.BranchPoint(null, null, PathConstraint.BranchPoint.State.None, pc);	
				}
				else 
				{
					throw new Exception("Not reachable");
				}
			}
		}
	}

#if NO_MERGE_VS
	public static PathConstraint.BranchPoint _Cond(this ValueSummary<bool> b)
	{
		return _Cond(b.Cast<SymbolicBool>((arg) => (SymbolicBool)arg));
	}
#else
	public static PathConstraint.BranchPoint _Cond(this ValueSummary<bool> b)
	{
		var pc = PathConstraint.GetPC();
		if (b.values.Count > 2) 
		{ 
			Debugger.Break();
		}
		if (b.values.Count == 1) {
			var bddForm = b.values[0].bddForm.And(pc);
#if DEBUG_VS
			if (!bddForm.FormulaBDDSolverSAT()) 
			{
				Debugger.Break();
			}
#endif
			if (b.values[0].value) {

				return new PathConstraint.BranchPoint(bddForm, null, PathConstraint.BranchPoint.State.True, pc);
			}
			else {
				return new PathConstraint.BranchPoint(null, bddForm, PathConstraint.BranchPoint.State.False, pc);
			}
		}
		else if (b.values.Count == 2) {
			if (b.values[0].value) {
				return CondConcreteHelper(b.values[0].bddForm.And(pc), b.values[1].bddForm.And(pc));
			}
			else {
				return CondConcreteHelper(b.values[1].bddForm.And(pc), b.values[0].bddForm.And(pc));
			}
		}
		else {
				if (!PathConstraint.EvalPc())
				{
					return new PathConstraint.BranchPoint(null, null, PathConstraint.BranchPoint.State.None, pc);	
				}
				else 
				{
					throw new Exception("Not reachable");
				}
		}
	}
#endif

	public static PathConstraint.BranchPoint Cond(this ValueSummary<bool> b) 
	{
		var ret = _Cond(b);
		return ret;
 	}
		
	private static PathConstraint.BranchPoint _CondHelper<T>(ValueSummary<T> b, Func<T, SymbolicBool> extract)
	{
		var pc = PathConstraint.GetPC();
		bdd predTrue = bdd.bddfalse, predFalse = bdd.bddfalse;
		foreach (var guardedBooleanVal in b.values) {
			var guardedValuePcPred = guardedBooleanVal.bddForm.And(pc);
			var symbVal = extract.Invoke(guardedBooleanVal.value);
			if (guardedValuePcPred.FormulaBDDSAT()) {
				if (symbVal.IsAbstract()) {
					var c = symbVal.AbstractValue;
					if (c.EqualEqual(bdd.bddtrue)) {
						if (guardedValuePcPred.FormulaBDDSolverSAT()) {
							predTrue = predTrue.Or(guardedValuePcPred);
						}
					}
					else if (c.EqualEqual(bdd.bddfalse)) {
						if (guardedValuePcPred.FormulaBDDSolverSAT()) {
							predFalse = predFalse.Or(guardedValuePcPred);
						}
					}
					else {
						var tmp = guardedValuePcPred.And(c);
						var trueFeasible = tmp.FormulaBDDSolverSAT();
						if (trueFeasible) {
							predTrue = predTrue.Or(tmp);
						}
						tmp = guardedValuePcPred.And(c.Not());
						var falseFeasible = tmp.FormulaBDDSolverSAT();
						if (falseFeasible) {
							predFalse = predFalse.Or(tmp);
						}
					}
				}
				else {
					if (guardedValuePcPred.FormulaBDDSolverSAT()) {
						if (symbVal.ConcreteValue) {
							predTrue = predTrue.Or(guardedValuePcPred);
						}
						else {
							predFalse = predFalse.Or(guardedValuePcPred);
						}
					}
				}
			}
		}
		if (predTrue != bdd.bddfalse) {
			if (predFalse != bdd.bddfalse) {
				return new PathConstraint.BranchPoint(predTrue, predFalse, PathConstraint.BranchPoint.State.Both, pc);
			}
			else {
				return new PathConstraint.BranchPoint(predTrue, null, PathConstraint.BranchPoint.State.True, pc);
			}
		}
		else {
			if (predFalse != bdd.bddfalse) {
				return new PathConstraint.BranchPoint(null, predFalse, PathConstraint.BranchPoint.State.False, pc);
			}
			else {
				if (!PathConstraint.EvalPc())
				{
					return new PathConstraint.BranchPoint(null, null, PathConstraint.BranchPoint.State.None, pc);	
				}
				else 
				{
					throw new Exception("Not reachable");
				}
			}
		}
	} 
	
	public static PathConstraint.BranchPoint _Cond(this ValueSummary<SymbolicBool> b)
	{
		return _CondHelper(b, (arg) => arg);
	}
	
	public static PathConstraint.BranchPoint Cond(this ValueSummary<SymbolicBool> b) 
	{
		var ret = _Cond(b);
		return ret;
 	}
	
	public static PathConstraint.BranchPoint _Cond(this ValueSummary<PBool> b)
	{
		return _CondHelper(b, (arg) => (SymbolicBool)arg);
	}
	
	public static PathConstraint.BranchPoint Cond(this ValueSummary<PBool> b) 
	{
		var ret = _Cond(b);
		return ret;
 	}


#region DEBUG_VS
	public static void AssertPredExclusion<T>(this ValueSummary<T> x)
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