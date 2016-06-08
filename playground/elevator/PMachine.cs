#include "common_Macros.h"

using System;
using System.Collections.Generic;

abstract class PMachine {
    
    protected delegate int TransitionFunction();

	protected int retcode;
    protected int state;

    private Scheduler scheduler;
    private List<ReceiveQueueItem> receiveQueue = new List<ReceiveQueueItem>();

    protected bool[,] DeferedSet;

    public virtual void StartMachine(Scheduler s) {
        this.scheduler = s;
    }
    protected abstract void ServeEvent(int e, object payload);

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
    	ReceiveQueueItem item = DequeueReceive();
        if (item == null)
            return;
        int e = item.e;
        object payload = item.payload;
        this.retcode = EXECUTE_FINISHED;
        this.ServeEvent(e, payload);
    }
}