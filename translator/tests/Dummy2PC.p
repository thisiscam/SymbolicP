/* Dummy Two Phase Commit Protocol */

event eTransaction: int;
event eCommit;
event eAbort;
event eSuccess;
event eFailure;

machine Main {
	start state Init {
		entry {
			var coor: machine;
			var index: int;
			coor = new CoordinateMachine();

			index = 0;
			while(index < 5)
			{
				if($)
				{
					send coor, eTransaction, index;
				}
			    index = index + 1;
			}
			raise halt;
		}
	}
}

machine CoordinateMachine {
	var participants: seq[machine];

	start state Init {
		entry {
			var index : int;
			var temp: machine;
			index = 0;
			while(index < 3)
			{	
				temp = new ParticipantMachine(this);
				participants += (index, temp);
			    index = index + 1;
			}
			goto WaitForRequest;
		}	
	}

	state WaitForRequest {
		on eTransaction goto TransactionState;
	}

	fun ChooseParticipantNonDet(): machine {
		var index: int;
		index = 0;
		while(index < sizeof(participants))
		{
			if($)
			{
				return participants[index];
			}
		    index = index + 1;
		}
		return participants[0];
	}
	state TransactionState {
		defer eTransaction;
		entry {
			var p : machine;
			p  = ChooseParticipantNonDet();
			if($)
			{
				send p, eCommit;
			}
			else
			{
				send p, eAbort;
			}
		}

		on eSuccess, eFailure goto WaitForRequest;
	}
}

machine ParticipantMachine {
	var coor: machine;
	start state Init {
		entry (payload: machine) {
			coor = payload;
			goto WaitForRequest;
		}
	}
	
	state WaitForRequest {
		on eCommit, eAbort do {
			if($)
			{
				send coor, eSuccess;
			}
			else
			{
				send coor, eFailure;
			}
		}
	}

}