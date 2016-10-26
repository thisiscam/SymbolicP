using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Z3;

class Scheduler
{
    private class SchedulerChoice
    {
        public ValueSummary<PMachine> sourceMachine = new ValueSummary<PMachine>(default (PMachine));
        public ValueSummary<int> sourceMachineSendQueueIndex = new ValueSummary<int>(default (int));
        public ValueSummary<int> targetMachineStateIndex = new ValueSummary<int>(default (int));
        public SchedulerChoice(ValueSummary<PMachine> sourceMachine, ValueSummary<int> sourceMachineSendQueueIndex, ValueSummary<int> targetMachineStateIndex)
        {
            this.sourceMachine.Assign(ValueSummary<PMachine>.InitializeFrom(sourceMachine));
            this.sourceMachineSendQueueIndex.Assign(ValueSummary<int>.InitializeFrom(sourceMachineSendQueueIndex));
            this.targetMachineStateIndex.Assign(ValueSummary<int>.InitializeFrom(targetMachineStateIndex));
        }
    }

    ValueSummary<List<PMachine>> machines = new ValueSummary<List<PMachine>>(new List<PMachine>());

    public ValueSummary<bool> ChooseAndRunMachine()
    {
        PathConstraint.PushFrame();
        var vs_ret_0 = new ValueSummary<bool>();
        ValueSummary<// Collect all servable events
        List<SchedulerChoice>> choices = new ValueSummary<List<Scheduler.SchedulerChoice>>(new List<SchedulerChoice>());
        var vs_cond_53 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = 0; vs_cond_53.Loop(i.InvokeBinary<int, bool>((l, r) => l < r, this.machines.GetField<int>(_ => _.Count))); i.Increment())
        {
            ValueSummary<bool> vs_lgc_tmp_0;
            ValueSummary<PMachine> machine = this.machines.InvokeMethod<int, PMachine>((_, a0) => _[a0], i);
            ValueSummary<// Collect from send queue
            List<SendQueueItem>> sendQueue = machine.GetConstField<List<SendQueueItem>>(_ => _.sendQueue);
            ValueSummary<bool> do_loop = true;
            var vs_cond_50 = PathConstraint.BeginLoop();
            for (ValueSummary<int> j = 0; vs_cond_50.Loop((new Func<ValueSummary<bool>>(() =>
            {
                var vs_cond_64 = ((vs_lgc_tmp_0 = ValueSummary<bool>.InitializeFrom(do_loop))).Cond();
                var vs_cond_ret_64 = new ValueSummary<bool>();
                if (vs_cond_64.CondTrue())
                    vs_cond_ret_64.Merge(vs_lgc_tmp_0.InvokeBinary<bool, bool>((l, r) => l & r, j.InvokeBinary<int, bool>((l, r) => l < r, sendQueue.GetField<int>(_ => _.Count))));
                if (vs_cond_64.CondFalse())
                    vs_cond_ret_64.Merge(vs_lgc_tmp_0);
                vs_cond_64.MergeBranch();
                return vs_cond_ret_64;
            }

            )())); j.Increment())
            {
                ValueSummary<SendQueueItem> item = sendQueue.InvokeMethod<int, SendQueueItem>((_, a0) => _[a0], j);
                var vs_cond_49 = (item.GetField<PInteger>(_ => _.e).InvokeBinary<PInteger, PBool>((l, r) => l == r, (PInteger)Constants.EVENT_NEW_MACHINE)).Cond();
                {
                    if (vs_cond_49.CondTrue())
                    {
                        choices.InvokeMethod<Scheduler.SchedulerChoice>((_, a0) => _.Add(a0), new ValueSummary<Scheduler.SchedulerChoice>(new SchedulerChoice(machine, j, -1)));
                        do_loop.Assign<bool>(false);
                    }

                    if (vs_cond_49.CondFalse())
                    {
                        ValueSummary<int> state_idx = item.GetField<PMachine>(_ => _.target).InvokeMethod<PInteger, int>((_, a0) => _.CanServeEvent(a0), item.GetField<PInteger>(_ => _.e));
                        var vs_cond_48 = (state_idx.InvokeBinary<int, bool>((l, r) => l >= r, 0)).Cond();
                        {
                            if (vs_cond_48.CondTrue())
                            {
                                choices.InvokeMethod<Scheduler.SchedulerChoice>((_, a0) => _.Add(a0), new ValueSummary<Scheduler.SchedulerChoice>(new SchedulerChoice(machine, j, state_idx)));
                                do_loop.Assign<bool>(false);
                            }
                        }

                        vs_cond_48.MergeBranch();
                    }
                }

                vs_cond_49.MergeBranch();
            }

            vs_cond_50.MergeBranch();
            ValueSummary<// Machine is state that can serve null event?
            int> null_state_idx = machine.InvokeMethod<PInteger, int>((_, a0) => _.CanServeEvent(a0), (PInteger)Constants.EVENT_NULL);
            var vs_cond_51 = (null_state_idx.InvokeBinary<int, bool>((l, r) => l >= r, 0)).Cond();
            {
                if (vs_cond_51.CondTrue())
                {
                    choices.InvokeMethod<Scheduler.SchedulerChoice>((_, a0) => _.Add(a0), new ValueSummary<Scheduler.SchedulerChoice>(new SchedulerChoice(machine, -1, null_state_idx)));
                }
            }

            vs_cond_51.MergeBranch();
        }

        vs_cond_53.MergeBranch();
        var vs_cond_54 = (choices.GetField<int>(_ => _.Count).InvokeBinary<int, bool>((l, r) => l == r, 0)).Cond();
        {
            if (vs_cond_54.CondTrue())
            {
                PathConstraint.RecordReturnPath(vs_ret_0, false, vs_cond_54);
            }
        }

        if (vs_cond_54.MergeBranch())
        {
			ValueSummary<SchedulerChoice> chosen = PathConstraint.ChooseRandomFromList<SchedulerChoice>(choices);        	
	
            ValueSummary<int> sourceMachineSendQueueIndex = chosen.GetField<int>(_ => _.sourceMachineSendQueueIndex);
            var vs_cond_55 = (sourceMachineSendQueueIndex.InvokeBinary<int, bool>((l, r) => l < r, 0)).Cond();
            {
                if (vs_cond_55.CondTrue())
                {
                    ValueSummary<PMachine> chosenTargetMachine = chosen.GetField<PMachine>(_ => _.sourceMachine);
                    Console.WriteLine(chosenTargetMachine.ToString() + chosenTargetMachine.GetHashCode() + " executes EVENT_NULL");
                    chosenTargetMachine.InvokeMethod<int, PInteger, IPType>((_, a0, a1, a2) => _.RunStateMachine(a0, a1, a2), chosen.GetField<int>(_ => _.targetMachineStateIndex), (PInteger)Constants.EVENT_NULL, new ValueSummary<IPType>(null));
                }

                if (vs_cond_55.CondFalse())
                {
                    ValueSummary<PMachine> chosenSourceMachine = chosen.GetField<PMachine>(_ => _.sourceMachine);
                    ValueSummary<SendQueueItem> dequeuedItem = chosenSourceMachine.GetConstField<List<SendQueueItem>>(_ => _.sendQueue).InvokeMethod<int, SendQueueItem>((_, a0) => _[a0], sourceMachineSendQueueIndex);
                	chosenSourceMachine.GetConstField<List<SendQueueItem>>(_ => _.sendQueue).InvokeMethod<SymbolicInteger>((_, a0) => _.RemoveAt(a0), sourceMachineSendQueueIndex.Cast<SymbolicInteger>(arg => (SymbolicInteger)arg));
                    var vs_cond_52 = (dequeuedItem.GetField<PInteger>(_ => _.e).InvokeBinary<PInteger, PBool>((l, r) => l == r, (PInteger)Constants.EVENT_NEW_MACHINE)).Cond();
                    {
                        if (vs_cond_52.CondTrue())
                        {
                            ValueSummary<PMachine> newMachine = dequeuedItem.GetField<PMachine>(_ => _.target);
                            Console.WriteLine(chosenSourceMachine.ToString() + chosenSourceMachine.GetHashCode() + " creates " + newMachine.ToString() + chosenSourceMachine.GetHashCode());
                            this.StartMachine(newMachine, dequeuedItem.GetField<IPType>(_ => _.payload));
                        }

                        if (vs_cond_52.CondFalse())
                        {
                            ValueSummary<PMachine> targetMachine = dequeuedItem.GetField<PMachine>(_ => _.target);
                            Console.WriteLine(chosenSourceMachine.ToString() + chosenSourceMachine.GetHashCode() + " sends event " + dequeuedItem.GetField<PInteger>(_ => _.e).ToString() + " to " + targetMachine.ToString() + targetMachine.GetHashCode());
                            targetMachine.InvokeMethod<int, PInteger, IPType>((_, a0, a1, a2) => _.RunStateMachine(a0, a1, a2), chosen.GetField<int>(_ => _.targetMachineStateIndex), dequeuedItem.GetField<PInteger>(_ => _.e), dequeuedItem.GetField<IPType>(_ => _.payload));
                        }
                    }

                    vs_cond_52.MergeBranch();
                }
            }

            vs_cond_55.MergeBranch();
            PathConstraint.RecordReturnPath(vs_ret_0, true);
        }

        PathConstraint.PopFrame();
        return vs_ret_0;
    }

    public void SendMsg(ValueSummary<PMachine> source, ValueSummary<PMachine> target, ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        source.GetConstField<List<SendQueueItem>>(_ => _.sendQueue).InvokeMethod<SendQueueItem>((_, a0) => _.Add(a0), new ValueSummary<SendQueueItem>(new SendQueueItem(target, e, payload)));
    }

    public void NewMachine(ValueSummary<PMachine> source, ValueSummary<PMachine> newMachine, ValueSummary<IPType> payload)
    {
        source.GetConstField<List<SendQueueItem>>(_ => _.sendQueue).InvokeMethod<SendQueueItem>((_, a0) => _.Add(a0), new ValueSummary<SendQueueItem>(new SendQueueItem(newMachine, (PInteger)Constants.EVENT_NEW_MACHINE, payload)));
    }

    public ValueSummary<SymbolicBool> RandomBool()
    {
        return PathConstraint.NewSymbolicBoolVar("RB");
    }

    public void StartMachine(ValueSummary<PMachine> machine, ValueSummary<IPType> payload)
    {
        this.machines.InvokeMethod<PMachine>((_, a0) => _.Add(a0), machine);
        machine.InvokeMethod<Scheduler, IPType>((_, a0, a1) => _.StartMachine(a0, a1), this, ValueSummary<IPType>.InitializeFrom(payload));
    }
}