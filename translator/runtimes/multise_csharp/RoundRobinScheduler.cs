#if ROUND_ROBIN_SCHEDULER
using System;
using System.Diagnostics;

partial class Scheduler
{
    public ValueSummary<int> current_schedule_machine_idx = 0;
    public ValueSummary<int> delayBudget = 1;
    private ValueSummary<SchedulerChoice> ChooseMachine()
    {
        PathConstraint.PushFrame();
        var vs_ret_0 = new ValueSummary<SchedulerChoice>();
        ValueSummary<bool> vs_lgc_tmp_0;
        ValueSummary<List<SchedulerChoice>> choices = new ValueSummary<List<Scheduler.SchedulerChoice>>(new List<SchedulerChoice>());
        ValueSummary<int> i = 0;
        var vs_cond_49 = PathConstraint.BeginLoop();
        for (; vs_cond_49.Loop((new Func<ValueSummary<bool>>(() =>
        {
            var vs_cond_57 = ((vs_lgc_tmp_0 = ValueSummary<bool>.InitializeFrom(i.InvokeBinary<int, bool>((l, r) => l < r, this.machines.GetField<int>(_ => _.Count))))).Cond();
            var vs_cond_ret_57 = new ValueSummary<bool>();
            if (vs_cond_57.CondTrue())
                vs_cond_ret_57.Merge(vs_lgc_tmp_0.InvokeBinary<bool, bool>((l, r) => l & r, choices.GetField<int>(_ => _.Count).InvokeBinary<int, bool>((l, r) => l <= r, this.delayBudget)));
            if (vs_cond_57.CondFalse())
                vs_cond_ret_57.Merge(vs_lgc_tmp_0);
            vs_cond_57.MergeBranch();
            return vs_cond_ret_57;
        }

        )())); i.Increment())
        {
            ValueSummary<bool> vs_lgc_tmp_1;
            ValueSummary<int> machine_idx = (i.InvokeBinary<int, int>((l, r) => l + r, this.current_schedule_machine_idx)).InvokeBinary<int, int>((l, r) => l % r, this.machines.GetField<int>(_ => _.Count));
            ValueSummary<PMachine> machine = this.machines.InvokeMethod<int, PMachine>((_, a0) => _[a0], machine_idx);
            ValueSummary<// Collect from send queue
            List<SendQueueItem>> sendQueue = machine.GetConstField<List<SendQueueItem>>(_ => _.sendQueue);
            ValueSummary<bool> do_loop = true;
            var vs_cond_47 = PathConstraint.BeginLoop();
            for (ValueSummary<int> j = 0; vs_cond_47.Loop((new Func<ValueSummary<bool>>(() =>
            {
                var vs_cond_58 = ((vs_lgc_tmp_1 = ValueSummary<bool>.InitializeFrom(do_loop))).Cond();
                var vs_cond_ret_58 = new ValueSummary<bool>();
                if (vs_cond_58.CondTrue())
                    vs_cond_ret_58.Merge(vs_lgc_tmp_1.InvokeBinary<bool, bool>((l, r) => l & r, j.InvokeBinary<int, bool>((l, r) => l < r, sendQueue.GetField<int>(_ => _.Count))));
                if (vs_cond_58.CondFalse())
                    vs_cond_ret_58.Merge(vs_lgc_tmp_1);
                vs_cond_58.MergeBranch();
                return vs_cond_ret_58;
            }

            )())); j.Increment())
            {
                ValueSummary<SendQueueItem> item = sendQueue.InvokeMethod<int, SendQueueItem>((_, a0) => _[a0], j);
                var vs_cond_45 = (item.GetField<PInteger>(_ => _.e).InvokeBinary<PInteger, PBool>((l, r) => l == r, (PInteger)Constants.EVENT_NEW_MACHINE)).Cond();
                {
                    if (vs_cond_45.CondTrue())
                    {
                        choices.InvokeMethod<Scheduler.SchedulerChoice>((_, a0) => _.Add(a0), new ValueSummary<Scheduler.SchedulerChoice>(new SchedulerChoice(machine, j, -1)));
                        do_loop.Assign<bool>(false);
                    }

                    if (vs_cond_45.CondFalse())
                    {
                        ValueSummary<int> state_idx = item.GetField<PMachine>(_ => _.target).InvokeMethod<PInteger, int>((_, a0) => _.CanServeEvent(a0), item.GetField<PInteger>(_ => _.e));
                        var vs_cond_44 = (state_idx.InvokeBinary<int, bool>((l, r) => l >= r, 0)).Cond();
                        {
                            if (vs_cond_44.CondTrue())
                            {
                                choices.InvokeMethod<Scheduler.SchedulerChoice>((_, a0) => _.Add(a0), new ValueSummary<Scheduler.SchedulerChoice>(new SchedulerChoice(machine, j, state_idx)));
                                do_loop.Assign<bool>(false);
                            }
                        }

                        vs_cond_44.MergeBranch();
                    }
                }

                vs_cond_45.MergeBranch();
            }

            vs_cond_47.MergeBranch();
            var vs_cond_48 = (choices.GetField<int>(_ => _.Count).InvokeBinary<int, bool>((l, r) => l <= r, this.delayBudget)).Cond();
            {
                if (vs_cond_48.CondTrue())
                {
                    ValueSummary<// Machine is state that can serve null event?
                    int> null_state_idx = machine.InvokeMethod<PInteger, int>((_, a0) => _.CanServeEvent(a0), (PInteger)Constants.EVENT_NULL);
                    var vs_cond_46 = (null_state_idx.InvokeBinary<int, bool>((l, r) => l >= r, 0)).Cond();
                    {
                        if (vs_cond_46.CondTrue())
                        {
                            choices.InvokeMethod<Scheduler.SchedulerChoice>((_, a0) => _.Add(a0), new ValueSummary<Scheduler.SchedulerChoice>(new SchedulerChoice(machine, -1, null_state_idx)));
                        }
                    }

                    vs_cond_46.MergeBranch();
                }
            }

            vs_cond_48.MergeBranch();
        }

        vs_cond_49.MergeBranch();
        var vs_cond_50 = (choices.GetField<int>(_ => _.Count).InvokeBinary<int, bool>((l, r) => l == r, 0)).Cond();
        {
            if (vs_cond_50.CondTrue())
            {
                PathConstraint.RecordReturnPath(vs_ret_0, new ValueSummary<Scheduler.SchedulerChoice>(null), vs_cond_50);
            }
        }

        if (vs_cond_50.MergeBranch())
        {
            ValueSummary<int> idx = PathConstraint.ChooseRandomIndex(choices.GetField<int>((_) => _.Count));
            this.delayBudget.Assign<int>(this.delayBudget.InvokeBinary<int, int>((l, r) => l - r, idx));
            this.current_schedule_machine_idx.Assign<int>((i.InvokeBinary<int, int>((l, r) => l + r, this.current_schedule_machine_idx)).InvokeBinary<int, int>((l, r) => l % r, this.machines.GetField<int>(_ => _.Count)));
            PathConstraint.RecordReturnPath(vs_ret_0, choices.InvokeMethod<int, Scheduler.SchedulerChoice>((_, a0) => _[a0], idx));
        }

        PathConstraint.PopFrame();
        return vs_ret_0;
    }
}
#endif
