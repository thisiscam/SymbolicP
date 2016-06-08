#include "elevator_Macros.h"

using System;

class MachineElevator : PMachine {
    private readonly static bool[,] _DeferedSet = {
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,true ,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,true ,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {true ,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,true ,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,true ,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {true ,true ,false,false,false,false,true ,false,false,false,false,false,false,false,false,false,false,false,false},
        {true ,true ,false,false,false,false,true ,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false}
    }
    
    /* P local vars */
    PMachine TimerV, DoorV;

    public MachineElevator() {
        this.DeferedSet = _DeferedSet;
    }
    public override void StartMachine(Scheduler s) {
        base.StartMachine(s);
        this.state = MachineServer_STATE_WaitPing;    
    }
    

    /* Transition Functions */
    private void Init_eUnit() {
        this.state = MachineElevator_STATE_DoorClosed;
        this.DoorClosedEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void DoorClosed_eOpenDoor() {
        this.state = MachineElevator_STATE_DoorOpening;
        this.DoorOpeningEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void DoorOpening_eDoorOpened() {
        this.state = MachineElevator_STATE_DoorOpened;
        this.DoorOpenedEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void DoorOpened_eTimerFired() {
        this.state = MachineElevator_STATE_DoorOpenedOkToClose;
        this.DoorOpenedOkToCloseEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void DoorOpened_eStopTimerReturned () {
        this.state = MachineElevator_STATE_DoorOpened;
        this.DoorOpenedEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void DoorOpened_eOpenDoor() {
        this.state = MachineElevator_STATE_StoppingTimer;
        this.StoppingTimerEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void DoorOpenedOkToClose_eStopTimerReturned_eTimerFired() {
        this.state = MachineElevator_STATE_DoorClosing;
        this.DoorClosingEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void DoorOpenedOkToClose_eCloseDoor() {
        this.state = MachineElevator_STATE_StoppingTimer;
        this.StoppingTimerEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void DoorClosing_eOpenDoor() {
        this.state = MachineElevator_STATE_StoppingDoor;
        this.StoppingDoorEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void DoorClosing_eDoorClosed() {
        this.state = MachineElevator_STATE_DoorClosed;
        this.DoorClosedEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void DoorClosing_eObjectDetected() {
        this.state = MachineElevator_STATE_DoorOpening;
        this.DoorOpeningEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void StoppingDoor_eDoorOpened() {
        this.state = MachineElevator_STATE_DoorOpened;
        this.DoorOpenedEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void StoppingDoor_eDoorClosed() {
        this.state = MachineElevator_STATE_DoorClosed;
        this.DoorClosedEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void StoppingDoor_eDoorStopped() {
        this.state = MachineElevator_STATE_DoorOpening;
        this.DoorOpeningEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void StoppingTimer_eOperationSuccess() {
        this.state = MachineElevator_STATE_ReturnState;
        this.ReturnStateEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void StoppingTimer_eOperationFailure() {
        this.state = MachineElevator_STATE_WaitingForTimer;
        this.WaitingForTimerEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void WaitingForTimer_eTimerFired() {
        this.state = MachineElevator_STATE_ReturnState;
        this.ReturnStateEntry();
        if (retcode == RAISED_EVENT) return;
    }


    /* Entry Functions */
    private void InitEntry() {
        this.TimerV = NewMachine(new MachineTimer(), this);
        this.DoorV = NewMachine(new MachineDoor(), this);
        ServeEvent(eUnit); retcode = RAISED_EVENT; return;
    }
    private void DoorClosedEntry() {
        SendMsg(this.DoorV, eSendCommandToResetDoor, null);
    }
    private void DoorOpeningEntry() {
        SendMsg(this.DoorV, eSendCommandToOpenDoor, null);
    }
    private void DoorOpenedEntry() {
        SendMsg(this.DoorV, eSendCommandToResetDoor, null);
        SendMsg(this.TimerV, eStartDoorCloseTimer, null);
    }
    private void DoorOpenedOkToCloseEntry() {
        SendMsg(this.TimerV, eStartDoorCloseTimer, null);
    }
    private void DoorClosingEntry() {
        SendMsg(this.DoorV, eSendCommandToCloseDoor, null);
    }
    private void StoppingDoorEntry() {
        SendMsg(this.DoorV, eSendCommandToStopDoor, null);
    }
    private void StoppingTimerEntry() {
        SendMsg(this.TimerV, eStopDoorCloseTimer, null);
    }
    private void WaitingForTimerEntry() {
        SendMsg(this.TimerV, eTimerFired, null);
    }
    private void ReturnStateEntry() {
        ServeEvent(eStopTimerReturned); retcode = RAISED_EVENT; return;
    }
}