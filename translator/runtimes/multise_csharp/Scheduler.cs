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
            this.sourceMachine = ValueSummary<PMachine>.InitializeFrom(sourceMachine);
            this.sourceMachineSendQueueIndex = ValueSummary<int>.InitializeFrom(sourceMachineSendQueueIndex);
            this.targetMachineStateIndex = ValueSummary<int>.InitializeFrom(targetMachineStateIndex);
        }
    }

    ValueSummary<List<PMachine>> machines = ValueSummary<List<PMachine>>.InitializeFrom(new ValueSummary<List<PMachine>>(new List<PMachine>()));

    public ValueSummary<bool> ChooseAndRunMachine()
    {
        PathConstraint.PushFrame();
        var vs_ret_0 = new ValueSummary<bool>();
        
        ValueSummary<List<SchedulerChoice>> choices = new List<SchedulerChoice>();
        {
            PathConstraint.BeginLoop();
            for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this.machines.GetField<int>(_ => _.Count)).Loop(); i.Increment())
            {
                ValueSummary<bool> vs_lgc_tmp_0;
                ValueSummary<PMachine> machine = ValueSummary<PMachine>.InitializeFrom(this.machines.InvokeMethod<int, PMachine>((_, a0) => _[a0], i));
                ValueSummary<// Collect from send queue
                List<SendQueueItem>> sendQueue = ValueSummary<// Collect from send queue
                List<SendQueueItem>>.InitializeFrom(machine.GetConstField<List<SendQueueItem>>(_ => _.sendQueue));
                ValueSummary<bool> do_loop = ValueSummary<bool>.InitializeFrom(true);
                {
                    PathConstraint.BeginLoop();
                    for (ValueSummary<int> j = 0; (new  Func<ValueSummary<bool>>(() =>
                    {
                        var vs_cond_8 = ((vs_lgc_tmp_0 = do_loop)).Cond();
                        var vs_cond_ret_8 = new ValueSummary<bool>();
                        if (vs_cond_8.CondTrue())
                            vs_cond_ret_8.Merge(vs_lgc_tmp_0.InvokeBinary<bool, bool>((l, r) => l & r, j.InvokeBinary<int, bool>((l, r) => l < r, sendQueue.GetField<int>(_ => _.Count))));
                        if (vs_cond_8.CondFalse())
                            vs_cond_ret_8.Merge(vs_lgc_tmp_0);
                        vs_cond_8.MergeBranch();
                        return vs_cond_ret_8;
                    }

                    )()).Loop(); j.Increment())
                    {
                        ValueSummary<SendQueueItem> item = ValueSummary<SendQueueItem>.InitializeFrom(sendQueue.InvokeMethod<int, SendQueueItem>((_, a0) => _[a0], j));
                        {
                            var vs_cond_43 = (item.GetField<PInteger>(_ => _.e).InvokeBinary<PInteger, PBool>((l, r) => l == r, (PInteger)Constants.EVENT_NEW_MACHINE)).Cond();
                            if (vs_cond_43.CondTrue())
                            {
                                choices.InvokeMethod((_) => _.Add(new SchedulerChoice(machine, j, -1)));                                
                                do_loop.Assign<bool>(false);
                            }

                            if (vs_cond_43.CondFalse())
                            {
                                ValueSummary<int> state_idx = ValueSummary<int>.InitializeFrom(item.GetField<PMachine>(_ => _.target).InvokeMethod<PInteger, int>((_, a0) => _.CanServeEvent(a0), item.GetField<PInteger>(_ => _.e)));
                                {
                                    var vs_cond_44 = (state_idx.InvokeBinary<int, bool>((l, r) => l >= r, 0)).Cond();
                                    if (vs_cond_44.CondTrue())
                                    {
                                        choices.InvokeMethod((_) => _.Add(new SchedulerChoice(machine, j, state_idx)));                                
                                        do_loop.Assign<bool>(false);
                                    }

                                    vs_cond_44.MergeBranch();
                                }
                            }

                            vs_cond_43.MergeBranch();
                        }
                    }
                }
				
                ValueSummary<// Machine is state that can serve null event?
                int> null_state_idx = ValueSummary<// Machine is state that can serve null event?
                int>.InitializeFrom(machine.InvokeMethod<PInteger, int>((_, a0) => _.CanServeEvent(a0), (PInteger)Constants.EVENT_NULL));
                {
                    var vs_cond_45 = (null_state_idx.InvokeBinary<int, bool>((l, r) => l >= r, 0)).Cond();
                    if (vs_cond_45.CondTrue())
                    {
                        choices.InvokeMethod((_) => _.Add(new SchedulerChoice(machine, -1, null_state_idx)));                                
                    }

                    vs_cond_45.MergeBranch();
                }
            }
        }

        {
            var vs_cond_46 = choices.GetField((_) => _.Count).InvokeBinary<int, bool>((l, r) => l == r, 0).Cond();
            if (vs_cond_46.CondTrue())
            {
                vs_ret_0.RecordReturn(false);
            }
			
			vs_cond_46.MergeBranch();
			
            if (PathConstraint.MergedPcFeasible())
            {
                ValueSummary<// Choose one and remove from send queue
				SymbolicInteger> idx = PathConstraint.NewSymbolicIntVar("SI", 0, choices.GetField((_) => _.Count));
		
				ValueSummary<SchedulerChoice> chosen = choices.InvokeMethod((arg) => arg[idx]);
                ValueSummary<int> sourceMachineSendQueueIndex = ValueSummary<int>.InitializeFrom(chosen.GetField<int>(_ => _.sourceMachineSendQueueIndex));
                {
                    var vs_cond_47 = (sourceMachineSendQueueIndex.InvokeBinary<int, bool>((l, r) => l < r, 0)).Cond();
                    if (vs_cond_47.CondTrue())
                    {
                        ValueSummary<PMachine> chosenTargetMachine = ValueSummary<PMachine>.InitializeFrom(chosen.GetField<PMachine>(_ => _.sourceMachine));
                        Console.WriteLine(chosenTargetMachine.ToString() + chosenTargetMachine.GetHashCode() + " executes EVENT_NULL");
                        chosenTargetMachine.InvokeMethod<int, PInteger, IPType>((_, a0, a1, a2) => _.RunStateMachine(a0, a1, a2), chosen.GetField<int>(_ => _.targetMachineStateIndex), (PInteger)Constants.EVENT_NULL, new ValueSummary<IPType>(null));
                    }

                    if (vs_cond_47.CondFalse())
                    {
                        ValueSummary<PMachine> chosenSourceMachine = ValueSummary<PMachine>.InitializeFrom(chosen.GetField<PMachine>(_ => _.sourceMachine));
                        ValueSummary<SendQueueItem> dequeuedItem = ValueSummary<SendQueueItem>.InitializeFrom(chosenSourceMachine.GetConstField<List<SendQueueItem>>(_ => _.sendQueue).InvokeMethod<int, SendQueueItem>((_, a0) => _[a0], sourceMachineSendQueueIndex));
						chosenSourceMachine.GetConstField<List<SendQueueItem>>(_ => _.sendQueue).InvokeMethod<int>((_, a0) => _.RemoveAt(a0), sourceMachineSendQueueIndex);
                        {
                            var vs_cond_48 = (dequeuedItem.GetField<PInteger>(_ => _.e).InvokeBinary<PInteger, PBool>((l, r) => l == r, (PInteger)Constants.EVENT_NEW_MACHINE)).Cond();
                            if (vs_cond_48.CondTrue())
                            {
                                ValueSummary<PMachine> newMachine = ValueSummary<PMachine>.InitializeFrom(dequeuedItem.GetField<PMachine>(_ => _.target));
                                Console.WriteLine(chosenSourceMachine.ToString() + chosenSourceMachine.GetHashCode() + " creates " + newMachine.ToString() + chosenSourceMachine.GetHashCode());
								this.StartMachine(newMachine, dequeuedItem.GetField<IPType>(_ => _.payload));
                            }

                            if (vs_cond_48.CondFalse())
                            {
                                ValueSummary<PMachine> targetMachine = ValueSummary<PMachine>.InitializeFrom(dequeuedItem.GetField<PMachine>(_ => _.target));
                                Console.WriteLine(chosenSourceMachine.ToString() + chosenSourceMachine.GetHashCode() + " sends event " + dequeuedItem.GetField<PInteger>(_ => _.e).ToString() + " to " + targetMachine.ToString() + targetMachine.GetHashCode());
                                targetMachine.InvokeMethod<int, PInteger, IPType>((_, a0, a1, a2) => _.RunStateMachine(a0, a1, a2), chosen.GetField<int>(_ => _.targetMachineStateIndex), dequeuedItem.GetField<PInteger>(_ => _.e), dequeuedItem.GetField<IPType>(_ => _.payload));
                            }

                            vs_cond_48.MergeBranch();
                        }
                    }

                    vs_cond_47.MergeBranch();
                }

                vs_ret_0.RecordReturn(true);
            }
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
        machine.InvokeMethod<Scheduler, IPType>((_, a0, a1) => _.StartMachine(a0, a1), this, payload);
    }
}