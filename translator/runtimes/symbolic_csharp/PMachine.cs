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

    public virtual void StartMachine(Scheduler s, IPType payload) {
        this.scheduler = s;
    }
    
    /* Returns the index that can serve this event */
    public int CanServeEvent(PInteger e) {
        for(int i=this.states.Count-1; i >= 0; i--) {
            int state = this.states[i];
            if(this.DeferedSet[state, e]) {
                return -1;
            } else {
                if(this.Transitions[state, e] != null) {
                    return i;
                }
            }
        }
        throw new Exception("Unhandled event");
    }

    protected void RaiseEvent(PInteger e, IPType payload) {
        for(int i=this.states.Count-1; i >= 0; i--) {
            int state = this.states[i];
            if(this.Transitions[state, e] != null) {
                this.Transitions[state, e](payload);
                return;
            } else {
                PopState();
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
        int last = states.Count-1;
        int current_state = this.states[last];
        this.states.RemoveRange(last);
        if(this.ExitFunctions[current_state] != null) {
            this.ExitFunctions[current_state]();
        }
    }

	protected PBool RandomBool() {
        return (PBool)this.scheduler.RandomBool();
    }

#if MULTISE_RUNTIME
    protected PBool RandomBool(int site, ValueSummary<int> cnt, System.Collections.Generic.List<PBool> list) {
        return null;
    }
#endif
    
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
            for(int i=this.states.Count-1; i > state_idx; i--)
            {
                this.PopState();
            }
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

	public SymbolicBool PTypeEquals(PMachine other) {
		return other == this;
	}
}