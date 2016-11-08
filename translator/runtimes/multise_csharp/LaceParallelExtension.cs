using System;
using System.Diagnostics;

#if USE_SYLVAN
using SylvanSharp;
#endif

public partial class ValueSummary<T>
{
	public ValueSummary<R> InvokeMethodParallelHelper<R>(Func<T, ValueSummary<R>> f)
	{
#if USE_SYLVAN
		var ret = new ValueSummary<R>();
		var pc = PathConstraint.GetPC();
		SylvanSharp.Lace.ParallelFor((i) => {
			PathConstraint.ForceSetPC(pc);
			var guardedTarget = this.values[i];
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
		}, this.values.Count);
		if(!PathConstraint.GetPC().EqualEqual(pc)) {
			Debugger.Break();
		}
		return ret;
#else
		return InvokeMethodHelper(f);
#endif
	}
	
	public void InvokeMethodParallelHelper(Action<T> f)
	{
#if USE_SYLVAN
		var pc = PathConstraint.GetPC();

		SylvanSharp.Lace.ParallelFor((i) => {
			PathConstraint.ForceSetPC(pc);
			var guardedTarget = this.values[i];
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
		}, this.values.Count);
		if(!PathConstraint.GetPC().EqualEqual(pc)) {
			Debugger.Break();
		}
#else
		InvokeMethodHelper(f);
#endif
	}

	public ValueSummary<R> InvokeMethodParallel<R>(Func<T, ValueSummary<R>> f)
	{
		return InvokeMethodParallelHelper((v) => f.Invoke(v));
	}

	public ValueSummary<R> InvokeMethodParallel<A1, R>(Func<T, ValueSummary<A1>, ValueSummary<R>> f, ValueSummary<A1> arg1)
	{
		return InvokeMethodParallelHelper((v) => f.Invoke(v, arg1));
	}

	public ValueSummary<R> InvokeMethodParallel<A1, A2, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>,
														ValueSummary<R>> f,
														ValueSummary<A1> arg1,
														ValueSummary<A2> arg2)
	{
		return InvokeMethodParallelHelper((v) => f.Invoke(v, arg1, arg2));
	}

	public ValueSummary<R> InvokeMethodParallel<A1, A2, A3, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>, ValueSummary<R>> f,
														ValueSummary<A1> arg1,
														ValueSummary<A2> arg2,
														ValueSummary<A3> arg3)
	{
		return InvokeMethodParallelHelper((v) => f.Invoke(v, arg1, arg2, arg3));
	}

	public ValueSummary<R> InvokeMethodParallel<A1, A2, A3, A4, R>(Func<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>, ValueSummary<A4>, ValueSummary<R>> f,
														ValueSummary<A1> arg1,
														ValueSummary<A2> arg2,
														ValueSummary<A3> arg3,
														ValueSummary<A4> arg4)
	{
		return InvokeMethodParallelHelper((v) => f.Invoke(v, arg1, arg2, arg3, arg4));
	}

	public void InvokeMethodParallel(Action<T> f)
	{
		InvokeMethodParallelHelper((v) => f.Invoke(v));
	}

	public void InvokeMethodParallel<A1>(Action<T, ValueSummary<A1>> f, ValueSummary<A1> arg1)
	{
		InvokeMethodParallelHelper((v) => f.Invoke(v, arg1));
	}

	public void InvokeMethodParallel<A1, A2>(Action<T, ValueSummary<A1>, ValueSummary<A2>> f,
														ValueSummary<A1> arg1,
														ValueSummary<A2> arg2)
	{
		InvokeMethodParallelHelper((v) => f.Invoke(v, arg1, arg2));
	}

	public void InvokeMethodParallel<A1, A2, A3>(Action<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>> f,
														ValueSummary<A1> arg1,
														ValueSummary<A2> arg2,
														ValueSummary<A3> arg3)
	{
		InvokeMethodParallelHelper((v) => f.Invoke(v, arg1, arg2, arg3));
	}

	public void InvokeMethodParallel<A1, A2, A3, A4>(Action<T, ValueSummary<A1>, ValueSummary<A2>, ValueSummary<A3>, ValueSummary<A4>> f,
														ValueSummary<A1> arg1,
														ValueSummary<A2> arg2,
														ValueSummary<A3> arg3,
														ValueSummary<A4> arg4)
	{
		InvokeMethodParallelHelper((v) => f.Invoke(v, arg1, arg2, arg3, arg4));
	}
}
