using System;
using System.Collections.Generic;
using System.Diagnostics;

class MachineClient : PMachine
{
    private readonly static bool[, ] _DeferedSet = {{false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, };
    private readonly static bool[, ] _IsGotoTransition = {{false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, true, false, true, false, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, };
    ValueSummary</* P local vars */
    PMachine> coordinator = ValueSummary<PMachine>.Null;
    ValueSummary<PInteger> val = new ValueSummary<PInteger>(new PInteger(0));
    ValueSummary<PInteger> idx = new ValueSummary<PInteger>(new PInteger(0));
    public MachineClient()
    {
        this.DeferedSet = _DeferedSet;
        this.IsGotoTransition = _IsGotoTransition;
        this.Transitions = new TransitionFunction[4, 22];
        this.Transitions[Constants.MachineClient_S_DoRead, Constants.READ_SUCCESS] = DoRead_on_READ_SUCCESS_READ_FAIL;
        this.Transitions[Constants.MachineClient_S_DoRead, Constants.READ_FAIL] = DoRead_on_READ_SUCCESS_READ_FAIL;
        this.Transitions[Constants.MachineClient_S_DoWrite, Constants.WRITE_FAIL] = DoWrite_on_WRITE_FAIL;
        this.Transitions[Constants.MachineClient_S_DoWrite, Constants.WRITE_SUCCESS] = DoWrite_on_WRITE_SUCCESS;
        this.Transitions[Constants.MachineClient_S_Init, Constants.Unit] = Init_on_Unit;
        this.ExitFunctions = new ExitFunction[4];
    }

    public override void StartMachine(ValueSummary<Scheduler> scheduler, ValueSummary<IPType> payload)
    {
        this.scheduler = scheduler;
        this.states.InvokeMethod<SymbolicInteger, int>((_, a0, a1) => _.Insert(a0, a1), (SymbolicInteger)0, Constants.MachineClient_S_Init);
        this.Init_Entry(payload.Cast<PMachine>(_ => (PMachine)_));
    }

    private void DoRead_on_READ_SUCCESS_READ_FAIL(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineClient_S_End);
    }

    private void DoWrite_on_WRITE_FAIL(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineClient_S_End);
    }

    private void DoWrite_on_WRITE_SUCCESS(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineClient_S_DoRead);
        this.DoRead_Entry();
    }

    private void Init_on_Unit(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineClient_S_DoWrite);
        this.DoWrite_Entry();
    }

    private void Init_Entry(ValueSummary<PMachine> payload)
    {
        this.coordinator = payload;
        this.RaiseEvent((PInteger)Constants.Unit, ValueSummary<IPType>.Null);
        this.retcode = Constants.RAISED_EVENT;
        return;
    }

    private void DoRead_Entry()
    {
        this.SendMsg(this.coordinator, (PInteger)Constants.READ_REQ, new ValueSummary<PTuple<PMachine, PInteger>>(new PTuple<PMachine, PInteger>(this, this.idx)));
    }

    private void DoWrite_Entry()
    {
        this.idx = this.ChooseIndex();
        this.val = this.ChooseValue();
        this.SendMsg(this.coordinator, (PInteger)Constants.WRITE_REQ, new ValueSummary<PTuple<PMachine, PInteger, PInteger>>(new PTuple<PMachine, PInteger, PInteger>(this, this.idx, this.val)));
    }

    private ValueSummary<PInteger> ChooseIndex()
    {
        if (this.RandomBool().Cond())
        {
            return new ValueSummary<ValueSummary<PInteger>>(new PInteger(0));
        }
        else
        {
            return new ValueSummary<ValueSummary<PInteger>>(new PInteger(1));
        }
    }

    private ValueSummary<PInteger> ChooseValue()
    {
        if (this.RandomBool().Cond())
        {
            return new ValueSummary<ValueSummary<PInteger>>(new PInteger(0));
        }
        else
        {
            return new ValueSummary<ValueSummary<PInteger>>(new PInteger(1));
        }
    }
}