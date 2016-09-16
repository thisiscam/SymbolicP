using System;
using System.Collections.Generic;

abstract class MonitorPMachine
{
    protected delegate void TransitionFunction(ValueSummary<IPType> payload);
    protected delegate void ExitFunction();
    protected ValueSummary<int> retcode = new ValueSummary<int>(default (int));
    protected ValueSummary<List<int>> states = ValueSummary<List<int>>.InitializeFrom(new ValueSummary<List<int>>(new List<int>()));
    protected bool[, ] DeferedSet;
    protected bool[, ] IsGotoTransition;
    protected TransitionFunction[, ] Transitions;
    protected ExitFunction[] ExitFunctions;
    public void ServeEvent(ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this.states.GetField<int>(_ => _.Count)).Cond(); i.Increment())
        {
            ValueSummary<int> state = ValueSummary<int>.InitializeFrom(this.states.InvokeMethod<int, int>((_, a0) => _[a0], i));
            if (this.Transitions.GetIndex(state, e).InvokeBinary<MonitorPMachine.TransitionFunction, bool>((l, r) => l != r, new ValueSummary<MonitorPMachine.TransitionFunction>(default (MonitorPMachine.TransitionFunction))).Cond())
            {
                if (this.IsGotoTransition.GetIndex(state, e).Cond())
                {
                    this.states.InvokeMethod<int, int>((_, a0, a1) => _.RemoveRange(a0, a1), 0, i);
                }

                this.retcode = Constants.EXECUTE_FINISHED;
                ValueSummary<TransitionFunction> transition_fn = ValueSummary<TransitionFunction>.InitializeFrom(this.Transitions.GetIndex(state, e));
                transition_fn.Invoke(payload);
                return;
            }
        }

        throw new SystemException("Unhandled event");
    }

    protected void Transition_Ignore(ValueSummary<IPType> payload)
    {
        return;
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
}