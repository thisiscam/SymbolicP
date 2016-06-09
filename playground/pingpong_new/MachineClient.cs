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
        this.Transitions = new TransitionFunction[,] {
                                {null, null, Init_SUCCESS},
                                {null, null, SendPing_SUCCESS},
                                {null, WaitPong_PONG, null},
                            };
    }
    public override void StartMachine(Scheduler s) {
        base.StartMachine(s);
        this.state = MachineClient_STATE_Init;
        this.InitEntry();
    }

    /* Transition Functions */
    private void Init_SUCCESS(object payload) {
        this.state = MachineClient_STATE_SendPing;
        this.SendPingEntry();
    }
    private void SendPing_SUCCESS(object payload) {
        this.state = MachineClient_STATE_WaitPong;
    }
    private void WaitPong_PONG(object payload) {
        this.state = MachineClient_STATE_SendPing;
        this.SendPingEntry();
    }

    /* Entry Functions */
    private void InitEntry() {
        this.server = NewMachine(new MachineServer());
        ServeEvent(SUCCESS, null); retcode = RAISED_EVENT; return;
    }
    private void SendPingEntry() {
        SendMsg(this.server, PING, this);
        ServeEvent(SUCCESS, null); retcode = RAISED_EVENT; return;
    }
}