using CommandLine;

public class CommandLineOptions
{
#if USE_SYLVAN
	[Option('w', "lace-num-workers", DefaultValue=0, HelpText = "Number of Lace workers, default=0 for auto-detection")]
    public int SylvanLaceNWorkers { get; set; }
    
    [Option('s', "lace-stack-size", DefaultValue=100000UL, HelpText = "Lace stack size, usually no need to change")]
    public ulong SylvanLaceStack { get; set; }
    
	[Option('n', "sylvan-init-nodes", DefaultValue=24, HelpText = "Number of initial BDD(lg2) nodes used in Sylvan library")]
    public int SylvanNumInitialNodesLg2 { get; set; }
	    
    [Option('m', "sylvan-max-nodes", DefaultValue=27, HelpText = "Number of maxium BDD(lg2) nodes used in Sylvan library")]
    public int SylvanNumMaxNodesLg2 { get; set; }
    
	[Option('c', "sylvan-init-cachesize", DefaultValue=24, HelpText = "Initial cache size(lg2) used in Sylvan library")]
    public int SylvanNumInitialCachesizeLg2 { get; set; }
	    
    [Option('x', "sylvan-max-cachesize", DefaultValue=27, HelpText = "Maxium cache size(lg2) used in Sylvan library")]
    public int SylvanNumMaxCachesizeLg2 { get; set; }
    
    [Option('g', "sylvan-granularity", DefaultValue=1, HelpText = "Maxium cache size(lg2) used in Sylvan library")]
    public int SylvanGranularity { get; set; }
#else
	[Option('b', "num-bdd-nodes", DefaultValue=5000000, HelpText = "Number of initial BDD nodes used in BuDDy library")]
    public int BDDNumInitialNodes { get; set; }

	[Option('c', "bdd-cache-ratio", DefaultValue=1, HelpText = "Cache ratio of BuDDy library")]
    public int BDDCacheRatio { get; set; }
    
    [Option('v', "bdd-num-vars", DefaultValue=10000, HelpText = "Number of BDD variables")]
    public int BDDNumVars { get; set; }
    
    [Option('m', "bdd-max-increase", DefaultValue=1000000, HelpText = "BuDDy max increase after BDD node GC")]
    public int BDDMaxIncrease { get; set; }
#endif

    [Option('i', "num-iter", DefaultValue=200, HelpText = "Max number of scheduler steps to explore")]
    public int MaxNumSchedulerIterations { get; set; }

#if ROUND_ROBIN_SCHEDULER
	[Option('d', "delay-budget", DefaultValue=1, HelpText = "Delay budget for scheduler")]
    public int DelayBudget { get; set; }
#endif
}