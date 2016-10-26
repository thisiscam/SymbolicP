using System;
using System.Diagnostics;

class Program {
    static int Main(string[] args) {
        int maxExplorationSteps = 200;

        Scheduler scheduler = new Scheduler();

        ValueSummary<PMachine> mainMachine = MachineController.CreateMainMachine();
        scheduler.StartMachine(mainMachine, new ValueSummary<IPType>(null));
	
		Stopwatch Watch = new Stopwatch();
		var loop = PathConstraint.BeginLoop();
        for(int i=0; i < maxExplorationSteps; i++) {
        	Watch.Start();
			if (!loop.Loop(scheduler.ChooseAndRunMachine())) { 
				break; 
			}
			Watch.Stop();
			Console.WriteLine("==== Iter {0}========", i);
			Console.WriteLine("Solver: {0}, Total: {1}, Convert: {2}", PathConstraint.SolverStopWatch.Elapsed, Watch.Elapsed, BDDToZ3Wrap.Converter.Watch.Elapsed);
			PathConstraint.SolverStopWatch.Reset();
			Watch.Reset();
			BDDToZ3Wrap.Converter.Watch.Reset();
        }
        return 0;
    }
}