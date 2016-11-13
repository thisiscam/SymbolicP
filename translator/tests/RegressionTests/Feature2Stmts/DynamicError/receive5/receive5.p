// This sample tests raise of event inside receive.
event E;
event F;

main machine Main {
	var x: int;
	start state Init {
		entry {
			x = x + 1;
			assert x == 1;
			foo();
			assert x == 2;
		}
	}
	fun foo() {
		send this, E;
		receive { 
			case E: { pop; } 
		}
		x = x + 1;
	}
}
