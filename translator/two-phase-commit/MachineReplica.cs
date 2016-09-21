using System;
using System.Collections.Generic;
using System.Diagnostics;

class MachineReplica : PMachine {

private readonly static bool[,] _DeferedSet = {
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
};

private readonly static bool[,] _IsGotoTransition = {
{false,false,false,false,false,false,false,false,false,false,false,false,false,true, false,false,false,false,false,false,false,false,},
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
};
/* P local vars */
PMachine coordinator=null;
PBool shouldCommit=new PBool(false);
PMap<PInteger,PInteger> data=new PMap<PInteger,PInteger>();
PTuple<PInteger,PInteger,PInteger> pendingWriteReq=new PTuple<PInteger,PInteger,PInteger>(new PInteger(0),new PInteger(0),new PInteger(0));
PInteger lastSeqNum=new PInteger(0);


public MachineReplica () {
this.DeferedSet = _DeferedSet;
this.IsGotoTransition = _IsGotoTransition;
this.Transitions = new TransitionFunction[2,22];
this.Transitions[Constants.MachineReplica_S_Loop,Constants.GLOBAL_COMMIT] = Loop_on_GLOBAL_COMMIT;
this.Transitions[Constants.MachineReplica_S_Loop,Constants.GLOBAL_ABORT] = Loop_on_GLOBAL_ABORT;
this.Transitions[Constants.MachineReplica_S_Loop,Constants.REQ_REPLICA] = Loop_on_REQ_REPLICA;
this.Transitions[Constants.MachineReplica_S_Init,Constants.Unit] = Init_on_Unit;
this.ExitFunctions = new ExitFunction[2];
}
public override void StartMachine (Scheduler scheduler, IPType payload) {
this.scheduler = scheduler;
this.states.Insert(0, Constants.MachineReplica_S_Init);
Init_Entry((PMachine)payload);
}
private void Init_on_Unit (IPType payload) {
this.states[0] = Constants.MachineReplica_S_Loop;
}
private void Loop_on_GLOBAL_COMMIT (IPType _payload) {
PInteger payload = (PInteger)_payload;
Assert((pendingWriteReq.Item1 >= payload));
if(pendingWriteReq.Item1 == payload) {
data[pendingWriteReq.Item2] = pendingWriteReq.Item3;
lastSeqNum = payload;
}
}
private void HandleReqReplica (PTuple<PInteger,PInteger,PInteger> payload) {
pendingWriteReq = payload.DeepCopy();
Assert((pendingWriteReq.Item1 > lastSeqNum));
shouldCommit = ShouldCommitWrite();
if(shouldCommit) {
this.SendMsg(coordinator,Constants.RESP_REPLICA_COMMIT,pendingWriteReq.Item1);
} else {
this.SendMsg(coordinator,Constants.RESP_REPLICA_ABORT,pendingWriteReq.Item1);
}
}
private void Loop_on_REQ_REPLICA (IPType _payload) {
PTuple<PInteger,PInteger,PInteger> payload = (PTuple<PInteger,PInteger,PInteger>)_payload;
HandleReqReplica(payload);
}
private void Init_Entry (PMachine payload) {
coordinator = payload;
lastSeqNum = new PInteger(0);
this.RaiseEvent(Constants.Unit, null); retcode = Constants.RAISED_EVENT; return;
}
private void Loop_on_GLOBAL_ABORT (IPType _payload) {
PInteger payload = (PInteger)_payload;
Assert((pendingWriteReq.Item1 >= payload));
if(pendingWriteReq.Item1 == payload) {
lastSeqNum = payload;
}
}
private PBool ShouldCommitWrite () {
return RandomBool();
}
}