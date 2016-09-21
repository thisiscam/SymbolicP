using System;
using System.Collections.Generic;
using System.Diagnostics;

class MachinePING : PMachine
{
    private readonly static bool[, ] _DeferedSet = {{false, false, false, false, }, {false, false, false, false, }, {false, false, false, false, }, {false, false, false, false, }, };
    private readonly static bool[, ] _IsGotoTransition = {{false, false, false, true, }, {false, false, false, true, }, {false, false, true, false, }, {false, false, false, false, }, };
    ValueSummary</* P local vars */
    PMachine> pongId = ValueSummary</* P local vars */
    PMachine>.InitializeFrom(new ValueSummary<PMachine>(null));
    public MachinePING()
    {
        this.DeferedSet = _DeferedSet;
        this.IsGotoTransition = _IsGotoTransition;
        this.Transitions = new TransitionFunction[4, 4];
        this.Transitions[Constants.MachinePING_S_Ping_SendPing, Constants.Success] = Ping_SendPing_on_Success;
        this.Transitions[Constants.MachinePING_S_Ping_WaitPong, Constants.Pong] = Ping_WaitPong_on_Pong;
        this.Transitions[Constants.MachinePING_S_Ping_Init, Constants.Success] = Ping_Init_on_Success;
        this.ExitFunctions = new ExitFunction[4];
    }

    public override void StartMachine(ValueSummary<Scheduler> scheduler, ValueSummary<IPType> payload)
    {
        this.scheduler = scheduler;
        this.states.InvokeMethod<SymbolicInteger, int>((_, a0, a1) => _.Insert(a0, a1), (SymbolicInteger)0, Constants.MachinePING_S_Ping_Init);
        this.Ping_Init_Entry();
    }

    private void Ping_SendPing_on_Success(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachinePING_S_Ping_WaitPong);
    }

    private void Ping_WaitPong_on_Pong(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachinePING_S_Ping_SendPing);
        this.Ping_SendPing_Entry();
    }

    private void Ping_Init_on_Success(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachinePING_S_Ping_SendPing);
        this.Ping_SendPing_Entry();
    }

    private void Ping_SendPing_Entry()
    {
        this.SendMsg(this.pongId, (PInteger)Constants.Ping, this);
        this.RaiseEvent((PInteger)Constants.Success, new ValueSummary<IPType>(null));
        this.retcode = Constants.RAISED_EVENT;
        return;
    }

    private void Ping_Init_Entry()
    {
        this.pongId = this.NewMachine(new ValueSummary<PMachine>(new MachinePONG()), new ValueSummary<IPType>(null));
        this.RaiseEvent((PInteger)Constants.Success, new ValueSummary<IPType>(null));
        this.retcode = Constants.RAISED_EVENT;
        return;
    }
}