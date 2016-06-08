#include "common_Macros.h"

#define eOpenDoor 0
#define eCloseDoor 1
#define eResetDoor 2
#define eDoorOpened 3
#define eDoorClosed 4
#define eDoorStopped 5
#define eObjectDetected 6
#define eTimerFired 7
#define eOperationSuccess 8
#define eOperationFailure 9
#define eSendCommandToOpenDoor 10
#define eSendCommandToCloseDoor 11
#define eSendCommandToStopDoor 12
#define eSendCommandToResetDoor 13
#define eStartDoorCloseTimer 14
#define eStopDoorCloseTimer 15
#define eUnit 16
#define eStopTimerReturned 17
#define eObjectEncountered 18

#define MachineElevator_STATE_Init 0
#define MachineElevator_STATE_DoorClosed 1
#define MachineElevator_STATE_DoorOpening 2
#define MachineElevator_STATE_DoorOpened 3
#define MachineElevator_STATE_DoorOpenedOkToClose 4
#define MachineElevator_STATE_DoorClosing 5
#define MachineElevator_STATE_StoppingDoor 6
#define MachineElevator_STATE_StoppingTimer 7
#define MachineElevator_STATE_WaitingForTimer 8
#define MachineElevator_STATE_ReturnState 9

#define MachineUser_STATE_Init 0
#define MachineUser_STATE_Loop 1

#define MachineDoor_STATE__Init 0
#define MachineDoor_STATE_Init 1
#define MachineDoor_STATE_OpenDoor 2
#define MachineDoor_STATE_ConsiderClosingDoor 3
#define MachineDoor_STATE_ObjectEncountered 4
#define MachineDoor_STATE_CloseDoor 5
#define MachineDoor_STATE_StopDoor 6
#define MachineDoor_STATE_ResetDoor 7

#define MachineTimer_STATE__Init 0
#define MachineTimer_STATE_Init 1
#define MachineTimer_STATE_TimerStarted 2
#define MachineTimer_STATE_SendTimerFired 3
#define MachineTimer_STATE_ConsiderStopping 4