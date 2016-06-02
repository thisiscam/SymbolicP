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

using System;
using System.Diagnostics;

class Dispatcher {
	public void StartMainMachine() {
		PMachine m = new MachineClient();
		Scheduler.machines.Add(m);
	}

	public void RunStateMachine(PMachine machine, int e, object[] payload) {
		int L0 = -1, L1 = -1;
		int pc = 0;
		while(true) {
		switch(pc) {
		case  0:		if (!(machine is MachineServer)) {pc = 13; break;}		
						L0 = machine.state;									
						if(L0 != 0) {pc = 8; break;}							
						if(e != 0) {pc = 7; break;}								
						machine.state = 1;										
						((MachineServer)machine).SendPongEntry(payload);		
						pc = -1; break;											goto case 7;
		case  7:		throw new SystemException("Unhandled event");			goto case 8;
		case  8:		/** else L0 = 1 */										
						if(e != 2) {pc = 11 ; break;}							
						machine.state = 0;										goto case 11;
		case 11:		pc = -1; break;											
						throw new SystemException("Unhandled event");			goto case 13;
		case 13:		/** else typeof(machine) == MachineClient  */			
						L1 = machine.state;									
						if(L1 != 0) {pc = 21; break;}							
						if (e != 2) {pc = 20; break;}							
						machine.state = 1;										
						((MachineClient)machine).SendPingEntry();				
						pc = -1; break;											goto case 20;		
		case 20:		throw new SystemException("Unhandled event");			goto case 21;
		case 21:		if (L1 != 1) {pc = 26; break;}							
						if(e != 0) {pc = 25; break;}							
						machine.state = 2;										
						pc = -1; break;											goto case 25;
		case 25:		throw new SystemException("Unhandled event");			goto case 26;
		case 26:		/* else L1 = 2 */										
						if(e != 1) {pc = 31; break;}							
						machine.state = 1;										
						((MachineClient)machine).SendPingEntry();				
						pc = -1; break;											
		case 31:		throw new SystemException("Unhandled event");			goto case -1;
		case -1:		return;														
		}}
	}
}