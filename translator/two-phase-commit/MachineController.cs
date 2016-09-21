class MachineController {

static MonitorPMachine monitor_m = new MachineM();

public static PMachine CreateMainMachine() {
return new MachineTwoPhaseCommit();
}

/* Observers */
public static void AnnounceEvent(PInteger e, IPType payload) {
switch(e) {case Constants.MONITOR_READ_SUCCESS: {
monitor_m.ServeEvent(Constants.MONITOR_READ_SUCCESS, payload);
break;
}
case Constants.MONITOR_READ_UNAVAILABLE: {
monitor_m.ServeEvent(Constants.MONITOR_READ_UNAVAILABLE, payload);
break;
}
case Constants.MONITOR_WRITE: {
monitor_m.ServeEvent(Constants.MONITOR_WRITE, payload);
break;
}
}
}
}
