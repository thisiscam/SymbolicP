using System;
using System.Diagnostics;

public class Program {
	public static CommandLineOptions options = new CommandLineOptions();
	
    static int Main(string[] args) {
		CommandLine.Parser.Default.ParseArguments(args, options);
    	
        int maxExplorationSteps = options.MaxNumSchedulerIterations;

        Scheduler scheduler = new Scheduler();
		try {
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
	    } catch (Exception ex) {
			Console.WriteLine("Program encountered exception at PC = {0}(in BDD form), exception trace follows", PathConstraint.GetPC());
			Console.WriteLine(ex);
		}
        return 0;
    }
}