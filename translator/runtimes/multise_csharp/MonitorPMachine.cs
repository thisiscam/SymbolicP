using System;
using System.Collections.Generic;

abstract class MonitorPMachine
{
    protected delegate void TransitionFunction(ValueSummary<MonitorPMachine> self, ValueSummary<IPType> payload);
    protected delegate void ExitFunction(ValueSummary<MonitorPMachine> self);
    protected ValueSummary<int> retcode;
    protected ValueSummary<List<int>> states = new ValueSummary<List<int>>(new List<int>());
    protected ValueSummary<ValueSummary<bool>[, ]> DeferedSet;
    protected ValueSummary<ValueSummary<bool>[, ]> IsGotoTransition;
    protected ValueSummary<ValueSummary<TransitionFunction>[, ]> Transitions;
    protected ValueSummary<ValueSummary<ExitFunction>[]> ExitFunctions;
    public void ServeEvent(ValueSummary<MonitorPMachine> self, ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, self.GetField<List<int>>(_ => _.states).GetField<int>(_ => _.Count)); i.InvokeMethod((_) => _++))
        {
            ValueSummary<int> state = self.GetField<List<int>>(_ => _.states).GetIndex((_, a0) => _[a0], i);
            if (self.GetField<MonitorPMachine.TransitionFunction[, ]>(_ => _.Transitions).GetIndex((_, a0, a1) => _[a0, a1], state, e).InvokeBinary<MonitorPMachine.TransitionFunction, bool>((l, r) => l != r, null))
            {
                if (self.GetField<bool[, ]>(_ => _.IsGotoTransition).GetIndex((_, a0, a1) => _[a0, a1], state, e))
                {
                    self.GetField<List<int>>(_ => _.states).InvokeMethod<int, int>((_, s, a0, a1) => _.RemoveRange(s, a0, a1), 0, i);
                }

                self.GetField<int>(_ => _.retcode).Assign(Constants.EXECUTE_FINISHED);
                ValueSummary<TransitionFunction> transition_fn = self.GetField<MonitorPMachine.TransitionFunction[, ]>(_ => _.Transitions).GetIndex((_, a0, a1) => _[a0, a1], state, e);
                transition_fn.Invoke(payload);
                return;
            }
        }

        throw new SystemException("Unhandled event");
    }

    protected void Transition_Ignore(ValueSummary<MonitorPMachine> self, ValueSummary<IPType> payload)
    {
        return;
    }

    protected void Assert(ValueSummary<MonitorPMachine> self, ValueSummary<PBool> cond, ValueSummary<string> msg)
    {
        if (cond.InvokeMethod((_) => !_))
        {
            throw new SystemException(msg);
        }
    }

    protected void Assert(ValueSummary<MonitorPMachine> self, ValueSummary<PBool> cond)
    {
        if (cond.InvokeMethod((_) => !_))
        {
            throw new SystemException("Assertion failure");
        }
    }
}