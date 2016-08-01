using System;
using System.Collections.Generic;
using System.Diagnostics;

class MachineTwoPhaseCommit : PMachine
{
    private readonly static bool[, ] _DeferedSet = {{false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, };
    private readonly static bool[, ] _IsGotoTransition = {{false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, };
    ValueSummary</* P local vars */
    PMachine> coordinator = ValueSummary<PMachine>.Null;
    ValueSummary<PMachine> client = ValueSummary<PMachine>.Null;
    public MachineTwoPhaseCommit()
    {
        this.DeferedSet = _DeferedSet;
        this.IsGotoTransition = _IsGotoTransition;
        this.Transitions = new TransitionFunction[1, 22];
        this.ExitFunctions = new ExitFunction[1];
    }

    public override void StartMachine(ValueSummary<Scheduler> scheduler, ValueSummary<IPType> payload)
    {
        this.scheduler = scheduler;
        this.states.InvokeMethod<SymbolicInteger, int>((_, a0, a1) => _.Insert(a0, a1), (SymbolicInteger)0, Constants.MachineTwoPhaseCommit_S_Init);
        this.Init_Entry();
    }

    private void Init_Entry()
    {
        this.coordinator = this.NewMachine(new ValueSummary<PMachine>(new MachineCoordinator()), new ValueSummary<PInteger>(new PInteger(2)));
        this.client = this.NewMachine(new ValueSummary<PMachine>(new MachineClient()), this.coordinator);
        this.client = this.NewMachine(new ValueSummary<PMachine>(new MachineClient()), this.coordinator);
    }
}