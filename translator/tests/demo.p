event eEchoBack: int;

main machine Main {
    var x: int;
    var y: int;

    start state Init {
        entry {
            var i:int;
            var v:int;
            v = 1;
            while(i < 5) {
                new Node(this, v);
                i = i + 1;
                v = v * 2;
            }
        }
        on eEchoBack goto Receive;
    }

    state Receive {
        entry(msg:int) {
            if(x < msg) {
                x = x + msg;
            }
            y = y + msg;
        }
        on eEchoBack goto Receive;
     }
}

machine Node {
    start state Init {
        entry (payload:(machine, int)) {
            send payload.0, eEchoBack, payload.1;
        }
    }
}