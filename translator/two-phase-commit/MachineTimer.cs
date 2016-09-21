using System;
using System.Collections.Generic;
using System.Diagnostics;

class MachineTimer : PMachine {

private readonly static bool[,] _DeferedSet = {
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
};

private readonly static bool[,] _IsGotoTransition = {
{false,false,false,false,false,false,false,false,false,false,false,false,false,true, false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,true, false,false,false,false,false,false,},
{false,false,false,false,false,false,false,false,false,false,false,false,false,true, false,false,true, false,false,false,false,false,},
};
/* P local vars */
PMachine target=null;


public MachineTimer () {
this.DeferedSet = _DeferedSet;
this.IsGotoTransition = _IsGotoTransition;
this.Transitions = new TransitionFunction[3,22];
this.Transitions[Constants.MachineTimer_S_TimerStarted,Constants.CancelTimer] = TimerStarted_on_CancelTimer;
this.Transitions[Constants.MachineTimer_S_Init,Constants.Unit] = Init_on_Unit;
this.Transitions[Constants.MachineTimer_S_Loop,Constants.StartTimer] = Loop_on_StartTimer;
this.Transitions[Constants.MachineTimer_S_TimerStarted,Constants.Unit] = TimerStarted_on_Unit;
this.Transitions[Constants.MachineTimer_S_Loop,Constants.CancelTimer] = Transition_Ignore;
this.ExitFunctions = new ExitFunction[3];
}
public override void StartMachine (Scheduler scheduler, IPType payload) {
this.scheduler = scheduler;
this.states.Insert(0, Constants.MachineTimer_S_Init);
Init_Entry((PMachine)payload);
}
private void Init_on_Unit (IPType payload) {
this.states[0] = Constants.MachineTimer_S_Loop;
}
private void Loop_on_StartTimer (IPType payload) {
this.states[0] = Constants.MachineTimer_S_TimerStarted;
TimerStarted_Entry();
}
private void TimerStarted_on_Unit (IPType payload) {
this.states[0] = Constants.MachineTimer_S_Loop;
}
private void Init_Entry (PMachine payload) {
target = payload;
this.RaiseEvent(Constants.Unit, null); retcode = Constants.RAISED_EVENT; return;
}
private void TimerStarted_on_CancelTimer (IPType _payload) {
if(RandomBool()) {
this.SendMsg(target,Constants.CancelTimerFailure,null);
this.SendMsg(target,Constants.Timeout,null);
} else {
this.SendMsg(target,Constants.CancelTimerSuccess,null);
}
this.states[0] = Constants.MachineTimer_S_Loop;
}
private void TimerStarted_Entry () {
if(RandomBool()) {
this.SendMsg(target,Constants.Timeout,null);
this.RaiseEvent(Constants.Unit, null); retcode = Constants.RAISED_EVENT; return;
}
}
}