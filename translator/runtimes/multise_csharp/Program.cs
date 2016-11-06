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
	
    static int Main(string[] args) {
		CommandLine.Parser.Default.ParseArguments(args, options);
		try {
	        Run();
	    } catch (Exception e) {
	    	var counterExampleBDD = PathConstraint.GetPC();
			Console.WriteLine("Program encountered exception at PC = {0}(in BDD form), exception trace follows", counterExampleBDD);
			counterExampleBDD = PathConstraint.ExtractOneCounterExampleFromAggregatePC(counterExampleBDD);
			PathConstraint.BeginRecover();
			ResetMachines();
			PathConstraint.AddAxiom(counterExampleBDD);
			try {
				Console.WriteLine("Rerun recovery trace, with counter example PC = {0}", counterExampleBDD);
				Console.WriteLine("Used BDD vars mapped as follow: ");
				BDDToZ3Wrap.PInvoke.debug_print_used_bdd_vars();
				Run();
			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
		}
        return 0;
    }
}