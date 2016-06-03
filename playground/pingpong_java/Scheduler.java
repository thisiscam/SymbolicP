import java.util.*;

class Scheduler {

    public static List<PMachine> machines = new ArrayList<PMachine>();

    static int pickEvent(PMachine machine) {
        return 0;
    }

    static PMachine chooseMachine() {
        //TODO, this is a very naive implementation
        for(int i=0; i < machines.size(); i++) {
            if(machines.get(i).events.size() > 0) {
                PMachine m = machines.get(i);
                return m;
            }
        }
        return null;
    }

    public static void main(String[] args) {
        Dispatcher d = new Dispatcher();
        d.StartMainMachine();
        while(true) {
            PMachine machine = chooseMachine();
            if (machine == null) {
                break;
            }
            int e_idx = pickEvent(machine);
            int e = machine.events.remove(e_idx);
            Object payload = machine.payloads.remove(e_idx);
            d.RunStateMachine(machine, e, payload);
        }
    }
}