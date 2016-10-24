using System;
using System.Collections.Generic;

abstract class MonitorPMachine {
    protected delegate void TransitionFunction(IPType payload);
    protected delegate void ExitFunction();

    protected int retcode;
    protected List<int> states = new List<int>();

    protected bool[,] DeferedSet;

    protected bool[,] IsGotoTransition;
    protected TransitionFunction[,] Transitions;
    protected ExitFunction[] ExitFunctions;

    public void ServeEvent(PInteger e, IPType payload) {
        for(int i=0; i < this.states.Count; i++) {
            int state = this.states[i];
            if(!this.DeferedSet[state, e] && this.Transitions[state, e] != null) {
                if(this.IsGotoTransition[state, e]) {
                    this.states.RemoveRange(0, i);
                }
                this.retcode = Constants.EXECUTE_FINISHED;
                TransitionFunction transition_fn = this.Transitions[state, e];
                transition_fn(payload);
                return;
            }
        }
        throw new SystemException("Unhandled event");
    }

    protected void RaiseEvent(PInteger e, IPType payload) {
        ServeEvent(e, payload);
    }

    protected void PopState() {
        this.retcode = Constants.EXECUTE_FINISHED;
        int current_state = this.states[0];
        this.states.RemoveAt(0);
        if(this.ExitFunctions[current_state] != null) {
            this.ExitFunctions[current_state]();
        }
    }

    protected void Transition_Ignore(IPType payload) {
        return;
    }

    protected void Assert(PBool cond, string msg) {
        if(!cond) {
            throw new SystemException(msg);
        }
    }

    protected void Assert(PBool cond) {
        if(!cond) {
            throw new SystemException("Assertion failure");
        }
    }
}