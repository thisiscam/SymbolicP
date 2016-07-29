using System;

class Program {
    static int Main(string[] args) {
        int maxExplorationSteps = 20;
        Random rng = new Random();

        int iteration = 0;
        while(true) {
            Console.WriteLine(String.Format("========BEGIN NEW TRACE {0}=========", iteration));
            Scheduler scheduler = new Scheduler(rng);

            PMachine mainMachine = MachineController.CreateMainMachine();
            scheduler.StartMachine(mainMachine, null);

            for(int i=0; i < maxExplorationSteps; i++) { 
                if(!scheduler.ChooseAndRunMachine()) {
                    break;
                }
            }
            Console.WriteLine("===========END TRACE===========");
            iteration ++;
        }
        
        return 0;
    }
}