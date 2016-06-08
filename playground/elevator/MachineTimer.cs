#include "elevator_Macros.h"

using System;

class MachineTimer : PMachine {
    private readonly static bool[,] _DeferedSet = {
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,true ,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,true ,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,true ,false,false,false,false}
    };

    /* P local vars */
    PMachine ElevatorV;

    public MachineTimer() {
        this.DeferedSet = _DeferedSet;
        this.Transitions = new TransitionFunction[5, 19];
        this.Transitions[MachineTimer_STATE__Init, eUnit] = _Init_eUnit;
        this.Transitions[MachineTimer_STATE_Init, eStartDoorCloseTimer] = Init_eStartDoorCloseTimer;
        this.Transitions[MachineTimer_STATE_TimerStarted, eUnit] = TimerStarted_eUnit;
        this.Transitions[MachineTimer_STATE_TimerStarted, eStopDoorCloseTimer] = TimerStarted_eStopDoorCloseTimer;
        this.Transitions[MachineTimer_STATE_SendTimerFired, eUnit] = SendTimerFired_eUnit;
        this.Transitions[MachineTimer_STATE_ConsiderStopping, eUnit] = ConsiderStopping_eUnit;
    }

    public override void StartMachine(Scheduler s, object payload) {
        base.StartMachine(s, payload);
        this.state = MachineTimer_STATE__Init;
        this._InitEntry((PMachine)payload);
    }

    /* Transition Functions */
    private void _Init_eUnit(object payload) {
        this.state = MachineTimer_STATE_Init;
        this.InitEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void Init_eStartDoorCloseTimer(object payload) {
        this.state = MachineTimer_STATE_TimerStarted;
        this.TimerStartedEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void TimerStarted_eUnit(object payload) {
        this.state = MachineTimer_STATE_SendTimerFired;
        this.SendTimerFiredEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void TimerStarted_eStopDoorCloseTimer(object payload) {
        this.state = MachineTimer_STATE_ConsiderStopping;
        this.ConsiderStoppingEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void SendTimerFired_eUnit(object payload) {
        this.state = MachineTimer_STATE_Init;
        this.InitEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void ConsiderStopping_eUnit(object payload) {
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
}