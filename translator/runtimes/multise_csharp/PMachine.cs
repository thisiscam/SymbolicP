using System;
using System.Collections.Generic;

abstract class PMachine : IPType<PMachine>
{
    protected delegate void TransitionFunction(ValueSummary<IPType> payload);
    protected delegate void ExitFunction();
    protected ValueSummary<int> retcode = new ValueSummary<int>(default (int));
    protected ValueSummary<List<int>> states = ValueSummary<List<int>>.InitializeFrom(new ValueSummary<List<int>>(new List<int>()));
    protected Scheduler scheduler;
    protected bool[, ] DeferedSet;
    protected bool[, ] IsGotoTransition;
    protected TransitionFunction[, ] Transitions;
    protected ExitFunction[] ExitFunctions;
    public List<SendQueueItem> sendQueue = new List<SendQueueItem>();
    public virtual void StartMachine(ValueSummary<Scheduler> s, ValueSummary<IPType> payload)
    {
        this.scheduler = s;
    }

    /* Returns the index that can serve this event */
    public ValueSummary<int> CanServeEvent(ValueSummary<PInteger> e)
    {
        PathConstraint.PushFrame();
        var vs_ret_1 = new ValueSummary<int>();
        {
            PathConstraint.BeginLoop();
            for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this.states.GetField<int>(_ => _.Count)).Loop(); i.Increment())
            {
                ValueSummary<bool> vs_lgc_tmp_0;
                ValueSummary<int> state = ValueSummary<int>.InitializeFrom(this.states.InvokeMethod<int, int>((_, a0) => _[a0], i));
                {
                    var vs_cond_26 = ((new Func<ValueSummary<bool>>(() =>
                    {
                        var vs_cond_0 = ((vs_lgc_tmp_0 = ValueSummary<bool>.InitializeFrom(this.DeferedSet.GetIndex(state, e).InvokeUnary<bool>(_ => !_)))).Cond();
                        var vs_cond_ret_0 = new ValueSummary<bool>();
                        if (vs_cond_0.CondTrue())
                            vs_cond_ret_0.Merge(vs_lgc_tmp_0.InvokeBinary<bool, bool>((l, r) => l & r, this.Transitions.GetIndex(state, e).InvokeBinary<PMachine.TransitionFunction, bool>((l, r) => l != r, new ValueSummary<PMachine.TransitionFunction>(null))));
                        if (vs_cond_0.CondFalse())
                            vs_cond_ret_0.Merge(vs_lgc_tmp_0);
                        vs_cond_0.MergeBranch();
                        return vs_cond_ret_0;
                    }

                    )())).Cond();
                    if (vs_cond_26.CondTrue())
                    {
                        vs_ret_1.RecordReturn(i);
                    }

                    vs_cond_26.MergeBranch();
                }
            }
        }

        if (PathConstraint.MergedPcFeasible())
        {
            vs_ret_1.RecordReturn(-1);
        }

        PathConstraint.PopFrame();
        return vs_ret_1;
    }

    protected void RaiseEvent(ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        PathConstraint.PushFrame();
        {
            PathConstraint.BeginLoop();
            for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this.states.GetField<int>(_ => _.Count)).Loop(); i.Increment())
            {
                ValueSummary<int> state = ValueSummary<int>.InitializeFrom(this.states.InvokeMethod<int, int>((_, a0) => _[a0], i));
                {
                    var vs_cond_27 = (this.Transitions.GetIndex(state, e).InvokeBinary<PMachine.TransitionFunction, bool>((l, r) => l != r, new ValueSummary<PMachine.TransitionFunction>(null))).Cond();
                    if (vs_cond_27.CondTrue())
                    {
                        this.RunStateMachine(i, e, payload);
                        PathConstraint.RecordReturnPath();
                    }

                    vs_cond_27.MergeBranch();
                }
            }
        }

        if (PathConstraint.MergedPcFeasible())
        {
            throw new SystemException("Unhandled event");
        }

        PathConstraint.PopFrame();
    }

    protected void SendMsg(ValueSummary<PMachine> other, ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        this.scheduler.SendMsg(this, other, e, payload);
    }

    protected ValueSummary<PMachine> NewMachine(ValueSummary<PMachine> newMachine, ValueSummary<IPType> payload)
    {
        this.scheduler.NewMachine(this, newMachine, payload);
        return newMachine;
    }

    protected void PopState()
    {
        this.retcode.Assign<int>(Constants.EXECUTE_FINISHED);
        ValueSummary<int> current_state = ValueSummary<int>.InitializeFrom(this.states.InvokeMethod<int, int>((_, a0) => _[a0], 0));
        this.states.InvokeMethod<int>((_, a0) => _.RemoveAt(a0), 0);
        {
            var vs_cond_28 = (this.ExitFunctions.GetIndex(current_state).InvokeBinary<PMachine.ExitFunction, bool>((l, r) => l != r, new ValueSummary<PMachine.ExitFunction>(null))).Cond();
            if (vs_cond_28.CondTrue())
            {
                this.ExitFunctions.GetIndex(current_state).Invoke();
            }

            vs_cond_28.MergeBranch();
        }
    }

    protected ValueSummary<PBool> RandomBool()
    {
        return this.scheduler.RandomBool().Cast<PBool>(_ => (PBool)_);
    }

    protected void Assert(ValueSummary<PBool> cond, ValueSummary<string> msg)
    {
        {
            var vs_cond_29 = (cond.InvokeUnary<PBool>(_ => !_)).Cond();
            if (vs_cond_29.CondTrue())
            {
                throw new SystemException(msg);
            }

            vs_cond_29.MergeBranch();
        }
    }

    protected void Assert(ValueSummary<PBool> cond)
    {
        {
            var vs_cond_30 = (cond.InvokeUnary<PBool>(_ => !_)).Cond();
            if (vs_cond_30.CondTrue())
            {
                throw new SystemException("Assertion failure");
            }

            vs_cond_30.MergeBranch();
        }
    }

    public void RunStateMachine(ValueSummary<int> state_idx, ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        ValueSummary<int> state = ValueSummary<int>.InitializeFrom(this.states.InvokeMethod<int, int>((_, a0) => _[a0], state_idx));
        {
            var vs_cond_31 = (this.IsGotoTransition.GetIndex(state, e)).Cond();
            if (vs_cond_31.CondTrue())
            {
                this.states.InvokeMethod<int, int>((_, a0, a1) => _.RemoveRange(a0, a1), 0, state_idx);
            }

            vs_cond_31.MergeBranch();
        }

        this.retcode.Assign<int>(Constants.EXECUTE_FINISHED);
        ValueSummary<TransitionFunction> transition_fn = ValueSummary<TransitionFunction>.InitializeFrom(this.Transitions.GetIndex(state, e));
        transition_fn.Invoke(payload);
    }

    protected void Transition_Ignore(ValueSummary<IPType> payload)
    {
        return;
    }

    public ValueSummary<PMachine> DeepCopy()
    {
        return this;
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode()
    {
        return new ValueSummary<SymbolicInteger>(new SymbolicInteger(this.GetHashCode()));
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PMachine> other)
    {
        return other.InvokeBinary<object, SymbolicBool>((l, r) => l == r, this);
    }
}