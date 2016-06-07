#include "pingpong_Macros.h"

using System;

class MachineServer : PMachine {
    public MachineServer() {
        state = 0;
    }
    public void SendPongEntry(object[] payload) {
        PMachine arg0 = (PMachine)payload[0];
        SendMsg(arg0, PONG, null);
        SendPong_RaiseEvent(SUCCESS); return;
    }
    public void SendPong_RaiseEvent(int e) {
        int pc = 0;
        while(true) {
        switch(pc) {
            case 0:     if(e != SUCCESS) {pc = 3 ; break;}                  
                        this.state = Inst.Set_Prop<int>(this.state, MachineServer_STATE_WaitPing);                              
                        pc = -1; break;                                 
            CASE(3):    throw new SystemException("Unhandled event");   
            CASE(-1):   return;
        }}
    }
}