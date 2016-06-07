#include "common_Macros.h"

using System;
using System.Collections.Generic;

abstract class PMachine {
	protected int retcode;
    protected int state;
    protected List<SendQueueItem> sendQueue = new List<SendQueueItem>();
    protected List<ReceiveQueueItem> receiveQueue = new List<ReceiveQueueItem>();

    protected bool[,] DeferedSet;

    public abstract void StartMachine();
    protected abstract void ServeEvent(int e, object payload);

    protected void sendMsg(PMachine other, int e, object payload) {
        Console.WriteLine(this.ToString() + " send event " + e.ToString() + " to " + other.ToString());
        this.sendQueue.Add(new SendQueueItem(other, e, payload));
    }

    protected PMachine newMachine(PMachine new_machine) {
    	this.sendQueue.Add(new SendQueueItem(new_machine, EVENT_NEW_MACHINE, null));
    	return new_machine;
    }

    protected ReceiveQueueItem dequeueReceive() {
    	int state = this.state;
    	for(int i=0; i < receiveQueue.Count; i++) {
    		ReceiveQueueItem item = receiveQueue[i];
    		if (!this.DeferedSet[state, item.e]) {
    			return item;
    		}
    	}
    	return null;
    }

    public SendQueueItem dequeueSend() {
    	SendQueueItem r = this.sendQueue[0];
    	this.sendQueue.RemoveAt(0);
    	return r;
    }

    public void enqueueReceive(int e, object payload) {
    	this.receiveQueue.Add(new ReceiveQueueItem(e, payload));
    }

    public void RunStateMachine() {
    	ReceiveQueueItem item = dequeueReceive();
        if (item == null)
            return;
        int e = item.e;
        object payload = item.payload;
        this.retcode = EXECUTE_FINISHED;
        this.ServeEvent(e, payload);
    }
}