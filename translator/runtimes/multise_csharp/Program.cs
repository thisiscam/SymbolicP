using System;
using System.Diagnostics;
using System.Linq;
using BDDToZ3Wrap; 
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
		Console.WriteLine("Current used BDDs:");
		BDDToZ3Wrap.PInvoke.debug_print_used_bdd_vars();
    	Console.WriteLine("Saving program trace log to {0}...", options.ErrorTraceFile);
		PathConstraint.SaveTrace(options.ErrorTraceFile);
		Console.WriteLine("You can rerun with -r to inspect the trace! Good Bye!");
		Process.GetCurrentProcess().Kill(); //TODO better way to shut down the system
	}
	
	private static void ResetMachines()
	{
		var subclasses =
		from assembly in AppDomain.CurrentDomain.GetAssemblies()
		    from type in assembly.GetTypes()
		    where type.IsSubclassOf(typeof(PMachine))
		    select type;
		foreach(var machineSubclass in subclasses)
		{
			var allocsField = machineSubclass.GetField("_allAllocsCounter", System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
			allocsField.SetValue(null, new ValueSummary<int>(0));
			var allAllocsField = machineSubclass.GetField("_allAllocs", System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
			allAllocsField.SetValue(null, new System.Collections.Generic.List<PMachine>());
		}
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
		    } catch(Exception e) {
		    	HandleProgramException(e);
			}
		}
        return 0;
    }
}