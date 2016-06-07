#include "pingpong_Macros.h"

using System;

class MachineServer : PMachine {
    private readonly static bool[,] _DeferedSet = new bool[,] {
                                            {false, false, false},
                                            {false, false, false},
                                        };
    public MachineServer() {
        this.DeferedSet = _DeferedSet;
    }
    public override void StartMachine() {
        this.state = MachineServer_STATE_WaitPing;    
    }
    protected override void ServeEvent (int e, object payload) {
        if (this.state == MachineServer_STATE_WaitPing) {
            if (e == PING) {
                this.WaitPing_PING(payload);
                if (retcode == RAISED_EVENT) return;
            } else {
                throw new SystemException("Unhandled Event");
            }
        } else if(this.state == MachineServer_STATE_SendPong) {
            if (e == SUCCESS) {
                this.SendPong_SUCCESS();
                if (retcode == RAISED_EVENT) return;
            } else {
                throw new SystemException("Unhandled Event");
            }
        }
    }

    /* Transition Functions */
    private void WaitPing_PING(object payload) {
        this.state = MachineServer_STATE_SendPong;
        this.SendPongEntry((PMachine)payload);
        if (retcode == RAISED_EVENT) return;
    }
    private void SendPong_SUCCESS() {
        this.state = MachineServer_STATE_WaitPing;
    }

    /* Entry Functions */
    private void SendPongEntry(PMachine payload) {
        sendMsg(payload, PONG, null);
        SendPong_RaiseEvent(SUCCESS); retcode = RAISED_EVENT; return;
    }

    /* Raise Events */
    private void SendPong_RaiseEvent(int e) {
        if (e == SUCCESS) {
            this.state = MachineServer_STATE_WaitPing;
        } else {
            throw new SystemException("Unhandled Event");
        }
    }
}