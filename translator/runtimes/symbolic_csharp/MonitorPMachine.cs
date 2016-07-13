using System;
using System.Collections.Generic;

abstract class MonitorPMachine {
    protected delegate void TransitionFunction(IPType payload);
    protected delegate void ExitFunction();

	protected int retcode;
	protected List<SymbolicInteger> states = new List<SymbolicInteger>();

    protected bool[,] DeferedSet;

    protected bool[,] IsGotoTransition;
    protected TransitionFunction[,] Transitions;
    protected ExitFunction[] ExitFunctions;

	public void ServeEvent(SymbolicInteger e, IPType payload) {
		for(SymbolicInteger i=0; i < this.states.Count; i++) {
			SymbolicInteger state = this.states[i];
            if(this.Transitions[state, e] != null) {
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

    protected void Transition_Ignore(IPType payload) {
        return;
    }

	protected void Assert(SymbolicBool cond, string msg) {
        if(!cond) {
            throw new SystemException(msg);
        }
    }

	protected void Assert(SymbolicBool cond) {
        if(!cond) {
            throw new SystemException("Assertion failure");
        }
    }
}