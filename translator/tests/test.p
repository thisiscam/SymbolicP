event RESP_REPLICA_COMMIT;
event WRITE_SUCCESS;
event READ_REQ;
event REP_READ_FAIL;
event Unit;

machine Replica {
    start state Init {
	    entry(coordinator:machine) {
		    send coordinator, RESP_REPLICA_COMMIT;
		}
	}
}

machine Coordinator {
	start state Init {
		entry {
			new Replica(this);
			raise(Unit);
		}
		on Unit goto Loop;
	}

	state DoRead {
		entry {
			send this, REP_READ_FAIL;
		}
		on REP_READ_FAIL goto Loop;
	}

	state Loop {
		on READ_REQ goto DoRead;
		ignore RESP_REPLICA_COMMIT;
	}
}

machine Client {
	start state Init {
	    entry(coordinator:machine) {
			send coordinator, READ_REQ;
		}
	}
}

main machine TwoPhaseCommit {
    start state Init {
	    entry {
	    	var coordinator: machine;
	        coordinator = new Coordinator();
			new Client(coordinator);
		}
	}
}