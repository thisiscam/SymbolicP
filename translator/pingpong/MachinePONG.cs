using System;

class MachinePONG : PMachine {
/* P local vars */

private readonly static bool[,] _DeferedSet = {
{false,false,false,},
{false,false,false,},
};
public MachinePONG () {
this.DeferedSet = _DeferedSet;
this.Transitions = new TransitionFunction[2][3];
this.Transitions[MachinePONG_S_Pong_SendPong,Success] = Pong_SendPong_on_Success;
this.Transitions[MachinePONG_S_Pong_WaitPing,Ping] = Pong_WaitPing_on_Ping;
}
public override void StartMachine (Scheduler s, object payload) {
base.StartMachine(s, payload);
this.state = MachinePONG_S_Pong_WaitPing;
Pong_WaitPing_Entry();
}
private void Pong_SendPong_on_Success (object payload) {
this.state = MachinePONG_S_Pong_WaitPing;
Pong_WaitPing_Entry();
}
private void Pong_WaitPing_on_Ping (object payload) {
this.state = MachinePONG_S_Pong_SendPong;
Pong_SendPong_Entry((PMachine)payload);
}
private void Pong_SendPong_Entry (PMachine payload) {
this.SendMsg(payload, Pong, null);
this.ServeEvent(Success, null); retcode = RAISED_EVENT; return;
}
private void Pong_WaitPing_Entry () {
}
}