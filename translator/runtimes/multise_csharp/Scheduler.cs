using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Z3;

partial class Scheduler
{
    public class SchedulerChoice
    {
        public ValueSummary<PMachine> sourceMachine = new ValueSummary<PMachine>(default (PMachine));
        public ValueSummary<int> sourceMachineSendQueueIndex = new ValueSummary<int>(default (int));
        public ValueSummary<int> targetMachineStateIndex = new ValueSummary<int>(default (int));
        public SchedulerChoice(ValueSummary<PMachine> sourceMachine, ValueSummary<int> sourceMachineSendQueueIndex, ValueSummary<int> targetMachineStateIndex)
        {
            this.sourceMachine.Assign<PMachine>(sourceMachine);
            this.sourceMachineSendQueueIndex.Assign<int>(sourceMachineSendQueueIndex);
            this.targetMachineStateIndex.Assign<int>(targetMachineStateIndex);
        }
    }

    ValueSummary<List<PMachine>> machines = new ValueSummary<List<PMachine>>(new List<PMachine>());
    public ValueSummary<bool> ChooseAndRunMachine()
    {
        PathConstraint.PushFrame();
        var vs_ret_0 = new ValueSummary<bool>();
        ValueSummary<SchedulerChoice> chosen = this.ChooseMachine();
        var vs_cond_53 = (chosen.InvokeBinary<object, bool>((l, r) => l == r, new ValueSummary<object>(null))).Cond();
        {
            if (vs_cond_53.CondTrue())
            {
                PathConstraint.RecordReturnPath(vs_ret_0, false, vs_cond_53);
            }

            if (vs_cond_53.CondFalse())
            {
                ValueSummary<int> sourceMachineSendQueueIndex = chosen.GetField<int>(_ => _.sourceMachineSendQueueIndex);
                var vs_cond_52 = (sourceMachineSendQueueIndex.InvokeBinary<int, bool>((l, r) => l < r, 0)).Cond();
                {
                    if (vs_cond_52.CondTrue())
                    {
                        ValueSummary<PMachine> chosenTargetMachine = chosen.GetField<PMachine>(_ => _.sourceMachine);
                        Console.WriteLine(chosenTargetMachine.ToString() + chosenTargetMachine.GetHashCode() + " executes EVENT_NULL");
                        chosenTargetMachine.InvokeMethod<int, PInteger, IPType>((_, a0, a1, a2) => _.RunStateMachine(a0, a1, a2), chosen.GetField<int>(_ => _.targetMachineStateIndex), (PInteger)Constants.EVENT_NULL, new ValueSummary<IPType>(null));
                    }

                    if (vs_cond_52.CondFalse())
                    {
                        ValueSummary<PMachine> chosenSourceMachine = chosen.GetField<PMachine>(_ => _.sourceMachine);
                        ValueSummary<SendQueueItem> dequeuedItem = chosenSourceMachine.GetConstField<List<SendQueueItem>>(_ => _.sendQueue).InvokeMethod<int, SendQueueItem>((_, a0) => _[a0], sourceMachineSendQueueIndex);
                        chosenSourceMachine.GetConstField<List<SendQueueItem>>(_ => _.sendQueue).InvokeMethod<SymbolicInteger>((_, a0) => _.RemoveAt(a0), sourceMachineSendQueueIndex.Cast<SymbolicInteger>(_ => (SymbolicInteger)_));
                        var vs_cond_51 = (dequeuedItem.GetField<PInteger>(_ => _.e).InvokeBinary<PInteger, PBool>((l, r) => l == r, (PInteger)Constants.EVENT_NEW_MACHINE)).Cond();
                        {
                            if (vs_cond_51.CondTrue())
                            {
                                ValueSummary<PMachine> newMachine = dequeuedItem.GetField<PMachine>(_ => _.target);
                                Console.WriteLine(chosenSourceMachine.ToString() + chosenSourceMachine.GetHashCode() + " creates " + newMachine.ToString() + chosenSourceMachine.GetHashCode());
                                this.StartMachine(newMachine, dequeuedItem.GetField<IPType>(_ => _.payload));
                            }

                            if (vs_cond_51.CondFalse())
                            {
                                ValueSummary<PMachine> targetMachine = dequeuedItem.GetField<PMachine>(_ => _.target);
                                Console.WriteLine(chosenSourceMachine.ToString() + chosenSourceMachine.GetHashCode() + " sends event " + dequeuedItem.GetField<PInteger>(_ => _.e).ToString() + " to " + targetMachine.ToString() + targetMachine.GetHashCode());
                                targetMachine.InvokeMethod<int, PInteger, IPType>((_, a0, a1, a2) => _.RunStateMachine(a0, a1, a2), chosen.GetField<int>(_ => _.targetMachineStateIndex), dequeuedItem.GetField<PInteger>(_ => _.e), dequeuedItem.GetField<IPType>(_ => _.payload));
                            }
                        }

                        vs_cond_51.MergeBranch();
                    }
                }

                vs_cond_52.MergeBranch();
                PathConstraint.RecordReturnPath(vs_ret_0, true, vs_cond_53);
            }
        }

        vs_cond_53.MergeBranch();
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