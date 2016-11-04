using CommandLine;

public class CommandLineOptions
{
	[Option('b', "num-bdd-nodes", DefaultValue=5000000, HelpText = "Number of initial BDD nodes used in BuDDy library")]
    public int BDDNumInitialNodes { get; set; }

	[Option('c', "bdd-cache-ratio", DefaultValue=1, HelpText = "Cache ratio of BuDDy library")]
    public int BDDCacheRatio { get; set; }
    
    [Option('v', "bdd-num-vars", DefaultValue=10000, HelpText = "Number of BDD variables")]
    public int BDDNumVars { get; set; }
    
    [Option('m', "bdd-max-increase", DefaultValue=1000000, HelpText = "BuDDy max increase after BDD node GC")]
    public int BDDMaxIncrease { get; set; }
    
    [Option('i', "num-iter", DefaultValue=200, HelpText = "Max number of scheduler steps to explore")]
    public int MaxNumSchedulerIterations { get; set; }

#if ROUND_ROBIN_SCHEDULER
	[Option('d', "delay-budget", DefaultValue=1, HelpText = "Delay budget for scheduler")]
    public int DelayBudget { get; set; }
#endif
}