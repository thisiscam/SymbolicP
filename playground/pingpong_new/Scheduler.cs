#include "common_Macros.h"

using System.Collections.Generic;
using System.Diagnostics;

class Scheduler {

    List<PMachine> machines = new List<PMachine>();

    private int ChooseSenderMachineIndex() {
        return 0;
    }

    static int Main(string[] args) {
        Scheduler scheduler = new Scheduler();
        
        /* Scheduler creates first machine and directly starts it */
        PMachine mainMachine = MachineStarter.CreateMainMachine();
        scheduler.machines.Add(mainMachine);
        mainMachine.StartMachine();

        while(scheduler.machines.Count > 0) {
            int senderMachineIndx = scheduler.ChooseSenderMachineIndex();
            SendQueueItem dequeuedItem = scheduler.machines[senderMachineIndx].dequeueSend();
            int e = dequeuedItem.e;
            if (e == EVENT_NEW_MACHINE) {
                PMachine newMachine = dequeuedItem.target;
                scheduler.machines.Add(newMachine);
                newMachine.StartMachine();
            } else {
                PMachine targetMachine = dequeuedItem.target;
                targetMachine.enqueueReceive(e, dequeuedItem.payload);
                targetMachine.RunStateMachine();
            }
        }
        return 0;
    }
}