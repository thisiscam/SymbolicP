using System;
using System.Collections.Generic;
using System.Diagnostics;

class MachinePONG : PMachine
{
    private readonly static bool[, ] _DeferedSet = {{false, false, false, false, }, {false, false, false, false, }, };
    private readonly static bool[, ] _IsGotoTransition = {{false, true, false, false, }, {false, false, false, true, }, };
    /* P local vars */
    public MachinePONG()
    {
        this.DeferedSet = _DeferedSet;
        this.IsGotoTransition = _IsGotoTransition;
        this.Transitions = new TransitionFunction[2, 4];
        this.Transitions[Constants.MachinePONG_S_Pong_SendPong, Constants.Success] = Pong_SendPong_on_Success;
        this.Transitions[Constants.MachinePONG_S_Pong_WaitPing, Constants.Ping] = Pong_WaitPing_on_Ping;
        this.ExitFunctions = new ExitFunction[2];
    }

    public override void StartMachine(ValueSummary<Scheduler> scheduler, ValueSummary<IPType> payload)
    {
        this.scheduler = scheduler;
        this.states.InvokeMethod<SymbolicInteger, int>((_, a0, a1) => _.Insert(a0, a1), (SymbolicInteger)0, Constants.MachinePONG_S_Pong_WaitPing);
        this.Pong_WaitPing_Entry();
    }

    private void Pong_SendPong_on_Success(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachinePONG_S_Pong_WaitPing);
        this.Pong_WaitPing_Entry();
    }

    private void Pong_WaitPing_on_Ping(ValueSummary<IPType> payload)
    {
        this.states.InvokeMethod<int, int>((_, a0, r) => _[a0] = r, 0, Constants.MachinePONG_S_Pong_SendPong);
        this.Pong_SendPong_Entry(payload.Cast<PMachine>(_ => (PMachine)_));
    }

    private void Pong_SendPong_Entry(ValueSummary<PMachine> payload)
    {
        this.SendMsg(payload, (PInteger)Constants.Pong, new ValueSummary<IPType>(null));
        this.RaiseEvent((PInteger)Constants.Success, new ValueSummary<IPType>(null));
        this.retcode = Constants.RAISED_EVENT;
        return;
    }

    private void Pong_WaitPing_Entry()
    {
    }
}