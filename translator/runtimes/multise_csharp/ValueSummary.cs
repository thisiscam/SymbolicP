using System;
using Microsoft.Z3;
using BuDDySharp;
using SCG = System.Collections.Generic;
using System.Diagnostics;
using BDDToZ3Wrap;

internal class GuardedValue<T> {
	public bdd bddForm;
	public T value;

	public GuardedValue(bdd bddForm, T value) {
		this.bddForm = bddForm;
		this.value = value;
	}

	public GuardedValue(GuardedValue<T> other) {
		this.bddForm = other.bddForm;
		this.value = other.value;
	}
}

public class ValueSummary<T> {

	internal SCG.List<GuardedValue<T>> values = new SCG.List<GuardedValue<T>> ();

	#region constructor
	public ValueSummary() { }

	public ValueSummary(T t) {
		this.values.Add (new GuardedValue<T> (PathConstraint.GetPC (), t));
	}

	public static implicit operator T(ValueSummary<T> vs)
	{
		Debug.Assert (vs.values.Count == 1);
		return vs.values [0].value;
	}  

	public static ValueSummary<T> InitializeFrom(ValueSummary<T> t) {
		var ret = new ValueSummary<T>();
		var pc = PathConstraint.GetPC ();
		ret.values = new SCG.List<GuardedValue<T>> ();
		foreach (var val in t.values) {
			var bddForm = val.bddForm.And (pc);
			if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
				ret.values.Add (new GuardedValue<T> (bddForm, val.value));
			}
		}
		return ret;
	}

	public static ValueSummary<ValueSummary<T>[]> NewVSArray(ValueSummary<int> size) 
	{
		var pc = PathConstraint.GetPC ();
		var ret = new ValueSummary<ValueSummary<T>[]> ();
		foreach (var guardedSize in size.values) {
			var bddForm = pc.And (guardedSize.bddForm);
			if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
				var array = new ValueSummary<T>[guardedSize.value];
				for (int i = 0; i < guardedSize.value; i++) {
					array [i] = new ValueSummary<T> (default(T));
				}
				ret.values.Add (new GuardedValue<ValueSummary<T>[]>(bddForm, array));
			}
		}
		return ret;
	}

	public static implicit operator ValueSummary<T>(T t) 
	{ 
		return new ValueSummary<T> (t);
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

	public void Update<T2>(ValueSummary<T2> val, bdd pred) where T2: T
	{
		foreach (var guardedVal in val.values) {
			var p = guardedVal.bddForm.And (pred);
			Update (guardedVal.value, p);
		}
	}

	public void Update<T2>(T2 val, bdd pred) where T2 : T
	{
		if (!pred.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
			var newVals = new SCG.List<GuardedValue<T>> ();
			foreach (var guardedThisVal in this.values) {
				guardedThisVal.bddForm = guardedThisVal.bddForm.And (pred.Not());
				if (!guardedThisVal.bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
					newVals.Add (new GuardedValue<T>(guardedThisVal));
				}
			}
			this.values = newVals;
			AddValue (pred, val);
		}
	}
	#endregion


	public ValueSummary<R> GetField<R>(Func<T, ValueSummary<R>> f) {
		var ret = new ValueSummary<R> ();
		var pc = PathConstraint.GetPC ();
		foreach (var guardedVal in values) {
			var bddForm = guardedVal.bddForm.And (pc);
			Console.WriteLine (guardedVal.bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddtrue));
			Console.WriteLine (pc.EqualEqual (BuDDySharp.BuDDySharp.bddtrue));
			Console.WriteLine (bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddtrue));
			if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
				ret.Merge (f.Invoke (guardedVal.value), bddForm);
			}
		}
		return ret;
	}

	public void SetField<R>(Func<T, ValueSummary<R>> f, ValueSummary<R> v) {
		var ret = new ValueSummary<R> ();
		var pc = PathConstraint.GetPC ();
		foreach (var guardedVal in values) {
			var bddForm = guardedVal.bddForm.And (pc);
			if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
				f.Invoke (guardedVal.value).Assign (v);
			}
		}
	}

	public ValueSummary<R> GetConstField<R>(Func<T, R> f) {
		var ret = new ValueSummary<R> ();
		var pc = PathConstraint.GetPC ();
		foreach (var guardedVal in values) {
			var bddForm = guardedVal.bddForm.And (pc);
			if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
				ret.AddValue (bddForm, f.Invoke (guardedVal.value));
			}
		}
		return ret;
	}

	public void Assign<T2>(ValueSummary<T2> other) where T2 : T {
		Update (other, PathConstraint.GetPC ());
	}

	public void Invoke(params object[] args) {
		InvokeMethodHelper (t => (t as Delegate).DynamicInvoke (args));
	}

	public ValueSummary<R> Invoke<R>(params object[] args) {
		return InvokeMethodHelper<R> (t => (R)((t as Delegate).DynamicInvoke (args)));
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
		return InvokeMethodHelper ((v) => f.Invoke (v, arg1, arg2, arg3, arg4));
	}

	public void InvokeMethodHelper(Action<T> f){
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
		foreach (var guardedVal in this.values) {
			var bddForm = guardedVal.bddForm.And(PathConstraint.GetPC());
			if(!bddForm.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
				ret.AddValue(bddForm, f.Invoke (guardedVal.value));
			}
		}
		return ret;
	}

	public ValueSummary<R> Cast<R>(Func<T, R> f) {
		return InvokeUnary<R> (f);
	}

	public override string ToString ()
	{
		return "[VS]{" + values.ToString() + "}";
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
						ret.AddValue (bddForm.And(val.Item1), array[val.Item2]);
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
							ret.AddValue (bddForm.And(val.Item1), array[g1.value, val.Item2]);
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
						foreach (var t in PathConstraint.GetAllPossibleValues(guardedIndex.value.AbstractValue)) {
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

	public static ValueSummary<R> GetIndex<R>(this ValueSummary<ValueSummary<R>[]> vs_array, ValueSummary<PInteger> index) {
		var ret = new ValueSummary<R> ();
		foreach(var guardedIndex in index.values) {
			foreach(var guardedArray in vs_array.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And (PathConstraint.GetPC ()));
				if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
					if (guardedIndex.value.value.IsAbstract ()) {
						foreach (var t in PathConstraint.GetAllPossibleValues(guardedIndex.value.value.AbstractValue)) {
							ret.Merge(guardedArray.value [t.Item2], bddForm.And(t.Item1));
						}
					} else {
						ret.Merge (guardedArray.value [guardedIndex.value.value.ConcreteValue], bddForm);
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
					guardedArray.value [guardedIndex.value].Update (val, bddForm);
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
							guardedArray.value [t.Item2].Update (val, bddForm.And(t.Item1));
						}
					} else {
						guardedArray.value [guardedIndex.value.ConcreteValue].Update (val, bddForm);
					}
				}
			}
		}
	}

	public static void SetIndex<R>(this ValueSummary<ValueSummary<R>[]> vs_array, ValueSummary<PInteger> index, ValueSummary<R> val) {
		foreach(var guardedIndex in index.values) {
			foreach(var guardedArray in vs_array.values) {
				var bddForm = guardedArray.bddForm.And(guardedIndex.bddForm.And (PathConstraint.GetPC ()));
				if (!bddForm.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
					if (guardedIndex.value.value.IsAbstract ()) {
						foreach (Tuple<bdd, int> t in PathConstraint.GetAllPossibleValues(guardedIndex.value.value.AbstractValue)) {
							guardedArray.value [t.Item2].Update (val, bddForm.And(t.Item1));
						}
					} else {
						guardedArray.value [guardedIndex.value.value.ConcreteValue].Update (val, bddForm);
					}
				}
			}
		}
	}

	public static ValueSummary<int> Increment(this ValueSummary<int> i) {
		var ret = ValueSummary<int>.InitializeFrom (i);
		foreach (var val in i.values) {
			val.value += 1;
		}
		return ret;
	} 

	public static ValueSummary<SymbolicInteger> Increment(this ValueSummary<SymbolicInteger> i) {
		var ret = ValueSummary<SymbolicInteger>.InitializeFrom (i);
		foreach (var val in i.values) {
			val.value += 1;
		}
		return ret;
	}

	public static ValueSummary<int> Decrement(this ValueSummary<int> i) {
		var ret = ValueSummary<int>.InitializeFrom (i);
		foreach (var val in i.values) {
			val.value -= 1;
		}
		return ret;
	} 

	public static ValueSummary<SymbolicInteger> Decrement(this ValueSummary<SymbolicInteger> i) {
		var ret = ValueSummary<SymbolicInteger>.InitializeFrom (i);
		foreach (var val in ret.values) {
			val.value -= 1;
		}
		return ret;
	}

	private static bool CondConcreteHelper(bdd trueBDD, bdd falseBDD)
	{
		var trueFeasible = !trueBDD.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) && PathConstraint.SolveBooleanExpr (trueBDD.ToZ3Expr ());
		var falseFeasible = !falseBDD.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) && PathConstraint.SolveBooleanExpr(falseBDD.ToZ3Expr());
		if(trueFeasible) {
			if(falseFeasible) {
				PathConstraint.NewBranchPoint(trueBDD, PathConstraint.BranchPoint.State.Both);
				return true;
			} else {
				PathConstraint.NewBranchPoint(trueBDD, PathConstraint.BranchPoint.State.True);
				return true;
			}
		} else {
			if(falseFeasible) {
				PathConstraint.NewBranchPoint(trueBDD, PathConstraint.BranchPoint.State.False);
				return false;
			} else {
				Console.WriteLine("Error, if branch is not reachable in both branches");
				Debugger.Break();
				throw new Exception ("Not reachable");
			}
		}
	}

	public static bool Cond(this ValueSummary<bool> b) {
		if (PathConstraint.IsRecovering()) {
			return PathConstraint.TakeBranch ();
		}
		var pc = PathConstraint.GetPC ();
		Debug.Assert (b.values.Count <= 2);
		if (b.values.Count == 1) {
			PathConstraint.AddAxiom (b.values [0].bddForm.And(pc));
			return b.values [0].value;
		} else {
			if (b.values [0].value) {
				return CondConcreteHelper (b.values [0].bddForm.And(pc), b.values [1].bddForm.And(pc));
			} else {
				return CondConcreteHelper (b.values [1].bddForm.And(pc), b.values [0].bddForm.And(pc));
			}
		}
	}

	public static bool Cond(this ValueSummary<SymbolicBool> b) {
		if (PathConstraint.IsRecovering()) {
			return PathConstraint.TakeBranch ();
		}
		var pc = PathConstraint.GetPC ();
		bdd predTrue = BuDDySharp.BuDDySharp.bddfalse, predFalse = BuDDySharp.BuDDySharp.bddfalse;
		foreach (var guardedBooleanVal in b.values) {
			var guardedValuePcPred = guardedBooleanVal.bddForm.And (pc);
			if (!guardedValuePcPred.EqualEqual (BuDDySharp.BuDDySharp.bddfalse)) {
				if (guardedBooleanVal.value.IsAbstract ()) {
					var c = guardedBooleanVal.value.AbstractValue.ToBDD ();
					if(c.EqualEqual(BuDDySharp.BuDDySharp.bddtrue)) {
						predTrue = predTrue.Or (guardedBooleanVal.bddForm);
					} else if(c.EqualEqual(BuDDySharp.BuDDySharp.bddfalse)) {
						predFalse = predFalse.Or (guardedValuePcPred);
					} else {
						var tmp = guardedBooleanVal.bddForm.And (c);
						var trueFeasible = !tmp.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) && PathConstraint.SolveBooleanExpr (tmp.ToZ3Expr());
						if (trueFeasible) {
							predTrue = predTrue.Or (tmp);
						}
						tmp = tmp.Not();
						var falseFeasible = !tmp.EqualEqual(BuDDySharp.BuDDySharp.bddfalse) && PathConstraint.SolveBooleanExpr (tmp.ToZ3Expr ());
						if (falseFeasible) {
							predFalse = predFalse.Or (tmp);
						}
					}
				} else {
					if (guardedBooleanVal.value.ConcreteValue) {
						predTrue = predTrue.Or (guardedValuePcPred);
					} else {
						predFalse = predFalse.Or (guardedValuePcPred);
					}
				}
			}
		}
		if(predTrue != BuDDySharp.BuDDySharp.bddfalse) {
			if(predFalse != BuDDySharp.BuDDySharp.bddfalse) {
				PathConstraint.NewBranchPoint(predTrue, PathConstraint.BranchPoint.State.Both);
				return true;
			} else {
				PathConstraint.NewBranchPoint(predTrue, PathConstraint.BranchPoint.State.True);
				return true;
			}
		} else {
			if(predFalse != BuDDySharp.BuDDySharp.bddfalse) {
				PathConstraint.NewBranchPoint(predFalse, PathConstraint.BranchPoint.State.False);
				return false;
			} else {
				Console.WriteLine("Error, if branch is not reachable in both branches");
				Debugger.Break();
				throw new Exception ("Not reachable");
			}
		}
	}
	public static bool Cond(this ValueSummary<PBool> b) {
		//TODO make this more efficient
		var tmp = new ValueSummary<SymbolicBool> ();
		foreach (var v in b.values) {
			tmp.values.Add (new GuardedValue<SymbolicBool>(v.bddForm, (SymbolicBool)v.value));
		}
		return Cond (tmp);
	}
}