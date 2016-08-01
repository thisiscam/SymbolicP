using System;
using System.Collections.Generic;
using System.Diagnostics;

class MachineReplica : PMachine
{
    private readonly static bool[, ] _DeferedSet = {{false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, };
    private readonly static bool[, ] _IsGotoTransition = {{false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, };
    ValueSummary</* P local vars */
    PMachine> coordinator = ValueSummary<PMachine>.Null;
    ValueSummary<PBool> shouldCommit = new ValueSummary<PBool>(new PBool((SymbolicBool)false));
    ValueSummary<PMap<PInteger, PInteger>> data = new ValueSummary<PMap<PInteger, PInteger>>(new PMap<PInteger, PInteger>());
    ValueSummary<PTuple<PInteger, PInteger, PInteger>> pendingWriteReq = new ValueSummary<PTuple<PInteger, PInteger, PInteger>>(new PTuple<PInteger, PInteger, PInteger>(new PInteger(0), new PInteger(0), new PInteger(0)));
    ValueSummary<PInteger> lastSeqNum = new ValueSummary<PInteger>(new PInteger(0));
    public MachineReplica()
    {
        this.DeferedSet = _DeferedSet;
        this.IsGotoTransition = _IsGotoTransition;
        this.Transitions = new TransitionFunction[2, 22];
        this.Transitions[Constants.MachineReplica_S_Loop, Constants.REQ_REPLICA] = Loop_on_REQ_REPLICA;
        this.Transitions[Constants.MachineReplica_S_Loop, Constants.GLOBAL_ABORT] = Loop_on_GLOBAL_ABORT;
        this.Transitions[Constants.MachineReplica_S_Loop, Constants.GLOBAL_COMMIT] = Loop_on_GLOBAL_COMMIT;
        this.Transitions[Constants.MachineReplica_S_Init, Constants.Unit] = Init_on_Unit;
        this.ExitFunctions = new ExitFunction[2];
    }

    public override void StartMachine(ValueSummary<Scheduler> scheduler, ValueSummary<IPType> payload)
    {
        this.scheduler = scheduler;
        this.states.InvokeMethod<SymbolicInteger, int>((_, a0, a1) => _.Insert(a0, a1), (SymbolicInteger)0, Constants.MachineReplica_S_Init);
        this.Init_Entry(payload.Cast<PMachine>(_ => (PMachine)_));
    }

    private void Init_on_Unit(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineReplica_S_Loop);
    }

    private void Loop_on_GLOBAL_COMMIT(ValueSummary<IPType> _payload)
    {
        ValueSummary<PInteger> payload = _payload.Cast<PInteger>(_ => (PInteger)_);
        this.Assert((pendingWriteReq.GetField<PInteger>(_ => _.Item1).InvokeBinary<PInteger, PBool>((l, r) => l >= r, payload)));
        if (pendingWriteReq.GetField<PInteger>(_ => _.Item1).InvokeBinary<PInteger, PBool>((l, r) => l == r, payload).Cond())
        {
            this.data.InvokeMethod<PInteger, PInteger>((_, a0, r) => _[a0] = r, pendingWriteReq.GetField<PInteger>(_ => _.Item2), pendingWriteReq.GetField<PInteger>(_ => _.Item3));
            this.lastSeqNum = payload;
        }
    }

    private void HandleReqReplica(ValueSummary<PTuple<PInteger, PInteger, PInteger>> payload)
    {
        this.pendingWriteReq = payload.InvokeMethod((_) => _.DeepCopy());
        this.Assert((pendingWriteReq.GetField<PInteger>(_ => _.Item1).InvokeBinary<PInteger, PBool>((l, r) => l > r, this.lastSeqNum)));
        this.shouldCommit = this.ShouldCommitWrite();
        if (this.shouldCommit.Cond())
        {
            this.SendMsg(this.coordinator, (PInteger)Constants.RESP_REPLICA_COMMIT, pendingWriteReq.GetField<PInteger>(_ => _.Item1));
        }
        else
        {
            this.SendMsg(this.coordinator, (PInteger)Constants.RESP_REPLICA_ABORT, pendingWriteReq.GetField<PInteger>(_ => _.Item1));
        }
    }

    private void Loop_on_REQ_REPLICA(ValueSummary<IPType> _payload)
    {
        ValueSummary<PTuple<PInteger, PInteger, PInteger>> payload = _payload.Cast<PTuple<PInteger, PInteger, PInteger>>(_ => (PTuple<PInteger, PInteger, PInteger>)_);
        this.HandleReqReplica(payload);
    }

    private void Init_Entry(ValueSummary<PMachine> payload)
    {
        this.coordinator = payload;
        this.lastSeqNum = new ValueSummary<PInteger>(new PInteger(0));
        this.RaiseEvent((PInteger)Constants.Unit, ValueSummary<IPType>.Null);
        this.retcode = Constants.RAISED_EVENT;
        return;
    }

    private void Loop_on_GLOBAL_ABORT(ValueSummary<IPType> _payload)
    {
        ValueSummary<PInteger> payload = _payload.Cast<PInteger>(_ => (PInteger)_);
        this.Assert((pendingWriteReq.GetField<PInteger>(_ => _.Item1).InvokeBinary<PInteger, PBool>((l, r) => l >= r, payload)));
        if (pendingWriteReq.GetField<PInteger>(_ => _.Item1).InvokeBinary<PInteger, PBool>((l, r) => l == r, payload).Cond())
        {
            this.lastSeqNum = payload;
        }
    }

    private ValueSummary<PBool> ShouldCommitWrite()
    {
        return this.RandomBool();
    }
}