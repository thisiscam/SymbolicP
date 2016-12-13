event eUnit;
event eAStart:machine;
event eBStart:machine;
event eReady;
event eMyId:int;

fun max(a:int, b:int):int {
    if(a > b) {
        return a;
    } else {
        return b;
    }
}

main machine Main {
    var a, b, c: machine;
    start state Init {
        entry {
            a = new Node(this, 0);
            b = new Node(this, 1);
            send a, eAStart, b;
        }
        on eReady do {}
    }
}

machine Node {
    var myId: int;
    var leaderId:int;
    var mainMachine:machine;
    start state Init {
        defer eMyId;
        entry(payload:(machine, int)) { 
            mainMachine=payload.0; 
            myId = payload.1;
            leaderId = -1;
        }
        on eAStart goto LeaderElection with (payload:machine) { 
            send payload, eBStart, this; 
        }
        on eBStart goto LeaderElection;
    }

    state LeaderElection {
        entry (other:machine) {
            send other, eMyId, myId;
            send mainMachine, eReady;
        }
        on eMyId do (id:int) {
            leaderId = max(leaderId, id);
        }
    }
}
