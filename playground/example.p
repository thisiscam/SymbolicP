event E1 : (a: int, b: bool); //event with payload as a named tuple
event E2 : (int, seq[(int, int)]);
event E3 : seq[any]; //P supports any type which is a super type of all types.

//main machine is where the execution starts
main machine M1 {
	//each state machine has local variables
	var x1: int;
	var x2: any;
	var x3: map[int, seq[int]];
	var m: machine;

	//Each machine has a start state
	start state S1 {
		ignore E2;
		defer E3;
		entry {
			x1 = x1 + 1;
			x2 = Foo(x1);
			assert(x2 == x1* x1);
			m = new M1(x1, x2);
			raise E1, (a = x2 as int, b = x1);
		}

		on E1 goto S2 with (payload: (a:int, b:int)) {
			payload.a;
			Bar();
		}

		exit {
			send this, E2, x3;
		}

	}
	state S2 {
		// can use function as entry function
		entry Bar;

		on E2 do { // possible payload is simply ignored
			halt
		};

	}

	fun Bar(payload : seq[any]) { }
	//state machines have local functions
	fun Foo(x:int):int
	{
		x3 
		return x3[x * x][0];
	}
}

}