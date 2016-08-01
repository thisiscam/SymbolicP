using System;
using System.Collections.Generic;
using System.Diagnostics;

class MachineM : MonitorPMachine
{
    private readonly static bool[, ] _DeferedSet = {{false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, };
    private readonly static bool[, ] _IsGotoTransition = {{false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, };
    ValueSummary</* P local vars */
    PMap<PInteger, PInteger>> data = new ValueSummary<PMap<PInteger, PInteger>>(new PMap<PInteger, PInteger>());
    public MachineM()
    {
        this.DeferedSet = _DeferedSet;
        this.IsGotoTransition = _IsGotoTransition;
        this.Transitions = new TransitionFunction[1, 22];
        this.Transitions[Constants.MachineM_S_Init, Constants.MONITOR_WRITE] = Init_on_MONITOR_WRITE;
        this.Transitions[Constants.MachineM_S_Init, Constants.MONITOR_READ_SUCCESS] = Init_on_MONITOR_READ_SUCCESS;
        this.Transitions[Constants.MachineM_S_Init, Constants.MONITOR_READ_UNAVAILABLE] = Init_on_MONITOR_READ_UNAVAILABLE;
        this.ExitFunctions = new ExitFunction[1];
        this.states.InvokeMethod<SymbolicInteger, int>((_, a0, a1) => _.Insert(a0, a1), (SymbolicInteger)0, Constants.MachineM_S_Init);
    }

    private void Init_on_MONITOR_WRITE(ValueSummary<IPType> _payload)
    {
        ValueSummary<PTuple<PInteger, PInteger>> payload = _payload.Cast<PTuple<PInteger, PInteger>>(_ => (PTuple<PInteger, PInteger>)_);
        this.data.InvokeMethod<PInteger, PInteger>((_, a0, r) => _[a0] = r, payload.GetField<PInteger>(_ => _.Item1), payload.GetField<PInteger>(_ => _.Item2));
    }

    private void Init_on_MONITOR_READ_SUCCESS(ValueSummary<IPType> _payload)
    {
        ValueSummary<PTuple<PInteger, PInteger>> payload = _payload.Cast<PTuple<PInteger, PInteger>>(_ => (PTuple<PInteger, PInteger>)_);
        this.Assert((data.ContainsKey(payload.Item1)));
        this.Assert((this.data.InvokeMethod<PInteger, PInteger>((_, a0) => _[a0], payload.GetField<PInteger>(_ => _.Item1)).InvokeBinary<PInteger, PBool>((l, r) => l == r, payload.GetField<PInteger>(_ => _.Item2))));
    }

    private void Init_on_MONITOR_READ_UNAVAILABLE(ValueSummary<IPType> _payload)
    {
        ValueSummary<PInteger> payload = _payload.Cast<PInteger>(_ => (PInteger)_);
        this.Assert((!(data.ContainsKey(payload))));
    }
}