using System;
using System.Collections.Generic;

abstract class MonitorPMachine
{
    protected delegate void TransitionFunction(ValueSummary<IPType> payload);
    protected delegate void ExitFunction();
    protected ValueSummary<int> retcode = new ValueSummary<int>(default (int));
    protected ValueSummary<List<int>> states = new ValueSummary<List<int>>(new List<int>());
    protected bool[, ] DeferedSet;
    protected bool[, ] IsGotoTransition;
    protected TransitionFunction[, ] Transitions;
    protected ExitFunction[] ExitFunctions;
    
#if USE_SYLVAN
    private IntPtr _mutex = SylvanSharp.Lace.CreateMutex();
#endif

    public void ServeEvent(ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
#if USE_SYLVAN
		SylvanSharp.Lace.LockRegion(_mutex, () =>
#endif 
		{
	        var _frame_pc = PathConstraint.GetPC();
	        var vs_cond_9 = PathConstraint.BeginLoop();
	        for (ValueSummary<int> i = 0; vs_cond_9.Loop(i.InvokeBinary<int, bool>((l, r) => l < r, this.states.GetField<int>(_ => _.Count))); i.Increment())
	        {
	            ValueSummary<bool> vs_lgc_tmp_0;
	            ValueSummary<int> state = this.states.InvokeMethod<int, int>((_, a0) => _[a0], i);
	            var vs_cond_8 = ((new Func<ValueSummary<bool>>(() =>
	            {
	                var vs_cond_39 = ((vs_lgc_tmp_0 = ValueSummary<bool>.InitializeFrom(this.DeferedSet.GetIndex(state, e).InvokeUnary<bool>(_ => !_)))).Cond();
	                var vs_cond_ret_39 = new ValueSummary<bool>();
	                if (vs_cond_39.CondTrue())
	                    vs_cond_ret_39.Merge(vs_lgc_tmp_0.InvokeBinary<bool, bool>((l, r) => l & r, this.Transitions.GetIndex(state, e).InvokeBinary<MonitorPMachine.TransitionFunction, bool>((l, r) => l != r, new ValueSummary<MonitorPMachine.TransitionFunction>(null))));
	                if (vs_cond_39.CondFalse())
	                    vs_cond_ret_39.Merge(vs_lgc_tmp_0);
	                vs_cond_39.MergeBranch();
	                return vs_cond_ret_39;
	            }
	
	            )())).Cond();
	            {
	                if (vs_cond_8.CondTrue())
	                {
	                    var vs_cond_7 = (this.IsGotoTransition.GetIndex(state, e)).Cond();
	                    {
	                        if (vs_cond_7.CondTrue())
	                        {
	                            this.states.InvokeMethod<int, int>((_, a0, a1) => _.RemoveRange(a0, a1), 0, i);
	                        }
	                    }
	
	                    vs_cond_7.MergeBranch();
	                    this.retcode.Assign<int>(Constants.EXECUTE_FINISHED);
	                    ValueSummary<TransitionFunction> transition_fn = this.Transitions.GetIndex(state, e);
	                    transition_fn.Invoke(payload);
	                    PathConstraint.RecordReturnPath(vs_cond_8);
	                }
	            }
	
	            vs_cond_8.MergeBranch(vs_cond_9);
	        }
	
	        if (vs_cond_9.MergeBranch())
	        {
	            throw new SystemException("Unhandled event");
	        }
	
	        PathConstraint.RestorePC(_frame_pc);
	   	}
#if USE_SYLVAN
		);
#endif
    }

    protected void RaiseEvent(ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        this.ServeEvent(e, payload);
    }

    protected void PopState()
    {
        this.retcode.Assign<int>(Constants.EXECUTE_FINISHED);
        ValueSummary<int> current_state = this.states.InvokeMethod<int, int>((_, a0) => _[a0], 0);
        this.states.InvokeMethod<SymbolicInteger>((_, a0) => _.RemoveAt(a0), (SymbolicInteger)0);
        var vs_cond_10 = (this.ExitFunctions.GetIndex(current_state).InvokeBinary<MonitorPMachine.ExitFunction, bool>((l, r) => l != r, new ValueSummary<MonitorPMachine.ExitFunction>(null))).Cond();
        {
            if (vs_cond_10.CondTrue())
            {
                this.ExitFunctions.GetIndex(current_state).Invoke();
            }
        }

        vs_cond_10.MergeBranch();
    }

    protected void Transition_Ignore(ValueSummary<IPType> payload)
    {
        return;
    }

    protected void Assert(ValueSummary<PBool> cond, ValueSummary<string> msg)
    {
        var vs_cond_11 = (cond.InvokeUnary<PBool>(_ => !_)).Cond();
        {
            if (vs_cond_11.CondTrue())
            {
                throw new SystemException(msg);
            }
        }

        vs_cond_11.MergeBranch();
    }

    protected void Assert(ValueSummary<PBool> cond)
    {
        var vs_cond_12 = (cond.InvokeUnary<PBool>(_ => !_)).Cond();
        {
            if (vs_cond_12.CondTrue())
            {
                throw new SystemException("Assertion failure");
            }
        }

        vs_cond_12.MergeBranch();
    }
}