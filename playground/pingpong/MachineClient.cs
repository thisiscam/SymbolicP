#include "pingpong_Macros.h"

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
		Init_RaiseEvent(SUCCESS); return;
	}
	public void SendPingEntry() {
		sendMsg(this.server, PING, new object[]{this});
		SendPing_RaiseEvent(SUCCESS); return;
	}
	public void Init_RaiseEvent(int e) {		
		int pc = 0;
		while(true) {
		switch(pc) {
			case  0:	if(e != SUCCESS) {pc = 4; break;}						
						this.state = MachineClient_STATE_SendPing;								
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
			case  0:	if(e != SUCCESS) {pc = 3; break;}						
						this.state = MachineClient_STATE_WaitPong;																				
						pc = -1; break;									goto case 3;
			case  3:	throw new SystemException("Unhandled event");	goto case -1;
			case -1:	return;
		}}
	}

}