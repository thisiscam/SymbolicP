using System;
using System.Collections.Generic;

abstract class PMachine : IPType<PMachine> {
    protected delegate void TransitionFunction(IPType payload);
    protected delegate void ExitFunction();

    protected int retcode;
    protected List<int> states = new List<int>();

    protected Scheduler scheduler;

    protected bool[,] DeferedSet;

    protected bool[,] IsGotoTransition;
    protected TransitionFunction[,] Transitions;
    
    protected ExitFunction[] ExitFunctions;

    public List<SendQueueItem> sendQueue = new List<SendQueueItem>();

    public virtual void StartMachine(Scheduler scheduler, IPType payload) {
        this.scheduler = scheduler;
    }
    
    /* Returns the index that can serve this event */
    public int CanServeEvent(PInteger e) {
        for(int i=0; i < this.states.Count; i++) {
            int state = this.states[i];
            if(!this.DeferedSet[state, e] && this.Transitions[state, e] != null) {
                return i;
            }
        }
        return -1;
    }

    protected void RaiseEvent(PInteger e, IPType payload) {
        for(int i=0; i < this.states.Count; i++) {
            int state = this.states[i];
            if(this.Transitions[state, e] != null) {
                this.RunStateMachine(i, e, payload);
                return;
            }
        }
        throw new SystemException("Unhandled event");
    }

    protected void SendMsg(PMachine other, PInteger e, IPType payload) {
        this.scheduler.SendMsg(this, other, e, payload);
    }

    protected PMachine NewMachine(PMachine newMachine, IPType payload) {
        this.scheduler.NewMachine(this, newMachine, payload);
        return newMachine;
    }

    protected void PopState() {
        int current_state = this.states[0];
        this.states.RemoveAt(0);
        if(this.ExitFunctions[current_state] != null) {
            this.ExitFunctions[current_state]();
        }
    }

    protected bool RandomBool() {
        return this.scheduler.RandomBool();
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

    public void RunStateMachine(int state_idx, PInteger e, IPType payload) {
        int state = this.states[state_idx];
        if(this.IsGotoTransition[state, e]) {
            this.states.RemoveRange(0, state_idx);
        }
        this.retcode = Constants.EXECUTE_FINISHED;
        TransitionFunction transition_fn = this.Transitions[state, e];
        transition_fn(payload);
    }

    protected void Transition_Ignore(IPType payload) {
        return;
    }

    public PMachine DeepCopy() {
        return this;
    }
}