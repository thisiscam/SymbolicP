#include "pingpong_Macros.h"

using System;
using System.Diagnostics;

class MachineClient : PMachine {
    private readonly static bool[,] _DeferedSet = new bool[,] {
                                            {false, false, false},
                                            {false, false, false},
                                            {false, false, false}
                                        };

    /* P local vars */
    PMachine server;

    public MachineClient() {
        this.DeferedSet = _DeferedSet;
    }
    public override void StartMachine() {
        this.state = MachineClient_STATE_Init;
        this.InitEntry();
    }
    protected override void ServeEvent (int e, object payload) {
        if (this.state == MachineClient_STATE_Init) {
            if (e == SUCCESS) {
                this.Init_SUCCESS(); 
                if (retcode == RAISED_EVENT) return; 
            } else {
                throw new SystemException("Unhandled Event");
            }
        } else if (this.state == MachineClient_STATE_SendPing) {
            if (e == SUCCESS) {
                this.SendPing_SUCCESS(); 
                if (retcode == RAISED_EVENT) return; 
            } else {
                throw new SystemException("Unhandled Event");
            }
        } else if (this.state == MachineClient_STATE_WaitPong) {
            if (e == PONG) {
                this.WaitPong_PONG(); 
                if (retcode == RAISED_EVENT) return; 
            } else {
                throw new SystemException("Unhandled Event");
            }
        } else {
            throw new SystemException("Should not be here");
        }
    }

    /* Transition Functions */
    private void Init_SUCCESS () {
        this.state = MachineClient_STATE_SendPing;
        this.SendPingEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void SendPing_SUCCESS() {
        this.state = MachineClient_STATE_WaitPong;
    }
    private void WaitPong_PONG() {
        this.state = MachineClient_STATE_SendPing;
        this.SendPingEntry();
        if (retcode == RAISED_EVENT) return;
    }

    /* Entry Functions */
    private void InitEntry() {
        this.server = newMachine(new MachineServer()); // Alternatively could have passed in type 'MachineServer', and call constructor in newMachine; 
        Init_RaiseEvent(SUCCESS); retcode = RAISED_EVENT; return;
    }
    private void SendPingEntry() {
        sendMsg(this.server, PING, this);
        SendPing_RaiseEvent(SUCCESS); retcode = RAISED_EVENT; return;
    }

    /* Raise Events */
    private void Init_RaiseEvent(int e) {        
        if (e == SUCCESS) {
            this.state = MachineClient_STATE_SendPing;
            this.SendPingEntry();
        } else {
            throw new SystemException("Unhandled Event");
        }
    }
    private void SendPing_RaiseEvent(int e) {
        if (e == SUCCESS) {
            this.state = MachineClient_STATE_WaitPong;
        } else {
            throw new SystemException("Unhandled Event");
        }
    }
}