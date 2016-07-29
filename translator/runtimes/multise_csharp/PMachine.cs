using System;
using System.Collections.Generic;

abstract class PMachine : IPType<PMachine>
{
    protected delegate void TransitionFunction(ValueSummary<IPType> payload);
    protected delegate void ExitFunction();
    protected ValueSummary<int> retcode;
    protected ValueSummary<List<int>> states = new ValueSummary<List<int>>(new List<int>());
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
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this.states.GetField<int>(_ => _.Count)).Cond(); i.Increment())
        {
            ValueSummary<int> state = this.states.InvokeMethod<int, int>((_, a0) => _[a0], i);
            if (this.DeferedSet.GetIndex(state, e).InvokeUnary<bool>(_ => !_).InvokeBinary<bool, bool>((l, r) => l && r, this.Transitions.GetIndex(state, e).InvokeBinary<PMachine.TransitionFunction, bool>((l, r) => l != r, ValueSummary<PMachine.TransitionFunction>.Null)).Cond())
            {
                return i;
            }
        }

        return -1;
    }

    protected void RaiseEvent(ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this.states.GetField<int>(_ => _.Count)).Cond(); i.Increment())
        {
            ValueSummary<int> state = this.states.InvokeMethod<int, int>((_, a0) => _[a0], i);
            if (this.Transitions.GetIndex(state, e).InvokeBinary<PMachine.TransitionFunction, bool>((l, r) => l != r, ValueSummary<PMachine.TransitionFunction>.Null).Cond())
            {
                this.RunStateMachine(i, e, payload);
                return;
            }
        }

        throw new SystemException("Unhandled event");
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
        this.retcode = Constants.EXECUTE_FINISHED;
        ValueSummary<int> current_state = this.states.InvokeMethod<int, int>((_, a0) => _[a0], 0);
        this.states.InvokeMethod<int>((_, a0) => _.RemoveAt(a0), 0);
        if (this.ExitFunctions.GetIndex(current_state).InvokeBinary<PMachine.ExitFunction, bool>((l, r) => l != r, ValueSummary<PMachine.ExitFunction>.Null).Cond())
        {
            this.ExitFunctions.GetIndex(current_state).Invoke();
        }
    }

    protected ValueSummary<PBool> RandomBool()
    {
        return this.scheduler.RandomBool().Cast<PBool>(_ => (PBool)_);
    }

    protected void Assert(ValueSummary<PBool> cond, ValueSummary<string> msg)
    {
        if (cond.InvokeUnary<PBool>(_ => !_).Cond())
        {
            throw new SystemException(msg);
        }
    }

    protected void Assert(ValueSummary<PBool> cond)
    {
        if (cond.InvokeUnary<PBool>(_ => !_).Cond())
        {
            throw new SystemException("Assertion failure");
        }
    }

    public void RunStateMachine(ValueSummary<int> state_idx, ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        ValueSummary<int> state = this.states.InvokeMethod<int, int>((_, a0) => _[a0], state_idx);
        if (this.IsGotoTransition.GetIndex(state, e).Cond())
        {
            this.states.InvokeMethod<int, int>((_, a0, a1) => _.RemoveRange(a0, a1), 0, state_idx);
        }

        this.retcode = Constants.EXECUTE_FINISHED;
        ValueSummary<TransitionFunction> transition_fn = this.Transitions.GetIndex(state, e);
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
        return new ValueSummary<ValueSummary<SymbolicInteger>>(new SymbolicInteger(this.GetHashCode()));
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PMachine> other)
    {
        return other.InvokeBinary<object, SymbolicBool>((l, r) => l == r, this);
    }
}