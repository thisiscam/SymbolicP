using System;
using System.Collections.Generic;
using System.Diagnostics;

class MachineTwoPhaseCommit : PMachine {

private readonly static bool[,] _DeferedSet = {
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
};

private readonly static bool[,] _IsGotoTransition = {
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
};
/* P local vars */
PMachine coordinator=null;
PMachine client=null;


public MachineTwoPhaseCommit () {
this.DeferedSet = _DeferedSet;
this.IsGotoTransition = _IsGotoTransition;
this.Transitions = new TransitionFunction[1,22];
this.ExitFunctions = new ExitFunction[1];
}
public override void StartMachine (Scheduler scheduler, IPType payload) {
this.scheduler = scheduler;
this.states.Insert(0, Constants.MachineTwoPhaseCommit_S_Init);
Init_Entry();
}
private void Init_Entry () {
coordinator = NewMachine(new MachineCoordinator(), new PInteger(2));
client = NewMachine(new MachineClient(), coordinator);
client = NewMachine(new MachineClient(), coordinator);
}
}