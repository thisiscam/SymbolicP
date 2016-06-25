#include "CommonMacros.h"

using System;
using System.Collections.Generic;

abstract class MonitorPMachine {
    protected delegate void TransitionFunction(object payload);

	protected int retcode;
    protected List<int> states = new List<int>();

    protected bool[,] DeferedSet;

    protected bool[,] IsGotoTransition;
    protected TransitionFunction[,] Transitions;

    public void ServeEvent(int e, object payload) {
        for(int i=0; i < this.states.Count; i++) {
            int state = this.states[i];
            if(this.Transitions[state, e] != null) {
                if(this.IsGotoTransition[state, e]) {
                    this.states.RemoveRange(0, i);
                }
                this.retcode = EXECUTE_FINISHED;
                TransitionFunction transition_fn = this.Transitions[state, e];
                transition_fn(payload);                
                return;
            }
        }
        throw new SystemException("Unhandled event");
    }

    protected void Transition_Ignore(object payload) {
        return;
    }
}