#include "elevator_Macros.h"

using System;

class MachineTimer : PMachine {
    private readonly static bool[,] _DeferedSet = {
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,true ,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,true ,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,true ,false,false,false,false}
    }

    /* P local vars */
    PMachine ElevatorV;

    public MachineTimer() {
        this.DeferedSet = _DeferedSet;
    }

    public override void StartMachine(Scheduler s) {
        base.StartMachine(s);
        this.state = MachineServer_STATE__Init;
        this._InitEntry();
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
    private void _Init_eUnit() {
        this.state = MachineTimer_STATE_Init;
        this.InitEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void Init_eStartDoorCloseTimer() {
        this.state = MachineTimer_STATE_TimerStarted;
        this.TimerStartedEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void TimerStarted_eUnit() {
        this.state = MachineTimer_STATE_SendTimerFired;
        this.SendTimerFiredEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void TimerStarted_eStopDoorCloseTimer() {
        this.state = MachineTimer_STATE_ConsiderStopping;
        this.ConsiderStoppingEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void SendTimerFired_eUnit() {
        this.state = MachineTimer_STATE_Init;
        this.InitEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void ConsiderStopping_eUnit() {
        this.state = MachineTimer_STATE_Init;
        this.InitEntry();
        if (retcode == RAISED_EVENT) return;
    }

    /* Entry Functions */
    private void _InitEntry(PMachine payload) {
        this.ElevatorV = payload;
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
    }
    private void InitEntry() { }
    private void TimerStartedEntry() {
        if(RandomBool()) {
            ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
        }
    }
    private void SendTimerFiredEntry() {
        SendMsg(this.ElevatorV, eTimerFired, null);
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
    }
    private void ConsiderStoppingEntry() {
        if(RandomBool()) {
            SendMsg(this.ElevatorV, eOperationFailure, null);
            SendMsg(this.ElevatorV, eTimerFired, null);
        } else {
            SendMsg(this.ElevatorV, eOperationSuccess, null);
        }
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
    }

    // /* Raise Events */
    // private void SendPong_RaiseEvent(int e) {
    //     if (e == SUCCESS) {
    //         this.state = MachineServer_STATE_WaitPing;
    //     } else {
    //         throw new SystemException("Unhandled Event");
    //     }
    // }
}