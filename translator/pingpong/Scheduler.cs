#include "common_Macros.h"

using System;
using System.Collections.Generic;
using System.Diagnostics;

class Scheduler {

    Random rng;

    List<PMachine> machines = new List<PMachine>();

    Dictionary<PMachine, List<SendQueueItem>> sendQueues = new Dictionary<PMachine, List<SendQueueItem>>(); // To avoid dictionary, consider adding an machine_id field to each PMachine

    public Scheduler() {
        this.rng = new Random();
    }

    private SendQueueItem ChooseSendAndDequeue() {
        // Collect all servable events
        List<SendQueueItem> choices = new List<SendQueueItem>();
        List<int> choice_src_machines = new List<int>();
        for(int i=0; i < this.machines.Count; i++) {
            PMachine machine = machines[i];
            List<SendQueueItem> sendQueue = this.sendQueues[machine];
            for(int j=0; j < sendQueue.Count; j++) {
                SendQueueItem item = sendQueue[j]
                if(item.target.CanServeEvent(item.e)) {
                    choices.Add(item);
                    choice_src_machines.Add(i);
                }
            }
        }
        // choose one and remove from send queue
        int idx = this.rng.Next(0, choices.Count);
        this.sendQueues[choice_src_machines[i]].Remove(0);
        return choices[idx];
    }

    public void SendMsg(PMachine source, PMachine target, int e, object payload) {
        this.sendQueues[source].Add(new SendQueueItem(target, e, payload));
    }

    public void NewMachine(PMachine source, PMachine newMachine, object payload) {
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