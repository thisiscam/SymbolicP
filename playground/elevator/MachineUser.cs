#include "pingpong_Macros.h"

using System;
using System.Diagnostics;

class MachineClient : PMachine {
    private readonly static bool[,] _DeferedSet = {
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false}
    }

    /* P local vars */
    PMachine ElevatorV;

    public MachineUser() {
        this.DeferedSet = _DeferedSet;
    }
    public override void StartMachine(Scheduler s) {
        base.StartMachine(s);
        this.state = MachineUser_STATE_Init;
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
    private void Init_eUnit () {
        this.state = MachineUser_STATE_Loop;
        this.LoopEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void Loop_eUnit() {
        this.state = MachineClient_STATE_Loop;
        this.LoopEntry();
        if (retcode == RAISED_EVENT) return;
    }
    

    /* Entry Functions */
    private void InitEntry() {
        this.ElevatorV = NewMachine(new MachineElevator());
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
    }
    private void LoopEntry() {
        if(RandBool()) {
            SendMsg(this.ElevatorV, eOpenDoor, null);
        } else if (RandBool()) {
            SendMsg(this.ElevatorV, eCloseDoor, null);
        }
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
    }

    // /* Raise Events */
    // private void Init_RaiseEvent(int e) {        
    //     if (e == SUCCESS) {
    //         this.state = MachineClient_STATE_SendPing;
    //         this.SendPingEntry();
    //     } else {
    //         throw new SystemException("Unhandled Event");
    //     }
    // }
    // private void SendPing_RaiseEvent(int e) {
    //     if (e == SUCCESS) {
    //         this.state = MachineClient_STATE_WaitPong;
    //     } else {
    //         throw new SystemException("Unhandled Event");
    //     }
    // }
}