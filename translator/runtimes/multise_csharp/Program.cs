using System;

class Program {
    static int Main(string[] args) {
        int maxExplorationSteps = 200;

        int iteration = 0;
        Scheduler scheduler = new Scheduler();

        ValueSummary<PMachine> mainMachine = MachineController.CreateMainMachine();
        scheduler.StartMachine(mainMachine, null);

        for(int i=0; i < maxExplorationSteps; i++) { 
			if (scheduler.ChooseAndRunMachine ().Cond ()) {
				continue;
			} else {
				break;
			}
        }
        return 0;
    }
}