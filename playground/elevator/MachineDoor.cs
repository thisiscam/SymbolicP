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
    }

    /* P local vars */
    PMachine ElevatorV;

    public MachineDoor() {
        this.DeferedSet = _DeferedSet;
    }
    public override void StartMachine(Scheduler s) {
        base.StartMachine(s);
        this.state = MachineDoor_STATE__Init;
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
        this.state = MachineDoor_STATE_Init;
        this.InitEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void Init_eSendCommandToOpenDoor() {
        this.state = MachineDoor_STATE_OpenDoor;
        this.OpenDoorEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void Init_eSendCommandToCloseDoor() {
        this.state = MachineDoor_STATE_ConsiderClosingDoor;
        this.ConsiderClosingDoorEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void ConsiderClosingDoor_eUnit() {
        this.state = MachineDoor_STATE_CloseDoor;
        this.CloseDoorEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void ConsiderClosingDoor_eObjectEncountered() {
        this.state = MachineDoor_STATE_ObjectEncountered;
        this.ObjectEncounteredEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void ConsiderClosingDoor_eSendCommandToStopDoor() {
        this.state = MachineDoor_STATE_StopDoor;
        this.StopDoorEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void ObjectEncountered_eUnit() {
        this.state = MachineDoor_STATE_Init;
        this.InitEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void CloseDoor_eUnit() {
        this.state = MachineDoor_STATE_ResetDoor;
        this.ResetDoorEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void StopDoor_eUnit() {
        this.state = MachineDoor_STATE_OpenDoor;
        this.OpenDoorEntry();
        if (retcode == RAISED_EVENT) return;
    }
    private void ResetDoor_eSendCommandToResetDoor() {
        this.state = MachineDoor_STATE_Init;
        this.InitEntry();
        if (retcode == RAISED_EVENT) return;
    }


    /* Entry Functions */
    private void _InitEntry(PMachine payload) {
        this.ElevatorV = payload;
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
    }
    private void InitEntry(PMachine payload) { }
    private void OpenDoorEntry() {
        SendMsg(this.ElevatorV, eDoorOpened, null);
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
    }
    private void ConsiderClosingDoorEntry() {
        if (RandBool()) {
            ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
        } else if (RandBool()) {
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

    // /* Raise Events */
    // private void SendPong_RaiseEvent(int e) {
    //     if (e == SUCCESS) {
    //         this.state = MachineServer_STATE_WaitPing;
    //     } else {
    //         throw new SystemException("Unhandled Event");
    //     }
    // }
}