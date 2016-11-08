using System;
using System.Diagnostics;
using System.Linq;

public class Program {
	public static CommandLineOptions options = new CommandLineOptions();
	
	private static void Run()
	{
	    Scheduler scheduler = new Scheduler();
#if ROUND_ROBIN_SCHEDULER
		scheduler.DelayBudget = options.DelayBudget;
#endif
		ValueSummary<PMachine> mainMachine = MachineController.CreateMainMachine();
        scheduler.StartMachine(mainMachine, new ValueSummary<IPType>(null));
	
		Stopwatch Watch = new Stopwatch();
		var loop = PathConstraint.BeginLoop();
        for(int i=0; i < options.MaxNumSchedulerIterations; i++) {
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
	}
	
	static void HandleProgramException(Exception e)
	{
		Console.WriteLine("Encountered error during expoloration: {0}", e);
    	Console.WriteLine("Saving program trace log to {0}...", options.ErrorTraceFile);
		PathConstraint.SaveTrace(options.ErrorTraceFile);
		Console.WriteLine("You can rerun with -r to inspect the trace! Good Bye!");
		Process.GetCurrentProcess().Kill(); //TODO better way to shut down the system
	}
	
    static int Main(string[] args) 
	{
		CommandLine.Parser.Default.ParseArguments(args, options);
#if USE_SYLVAN
			SylvanSharp.Lace.Init(
				options.SylvanLaceNWorkers, 
				options.SylvanLaceStack, 
				ex_handler: HandleProgramException
			);
#endif
		if(options.Rerun) {
			PathConstraint.RecoverFromTrace(options.ErrorTraceFile);
			Run();
		} else {
			try {
		    	Run();
		    } catch(Exception ex) {
		    	HandleProgramException(ex);
			}
		}
        return 0;
    }
}