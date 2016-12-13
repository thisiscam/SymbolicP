event eFib: int;
event eDie;

main machine Main {
    fun localFib(n:int):int {
        if(n < 2) {
            return 1;
        } else {
            return localFib(n - 1) + localFib(n - 2);
        }
    }

    var n:int;

    start state Init {
        entry {
            n = 4;
            new Fib(this, n);
        }
        on eFib do (result:int) {
            assert result == localFib(n);
        }
    }
}

machine Fib {
    var master:machine;
    var result:int;
    start state Init {
        entry (payload:(master:machine, n:int)) {
            var n:int;
            master = payload.master;
            n = payload.n;
            if(n < 2) {
                send master, eFib, 1;
            } else {
                new Fib(this, n - 1);
                new Fib(this, n - 2);
            }
        }
        on eFib goto WaitForSecond;
    }
    state WaitForSecond {
        entry(subresult:int) {
            result = subresult;
        }
        on eFib do (subresult:int) {
            result = result + subresult;
            send master, eFib, result;
        }
    }
}