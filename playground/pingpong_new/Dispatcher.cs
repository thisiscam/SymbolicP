#include "pingpong_Macros.h"


using System;
using System.Diagnostics;

class Dispatcher {
    
    public void StartMainMachine() {
        PMachine m = new MachineClient();
        Scheduler.machines.Add(m);
    }

    public void RunStateMachine(PMachine machine, int e, object payload) {
        if(machine is MachineClient) {
            if (machine.state == MachineClient_STATE_Init) {
                if (e == SUCCESS) {
                    machine.state = MachineClient_STATE_SendPing;
                    ((MachineClient)machine).SendPingEntry();
                } else {
                    throw new SystemException("Unhandled Event");
                }
            } else if (machine.state == MachineClient_STATE_SendPing) {
                if (e == SUCCESS) {
                    machine.state = MachineClient_STATE_WaitPong;
                } else {
                    throw new SystemException("Unhandled Event");
                }
            } else if (machine.state == MachineClient_STATE_WaitPong) {
                if (e == PONG) {
                    machine.state = MachineClient_STATE_SendPing;
                    ((MachineClient)machine).SendPingEntry();
                } else {
                    throw new SystemException("Unhandled Event");
                }
            } else {
                throw new SystemException("Should not be here");
            }
        } else if (machine is MachineServer) {
            if (machine.state == MachineServer_STATE_WaitPing) {
                if (e == PING) {
                    machine.state = MachineServer_STATE_SendPong;
                    ((MachineServer)machine).SendPongEntry((PMachine)payload);
                } else {
                    throw new SystemException("Unhandled Event");
                }
            } else if(machine.state == MachineServer_STATE_SendPong) {
                if (e == SUCCESS) {
                    machine.state = MachineServer_STATE_WaitPing;
                } else {
                    throw new SystemException("Unhandled Event");
                }
            }
        } else {
            throw new SystemException("Should not be here");
        }
    }
}