/* 
	PING = 0
	PONG = 1
	SUCCESS = 2

	MachineServer.STATE_WaitPing = 0
	MachineServer.STATE_SendPong = 1
	
	MachineClient.STATE_Init = 0
	MachineClient.STATE_SendPing = 1
	MachineClient.STATE_WaitPong = 2
*/
#define PING 0
#define PONG 1
#define SUCCESS 2
#define MachineServer_STATE_WaitPing 0
#define MachineServer_STATE_SendPong 1
#define MachineClient_STATE_Init 0
#define MachineClient_STATE_SendPing 1
#define MachineClient_STATE_WaitPong 2
