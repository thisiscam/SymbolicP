using System;
using System.Collections.Generic;
using System.Diagnostics;

class MachineCoordinator : PMachine {

private readonly static bool[,] _DeferedSet = {
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,true ,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,true ,false,false,true ,false,false,false,false,false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,true ,false,false,true ,false,false,false,false,false,false,false,false,false,false,false,false,},
};

private readonly static bool[,] _IsGotoTransition = {
{false,false,false,false,false,false,false,false,false,false,false,false,false,true, false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,false,false,false,false,false,false,false,true, false,false,false,false,false,false,false,false,},
{false,false,true, false,false,false,false,false,false,false,false,false,false,true, true, false,false,false,false,false,false,false,},
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,true, false,false,true, true, false,false,false,},
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,true, false,false,false,false,false,false,false,},
};
/* P local vars */
PMachine replica=null;
PInteger numReplicas=new PInteger(0);
PInteger currSeqNum=new PInteger(0);
PList<PMachine> replicas=new PList<PMachine>();
PInteger i=new PInteger(0);
PMap<PInteger,PInteger> data=new PMap<PInteger,PInteger>();
PTuple<PMachine,PInteger,PInteger> pendingWriteReq=new PTuple<PMachine,PInteger,PInteger>(null,new PInteger(0),new PInteger(0));
PMachine timer=null;


public MachineCoordinator () {
this.DeferedSet = _DeferedSet;
this.IsGotoTransition = _IsGotoTransition;
this.Transitions = new TransitionFunction[5,22];
this.Transitions[Constants.MachineCoordinator_S_CountVote,Constants.Unit] = CountVote_on_Unit;
this.Transitions[Constants.MachineCoordinator_S_Loop,Constants.READ_REQ] = Loop_on_READ_REQ;
this.Transitions[Constants.MachineCoordinator_S_CountVote,Constants.READ_REQ] = CountVote_on_READ_REQ;
this.Transitions[Constants.MachineCoordinator_S_Loop,Constants.WRITE_REQ] = Loop_on_WRITE_REQ;
this.Transitions[Constants.MachineCoordinator_S_WaitForCancelTimerResponse,Constants.CancelTimerFailure] = WaitForCancelTimerResponse_on_CancelTimerFailure;
this.Transitions[Constants.MachineCoordinator_S_CountVote,Constants.RESP_REPLICA_COMMIT] = CountVote_on_RESP_REPLICA_COMMIT;
this.Transitions[Constants.MachineCoordinator_S_WaitForCancelTimerResponse,Constants.CancelTimerSuccess] = WaitForCancelTimerResponse_on_CancelTimerSuccess_Timeout;
this.Transitions[Constants.MachineCoordinator_S_WaitForCancelTimerResponse,Constants.Timeout] = WaitForCancelTimerResponse_on_CancelTimerSuccess_Timeout;
this.Transitions[Constants.MachineCoordinator_S_WaitForTimeout,Constants.Timeout] = WaitForTimeout_on_Timeout;
this.Transitions[Constants.MachineCoordinator_S_CountVote,Constants.Timeout] = CountVote_on_Timeout;
this.Transitions[Constants.MachineCoordinator_S_Loop,Constants.Unit] = Loop_on_Unit;
this.Transitions[Constants.MachineCoordinator_S_CountVote,Constants.RESP_REPLICA_ABORT] = CountVote_on_RESP_REPLICA_ABORT;
this.Transitions[Constants.MachineCoordinator_S_Init,Constants.Unit] = Init_on_Unit;
this.Transitions[Constants.MachineCoordinator_S_Loop,Constants.RESP_REPLICA_COMMIT] = Transition_Ignore;
this.Transitions[Constants.MachineCoordinator_S_Loop,Constants.RESP_REPLICA_ABORT] = Transition_Ignore;
this.Transitions[Constants.MachineCoordinator_S_WaitForCancelTimerResponse,Constants.RESP_REPLICA_COMMIT] = Transition_Ignore;
this.Transitions[Constants.MachineCoordinator_S_WaitForCancelTimerResponse,Constants.RESP_REPLICA_ABORT] = Transition_Ignore;
this.Transitions[Constants.MachineCoordinator_S_WaitForTimeout,Constants.RESP_REPLICA_COMMIT] = Transition_Ignore;
this.Transitions[Constants.MachineCoordinator_S_WaitForTimeout,Constants.RESP_REPLICA_ABORT] = Transition_Ignore;
this.ExitFunctions = new ExitFunction[5];
}
public override void StartMachine (Scheduler scheduler, IPType payload) {
this.scheduler = scheduler;
this.states.Insert(0, Constants.MachineCoordinator_S_Init);
Init_Entry((PInteger)payload);
}
private void CountVote_on_Unit (IPType payload) {
this.states[0] = Constants.MachineCoordinator_S_WaitForCancelTimerResponse;
}
private void WaitForCancelTimerResponse_on_CancelTimerFailure (IPType payload) {
this.states[0] = Constants.MachineCoordinator_S_WaitForTimeout;
}
private void WaitForCancelTimerResponse_on_CancelTimerSuccess_Timeout (IPType payload) {
this.states[0] = Constants.MachineCoordinator_S_Loop;
}
private void WaitForTimeout_on_Timeout (IPType payload) {
this.states[0] = Constants.MachineCoordinator_S_Loop;
}
private void Loop_on_Unit (IPType payload) {
this.states[0] = Constants.MachineCoordinator_S_CountVote;
CountVote_Entry();
}
private void Init_on_Unit (IPType payload) {
this.states[0] = Constants.MachineCoordinator_S_Loop;
}
private void Init_Entry (PInteger payload) {
numReplicas = payload;
Assert((numReplicas > new PInteger(0)));
i = new PInteger(0);
while(i < numReplicas) {
replica = NewMachine(new MachineReplica(), this);
replicas.Insert(new PTuple<PInteger,PMachine>(i,replica));
i = i + new PInteger(1);
}
currSeqNum = new PInteger(0);
timer = NewMachine(new MachineTimer(), this);
this.RaiseEvent(Constants.Unit, null); retcode = Constants.RAISED_EVENT; return;
}
private void CountVote_on_RESP_REPLICA_COMMIT (IPType _payload) {
PInteger payload = (PInteger)_payload;
if(currSeqNum == payload) {
i = i - new PInteger(1);
}
this.states[0] = Constants.MachineCoordinator_S_CountVote;
CountVote_Entry();
}
private void Loop_on_WRITE_REQ (IPType _payload) {
PTuple<PMachine,PInteger,PInteger> payload = (PTuple<PMachine,PInteger,PInteger>)_payload;
DoWrite(payload); if(retcode == Constants.RAISED_EVENT) { return ; }

}
private void CountVote_Entry () {
if(i == new PInteger(0)) {
while(i < replicas.Count) {
this.SendMsg(replicas[i],Constants.GLOBAL_COMMIT,currSeqNum);
i = i + new PInteger(1);
}
data[pendingWriteReq.Item2] = pendingWriteReq.Item3;
MachineController.AnnounceEvent(Constants.MONITOR_WRITE,new PTuple<PInteger,PInteger>(pendingWriteReq.Item2, pendingWriteReq.Item3));
this.SendMsg(pendingWriteReq.Item1,Constants.WRITE_SUCCESS,null);
this.SendMsg(timer,Constants.CancelTimer,null);
this.RaiseEvent(Constants.Unit, null); retcode = Constants.RAISED_EVENT; return;
}
}
private void CountVote_on_RESP_REPLICA_ABORT (IPType _payload) {
PInteger payload = (PInteger)_payload;
HandleAbort(payload); if(retcode == Constants.RAISED_EVENT) { return ; }

}
private void Loop_on_READ_REQ (IPType _payload) {
PTuple<PMachine,PInteger> payload = (PTuple<PMachine,PInteger>)_payload;
DoRead(payload);
}
private void DoGlobalAbort () {
i = new PInteger(0);
while(i < replicas.Count) {
this.SendMsg(replicas[i],Constants.GLOBAL_ABORT,currSeqNum);
i = i + new PInteger(1);
}
this.SendMsg(pendingWriteReq.Item1,Constants.WRITE_FAIL,null);
}
private void CountVote_on_Timeout (IPType _payload) {
DoGlobalAbort();
this.states[0] = Constants.MachineCoordinator_S_Loop;
}
private void DoRead (PTuple<PMachine,PInteger> payload) {
if(data.ContainsKey(payload.Item2)) {
MachineController.AnnounceEvent(Constants.MONITOR_READ_SUCCESS,new PTuple<PInteger,PInteger>(payload.Item2, data[payload.Item2]));
this.SendMsg(payload.Item1,Constants.READ_SUCCESS,data[payload.Item2]);
} else {
MachineController.AnnounceEvent(Constants.MONITOR_READ_UNAVAILABLE,payload.Item2);
this.SendMsg(payload.Item1,Constants.READ_UNAVAILABLE,null);
}
}
private void DoWrite (PTuple<PMachine,PInteger,PInteger> payload) {
pendingWriteReq = payload.DeepCopy();
currSeqNum = currSeqNum + new PInteger(1);
i = new PInteger(0);
while(i < replicas.Count) {
this.SendMsg(replicas[i],Constants.REQ_REPLICA,new PTuple<PInteger,PInteger,PInteger>(currSeqNum, pendingWriteReq.Item2,pendingWriteReq.Item3));
i = i + new PInteger(1);
}
this.SendMsg(timer,Constants.StartTimer,new PInteger(100));
this.RaiseEvent(Constants.Unit, null); retcode = Constants.RAISED_EVENT; return;
}
private void CountVote_on_READ_REQ (IPType _payload) {
PTuple<PMachine,PInteger> payload = (PTuple<PMachine,PInteger>)_payload;
DoRead(payload);
}
private void HandleAbort (PInteger payload) {
if(currSeqNum == payload) {
DoGlobalAbort();
this.SendMsg(timer,Constants.CancelTimer,null);
this.RaiseEvent(Constants.Unit, null); retcode = Constants.RAISED_EVENT; return;
}
}
}