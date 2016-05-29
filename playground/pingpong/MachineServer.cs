class MachineServer : PMachine {
	public enum State {
		STATE_WaitPing,
		STATE_SendPong
	}

	private MachineServer.State state;

	public MachineServer() {
		state = STATE_WaitPing;
	}
	public override void RunStatemachine(Pingpong_Event event) {
		if(state == STATE_WaitPing) {
			if (event == PING) {
				state = STATE_SendPong;
				SendPongEntry();
			} else {
				throw Exception("Unhandled event");
			}
		} else if (state == STATE_SendPong) {
			if (event == SUCCESS) {
				state = STATE_WaitPong;
			} else {
				throw Exception("Unhandled event");
			}
		}
	}
	private void SendPongEntry(PMachine payload) {
		sendMsg(payload, PONG);
		raiseEvent(SUCCESS);
	}
	private void raiseEvent(Pingpong_Event event) {
		return RunStatemachine(event);
	}
}