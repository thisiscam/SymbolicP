// This sample tests pop inside a top-level entry function.

main machine Main {
	start state Init {
		entry {
			pop;
		}
	}
}

