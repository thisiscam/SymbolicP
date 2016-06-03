using System.Collections.Generic;
using System.Diagnostics;

class Scheduler {

    public static List<PMachine> machines = new List<PMachine>();

    static int pickEvent(PMachine machine) {
        return machine.events.Count - 1;
    }

    static PMachine chooseMachine() {
        //TODO, this is a very naive implementation
        for(int i=0; i < machines.Count; i++) {
            if(machines[i].events.Count > 0) {
                PMachine m = machines[i];
                return m;
            }
        }
        return null;
    }

    static int Main(string[] args) {
        Dispatcher d = new Dispatcher();
        d.StartMainMachine();
        while(true) {
            PMachine machine = chooseMachine();
            if (machine == null) {
                break;
            }
            int e_idx = pickEvent(machine);
            int e = machine.events[e_idx];
            object payload = machine.payloads[e_idx];
            machine.events.RemoveAt(e_idx);
            machine.payloads.RemoveAt(e_idx);
            d.RunStateMachine(machine, e, payload);
        }
        return 0;
    }
}