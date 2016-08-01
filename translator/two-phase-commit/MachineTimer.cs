using System;
using System.Collections.Generic;
using System.Diagnostics;

class MachineTimer : PMachine
{
    private readonly static bool[, ] _DeferedSet = {{false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, }, };
    private readonly static bool[, ] _IsGotoTransition = {{false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, }, {false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, true, false, false, false, false, false, }, };
    ValueSummary</* P local vars */
    PMachine> target = ValueSummary<PMachine>.Null;
    public MachineTimer()
    {
        this.DeferedSet = _DeferedSet;
        this.IsGotoTransition = _IsGotoTransition;
        this.Transitions = new TransitionFunction[3, 22];
        this.Transitions[Constants.MachineTimer_S_TimerStarted, Constants.Unit] = TimerStarted_on_Unit;
        this.Transitions[Constants.MachineTimer_S_Init, Constants.Unit] = Init_on_Unit;
        this.Transitions[Constants.MachineTimer_S_TimerStarted, Constants.CancelTimer] = TimerStarted_on_CancelTimer;
        this.Transitions[Constants.MachineTimer_S_Loop, Constants.StartTimer] = Loop_on_StartTimer;
        this.Transitions[Constants.MachineTimer_S_Loop, Constants.CancelTimer] = Transition_Ignore;
        this.ExitFunctions = new ExitFunction[3];
    }

    public override void StartMachine(ValueSummary<Scheduler> scheduler, ValueSummary<IPType> payload)
    {
        this.scheduler = scheduler;
        this.states.InvokeMethod<SymbolicInteger, int>((_, a0, a1) => _.Insert(a0, a1), (SymbolicInteger)0, Constants.MachineTimer_S_Init);
        this.Init_Entry(payload.Cast<PMachine>(_ => (PMachine)_));
    }

    private void TimerStarted_on_Unit(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineTimer_S_Loop);
    }

    private void Init_on_Unit(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineTimer_S_Loop);
    }

    private void Loop_on_StartTimer(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineTimer_S_TimerStarted);
        this.TimerStarted_Entry();
    }

    private void Init_Entry(ValueSummary<PMachine> payload)
    {
        this.target = payload;
        this.RaiseEvent((PInteger)Constants.Unit, ValueSummary<IPType>.Null);
        this.retcode = Constants.RAISED_EVENT;
        return;
    }

    private void TimerStarted_on_CancelTimer(ValueSummary<IPType> _payload)
    {
        if (this.RandomBool().Cond())
        {
            this.SendMsg(this.target, (PInteger)Constants.CancelTimerFailure, ValueSummary<IPType>.Null);
            this.SendMsg(this.target, (PInteger)Constants.Timeout, ValueSummary<IPType>.Null);
        }
        else
        {
            this.SendMsg(this.target, (PInteger)Constants.CancelTimerSuccess, ValueSummary<IPType>.Null);
        }

        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachineTimer_S_Loop);
    }

    private void TimerStarted_Entry()
    {
        if (this.RandomBool().Cond())
        {
            this.SendMsg(this.target, (PInteger)Constants.Timeout, ValueSummary<IPType>.Null);
            this.RaiseEvent((PInteger)Constants.Unit, ValueSummary<IPType>.Null);
            this.retcode = Constants.RAISED_EVENT;
            return;
        }
    }
}