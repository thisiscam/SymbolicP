#include "CommonMacros.h"

using System;
using System.Collections.Generic;
using System.Diagnostics;

class Scheduler {

    private class SchedulerChoice {
        public PMachine sourceMachine;
        public int sourceMachineSendQueueIndex;
        public int targetMachineStateIndex;

        public SchedulerChoice(PMachine sourceMachine, int sourceMachineSendQueueIndex, int targetMachineStateIndex) {
            this.sourceMachine = sourceMachine;
            this.sourceMachineSendQueueIndex = sourceMachineSendQueueIndex;
            this.targetMachineStateIndex = targetMachineStateIndex;
        }
    }

    Random rng;

    List<PMachine> machines = new List<PMachine>();

    public Scheduler() {
        this.rng = new Random();
    }

    private bool ChooseAndRunMachine() {
        // Collect all servable events
        List<SchedulerChoice> choices = new List<SchedulerChoice>();
        for(int i=0; i < this.machines.Count; i++) {
            PMachine machine = machines[i];
            // Collect from send queue
            List<SendQueueItem> sendQueue = machine.sendQueue;
            for(int j=0; j < sendQueue.Count; j++) {
                SendQueueItem item = sendQueue[j];
                if(item.e == EVENT_NEW_MACHINE) {
                    choices.Add(new SchedulerChoice(machine, j, -1));
                    break;
                } else {
                    int state_idx = item.target.CanServeEvent(item.e);
                    if (state_idx >= 0) {
                        choices.Add(new SchedulerChoice(machine, j, state_idx));
                        break;
                    }
                }
            }
            // Machine is state that can serve null event?
            int null_state_idx = machine.CanServeEvent(EVENT_NULL);
            if(null_state_idx >= 0) {
                choices.Add(new SchedulerChoice(machine, -1, null_state_idx));
            }
        }
        if(choices.Count == 0) {
            return false;
        }

        // Choose one and remove from send queue
        int idx = this.rng.Next(0, choices.Count);
        SchedulerChoice chosen = choices[idx];
        int sourceMachineSendQueueIndex = chosen.sourceMachineSendQueueIndex;
        if (sourceMachineSendQueueIndex < 0) {
            PMachine chosenTargetMachine = chosen.sourceMachine;
            chosenTargetMachine.RunStateMachine(chosen.targetMachineStateIndex, EVENT_NULL, null);
        } else {
            PMachine chosenSourceMachine = chosen.sourceMachine;
            SendQueueItem dequeuedItem = chosenSourceMachine.sendQueue[sourceMachineSendQueueIndex];
            chosenSourceMachine.sendQueue.RemoveAt(sourceMachineSendQueueIndex);
            // Invoke Machine
            if(dequeuedItem.e == EVENT_NEW_MACHINE) {
                PMachine newMachine = dequeuedItem.target;
                Console.WriteLine(chosenSourceMachine.ToString() + " creates " + newMachine.ToString());
                this.StartMachine(newMachine, dequeuedItem.payload);
            } else{
                PMachine targetMachine = dequeuedItem.target;
                Console.WriteLine(chosenSourceMachine.ToString() + " sends event " + dequeuedItem.e.ToString() + " to " + targetMachine.ToString());
                targetMachine.RunStateMachine(chosen.targetMachineStateIndex, dequeuedItem.e, dequeuedItem.payload);
            }
        }
        return true;
    }

    public void SendMsg(PMachine source, PMachine target, int e, object payload) {
        source.sendQueue.Add(new SendQueueItem(target, e, payload));
    }

    public void NewMachine(PMachine source, PMachine newMachine, object payload) {
        source.sendQueue.Add(new SendQueueItem(newMachine, EVENT_NEW_MACHINE, payload));
    }

    public bool RandomBool() {
        /* Hack for now, need to decide on a better structure for "$" sign semantics of P */
        return this.rng.NextDouble() > 0.5;
    }

    private void StartMachine(PMachine machine, object payload) {
        this.machines.Add(machine);
        machine.StartMachine(this, payload);
    }

    static int Main(string[] args) {
        Scheduler scheduler = new Scheduler();

        PMachine mainMachine = MachineController.CreateMainMachine();
        scheduler.StartMachine(mainMachine, null);

        while(scheduler.ChooseAndRunMachine()) { Debugger.Break(); }
        return 0;
    }
}