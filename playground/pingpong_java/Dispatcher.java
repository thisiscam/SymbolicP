#include "pingpong_Macros.h"



class Dispatcher {
    
    public void StartMainMachine() {
        PMachine m = new MachineClient();
        Scheduler.machines.add(m);
    }

    public void RunStateMachine(PMachine machine, int e, Object payload) {
        if(machine instanceof MachineClient) {
            if (machine.state == MachineClient_STATE_Init) {
                if (e == SUCCESS) {
                    machine.state = MachineClient_STATE_SendPing;
                    ((MachineClient)machine).SendPingEntry();
                } else {
                    throw new RuntimeException("Unhandled Event");
                }
            } else if (machine.state == MachineClient_STATE_SendPing) {
                if (e == SUCCESS) {
                    machine.state = MachineClient_STATE_WaitPong;
                } else {
                    throw new RuntimeException("Unhandled Event");
                }
            } else if (machine.state == MachineClient_STATE_WaitPong) {
                if (e == PONG) {
                    machine.state = MachineClient_STATE_SendPing;
                    ((MachineClient)machine).SendPingEntry();
                } else {
                    throw new RuntimeException("Unhandled Event");
                }
            } else {
                throw new RuntimeException("Should not be here");
            }
        } else if (machine instanceof MachineServer) {
            if (machine.state == MachineServer_STATE_WaitPing) {
                if (e == PING) {
                    machine.state = MachineServer_STATE_SendPong;
                    ((MachineServer)machine).SendPongEntry((PMachine)payload);
                } else {
                    throw new RuntimeException("Unhandled Event");
                }
            } else if(machine.state == MachineServer_STATE_SendPong) {
                if (e == SUCCESS) {
                    machine.state = MachineServer_STATE_WaitPing;
                } else {
                    throw new RuntimeException("Unhandled Event");
                }
            }
        } else {
            throw new RuntimeException("Should not be here");
        }
    }
}