using System;
using System.Collections.Generic;
using System.Diagnostics;

class MachineCoordinator : PMachine
{
    private readonly static bool[, ] _DeferedSet = {{false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, true, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, true, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, }, };
    private readonly static bool[, ] _IsGotoTransition = {{false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, }, {false, false, true, false, false, false, false, false, false, false, false, false, false, true, true, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, true, true, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, }, };
    ValueSummary</* P local vars */
    PMachine> replica = ValueSummary<PMachine>.Null;
    ValueSummary<PInteger> numReplicas = new ValueSummary<PInteger>(new PInteger(0));
    ValueSummary<PInteger> currSeqNum = new ValueSummary<PInteger>(new PInteger(0));
    ValueSummary<PList<PMachine>> replicas = new ValueSummary<PList<PMachine>>(new PList<PMachine>());
    ValueSummary<PInteger> i = new ValueSummary<PInteger>(new PInteger(0));
    ValueSummary<PMap<PInteger, PInteger>> data = new ValueSummary<PMap<PInteger, PInteger>>(new PMap<PInteger, PInteger>());
    ValueSummary<PTuple<PMachine, PInteger, PInteger>> pendingWriteReq = new ValueSummary<PTuple<PMachine, PInteger, PInteger>>(new PTuple<PMachine, PInteger, PInteger>(ValueSummary<PMachine>.Null, new PInteger(0), new PInteger(0)));
    ValueSummary<PMachine> timer = ValueSummary<PMachine>.Null;
    public MachineCoordinator()
    {
        this.DeferedSet = _DeferedSet;
        this.IsGotoTransition = _IsGotoTransition;
        this.Transitions = new TransitionFunction[5, 22];
        this.Transitions[Constants.MachineCoordinator_S_WaitForCancelTimerResponse, Constants.CancelTimerSuccess] = WaitForCancelTimerResponse_on_CancelTimerSuccess_Timeout;
        this.Transitions[Constants.MachineCoordinator_S_WaitForCancelTimerResponse, Constants.Timeout] = WaitForCancelTimerResponse_on_CancelTimerSuccess_Timeout;
        this.Transitions[Constants.MachineCoordinator_S_CountVote, Constants.RESP_REPLICA_ABORT] = CountVote_on_RESP_REPLICA_ABORT;
        this.Transitions[Constants.MachineCoordinator_S_WaitForCancelTimerResponse, Constants.CancelTimerFailure] = WaitForCancelTimerResponse_on_CancelTimerFailure;
        this.Transitions[Constants.MachineCoordinator_S_Loop, Constants.WRITE_REQ] = Loop_on_WRITE_REQ;
        this.Transitions[Constants.MachineCoordinator_S_Init, Constants.Unit] = Init_on_Unit;
        this.Transitions[Constants.MachineCoordinator_S_CountVote, Constants.RESP_REPLICA_COMMIT] = CountVote_on_RESP_REPLICA_COMMIT;
        this.Transitions[Constants.MachineCoordinator_S_CountVote, Constants.Unit] = CountVote_on_Unit;
        this.Transitions[Constants.MachineCoordinator_S_CountVote, Constants.READ_REQ] = CountVote_on_READ_REQ;
        this.Transitions[Constants.MachineCoordinator_S_WaitForTimeout, Constants.Timeout] = WaitForTimeout_on_Timeout;
        this.Transitions[Constants.MachineCoordinator_S_CountVote, Constants.Timeout] = CountVote_on_Timeout;
        this.Transitions[Constants.MachineCoordinator_S_Loop, Constants.Unit] = Loop_on_Unit;
        this.Transitions[Constants.MachineCoordinator_S_Loop, Constants.READ_REQ] = Loop_on_READ_REQ;
        this.Transitions[Constants.MachineCoordinator_S_Loop, Constants.RESP_REPLICA_COMMIT] = Transition_Ignore;
        this.Transitions[Constants.MachineCoordinator_S_Loop, Constants.RESP_REPLICA_ABORT] = Transition_Ignore;
        this.Transitions[Constants.MachineCoordinator_S_WaitForCancelTimerResponse, Constants.RESP_REPLICA_COMMIT] = Transition_Ignore;
        this.Transitions[Constants.MachineCoordinator_S_WaitForCancelTimerResponse, Constants.RESP_REPLICA_ABORT] = Transition_Ignore;
        this.Transitions[Constants.MachineCoordinator_S_WaitForTimeout, Constants.RESP_REPLICA_COMMIT] = Transition_Ignore;
        this.Transitions[Constants.MachineCoordinator_S_WaitForTimeout, Constants.RESP_REPLICA_ABORT] = Transition_Ignore;
        this.ExitFunctions = new ExitFunction[5];
    }

    public override void StartMachine(ValueSummary<Scheduler> scheduler, ValueSummary<IPType> payload)
    {
        this.scheduler = scheduler;
        this.states.InvokeMethod<SymbolicInteger, int>((_, a0, a1) => _.Insert(a0, a1), (SymbolicInteger)0, Constants.MachineCoordinator_S_Init);
        this.Init_Entry(payload.Cast<PInteger>(_ => (PInteger)_));
    }

    private void WaitForCancelTimerResponse_on_CancelTimerSuccess_Timeout(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineCoordinator_S_Loop);
    }

    private void WaitForCancelTimerResponse_on_CancelTimerFailure(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineCoordinator_S_WaitForTimeout);
    }

    private void Init_on_Unit(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineCoordinator_S_Loop);
    }

    private void CountVote_on_Unit(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineCoordinator_S_WaitForCancelTimerResponse);
    }

    private void WaitForTimeout_on_Timeout(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineCoordinator_S_Loop);
    }

    private void Loop_on_Unit(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineCoordinator_S_CountVote);
        this.CountVote_Entry();
    }

    private void Init_Entry(ValueSummary<PInteger> payload)
    {
        this.numReplicas = payload;
        this.Assert((this.numReplicas.InvokeBinary<PInteger, PBool>((l, r) => l > r, new ValueSummary<PInteger>(new PInteger(0)))));
        this.i = new ValueSummary<PInteger>(new PInteger(0));
        while (this.i.InvokeBinary<PInteger, PBool>((l, r) => l < r, this.numReplicas).Cond())
        {
            this.replica = this.NewMachine(new ValueSummary<PMachine>(new MachineReplica()), this);
            replicas.InvokeMethod<PTuple<PInteger, PMachine>>((_, a0) => _.Insert(a0), new ValueSummary<PTuple<PInteger, PMachine>>(new PTuple<PInteger, PMachine>(this.i, this.replica)));
            this.i = this.i.InvokeBinary<PInteger, PInteger>((l, r) => l + r, new ValueSummary<PInteger>(new PInteger(1)));
        }

        this.currSeqNum = new ValueSummary<PInteger>(new PInteger(0));
        this.timer = this.NewMachine(new ValueSummary<PMachine>(new MachineTimer()), this);
        this.RaiseEvent((PInteger)Constants.Unit, ValueSummary<IPType>.Null);
        this.retcode = Constants.RAISED_EVENT;
        return;
    }

    private void CountVote_on_RESP_REPLICA_COMMIT(ValueSummary<IPType> _payload)
    {
        ValueSummary<PInteger> payload = _payload.Cast<PInteger>(_ => (PInteger)_);
        if (this.currSeqNum.InvokeBinary<PInteger, PBool>((l, r) => l == r, payload).Cond())
        {
            this.i = this.i.InvokeBinary<PInteger, PInteger>((l, r) => l - r, new ValueSummary<PInteger>(new PInteger(1)));
        }

        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineCoordinator_S_CountVote);
        this.CountVote_Entry();
    }

    private void Loop_on_WRITE_REQ(ValueSummary<IPType> _payload)
    {
        ValueSummary<PTuple<PMachine, PInteger, PInteger>> payload = _payload.Cast<PTuple<PMachine, PInteger, PInteger>>(_ => (PTuple<PMachine, PInteger, PInteger>)_);
        this.DoWrite(payload);
        if (this.retcode.InvokeBinary<int, bool>((l, r) => l == r, Constants.RAISED_EVENT).Cond())
        {
            return;
        }
    }

    private void CountVote_Entry()
    {
        if (this.i.InvokeBinary<PInteger, PBool>((l, r) => l == r, new ValueSummary<PInteger>(new PInteger(0))).Cond())
        {
            while (this.i.InvokeBinary<PInteger, PBool>((l, r) => l < r, replicas.GetField<int>(_ => _.Count)).Cond())
            {
                this.SendMsg(this.replicas.InvokeMethod<PInteger, PMachine>((_, a0) => _[a0], this.i), (PInteger)Constants.GLOBAL_COMMIT, this.currSeqNum);
                this.i = this.i.InvokeBinary<PInteger, PInteger>((l, r) => l + r, new ValueSummary<PInteger>(new PInteger(1)));
            }

            this.data.InvokeMethod<PInteger, PInteger>((_, a0, r) => _[a0] = r, pendingWriteReq.GetField<PInteger>(_ => _.Item2), pendingWriteReq.GetField<PInteger>(_ => _.Item3));
            MachineController.InvokeMethod<PInteger, IPType>((_, a0, a1) => _.AnnounceEvent(a0, a1), (PInteger)Constants.MONITOR_WRITE, new ValueSummary<PTuple<PInteger, PInteger>>(new PTuple<PInteger, PInteger>(pendingWriteReq.GetField<PInteger>(_ => _.Item2), pendingWriteReq.GetField<PInteger>(_ => _.Item3))));
            this.SendMsg(pendingWriteReq.GetField<PMachine>(_ => _.Item1), (PInteger)Constants.WRITE_SUCCESS, ValueSummary<IPType>.Null);
            this.SendMsg(this.timer, (PInteger)Constants.CancelTimer, ValueSummary<IPType>.Null);
            this.RaiseEvent((PInteger)Constants.Unit, ValueSummary<IPType>.Null);
            this.retcode = Constants.RAISED_EVENT;
            return;
        }
    }

    private void CountVote_on_RESP_REPLICA_ABORT(ValueSummary<IPType> _payload)
    {
        ValueSummary<PInteger> payload = _payload.Cast<PInteger>(_ => (PInteger)_);
        this.HandleAbort(payload);
        if (this.retcode.InvokeBinary<int, bool>((l, r) => l == r, Constants.RAISED_EVENT).Cond())
        {
            return;
        }
    }

    private void Loop_on_READ_REQ(ValueSummary<IPType> _payload)
    {
        ValueSummary<PTuple<PMachine, PInteger>> payload = _payload.Cast<PTuple<PMachine, PInteger>>(_ => (PTuple<PMachine, PInteger>)_);
        this.DoRead(payload);
    }

    private void DoGlobalAbort()
    {
        this.i = new ValueSummary<PInteger>(new PInteger(0));
        while (this.i.InvokeBinary<PInteger, PBool>((l, r) => l < r, replicas.GetField<int>(_ => _.Count)).Cond())
        {
            this.SendMsg(this.replicas.InvokeMethod<PInteger, PMachine>((_, a0) => _[a0], this.i), (PInteger)Constants.GLOBAL_ABORT, this.currSeqNum);
            this.i = this.i.InvokeBinary<PInteger, PInteger>((l, r) => l + r, new ValueSummary<PInteger>(new PInteger(1)));
        }

        this.SendMsg(pendingWriteReq.GetField<PMachine>(_ => _.Item1), (PInteger)Constants.WRITE_FAIL, ValueSummary<IPType>.Null);
    }

    private void CountVote_on_Timeout(ValueSummary<IPType> _payload)
    {
        this.DoGlobalAbort();
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineCoordinator_S_Loop);
    }

    private void DoRead(ValueSummary<PTuple<PMachine, PInteger>> payload)
    {
        if (data.ContainsKey(payload.Item2).Cond())
        {
            MachineController.InvokeMethod<PInteger, IPType>((_, a0, a1) => _.AnnounceEvent(a0, a1), (PInteger)Constants.MONITOR_READ_SUCCESS, new ValueSummary<PTuple<PInteger, PInteger>>(new PTuple<PInteger, PInteger>(payload.GetField<PInteger>(_ => _.Item2), this.data.InvokeMethod<PInteger, PInteger>((_, a0) => _[a0], payload.GetField<PInteger>(_ => _.Item2)))));
            this.SendMsg(payload.GetField<PMachine>(_ => _.Item1), (PInteger)Constants.READ_SUCCESS, this.data.InvokeMethod<PInteger, PInteger>((_, a0) => _[a0], payload.GetField<PInteger>(_ => _.Item2)));
        }
        else
        {
            MachineController.InvokeMethod<PInteger, IPType>((_, a0, a1) => _.AnnounceEvent(a0, a1), (PInteger)Constants.MONITOR_READ_UNAVAILABLE, payload.GetField<PInteger>(_ => _.Item2));
            this.SendMsg(payload.GetField<PMachine>(_ => _.Item1), (PInteger)Constants.READ_UNAVAILABLE, ValueSummary<IPType>.Null);
        }
    }

    private void DoWrite(ValueSummary<PTuple<PMachine, PInteger, PInteger>> payload)
    {
        this.pendingWriteReq = payload.InvokeMethod((_) => _.DeepCopy());
        this.currSeqNum = this.currSeqNum.InvokeBinary<PInteger, PInteger>((l, r) => l + r, new ValueSummary<PInteger>(new PInteger(1)));
        this.i = new ValueSummary<PInteger>(new PInteger(0));
        while (this.i.InvokeBinary<PInteger, PBool>((l, r) => l < r, replicas.GetField<int>(_ => _.Count)).Cond())
        {
            this.SendMsg(this.replicas.InvokeMethod<PInteger, PMachine>((_, a0) => _[a0], this.i), (PInteger)Constants.REQ_REPLICA, new ValueSummary<PTuple<PInteger, PInteger, PInteger>>(new PTuple<PInteger, PInteger, PInteger>(this.currSeqNum, pendingWriteReq.GetField<PInteger>(_ => _.Item2), pendingWriteReq.GetField<PInteger>(_ => _.Item3))));
            this.i = this.i.InvokeBinary<PInteger, PInteger>((l, r) => l + r, new ValueSummary<PInteger>(new PInteger(1)));
        }

        this.SendMsg(this.timer, (PInteger)Constants.StartTimer, new ValueSummary<PInteger>(new PInteger(100)));
        this.RaiseEvent((PInteger)Constants.Unit, ValueSummary<IPType>.Null);
        this.retcode = Constants.RAISED_EVENT;
        return;
    }

    private void CountVote_on_READ_REQ(ValueSummary<IPType> _payload)
    {
        ValueSummary<PTuple<PMachine, PInteger>> payload = _payload.Cast<PTuple<PMachine, PInteger>>(_ => (PTuple<PMachine, PInteger>)_);
        this.DoRead(payload);
    }

    private void HandleAbort(ValueSummary<PInteger> payload)
    {
        if (this.currSeqNum.InvokeBinary<PInteger, PBool>((l, r) => l == r, payload).Cond())
        {
            this.DoGlobalAbort();
            this.SendMsg(this.timer, (PInteger)Constants.CancelTimer, ValueSummary<IPType>.Null);
            this.RaiseEvent((PInteger)Constants.Unit, ValueSummary<IPType>.Null);
            this.retcode = Constants.RAISED_EVENT;
            return;
        }
    }
}