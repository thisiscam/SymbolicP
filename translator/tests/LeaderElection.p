/*
Leader election protocol with three machines
*/

event eAllNodesEvent: seq[machine];
event eMyId: int;

machine Main {
	var allNodes: seq[machine];
	start state Init {
		entry {
			//3 machines
			var index: int;
			var node: machine;
			index = 0;
			while(index < 3)
			{
				node = new Node(index);
				allNodes += (index, node);
			    index = index + 1;
			}
			index  = 0;
			while(index < sizeof(allNodes))
			{
				send allNodes[index], eAllNodesEvent, allNodes;
				index = index + 1;
			}
			raise halt;
		}
		
		
	}
}

machine Node {
	var id: int;
	var currLeader: int;
	var allNodes: seq[machine];

	start state Init {
		defer eMyId;
		entry (payload: int) {
			id = payload;
			currLeader = id;
		}
		on eAllNodesEvent goto StartBroadCasting with (payload: seq[machine]){
			allNodes = payload;
		}
	}

	var counter : int;
	state StartBroadCasting {
		entry {
			var index : int;
			index  = 0;
			counter = 0;
			while(index < sizeof(allNodes))
			{
				send allNodes[index], eMyId, id;
				index = index + 1;
			}
		}

		on eMyId do (payload: int) {
			counter = counter + 1;
			if(currLeader < payload)
			{
				currLeader = payload;
			}
			if(counter == sizeof(allNodes))
			{
				assert (currLeader == (sizeof(allNodes) -1));
			}
		}
	}
}
