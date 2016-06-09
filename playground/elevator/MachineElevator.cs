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
    };
    
    /* P local vars */
    PMachine TimerV, DoorV;

    public MachineElevator() {
        this.DeferedSet = _DeferedSet;
        this.Transitions = new TransitionFunction[10,19];
        this.Transitions[MachineElevator_STATE_Init, eUnit] = Init_eUnit;
        this.Transitions[MachineElevator_STATE_DoorClosed, eOpenDoor] = DoorClosed_eOpenDoor;
        this.Transitions[MachineElevator_STATE_DoorClosed, eCloseDoor] = Transition_Ignore;
        this.Transitions[MachineElevator_STATE_DoorOpening, eDoorOpened] = DoorOpening_eDoorOpened;
        this.Transitions[MachineElevator_STATE_DoorOpening, eOpenDoor] = Transition_Ignore;
        this.Transitions[MachineElevator_STATE_DoorOpened, eTimerFired] = DoorOpened_eTimerFired;
        this.Transitions[MachineElevator_STATE_DoorOpened, eStopTimerReturned ] = DoorOpened_eStopTimerReturned ;
        this.Transitions[MachineElevator_STATE_DoorOpened, eOpenDoor] = DoorOpened_eOpenDoor;
        this.Transitions[MachineElevator_STATE_DoorOpenedOkToClose, eStopTimerReturned] = DoorOpenedOkToClose_eStopTimerReturned_eTimerFired;
        this.Transitions[MachineElevator_STATE_DoorOpenedOkToClose, eTimerFired] = DoorOpenedOkToClose_eStopTimerReturned_eTimerFired;
        this.Transitions[MachineElevator_STATE_DoorOpenedOkToClose, eCloseDoor] = DoorOpenedOkToClose_eCloseDoor;
        this.Transitions[MachineElevator_STATE_DoorClosing, eOpenDoor] = DoorClosing_eOpenDoor;
        this.Transitions[MachineElevator_STATE_DoorClosing, eDoorClosed] = DoorClosing_eDoorClosed;
        this.Transitions[MachineElevator_STATE_DoorClosing, eObjectDetected] = DoorClosing_eObjectDetected;
        this.Transitions[MachineElevator_STATE_StoppingDoor, eDoorOpened] = StoppingDoor_eDoorOpened;
        this.Transitions[MachineElevator_STATE_StoppingDoor, eDoorClosed] = StoppingDoor_eDoorClosed;
        this.Transitions[MachineElevator_STATE_StoppingDoor, eDoorStopped] = StoppingDoor_eDoorStopped;
        this.Transitions[MachineElevator_STATE_StoppingDoor, eOpenDoor] = Transition_Ignore;
        this.Transitions[MachineElevator_STATE_StoppingDoor, eObjectDetected] = Transition_Ignore;
        this.Transitions[MachineElevator_STATE_StoppingTimer, eOperationSuccess] = StoppingTimer_eOperationSuccess;
        this.Transitions[MachineElevator_STATE_StoppingTimer, eOperationFailure] = StoppingTimer_eOperationFailure;
        this.Transitions[MachineElevator_STATE_WaitingForTimer, eTimerFired] = WaitingForTimer_eTimerFired;
    }

    public override void StartMachine(Scheduler s, object payload) {
        base.StartMachine(s, payload);
        this.state = MachineElevator_STATE_Init;
        this.InitEntry();    
    }
    
    /* Transition Functions */
    private void Init_eUnit(object payload) {
        this.state = MachineElevator_STATE_DoorClosed;
        this.DoorClosedEntry();
    }
    private void DoorClosed_eOpenDoor(object payload) {
        this.state = MachineElevator_STATE_DoorOpening;
        this.DoorOpeningEntry();
    }
    private void DoorOpening_eDoorOpened(object payload) {
        this.state = MachineElevator_STATE_DoorOpened;
        this.DoorOpenedEntry();
    }
    private void DoorOpened_eTimerFired(object payload) {
        this.state = MachineElevator_STATE_DoorOpenedOkToClose;
        this.DoorOpenedOkToCloseEntry();
    }
    private void DoorOpened_eStopTimerReturned(object payload) {
        this.state = MachineElevator_STATE_DoorOpened;
        this.DoorOpenedEntry();
    }
    private void DoorOpened_eOpenDoor(object payload) {
        this.state = MachineElevator_STATE_StoppingTimer;
        this.StoppingTimerEntry();
    }
    private void DoorOpenedOkToClose_eStopTimerReturned_eTimerFired(object payload) {
        this.state = MachineElevator_STATE_DoorClosing;
        this.DoorClosingEntry();
    }
    private void DoorOpenedOkToClose_eCloseDoor(object payload) {
        this.state = MachineElevator_STATE_StoppingTimer;
        this.StoppingTimerEntry();
    }
    private void DoorClosing_eOpenDoor(object payload) {
        this.state = MachineElevator_STATE_StoppingDoor;
        this.StoppingDoorEntry();
    }
    private void DoorClosing_eDoorClosed(object payload) {
        this.state = MachineElevator_STATE_DoorClosed;
        this.DoorClosedEntry();
    }
    private void DoorClosing_eObjectDetected(object payload) {
        this.state = MachineElevator_STATE_DoorOpening;
        this.DoorOpeningEntry();
    }
    private void StoppingDoor_eDoorOpened(object payload) {
        this.state = MachineElevator_STATE_DoorOpened;
        this.DoorOpenedEntry();
    }
    private void StoppingDoor_eDoorClosed(object payload) {
        this.state = MachineElevator_STATE_DoorClosed;
        this.DoorClosedEntry();
    }
    private void StoppingDoor_eDoorStopped(object payload) {
        this.state = MachineElevator_STATE_DoorOpening;
        this.DoorOpeningEntry();
    }
    private void StoppingTimer_eOperationSuccess(object payload) {
        this.state = MachineElevator_STATE_ReturnState;
        this.ReturnStateEntry();
    }
    private void StoppingTimer_eOperationFailure(object payload) {
        this.state = MachineElevator_STATE_WaitingForTimer;
        this.WaitingForTimerEntry();
    }
    private void WaitingForTimer_eTimerFired(object payload) {
        this.state = MachineElevator_STATE_ReturnState;
        this.ReturnStateEntry();
    }


    /* Entry Functions */
    private void InitEntry() {
        this.TimerV = NewMachine(new MachineTimer(), this);
        this.DoorV = NewMachine(new MachineDoor(), this);
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
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
        ServeEvent(eStopTimerReturned, null); retcode = RAISED_EVENT; return;
    }
}