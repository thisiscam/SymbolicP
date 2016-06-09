#include "common_Macros.h"

using System;
using System.Collections.Generic;
using System.Diagnostics;

class Scheduler {

    Random rng;

    List<PMachine> machines = new List<PMachine>();

    List<PMachine> sendQueueActiveMachines = new List<PMachine>(); // Ideally should be a hash-linkedlist

    Dictionary<PMachine, List<SendQueueItem>> sendQueues = new Dictionary<PMachine, List<SendQueueItem>>(); // To avoid dictionary, consider adding an machine_id field to each PMachine

    public Scheduler() {
        this.rng = new Random();
    }

    private SendQueueItem ChooseSendAndDequeue() {
        int idx = this.rng.Next(0, sendQueueActiveMachines.Count);
        PMachine chosenMachine = sendQueueActiveMachines[idx];
        SendQueueItem r = this.sendQueues[chosenMachine][0];
        this.sendQueues[chosenMachine].RemoveAt(0);
        if(this.sendQueues[chosenMachine].Count == 0) {
            this.sendQueueActiveMachines.Remove(chosenMachine);
        }
        return r;
    }

    public void SendMsg(PMachine source, PMachine target, int e, object payload) {
        if(this.sendQueues[source].Count == 0) {
            this.sendQueueActiveMachines.Add(source);
        }
        this.sendQueues[source].Add(new SendQueueItem(target, e, payload));
    }

    public void NewMachine(PMachine source, PMachine newMachine, object payload) {
        if(this.sendQueues[source].Count == 0) {
            this.sendQueueActiveMachines.Add(source);
        }
        this.sendQueues[source].Add(new SendQueueItem(newMachine, EVENT_NEW_MACHINE, payload));
    }

    public bool RandomBool() {
        /* Hack for now, need to decide on a better structure for "$" sign semantics of P */
        return this.rng.NextDouble() > 0.5;
    }

    private void StartMachine(PMachine machine, object payload) {
        this.machines.Add(machine);
        sendQueues.Add(machine, new List<SendQueueItem>());
        machine.StartMachine(this, payload);
    }

    static int Main(string[] args) {
        Scheduler scheduler = new Scheduler();

        PMachine mainMachine = MachineStarter.CreateMainMachine();
        scheduler.StartMachine(mainMachine, null);

        while(scheduler.sendQueueActiveMachines.Count > 0) {
            SendQueueItem dequeuedItem = scheduler.ChooseSendAndDequeue();
            int e = dequeuedItem.e;
            if (e == EVENT_NEW_MACHINE) {
                PMachine newMachine = dequeuedItem.target;
                scheduler.StartMachine(newMachine, dequeuedItem.payload);
            } else {
                PMachine targetMachine = dequeuedItem.target;
                targetMachine.EnqueueReceive(e, dequeuedItem.payload);
                targetMachine.RunStateMachine();
            }
        }
        return 0;
    }
}