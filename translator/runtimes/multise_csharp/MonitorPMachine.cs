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
        {
            PathConstraint.BeginLoop();
            for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this.states.GetField<int>(_ => _.Count)).Loop(); i.Increment())
            {
                ValueSummary<int> state = this.states.InvokeMethod<int, int>((_, a0) => _[a0], i);
                {
                    var vs_cond_20 = (this.Transitions.GetIndex(state, e).InvokeBinary<MonitorPMachine.TransitionFunction, bool>((l, r) => l != r, new ValueSummary<MonitorPMachine.TransitionFunction>(null))).Cond();
                    if (vs_cond_20.CondTrue())
                    {
                        {
                            var vs_cond_21 = (this.IsGotoTransition.GetIndex(state, e)).Cond();
                            if (vs_cond_21.CondTrue())
                            {
                                this.states.InvokeMethod<int, int>((_, a0, a1) => _.RemoveRange(a0, a1), 0, i);
                            }

                            vs_cond_21.MergeBranch();
                        }

                        this.retcode.Assign<int>(Constants.EXECUTE_FINISHED);
                        ValueSummary<TransitionFunction> transition_fn = this.Transitions.GetIndex(state, e);
                        transition_fn.Invoke(payload);
                        PathConstraint.RecordReturnPath();
                    }

                    vs_cond_20.MergeBranch();
                }
            }
        }

        if (PathConstraint.MergedPcFeasible())
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
        {
            var vs_cond_22 = (cond.InvokeUnary<PBool>(_ => !_)).Cond();
            if (vs_cond_22.CondTrue())
            {
                throw new SystemException(msg);
            }

            vs_cond_22.MergeBranch();
        }
    }

    protected void Assert(ValueSummary<PBool> cond)
    {
        {
            var vs_cond_23 = (cond.InvokeUnary<PBool>(_ => !_)).Cond();
            if (vs_cond_23.CondTrue())
            {
                throw new SystemException("Assertion failure");
            }

            vs_cond_23.MergeBranch();
        }
    }
}