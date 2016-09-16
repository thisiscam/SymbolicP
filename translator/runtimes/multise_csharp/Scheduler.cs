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

	ValueSummary<List<PMachine>> machines = new ValueSummary<List<PMachine>>(new List<PMachine>());
		
	public ValueSummary<bool> ChooseAndRunMachine()
	{
		// Collect all servable events
		List<SchedulerChoice> choices = new List<SchedulerChoice>();
		for (ValueSummary<int> i = 0; i.InvokeBinary<int, bool>((l, r) => l < r, this.machines.GetField<int>(_ => _.Count)).Cond(); i.Increment())
		{
			ValueSummary<PMachine> machine = this.machines.InvokeMethod<int, PMachine>((_, a0) => _[a0], i);
			ValueSummary<// Collect from send queue
			List<SendQueueItem>> sendQueue = machine.GetConstField<List<SendQueueItem>>(_ => _.sendQueue);
			for (ValueSummary<int> j = 0; j.InvokeBinary<int, bool>((l, r) => l < r, sendQueue.GetField<int>(_ => _.Count)).Cond(); j.Increment())
			{
				ValueSummary<SendQueueItem> item = sendQueue.InvokeMethod<int, SendQueueItem>((_, a0) => _[a0], j);
				if (item.GetField<PInteger>(_ => _.e).InvokeBinary<PInteger, PBool>((l, r) => l == r, (PInteger)Constants.EVENT_NEW_MACHINE).Cond())
				{
					choices.Add(new SchedulerChoice(machine, j, -1));
					break;
				}
				else
				{
					ValueSummary<int> state_idx = item.GetField<PMachine>(_ => _.target).InvokeMethod<PInteger, int>((_, a0) => _.CanServeEvent(a0), item.GetField<PInteger>(_ => _.e));
					if (state_idx.InvokeBinary<int, bool>((l, r) => l >= r, 0).Cond())
					{
						choices.Add(new SchedulerChoice(machine, j, state_idx));
						break;
					}
				}
			}

			ValueSummary<// Machine is state that can serve null event?
			int> null_state_idx = machine.InvokeMethod<PInteger, int>((_, a0) => _.CanServeEvent(a0), (PInteger)Constants.EVENT_NULL);
			if (null_state_idx.InvokeBinary<int, bool>((l, r) => l >= r, 0).Cond())
			{
				choices.Add(new SchedulerChoice(machine, -1, null_state_idx));
			}
		}

		if (choices.Count.InvokeBinary<int, bool>((l, r) => l == r, 0).Cond())
		{
			return false;
		}

		ValueSummary<// Choose one and remove from send queue
		SymbolicInteger> idx = PathConstraint.NewSymbolicIntVar("SI", 0, choices.Count);
		ValueSummary<SchedulerChoice> chosen = choices[idx];
		ValueSummary<int> sourceMachineSendQueueIndex = chosen.GetField<int>(_ => _.sourceMachineSendQueueIndex);
		if (sourceMachineSendQueueIndex.InvokeBinary<int, bool>((l, r) => l < r, 0).Cond())
		{
			ValueSummary<PMachine> chosenTargetMachine = chosen.GetField<PMachine>(_ => _.sourceMachine);
			Console.WriteLine(chosenTargetMachine.ToString() + chosenTargetMachine.GetHashCode() + " executes EVENT_NULL");
			chosenTargetMachine.InvokeMethod<int, PInteger, IPType>((_, a0, a1, a2) => _.RunStateMachine(a0, a1, a2), chosen.GetField<int>(_ => _.targetMachineStateIndex), (PInteger)Constants.EVENT_NULL, null);
		}
		else
		{
			ValueSummary<PMachine> chosenSourceMachine = chosen.GetField<PMachine>(_ => _.sourceMachine);
			ValueSummary<SendQueueItem> dequeuedItem = chosenSourceMachine.GetConstField<List<SendQueueItem>>(_ => _.sendQueue).InvokeMethod<int, SendQueueItem>((_, a0) => _[a0], sourceMachineSendQueueIndex);
			chosenSourceMachine.GetConstField<List<SendQueueItem>>(_ => _.sendQueue).InvokeMethod<int>((_, a0) => _.RemoveAt(a0), sourceMachineSendQueueIndex);
			// Invoke Machine
			if (dequeuedItem.GetField<PInteger>(_ => _.e).InvokeBinary<PInteger, PBool>((l, r) => l == r, (PInteger)Constants.EVENT_NEW_MACHINE).Cond())
			{
				ValueSummary<PMachine> newMachine = dequeuedItem.GetField<PMachine>(_ => _.target);
				Console.WriteLine(chosenSourceMachine.ToString() + chosenSourceMachine.GetHashCode() + " creates " + newMachine.ToString() + chosenSourceMachine.GetHashCode());
				this.StartMachine(newMachine, dequeuedItem.GetField<IPType>(_ => _.payload));
			}
			else
			{
				ValueSummary<PMachine> targetMachine = dequeuedItem.GetField<PMachine>(_ => _.target);
				Console.WriteLine(chosenSourceMachine.ToString() + chosenSourceMachine.GetHashCode() + " sends event " + dequeuedItem.GetField<PInteger>(_ => _.e).ToString() + " to " + targetMachine.ToString() + targetMachine.GetHashCode());
				targetMachine.InvokeMethod<int, PInteger, IPType>((_, a0, a1, a2) => _.RunStateMachine(a0, a1, a2), chosen.GetField<int>(_ => _.targetMachineStateIndex), dequeuedItem.GetField<PInteger>(_ => _.e), dequeuedItem.GetField<IPType>(_ => _.payload));
			}
		}

		return true;
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