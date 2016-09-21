public partial class Constants {
public const int REQ_REPLICA = 1;
public const int RESP_REPLICA_COMMIT = 2;
public const int RESP_REPLICA_ABORT = 3;
public const int GLOBAL_ABORT = 4;
public const int GLOBAL_COMMIT = 5;
public const int WRITE_REQ = 6;
public const int WRITE_FAIL = 7;
public const int WRITE_SUCCESS = 8;
public const int READ_REQ = 9;
public const int READ_FAIL = 10;
public const int READ_UNAVAILABLE = 11;
public const int READ_SUCCESS = 12;
public const int Unit = 13;
public const int Timeout = 14;
public const int StartTimer = 15;
public const int CancelTimer = 16;
public const int CancelTimerFailure = 17;
public const int CancelTimerSuccess = 18;
public const int MONITOR_WRITE = 19;
public const int MONITOR_READ_SUCCESS = 20;
public const int MONITOR_READ_UNAVAILABLE = 21;

public const int MachineTimer_S_Init = 0;
public const int MachineTimer_S_Loop = 1;
public const int MachineTimer_S_TimerStarted = 2;

public const int MachineReplica_S_Init = 0;
public const int MachineReplica_S_Loop = 1;

public const int MachineCoordinator_S_Init = 0;
public const int MachineCoordinator_S_Loop = 1;
public const int MachineCoordinator_S_CountVote = 2;
public const int MachineCoordinator_S_WaitForCancelTimerResponse = 3;
public const int MachineCoordinator_S_WaitForTimeout = 4;

public const int MachineClient_S_Init = 0;
public const int MachineClient_S_DoWrite = 1;
public const int MachineClient_S_DoRead = 2;
public const int MachineClient_S_End = 3;

public const int MachineM_S_Init = 0;

public const int MachineTwoPhaseCommit_S_Init = 0;
}