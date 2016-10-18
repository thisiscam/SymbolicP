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
    public void ServeEvent(ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        PathConstraint.PushFrame();
        var vs_cond_10 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = 0; vs_cond_10.Loop(i.InvokeBinary<int, bool>((l, r) => l < r, this.states.GetField<int>(_ => _.Count))); i.Increment())
        {
            ValueSummary<int> state = this.states.InvokeMethod<int, int>((_, a0) => _[a0], i);
            var vs_cond_9 = (this.Transitions.GetIndex(state, e).InvokeBinary<MonitorPMachine.TransitionFunction, bool>((l, r) => l != r, new ValueSummary<MonitorPMachine.TransitionFunction>(null))).Cond();
            {
                if (vs_cond_9.CondTrue())
                {
                    var vs_cond_8 = (this.IsGotoTransition.GetIndex(state, e)).Cond();
                    {
                        if (vs_cond_8.CondTrue())
                        {
                            this.states.InvokeMethod<int, int>((_, a0, a1) => _.RemoveRange(a0, a1), 0, i);
                        }
                    }

                    vs_cond_8.MergeBranch();
                    this.retcode.Assign<int>(Constants.EXECUTE_FINISHED);
                    ValueSummary<TransitionFunction> transition_fn = this.Transitions.GetIndex(state, e);
                    transition_fn.Invoke(payload);
                    PathConstraint.RecordReturnPath(vs_cond_9);
                }
            }

            vs_cond_9.MergeBranch(vs_cond_10);
        }

        if (vs_cond_10.MergeBranch())
        {
            throw new SystemException("Unhandled event");
        }

        PathConstraint.PopFrame();
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