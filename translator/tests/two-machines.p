event Success;

main machine A {
    var b: machine;
    var counter:int;

    start state Init {
        entry {
            counter = 0;
            b = new B();
            send this, Success;         
        }
        on Success goto Run;
    }

    state Run {
        entry {
            counter = counter + 1;
            send this, Success;
        }
        on Success goto Run;
     }
}

machine B {
    var counter:int;
    start state Init {
        entry { 
            counter = 0;
            send this, Success;         
        }
        on Success goto Run;
    }

    state Run {
        entry {
             counter = counter + 1;
             send this, Success;           
        }
        on Success goto Run;
    }
}
