using System;
using System.Diagnostics;

class MachineClient : PMachine {
	/* P local vars */
	PMachine server;

	public MachineClient() {
		state = 0;
		InitEntry();
	}
	public void InitEntry() {
		server = new MachineServer();
		Scheduler.machines.Add(server);
		Init_RaiseEvent(2); return;
	}
	public void SendPingEntry() {
		sendMsg(this.server, 0, new object[]{this});
		SendPing_RaiseEvent(2); return;
	}
	public void Init_RaiseEvent(int e) {		
		int pc = 0;
		while(true) {
		switch(pc) {
			case  0:	if(e != 2) {pc = 4; break;}						
						this.state = 1;								
						this.SendPingEntry();						
						pc = -1; break;									goto case 4;				
			case  4:	throw new SystemException("Unhandled event");	goto case -1;
			case -1:	return;
		}}
	}
	public void SendPing_RaiseEvent(int e) {
		int pc = 0;
		while(true) {
		switch(pc) {
			case  0:	if(e != 2) {pc = 3; break;}						
						this.state = 2;																				
						pc = -1; break;									goto case 3;
			case  3:	throw new SystemException("Unhandled event");	goto case -1;
			case -1:	return;
		}}
	}

}