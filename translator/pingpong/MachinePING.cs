using System;

class MachinePING : PMachine {
/* P local vars */

private readonly static bool[,] _DeferedSet = {
{false,false,false,},
{false,false,false,},
{false,false,false,},
{false,false,false,},
};
PMachine pongId;
public MachinePING () {
this.DeferedSet = _DeferedSet;
this.Transitions = new TransitionFunction[4][3];
this.Transitions[MachinePING_S_Ping_WaitPong,Pong] = Ping_WaitPong_on_Pong;
this.Transitions[MachinePING_S_Ping_Init,Success] = Ping_Init_on_Success;
this.Transitions[MachinePING_S_Ping_SendPing,Success] = Ping_SendPing_on_Success;
}
public override void StartMachine (Scheduler s, object payload) {
base.StartMachine(s, payload);
this.state = MachinePING_S_Ping_Init;
Ping_Init_Entry();
}
private void Ping_WaitPong_on_Pong (object payload) {
this.state = MachinePING_S_Ping_SendPing;
Ping_SendPing_Entry();
}
private void Ping_Init_on_Success (object payload) {
this.state = MachinePING_S_Ping_SendPing;
Ping_SendPing_Entry();
}
private void Ping_SendPing_on_Success (object payload) {
this.state = MachinePING_S_Ping_WaitPong;
}
private void Ping_SendPing_Entry () {
this.SendMsg(pongId, Ping, this);
this.ServeEvent(Success, null); retcode = RAISED_EVENT; return;
}
private void Ping_Init_Entry () {
pongId = NewMachine(new MachinePONG());
this.ServeEvent(Success, null); retcode = RAISED_EVENT; return;
}
}