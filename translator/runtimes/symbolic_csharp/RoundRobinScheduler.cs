#if ROUND_ROBIN_SCHEDULER
using System;
using System.Diagnostics;

partial class Scheduler {
	public int current_schedule_machine_idx = 0;
	public int delayBudget = 0;
	private SchedulerChoice ChooseMachine() {
		List<SchedulerChoice> choices = new List<SchedulerChoice>();
		int i = 0;
		for(; i < this.machines.Count && choices.Count <= this.delayBudget; i++)
		{
			int machine_idx = (i + current_schedule_machine_idx) % this.machines.Count;
	        PMachine machine = machines[machine_idx];
	        // Collect from send queue
	        List<SendQueueItem> sendQueue = machine.sendQueue;
	        bool do_loop = true;
			for(int j=0; do_loop && j < sendQueue.Count; j++) {
	            SendQueueItem item = sendQueue[j];
	            if(item.e == Constants.EVENT_NEW_MACHINE) {
	                choices.Add(new SchedulerChoice(machine, j, -1));
	                do_loop = false;
	            } else {
					int state_idx = item.target.CanServeEvent(item.e);
	                if (state_idx >= 0) {
	                    choices.Add(new SchedulerChoice(machine, j, state_idx));
	                    do_loop = false;
	                }
	            }
	        }
	        if(choices.Count <= this.delayBudget)
	        {
		        // Machine is state that can serve null event?
				int null_state_idx = machine.CanServeEvent(Constants.EVENT_NULL);
		        if(null_state_idx >= 0) {
		            choices.Add(new SchedulerChoice(machine, -1, null_state_idx));
		        }
		    }
	    }
	    if(choices.Count == 0) { return null; }
	    // Choose one and remove from send queue
        SymbolicInteger idx = SymbolicEngine.SE.NewSymbolicIntVar("SI", 0, choices.Count);
	    this.delayBudget = this.delayBudget - idx;
	    this.current_schedule_machine_idx = (i + current_schedule_machine_idx) % this.machines.Count;
	    return choices[idx];
	}
}
#endif