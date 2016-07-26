using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Z3;

class Scheduler
{
    private class SchedulerChoice
    {
        public ValueSummary<PMachine> sourceMachine;
        public ValueSummary<int> sourceMachineSendQueueIndex;
        public ValueSummary<int> targetMachineStateIndex;
        public SchedulerChoice(ValueSummary<PMachine> sourceMachine, ValueSummary<int> sourceMachineSendQueueIndex, ValueSummary<int> targetMachineStateIndex)
        {
            this.sourceMachine = sourceMachine;
            this.sourceMachineSendQueueIndex = sourceMachineSendQueueIndex;
            this.targetMachineStateIndex = targetMachineStateIndex;
        }
    }

    ValueSummary<Random> rng;
    ValueSummary<List<PMachine>> machines = new ValueSummary<List<PMachine>>(new List<PMachine>());
    public Scheduler(): this (new ValueSummary<System.Random>(new Random()))
    {
    }

    public Scheduler(ValueSummary<Random> rng)
    {
        this.rng = rng;
    }

    private ValueSummary<bool> ChooseAndRunMachine(ValueSummary<Scheduler> self)
    {
        ValueSummary<// Collect all servable events
        List<SchedulerChoice>> choices = new ValueSummary<List<Scheduler.SchedulerChoice>>(new List<SchedulerChoice>());
        for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, self.GetField<List<PMachine>>(_ => _.machines).GetField<int>(_ => _.Count)); i.InvokeMethod((_) => _++))
        {
            ValueSummary<PMachine> machine = self.GetField<List<PMachine>>(_ => _.machines).GetIndex((_, a0) => _[a0], i);
            ValueSummary<// Collect from send queue
            List<SendQueueItem>> sendQueue = machine.GetField<List<SendQueueItem>>(_ => _.sendQueue);
            for (ValueSummary<int> j = 0; j.InvokeBinary<int, bool>((l, r) => l < r, sendQueue.GetField<int>(_ => _.Count)); j.InvokeMethod((_) => _++))
            {
                ValueSummary<SendQueueItem> item = sendQueue.GetIndex((_, a0) => _[a0], j);
                if (item.GetField<PInteger>(_ => _.e).InvokeBinary<PInteger, PBool>((l, r) => l == r, Constants.EVENT_NEW_MACHINE))
                {
                    choices.Add(new ValueSummary<Scheduler.SchedulerChoice>(new SchedulerChoice(machine, j, -1)));
                    break;
                }
                else
                {
                    ValueSummary<int> state_idx = item.GetField<PMachine>(_ => _.target).InvokeMethod<PInteger>((_, s, a0) => _.CanServeEvent(s, a0), item.GetField<PInteger>(_ => _.e));
                    if (state_idx.InvokeBinary<int, bool>((l, r) => l >= r, 0))
                    {
                        choices.Add(new ValueSummary<Scheduler.SchedulerChoice>(new SchedulerChoice(machine, j, state_idx)));
                        break;
                    }
                }
            }

            ValueSummary<// Machine is state that can serve null event?
            int> null_state_idx = machine.InvokeMethod<PInteger>((_, s, a0) => _.CanServeEvent(s, a0), Constants.EVENT_NULL);
            if (null_state_idx.InvokeBinary<int, bool>((l, r) => l >= r, 0))
            {
                Debugger.Break();
                choices.Add(new ValueSummary<Scheduler.SchedulerChoice>(new SchedulerChoice(machine, -1, null_state_idx)));
            }
        }

        if (choices.GetField<int>(_ => _.Count).InvokeBinary<int, bool>((l, r) => l == r, 0))
        {
            return false;
        }

        ValueSummary<// Choose one and remove from send queue
        SymbolicInteger> idx = SymbolicEngine.SE.NewSymbolicIntVar("SI", 0, choices.GetField<int>(_ => _.Count));
        ValueSummary<SchedulerChoice> chosen = choices.GetIndex((_, a0) => _[a0], idx);
        ValueSummary<int> sourceMachineSendQueueIndex = chosen.GetField<int>(_ => _.sourceMachineSendQueueIndex);
        if (sourceMachineSendQueueIndex.InvokeBinary<int, bool>((l, r) => l < r, 0))
        {
            ValueSummary<PMachine> chosenTargetMachine = chosen.GetField<PMachine>(_ => _.sourceMachine);
            Console.WriteLine(chosenTargetMachine.ToString() + chosenTargetMachine.GetHashCode() + " executes EVENT_NULL");
            chosenTargetMachine.InvokeMethod<int, PInteger, IPType>((_, s, a0, a1, a2) => _.RunStateMachine(s, a0, a1, a2), chosen.GetField<int>(_ => _.targetMachineStateIndex), Constants.EVENT_NULL, null);
        }
        else
        {
            ValueSummary<PMachine> chosenSourceMachine = chosen.GetField<PMachine>(_ => _.sourceMachine);
            ValueSummary<SendQueueItem> dequeuedItem = chosenSourceMachine.GetField<List<SendQueueItem>>(_ => _.sendQueue).GetIndex((_, a0) => _[a0], sourceMachineSendQueueIndex);
            chosenSourceMachine.GetField<List<SendQueueItem>>(_ => _.sendQueue).InvokeMethod<int>((_, s, a0) => _.RemoveAt(s, a0), sourceMachineSendQueueIndex);
            // Invoke Machine
            if (dequeuedItem.GetField<PInteger>(_ => _.e).InvokeBinary<PInteger, PBool>((l, r) => l == r, Constants.EVENT_NEW_MACHINE))
            {
                ValueSummary<PMachine> newMachine = dequeuedItem.GetField<PMachine>(_ => _.target);
                Console.WriteLine(chosenSourceMachine.ToString() + chosenSourceMachine.GetHashCode() + " creates " + newMachine.ToString() + chosenSourceMachine.GetHashCode());
                self.InvokeMethod<PMachine, IPType>((_, s, a0, a1) => _.StartMachine(s, a0, a1), newMachine, dequeuedItem.GetField<IPType>(_ => _.payload));
            }
            else
            {
                ValueSummary<PMachine> targetMachine = dequeuedItem.GetField<PMachine>(_ => _.target);
                Console.WriteLine(chosenSourceMachine.ToString() + chosenSourceMachine.GetHashCode() + " sends event " + dequeuedItem.GetField<PInteger>(_ => _.e).ToString() + " to " + targetMachine.ToString() + targetMachine.GetHashCode());
                targetMachine.InvokeMethod<int, PInteger, IPType>((_, s, a0, a1, a2) => _.RunStateMachine(s, a0, a1, a2), chosen.GetField<int>(_ => _.targetMachineStateIndex), dequeuedItem.GetField<PInteger>(_ => _.e), dequeuedItem.GetField<IPType>(_ => _.payload));
            }
        }

        return true;
    }

    public void SendMsg(ValueSummary<Scheduler> self, ValueSummary<PMachine> source, ValueSummary<PMachine> target, ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        source.GetField<List<SendQueueItem>>(_ => _.sendQueue).Add(new ValueSummary<SendQueueItem>(new SendQueueItem(target, e, payload)));
    }

    public void NewMachine(ValueSummary<Scheduler> self, ValueSummary<PMachine> source, ValueSummary<PMachine> newMachine, ValueSummary<IPType> payload)
    {
        source.GetField<List<SendQueueItem>>(_ => _.sendQueue).Add(new ValueSummary<SendQueueItem>(new SendQueueItem(newMachine, Constants.EVENT_NEW_MACHINE, payload)));
    }

    public ValueSummary<SymbolicBool> RandomBool(ValueSummary<Scheduler> self)
    {
        return SymbolicEngine.SE.NewSymbolicBoolVar("RB");
    }

    private void StartMachine(ValueSummary<Scheduler> self, ValueSummary<PMachine> machine, ValueSummary<IPType> payload)
    {
        self.GetField<List<PMachine>>(_ => _.machines).InvokeMethod<PMachine>((_, s, a0) => _.Add(s, a0), machine);
        machine.InvokeDynamic<Scheduler, IPType>((_, s, a0, a1) => _.StartMachine(s, a0, a1), self, payload);
    }

    static ValueSummary<int> Main(ValueSummary<ValueSummary<string>[]> args)
    {
        ValueSummary<int> maxExplorationSteps = 200;
        ValueSummary<Random> rng = new ValueSummary<System.Random>(new Random());
        ValueSummary<int> iteration = 0;
        while (true)
        {
            Console.WriteLine(String.Format("========BEGIN NEW TRACE {0}=========", iteration));
            ValueSummary<Scheduler> scheduler = new ValueSummary<Scheduler>(new Scheduler(rng));
            ValueSummary<PMachine> mainMachine = MachineController.CreateMainMachine();
            scheduler.InvokeMethod<PMachine, IPType>((_, s, a0, a1) => _.StartMachine(s, a0, a1), mainMachine, null);
            for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, maxExplorationSteps); i.InvokeMethod((_) => _++))
            {
                if (scheduler.InvokeMethod((_, s) => _.ChooseAndRunMachine(s)).InvokeMethod((_) => !_))
                {
                    break;
                }
            }

            Console.WriteLine("===========END TRACE===========");
            iteration.InvokeMethod((_) => _++);
            if (SymbolicEngine.SE.Reset().InvokeMethod((_) => !_))
            {
                break;
            }
        }

        return 0;
    }
}