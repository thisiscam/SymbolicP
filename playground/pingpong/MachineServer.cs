#include "pingpong_Macros.h"

using System;

class MachineServer : PMachine {
	public MachineServer() {
		state = 0;
	}
	public void SendPongEntry(object[] payload) {
		PMachine arg0 = (PMachine)payload[0];
		sendMsg(arg0, PONG, null);
		SendPong_RaiseEvent(SUCCESS); return;
	}
	public void SendPong_RaiseEvent(int e) {
		int pc = 0;
		while(true) {
		switch(pc) {
			case 0:		if(e != SUCCESS) {pc = 3 ; break;}					
						this.state = MachineServer_STATE_WaitPing;								
						pc = -1; break;									goto case 3;
			case 3:		throw new SystemException("Unhandled event");	goto case -1;
			case -1:	return;
		}}
	}
}