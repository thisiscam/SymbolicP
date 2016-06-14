#include "common_Macros.h"

using System;
using System.Collections.Generic;

abstract class PMachine {
    protected delegate void TransitionFunction(object payload);

	protected int retcode;
    protected int state;

    private Scheduler scheduler;
    private List<ReceiveQueueItem> receiveQueue = new List<ReceiveQueueItem>();

    protected bool[,] DeferedSet;
    protected TransitionFunction[,] Transitions;

    public virtual void StartMachine(Scheduler s, object payload) {
        this.scheduler = s;
    }
    
    protected void ServeEvent(int e, object payload) {
        this.retcode = EXECUTE_FINISHED;
        TransitionFunction transition_fn = this.Transitions[this.state, e];
        if(transition_fn != null) {
            transition_fn(payload);
            if (retcode == RAISED_EVENT) return;
        } else {
            throw new SystemException("Unhandled Event");
        }
    }

    protected void SendMsg(PMachine other, int e, object payload) {
        Console.WriteLine(this.ToString() + " send event " + e.ToString() + " to " + other.ToString());
        this.scheduler.SendMsg(this, other, e, payload);
    }

    protected PMachine NewMachine(PMachine newMachine, object payload) {
    	this.scheduler.NewMachine(this, newMachine, payload);
    	return newMachine;
    }

    protected bool RandomBool() {
        return this.scheduler.RandomBool();
    }

    protected ReceiveQueueItem DequeueReceive() {
    	int state = this.state;
    	for(int i=0; i < receiveQueue.Count; i++) {
    		ReceiveQueueItem item = receiveQueue[i];
    		if (!this.DeferedSet[state, item.e]) {
    			return item;
    		}
    	}
    	return null;
    }

    public void EnqueueReceive(int e, object payload) {
    	this.receiveQueue.Add(new ReceiveQueueItem(e, payload));
    }

    public void RunStateMachine() {
        while(true) {
            ReceiveQueueItem item = DequeueReceive();
            if (item == null)
                return;
            int e = item.e;
            object payload = item.payload;
            this.ServeEvent(e, payload);        
        }
    }

    protected void Transition_Ignore(object payload) {
        return;
    }
}