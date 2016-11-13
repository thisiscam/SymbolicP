#if RANDOM_SCHEDULER
using System;
using System.Diagnostics;

partial class Scheduler
{
    VSSet<PMachine> machines = new VSSet<PMachine>();

    private ValueSummary<SchedulerChoice> ChooseMachine()
    {
        var _frame_pc = PathConstraint.GetPC();
        var vs_ret_0 = new ValueSummary<SchedulerChoice>();
        ValueSummary<List<SchedulerChoice>> choices = new ValueSummary<List<Scheduler.SchedulerChoice>>(new List<SchedulerChoice>());
		machines.ForEach((machine) =>
        {
            ValueSummary<bool> vs_lgc_tmp_0;
            ValueSummary<// Collect from send queue
            List<SendQueueItem>> sendQueue = machine.GetConstField<List<SendQueueItem>>(_ => _.sendQueue);
            ValueSummary<bool> do_loop = true;
            var vs_cond_40 = PathConstraint.BeginLoop();
            for (ValueSummary<int> j = 0; vs_cond_40.Loop((new Func<ValueSummary<bool>>(() =>
            {
                var vs_cond_56 = ((vs_lgc_tmp_0 = ValueSummary<bool>.InitializeFrom(do_loop))).Cond();
                var vs_cond_ret_56 = new ValueSummary<bool>();
                if (vs_cond_56.CondTrue())
                    vs_cond_ret_56.Merge(vs_lgc_tmp_0.InvokeBinary<bool, bool>((l, r) => l & r, j.InvokeBinary<int, bool>((l, r) => l < r, sendQueue.GetField<int>(_ => _.Count))));
                if (vs_cond_56.CondFalse())
                    vs_cond_ret_56.Merge(vs_lgc_tmp_0);
                vs_cond_56.MergeBranch();
                return vs_cond_ret_56;
            }

            )())); j.Increment())
            {
                ValueSummary<SendQueueItem> item = sendQueue.InvokeMethod<int, SendQueueItem>((_, a0) => _[a0], j);
                var vs_cond_39 = (item.GetField<PInteger>(_ => _.e).InvokeBinary<PInteger, PBool>((l, r) => l == r, (PInteger)Constants.EVENT_NEW_MACHINE)).Cond();
                {
                    if (vs_cond_39.CondTrue())
                    {
                        choices.InvokeMethod<Scheduler.SchedulerChoice>((_, a0) => _.Add(a0), new ValueSummary<Scheduler.SchedulerChoice>(new SchedulerChoice(machine, j, -1)));
                        do_loop.Assign<bool>(false);
                    }

                    if (vs_cond_39.CondFalse())
                    {
                        ValueSummary<int> state_idx = item.GetField<PMachine>(_ => _.target).InvokeMethod<PInteger, int>((_, a0) => _.CanServeEvent(a0), item.GetField<PInteger>(_ => _.e));
                        var vs_cond_38 = (state_idx.InvokeBinary<int, bool>((l, r) => l >= r, 0)).Cond();
                        {
                            if (vs_cond_38.CondTrue())
                            {
                                choices.InvokeMethod<Scheduler.SchedulerChoice>((_, a0) => _.Add(a0), new ValueSummary<Scheduler.SchedulerChoice>(new SchedulerChoice(machine, j, state_idx)));
                                do_loop.Assign<bool>(false);
                            }
                        }

                        vs_cond_38.MergeBranch();
                    }
                }

                vs_cond_39.MergeBranch();
            }

            vs_cond_40.MergeBranch();
            ValueSummary<// Machine is state that can serve null event?
            int> null_state_idx = machine.InvokeMethod<PInteger, int>((_, a0) => _.CanServeEvent(a0), (PInteger)Constants.EVENT_NULL);
            var vs_cond_41 = (null_state_idx.InvokeBinary<int, bool>((l, r) => l >= r, 0)).Cond();
            {
                if (vs_cond_41.CondTrue())
                {
                    choices.InvokeMethod<Scheduler.SchedulerChoice>((_, a0) => _.Add(a0), new ValueSummary<Scheduler.SchedulerChoice>(new SchedulerChoice(machine, -1, null_state_idx)));
                }
            }

            vs_cond_41.MergeBranch();
        });

        var vs_cond_43 = (choices.GetField<int>(_ => _.Count).InvokeBinary<int, bool>((l, r) => l == r, 0)).Cond();
        {
            if (vs_cond_43.CondTrue())
            {
                PathConstraint.RecordReturnPath(vs_ret_0, new ValueSummary<Scheduler.SchedulerChoice>(null), vs_cond_43);
            }
        }

        if (vs_cond_43.MergeBranch())
        {
            ValueSummary<int> idx = PathConstraint.ChooseRandomIndex(choices.GetField<int>((_) => _.Count));        	

            PathConstraint.RecordReturnPath(vs_ret_0, choices.InvokeMethod<int, Scheduler.SchedulerChoice>((_, a0) => _[a0], idx));
        }

        PathConstraint.RestorePC(_frame_pc);
        return vs_ret_0;
    }
    
	public void StartMachine(ValueSummary<PMachine> machine, ValueSummary<IPType> payload)
    {
        this.machines.Add(machine);
        machine.InvokeMethod<Scheduler, IPType>((_, a0, a1) => _.StartMachine(a0, a1), this, payload);
    }
}
#endif
