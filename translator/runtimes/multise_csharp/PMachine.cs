using System;
using System.Collections.Generic;

abstract class PMachine : IPType<PMachine>
{
    protected delegate void TransitionFunction(ValueSummary<PMachine> self, ValueSummary<IPType> payload);
    protected delegate void ExitFunction(ValueSummary<PMachine> self);
    protected ValueSummary<int> retcode;
    protected ValueSummary<List<int>> states = new ValueSummary<List<int>>(new List<int>());
    private ValueSummary<Scheduler> scheduler;
    protected ValueSummary<ValueSummary<bool>[, ]> DeferedSet;
    protected ValueSummary<ValueSummary<bool>[, ]> IsGotoTransition;
    protected ValueSummary<ValueSummary<TransitionFunction>[, ]> Transitions;
    protected ValueSummary<ValueSummary<ExitFunction>[]> ExitFunctions;
    public ValueSummary<List<SendQueueItem>> sendQueue = new ValueSummary<List<SendQueueItem>>(new List<SendQueueItem>());
    public virtual void StartMachine(ValueSummary<PMachine> self, ValueSummary<Scheduler> s, ValueSummary<IPType> payload)
    {
        self.GetField<Scheduler>(_ => _.scheduler).Assign(s);
    }

    /* Returns the index that can serve this event */
    public ValueSummary<int> CanServeEvent(ValueSummary<PMachine> self, ValueSummary<PInteger> e)
    {
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, self.GetField<List<int>>(_ => _.states).GetField<int>(_ => _.Count)); i.InvokeMethod((_) => _++))
        {
            ValueSummary<int> state = self.GetField<List<int>>(_ => _.states).GetIndex((_, a0) => _[a0], i);
            if (self.GetField<bool[, ]>(_ => _.DeferedSet).GetIndex((_, a0, a1) => _[a0, a1], state, e).InvokeMethod((_) => !_).InvokeBinary<bool, bool>((l, r) => l && r, self.GetField<PMachine.TransitionFunction[, ]>(_ => _.Transitions).GetIndex((_, a0, a1) => _[a0, a1], state, e).InvokeBinary<PMachine.TransitionFunction, bool>((l, r) => l != r, null)))
            {
                return i;
            }
        }

        return -1;
    }

    protected void RaiseEvent(ValueSummary<PMachine> self, ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, self.GetField<List<int>>(_ => _.states).GetField<int>(_ => _.Count)); i.InvokeMethod((_) => _++))
        {
            ValueSummary<int> state = self.GetField<List<int>>(_ => _.states).GetIndex((_, a0) => _[a0], i);
            if (self.GetField<PMachine.TransitionFunction[, ]>(_ => _.Transitions).GetIndex((_, a0, a1) => _[a0, a1], state, e).InvokeBinary<PMachine.TransitionFunction, bool>((l, r) => l != r, null))
            {
                self.InvokeMethod<int, PInteger, IPType>((_, s, a0, a1, a2) => _.RunStateMachine(s, a0, a1, a2), i, e, payload);
                return;
            }
        }

        throw new SystemException("Unhandled event");
    }

    protected void SendMsg(ValueSummary<PMachine> self, ValueSummary<PMachine> other, ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        self.GetField<Scheduler>(_ => _.scheduler).InvokeMethod<PMachine, PMachine, PInteger, IPType>((_, s, a0, a1, a2, a3) => _.SendMsg(s, a0, a1, a2, a3), self, other, e, payload);
    }

    protected ValueSummary<PMachine> NewMachine(ValueSummary<PMachine> self, ValueSummary<PMachine> newMachine, ValueSummary<IPType> payload)
    {
        self.GetField<Scheduler>(_ => _.scheduler).InvokeMethod<PMachine, PMachine, IPType>((_, s, a0, a1, a2) => _.NewMachine(s, a0, a1, a2), self, newMachine, payload);
        return newMachine;
    }

    protected void PopState(ValueSummary<PMachine> self)
    {
        self.GetField<int>(_ => _.retcode).Assign(Constants.EXECUTE_FINISHED);
        ValueSummary<int> current_state = self.GetField<List<int>>(_ => _.states).GetIndex((_, a0) => _[a0], 0);
        self.GetField<List<int>>(_ => _.states).InvokeMethod<int>((_, s, a0) => _.RemoveAt(s, a0), 0);
        if (self.GetField<PMachine.ExitFunction[]>(_ => _.ExitFunctions).GetIndex((_, a0) => _[a0], current_state).InvokeBinary<PMachine.ExitFunction, bool>((l, r) => l != r, null))
        {
            self.GetField<PMachine.ExitFunction[]>(_ => _.ExitFunctions).GetIndex((_, a0) => _[a0], current_state).Invoke();
        }
    }

    protected ValueSummary<PBool> RandomBool(ValueSummary<PMachine> self)
    {
        return self.GetField<Scheduler>(_ => _.scheduler).InvokeMethod((_, s) => _.RandomBool(s));
    }

    protected void Assert(ValueSummary<PMachine> self, ValueSummary<PBool> cond, ValueSummary<string> msg)
    {
        if (cond.InvokeMethod((_) => !_))
        {
            throw new SystemException(msg);
        }
    }

    protected void Assert(ValueSummary<PMachine> self, ValueSummary<PBool> cond)
    {
        if (cond.InvokeMethod((_) => !_))
        {
            throw new SystemException("Assertion failure");
        }
    }

    public void RunStateMachine(ValueSummary<PMachine> self, ValueSummary<int> state_idx, ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        ValueSummary<int> state = self.GetField<List<int>>(_ => _.states).GetIndex((_, a0) => _[a0], state_idx);
        if (self.GetField<bool[, ]>(_ => _.IsGotoTransition).GetIndex((_, a0, a1) => _[a0, a1], state, e))
        {
            self.GetField<List<int>>(_ => _.states).InvokeMethod<int, int>((_, s, a0, a1) => _.RemoveRange(s, a0, a1), 0, state_idx);
        }

        self.GetField<int>(_ => _.retcode).Assign(Constants.EXECUTE_FINISHED);
        ValueSummary<TransitionFunction> transition_fn = self.GetField<PMachine.TransitionFunction[, ]>(_ => _.Transitions).GetIndex((_, a0, a1) => _[a0, a1], state, e);
        transition_fn.Invoke(payload);
    }

    protected void Transition_Ignore(ValueSummary<PMachine> self, ValueSummary<IPType> payload)
    {
        return;
    }

    public ValueSummary<PMachine> DeepCopy(ValueSummary<PMachine> self)
    {
        return self;
    }

    public ValueSummary<SymbolicInteger> PTypeGetHashCode(ValueSummary<PMachine> self)
    {
        return self.GetHashCode();
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PMachine> self, ValueSummary<PMachine> other)
    {
        return self.InvokeBinary<object, SymbolicBool>((l, r) => l == r, other);
    }
}