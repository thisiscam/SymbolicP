#include "CommonMacros.h"

using System;
using System.Collections.Generic;

abstract class MonitorPMachine {
    protected delegate void TransitionFunction(object payload);

	protected int retcode;
    protected int state;

    protected bool[,] DeferedSet;
    protected TransitionFunction[,] Transitions;

    public void ServeEvent(int e, object payload) {
        if (this.DeferedSet[this.state, e]) {
            return;
        }
        this.retcode = EXECUTE_FINISHED;
        TransitionFunction transition_fn = this.Transitions[this.state, e];
        if(transition_fn != null) {
            transition_fn(payload);
            if (retcode == RAISED_EVENT) return;
        } else {
            throw new SystemException("Unhandled Event");
        }
    }

    protected void Transition_Ignore(object payload) {
        return;
    }
}