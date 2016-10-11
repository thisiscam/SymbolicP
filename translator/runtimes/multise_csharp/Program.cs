using System;

class Program {
    static int Main(string[] args) {
        int maxExplorationSteps = 200;

        Scheduler scheduler = new Scheduler();

        ValueSummary<PMachine> mainMachine = MachineController.CreateMainMachine();
        scheduler.StartMachine(mainMachine, null);

        for(int i=0; i < maxExplorationSteps; i++) {
			var vs_cond_main = scheduler.ChooseAndRunMachine().Cond();
			if (vs_cond_main.state == PathConstraint.BranchPoint.State.Both || vs_cond_main.state == PathConstraint.BranchPoint.State.True) {
				PathConstraint.AddAxiom(vs_cond_main.pc);
			}
			else {
				break;
			}
        }
        return 0;
    }
}