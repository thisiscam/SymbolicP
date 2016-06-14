#include "elevator_Macros.h"

using System;
using System.Diagnostics;

class MachineUser : PMachine {
    private readonly static bool[,] _DeferedSet = {
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false}
    };

    /* P local vars */
    PMachine ElevatorV;

    public MachineUser() {
        this.DeferedSet = _DeferedSet;
        this.Transitions = new TransitionFunction[2, 19];
        this.Transitions[MachineUser_STATE_Init, eUnit] = Init_eUnit;
        this.Transitions[MachineUser_STATE_Loop, eUnit] = Loop_eUnit;
    }

    public override void StartMachine(Scheduler s, object payload) {
        base.StartMachine(s, payload);
        this.state = MachineUser_STATE_Init;
        this.InitEntry();
    }

    /* Transition Functions */
    private void Init_eUnit(object payload) {
        this.state = MachineUser_STATE_Loop;
        this.LoopEntry();
    }
    private void Loop_eUnit(object payload) {
        this.state = MachineUser_STATE_Loop;
        this.LoopEntry();
    }

    /* Entry Functions */
    private void InitEntry() {
        this.ElevatorV = NewMachine(new MachineElevator(), null);
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
    }
    private void LoopEntry() {
        if(RandomBool()) {
            SendMsg(this.ElevatorV, eOpenDoor, null);
        } else if (RandomBool()) {
            SendMsg(this.ElevatorV, eCloseDoor, null);
        }
        ServeEvent(eUnit, null); retcode = RAISED_EVENT; return;
    }
}