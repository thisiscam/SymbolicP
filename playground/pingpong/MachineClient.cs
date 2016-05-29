class MachineClient : PMachine {
	public enum State {
		STATE_WaitPing,
		STATE_SendPong
	};

	MachineClient.State state;

	/* P local vars */
	PMachine server;

	public MachineClient() {
		state = STATE_Init;
		InitEntry();
	}
	public override void RunStatemachine(Pingpong_Event event) {
		if(state == STATE_Init) {
			if (event == SUCCESS) {
				state = STATE_SendPing;
				SendPingEntry();
			} else {
				throw Exception("Unhandled event");
			}
		} else if (state == STATE_SendPing) {
			if (event == PING) {
				state = STATE_WaitPong;
				WaitPingEntry();
			} else {
				throw Exception("Unhandled event");
			}
		} else if (state == STATE_WaitPong) {
			if (event == PONG) {
				state = STATE_SendPing;
				SendPingEntry();
			}
		}
	}
	private void InitEntry() {
		server = new MachineServer();
		raiseEvent(SUCCESS); return;
	}
	private void SendPingEntry() {
		sendMsg(this.server, PING, this);
		raiseEvent(SUCCESS); return;
	}
	private void WaitPingEntry() {

	}
	private void raiseEvent(Pingpong_Event event) {
		return RunStatemachine(event);
	}
}