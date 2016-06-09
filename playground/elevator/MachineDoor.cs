#include "elevator_Macros.h"

using System;

class MachineDoor : PMachine {
    private readonly static bool[,] _DeferedSet = {
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false}
    };

    /* P local vars */
    PMachine ElevatorV;

    public MachineDoor() {
        this.DeferedSet = _DeferedSet;
        this.Transitions = new TransitionFunction[8, 19];
        this.Transitions[MachineDoor_STATE__Init, eUnit] = _Init_eUnit;
        this.Transitions[MachineDoor_STATE_Init, eSendCommandToOpenDoor] = Init_eSendCommandToOpenDoor;
        this.Transitions[MachineDoor_STATE_Init, eSendCommandToCloseDoor] = Init_eSendCommandToCloseDoor;
        this.Transitions[MachineDoor_STATE_Init, eSendCommandToStopDoor] = Transition_Ignore;
        this.Transitions[MachineDoor_STATE_Init, eSendCommandToResetDoor] = Transition_Ignore;
        this.Transitions[MachineDoor_STATE_Init, eResetDoor] = Transition_Ignore;
        this.Transitions[MachineDoor_STATE_ConsiderClosingDoor, eUnit] = ConsiderClosingDoor_eUnit;
        this.Transitions[MachineDoor_STATE_ConsiderClosingDoor, eObjectEncountered] = ConsiderClosingDoor_eObjectEncountered;
        this.Transitions[MachineDoor_STATE_ConsiderClosingDoor, eSendCommandToStopDoor] = ConsiderClosingDoor_eSendCommandToStopDoor;
        this.Transitions[MachineDoor_STATE_ObjectEncountered, eUnit] = ObjectEncountered_eUnit;
        this.Transitions[MachineDoor_STATE_CloseDoor, eUnit] = CloseDoor_eUnit;
        this.Transitions[MachineDoor_STATE_StopDoor, eUnit] = StopDoor_eUnit;
        this.Transitions[MachineDoor_STATE_ResetDoor, eSendCommandToResetDoor] = ResetDoor_eSendCommandToResetDoor;
        this.Transitions[MachineDoor_STATE_ResetDoor, eSendCommandToOpenDoor] = Transition_Ignore;
        this.Transitions[MachineDoor_STATE_ResetDoor, eSendCommandToCloseDoor] = Transition_Ignore;
        this.Transitions[MachineDoor_STATE_ResetDoor, eSendCommandToStopDoor] = Transition_Ignore;
    }

    public override void StartMachine(Scheduler s, object payload) {
        base.StartMachine(s, payload);
        this.state = MachineDoor_STATE__Init;
        this._InitEntry((PMachine)payload);    
    }

    /* Transition Functions */
    private void _Init_eUnit(object payload) {
        this.state = MachineDoor_STATE_Init;
        this.InitEntry();
    }
    private void Init_eSendCommandToOpenDoor(object payload) {
        this.state = MachineDoor_STATE_OpenDoor;
        this.OpenDoorEntry();
    }
    private void Init_eSendCommandToCloseDoor(object payload) {
        this.state = MachineDoor_STATE_ConsiderClosingDoor;
        this.ConsiderClosingDoorEntry();
    }
    private void ConsiderClosingDoor_eUnit(object payload) {
        this.state = MachineDoor_STATE_CloseDoor;
        this.CloseDoorEntry();
    }
    private void ConsiderClosingDoor_eObjectEncountered(object payload) {
        this.state = MachineDoor_STATE_ObjectEncountered;
        this.ObjectEncounteredEntry();
    }
    private void ConsiderClosingDoor_eSendCommandToStopDoor(object payload) {
        this.state = MachineDoor_STATE_StopDoor;
        this.StopDoorEntry();
    }
    private void ObjectEncountered_eUnit(object payload) {
        this.state = MachineDoor_STATE_Init;
        this.InitEntry();
    }
    private void CloseDoor_eUnit(object payload) {
        this.state = MachineDoor_STATE_ResetDoor;
        this.ResetDoorEntry();
    }
    private void StopDoor_eUnit(object payload) {
        this.state = MachineDoor_STATE_OpenDoor;
        this.OpenDoorEntry();
    }
    private void ResetDoor_eSendCommandToResetDoor(object payload) {
        this.state = MachineDoor_STATE_Init;
        this.InitEntry();
    }


    /* Entry Functions */
    private void _InitEntry(PMachine payload) {
        this.ElevatorV = payload;
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
    }
    private void InitEntry() { }
    private void OpenDoorEntry() {
        SendMsg(this.ElevatorV, eDoorOpened, null);
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
    }
    private void ConsiderClosingDoorEntry() {
        if (RandomBool()) {
            ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
        } else if (RandomBool()) {
            ServeEvent(eObjectEncountered, null); retcode = RAISED_EVENT; return;
        }
    }
    private void ObjectEncounteredEntry() {
        SendMsg(this.ElevatorV, eObjectDetected, null);
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
    }
    private void CloseDoorEntry() {
        SendMsg(this.ElevatorV, eDoorClosed, null);
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
    }
    private void StopDoorEntry() {
        SendMsg(this.ElevatorV, eDoorStopped, null);
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
    }
    private void ResetDoorEntry() { }
}