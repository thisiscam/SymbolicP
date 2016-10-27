using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Z3;

partial class Scheduler {

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

    List<PMachine> machines = new List<PMachine>();

    public bool ChooseAndRunMachine() {
        SchedulerChoice chosen = ChooseMachine();
        if(chosen == null) {
            return false;
        } else {
            int sourceMachineSendQueueIndex = chosen.sourceMachineSendQueueIndex;
            if (sourceMachineSendQueueIndex < 0) {
                PMachine chosenTargetMachine = chosen.sourceMachine;
                Console.WriteLine(chosenTargetMachine.ToString() + chosenTargetMachine.GetHashCode() + " executes EVENT_NULL");
                chosenTargetMachine.RunStateMachine(chosen.targetMachineStateIndex, Constants.EVENT_NULL, null);
            } else {
                PMachine chosenSourceMachine = chosen.sourceMachine;
                SendQueueItem dequeuedItem = chosenSourceMachine.sendQueue[sourceMachineSendQueueIndex];
                chosenSourceMachine.sendQueue.RemoveAt(sourceMachineSendQueueIndex);
                // Invoke Machine
                if(dequeuedItem.e == Constants.EVENT_NEW_MACHINE) {
                    PMachine newMachine = dequeuedItem.target;
                    Console.WriteLine(chosenSourceMachine.ToString() + chosenSourceMachine.GetHashCode() + " creates " + newMachine.ToString() + chosenSourceMachine.GetHashCode());
                    this.StartMachine(newMachine, dequeuedItem.payload);
                } else{
                    PMachine targetMachine = dequeuedItem.target;
                    Console.WriteLine(chosenSourceMachine.ToString() + chosenSourceMachine.GetHashCode() + " sends event " + dequeuedItem.e.ToString() + " to " + targetMachine.ToString() + targetMachine.GetHashCode());
                    targetMachine.RunStateMachine(chosen.targetMachineStateIndex, dequeuedItem.e, dequeuedItem.payload);
                }
            }
            return true;
        }
    }

    public void SendMsg(PMachine source, PMachine target, PInteger e, IPType payload) {
        source.sendQueue.Add(new SendQueueItem(target, e, payload));
    }

    public void NewMachine(PMachine source, PMachine newMachine, IPType payload) {
        source.sendQueue.Add(new SendQueueItem(newMachine, Constants.EVENT_NEW_MACHINE, payload));
    }

	public SymbolicBool RandomBool() {
		return SymbolicEngine.SE.NewSymbolicBoolVar ("RB");
    }

    public void StartMachine(PMachine machine, IPType payload) {
        this.machines.Add(machine);
        machine.StartMachine(this, payload);
    }
}