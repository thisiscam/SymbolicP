#include "pingpong_Macros.h"

using System;

class MachineServer : PMachine {
    public MachineServer() {
        state = MachineServer_STATE_WaitPing;
    }
    public void SendPongEntry(PMachine payload) {
        sendMsg(payload, PONG, null);
        SendPong_RaiseEvent(SUCCESS); return;
    }
    public void SendPong_RaiseEvent(int e) {
        if (e == SUCCESS) {
            this.state = MachineServer_STATE_WaitPing;
        } else {
            throw new SystemException("Unhandled Event");
        }
    }
}