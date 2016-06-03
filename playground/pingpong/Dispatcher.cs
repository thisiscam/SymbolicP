#include "pingpong_Macros.h"


using System;
using System.Diagnostics;

class Dispatcher {
    public void StartMainMachine() {
        PMachine m = new MachineClient();
        Scheduler.machines.Add(m);
    }

    public void RunStateMachine(PMachine machine, int e, object[] payload) {
        int L0 = -1, L1 = -1;
        int pc = 0;
        while(true) {
        switch(pc) {
        case  0:        if (!(machine is MachineServer)) {pc = 13; break;}      
                        L0 = Inst.Get_Prop<int>(machine.state);                                 
                        if(L0 != MachineServer_STATE_WaitPing) {pc = 8; break;}                         
                        if(e != PING) {pc = 7; break;}                              
                        machine.state = Inst.Set_Prop<int>(machine.state, MachineServer_STATE_SendPong);                                        
                        ((MachineServer)machine).SendPongEntry(payload);        
                        pc = 32; break;                                         
        CASE(7):        throw new SystemException("Unhandled event");           
        CASE(8):        /** else L0 = MachineServer_STATE_SendPong */                                       
                        if(e != SUCCESS) {pc = 11 ; break;}                         
                        machine.state = Inst.Set_Prop<int>(machine.state, MachineServer_STATE_WaitPing);            
        CASE(11):       pc = 32; break;                                         
                        throw new SystemException("Unhandled event");           
        CASE(13):       /** else typeof(machine) == MachineClient  */           
                        L1 = Inst.Get_Prop<int>(machine.state);                                 
                        if(L1 != MachineClient_STATE_Init) {pc = 21; break;}                            
                        if (e != SUCCESS) {pc = 20; break;}                         
                        machine.state = Inst.Set_Prop<int>(machine.state, MachineClient_STATE_SendPing);                                        
                        ((MachineClient)machine).SendPingEntry();               
                        pc = 32; break;                                                 
        CASE(20):       throw new SystemException("Unhandled event");           
        CASE(21):       if (L1 != MachineClient_STATE_SendPing) {pc = 26; break;}                           
                        if(e != SUCCESS) {pc = 25; break;}                          
                        machine.state = Inst.Set_Prop<int>(machine.state, MachineClient_STATE_WaitPong);                                        
                        pc = 32; break;                                         
        CASE(25):       throw new SystemException("Unhandled event");           
        CASE(26):       /* else L1 = MachineClient_STATE_WaitPong */                                        
                        if(e != PONG) {pc = 31; break;}                         
                        machine.state = Inst.Set_Prop<int>(machine.state, MachineClient_STATE_SendPing);                                    
                        ((MachineClient)machine).SendPingEntry();               
                        pc = 32; break;                                         
        CASE(31):       throw new SystemException("Unhandled event");           
        CASE(32):       return;                                                     
        }}
    }
}