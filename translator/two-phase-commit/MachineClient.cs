using System;
using System.Collections.Generic;
using System.Diagnostics;

class MachineClient : PMachine {

private readonly static bool[,] _DeferedSet = {
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
};

private readonly static bool[,] _IsGotoTransition = {
{false,false,false,false,false,false,false,false,false,false,false,false,false,true, false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,false,true, true, false,false,false,false,false,false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,false,false,false,false,true, false,true, false,false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
};
/* P local vars */
PMachine coordinator=null;
PInteger val=new PInteger(0);
PInteger idx=new PInteger(0);


public MachineClient () {
this.DeferedSet = _DeferedSet;
this.IsGotoTransition = _IsGotoTransition;
this.Transitions = new TransitionFunction[4,22];
this.Transitions[Constants.MachineClient_S_DoRead,Constants.READ_SUCCESS] = DoRead_on_READ_SUCCESS_READ_FAIL;
this.Transitions[Constants.MachineClient_S_DoRead,Constants.READ_FAIL] = DoRead_on_READ_SUCCESS_READ_FAIL;
this.Transitions[Constants.MachineClient_S_DoWrite,Constants.WRITE_SUCCESS] = DoWrite_on_WRITE_SUCCESS;
this.Transitions[Constants.MachineClient_S_DoWrite,Constants.WRITE_FAIL] = DoWrite_on_WRITE_FAIL;
this.Transitions[Constants.MachineClient_S_Init,Constants.Unit] = Init_on_Unit;
this.ExitFunctions = new ExitFunction[4];
}
public override void StartMachine (Scheduler scheduler, IPType payload) {
this.scheduler = scheduler;
this.states.Insert(0, Constants.MachineClient_S_Init);
Init_Entry((PMachine)payload);
}
private void DoRead_on_READ_SUCCESS_READ_FAIL (IPType payload) {
this.states[0] = Constants.MachineClient_S_End;
}
private void DoWrite_on_WRITE_SUCCESS (IPType payload) {
this.states[0] = Constants.MachineClient_S_DoRead;
DoRead_Entry();
}
private void DoWrite_on_WRITE_FAIL (IPType payload) {
this.states[0] = Constants.MachineClient_S_End;
}
private void Init_on_Unit (IPType payload) {
this.states[0] = Constants.MachineClient_S_DoWrite;
DoWrite_Entry();
}
private void Init_Entry (PMachine payload) {
coordinator = payload;
this.RaiseEvent(Constants.Unit, null); retcode = Constants.RAISED_EVENT; return;
}
private void DoRead_Entry () {
this.SendMsg(coordinator,Constants.READ_REQ,new PTuple<PMachine,PInteger>(this, idx));
}
private void DoWrite_Entry () {
idx = ChooseIndex();
val = ChooseValue();
this.SendMsg(coordinator,Constants.WRITE_REQ,new PTuple<PMachine,PInteger,PInteger>(this, idx,val));
}
private PInteger ChooseIndex () {
if(RandomBool()) {
return new PInteger(0);
} else {
return new PInteger(1);
}
}
private PInteger ChooseValue () {
if(RandomBool()) {
return new PInteger(0);
} else {
return new PInteger(1);
}
}
}