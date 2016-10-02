event Success;

main machine A {
    var b: machine;

    start state Init {
        entry {
            b = new B();
            send this, Success;         
        }
        on Success goto Run;
    }

    state Run {
        entry {
            send this, Success;
        }
        on Success goto Run;
     }
}

machine B {
    start state Init {
        entry { 
            send this, Success;         
        }
        on Success goto Run;
    }

    state Run {
        entry {
             send this, Success;           
        }
        on Success goto Run;
    }
}
