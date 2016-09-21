using System;
using System.Collections.Generic;
using System.Diagnostics;

class MachineM : MonitorPMachine {

private readonly static bool[,] _DeferedSet = {
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
};

private readonly static bool[,] _IsGotoTransition = {
{false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,},
};
/* P local vars */
PMap<PInteger,PInteger> data=new PMap<PInteger,PInteger>();


public MachineM () {
this.DeferedSet = _DeferedSet;
this.IsGotoTransition = _IsGotoTransition;
this.Transitions = new TransitionFunction[1,22];
this.Transitions[Constants.MachineM_S_Init,Constants.MONITOR_WRITE] = Init_on_MONITOR_WRITE;
this.Transitions[Constants.MachineM_S_Init,Constants.MONITOR_READ_SUCCESS] = Init_on_MONITOR_READ_SUCCESS;
this.Transitions[Constants.MachineM_S_Init,Constants.MONITOR_READ_UNAVAILABLE] = Init_on_MONITOR_READ_UNAVAILABLE;
this.ExitFunctions = new ExitFunction[1];
this.states.Insert(0, Constants.MachineM_S_Init);
}
private void Init_on_MONITOR_WRITE (IPType _payload) {
PTuple<PInteger,PInteger> payload = (PTuple<PInteger,PInteger>)_payload;
data[payload.Item1] = payload.Item2;
}
private void Init_on_MONITOR_READ_SUCCESS (IPType _payload) {
PTuple<PInteger,PInteger> payload = (PTuple<PInteger,PInteger>)_payload;
Assert((data.ContainsKey(payload.Item1)));
Assert((data[payload.Item1] == payload.Item2));
}
private void Init_on_MONITOR_READ_UNAVAILABLE (IPType _payload) {
PInteger payload = (PInteger)_payload;
Assert((!(data.ContainsKey(payload))));
}
}