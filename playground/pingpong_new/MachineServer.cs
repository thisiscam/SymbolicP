#include "pingpong_Macros.h"

using System;

class MachineServer : PMachine {
    private readonly static bool[,] _DeferedSet = new bool[,] {
                                            {false, false, false},
                                            {false, false, false},
                                        };

    public MachineServer() {
        this.DeferedSet = _DeferedSet;
        this.Transitions = new TransitionFunction[,] {
                                {WaitPing_PING, null, null},
                                {null, null, SendPong_SUCCESS},
                            };
    }

    public override void StartMachine(Scheduler s, object payload) {
        base.StartMachine(s, payload);
        this.state = MachineServer_STATE_WaitPing;    
    }

    /* Transition Functions */
    private void WaitPing_PING(object payload) {
        this.state = MachineServer_STATE_SendPong;
        this.SendPongEntry((PMachine)payload);
    }
    private void SendPong_SUCCESS(object payload) {
        this.state = MachineServer_STATE_WaitPing;
    }

    /* Entry Functions */
    private void SendPongEntry(PMachine payload) {
        SendMsg(payload, PONG, null);
        ServeEvent(SUCCESS, null); retcode = RAISED_EVENT; return;
    }
}