#include "CommonMacros.h"

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
        List<PMachine> choice_src_machines = new List<PMachine>();
        List<int> indexes = new List<int>();
        for(int i=0; i < this.machines.Count; i++) {
            PMachine machine = machines[i];
            List<SendQueueItem> sendQueue = this.sendQueues[machine];
            for(int j=0; j < sendQueue.Count; j++) {
                SendQueueItem item = sendQueue[j];
                if(item.e == EVENT_NEW_MACHINE || item.target.CanServeEvent(item.e)) {
                    choice_src_machines.Add(machine);
                    indexes.Add(j);
                    break;
                }
            }
        }
        // choose one and remove from send queue
        if(indexes.Count == 0) {
            return null;
        }
        int idx = this.rng.Next(0, indexes.Count);
        SendQueueItem r = this.sendQueues[choice_src_machines[idx]][indexes[idx]];
        this.sendQueues[choice_src_machines[idx]].RemoveAt(indexes[idx]);
        return r;
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

        while(true) {
            SendQueueItem dequeuedItem = scheduler.ChooseSendAndDequeue();
            if(dequeuedItem == null) {
                break;
            }
            int e = dequeuedItem.e;
            if (e == EVENT_NEW_MACHINE) {
                PMachine newMachine = dequeuedItem.target;
                scheduler.StartMachine(newMachine, dequeuedItem.payload);
            } else {
                PMachine targetMachine = dequeuedItem.target;
                targetMachine.RunStateMachine(e, dequeuedItem.payload);
            }
        }
        return 0;
    }
}