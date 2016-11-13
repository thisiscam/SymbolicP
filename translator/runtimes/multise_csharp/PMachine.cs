using System;
using System.Collections.Generic;
using System.Diagnostics;
using BDDToZ3Wrap;
using SylvanSharp;
abstract class PMachine : IPType<PMachine>
{
    protected delegate void TransitionFunction(ValueSummary<IPType> payload);
    protected delegate void ExitFunction();
    protected ValueSummary<int> retcode = new ValueSummary<int>(default (int));
    protected ValueSummary<List<int>> states = new ValueSummary<List<int>>(new List<int>());
    protected Scheduler scheduler;
    protected bool[, ] DeferedSet;
    protected bool[, ] IsGotoTransition;
    protected TransitionFunction[, ] Transitions;
    protected ExitFunction[] ExitFunctions;
    public List<SendQueueItem> sendQueue = new List<SendQueueItem>();
	
	private static int mid_cnt = 0;
    public int _mid = mid_cnt++;

    public virtual void StartMachine(ValueSummary<Scheduler> s, ValueSummary<IPType> payload)
    {
        this.scheduler = s;
    }

    /* Returns the index that can serve this event */
    public ValueSummary<int> CanServeEvent(ValueSummary<PInteger> e)
    {
        var _frame_pc = PathConstraint.GetPC();
        var vs_ret_1 = new ValueSummary<int>();
        var vs_cond_22 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = this.states.GetField<int>(_ => _.Count).InvokeBinary<int, int>((l, r) => l - r, 1); vs_cond_22.Loop(i.InvokeBinary<int, bool>((l, r) => l >= r, 0)); i.Decrement())
        {
            ValueSummary<int> state = this.states.InvokeMethod<int, int>((_, a0) => _[a0], i);
            var vs_cond_21 = (this.DeferedSet.GetIndex(state, e)).Cond();
            {
                if (vs_cond_21.CondTrue())
                {
                    PathConstraint.RecordReturnPath(vs_ret_1, -1, vs_cond_21);
                }

                if (vs_cond_21.CondFalse())
                {
                    var vs_cond_20 = (this.Transitions.GetIndex(state, e).InvokeBinary<PMachine.TransitionFunction, bool>((l, r) => l != r, new ValueSummary<PMachine.TransitionFunction>(null))).Cond();
                    {
                        if (vs_cond_20.CondTrue())
                        {
                            PathConstraint.RecordReturnPath(vs_ret_1, i, vs_cond_20);
                        }
                    }

                    vs_cond_20.MergeBranch(vs_cond_21);
                }
            }

            vs_cond_21.MergeBranch(vs_cond_22);
        }

        if (vs_cond_22.MergeBranch())
        {
            throw new Exception("Unhandled event");
        }

        PathConstraint.RestorePC(_frame_pc);
        return vs_ret_1;
    }

    protected void RaiseEvent(ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        var _frame_pc = PathConstraint.GetPC();
        var vs_cond_24 = PathConstraint.BeginLoop();
        for (ValueSummary<int> i = this.states.GetField<int>(_ => _.Count).InvokeBinary<int, int>((l, r) => l - r, 1); vs_cond_24.Loop(i.InvokeBinary<int, bool>((l, r) => l >= r, 0)); i.Decrement())
        {
            ValueSummary<int> state = this.states.InvokeMethod<int, int>((_, a0) => _[a0], i);
            var vs_cond_23 = (this.Transitions.GetIndex(state, e).InvokeBinary<PMachine.TransitionFunction, bool>((l, r) => l != r, new ValueSummary<PMachine.TransitionFunction>(null))).Cond();
            {
                if (vs_cond_23.CondTrue())
                {
                    this.Transitions.GetIndex(state, e).Invoke(payload);
                    PathConstraint.RecordReturnPath(vs_cond_23);
                }

                if (vs_cond_23.CondFalse())
                {
                    this.PopState();
                }
            }

            vs_cond_23.MergeBranch(vs_cond_24);
        }

        if (vs_cond_24.MergeBranch())
        {
            throw new SystemException("Unhandled event");
        }

        PathConstraint.RestorePC(_frame_pc);
    }

    protected void SendMsg(ValueSummary<PMachine> other, ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        this.scheduler.SendMsg(this, other, e, payload);
    }

    protected ValueSummary<PMachine> NewMachine(ValueSummary<PMachine> newMachine, ValueSummary<IPType> payload)
    {
        this.scheduler.NewMachine(this, newMachine, payload);
        return newMachine;
    }

    protected void PopState()
    {
        ValueSummary<int> last = states.GetField<int>(_ => _.Count).InvokeBinary<int, int>((l, r) => l - r, 1);
        ValueSummary<int> current_state = this.states.InvokeMethod<int, int>((_, a0) => _[a0], last);
        this.states.InvokeMethod<int>((_, a0) => _.RemoveRange(a0), last);
        var vs_cond_25 = (this.ExitFunctions.GetIndex(current_state).InvokeBinary<PMachine.ExitFunction, bool>((l, r) => l != r, new ValueSummary<PMachine.ExitFunction>(null))).Cond();
        {
            if (vs_cond_25.CondTrue())
            {
                this.ExitFunctions.GetIndex(current_state).Invoke();
            }
        }

        vs_cond_25.MergeBranch();
    }

    protected ValueSummary<PBool> RandomBool(int site, ValueSummary<int> cnt, System.Collections.Generic.List<PBool> list)
    {
        return PathConstraint.Allocate<PBool>((idx) => 
           {
                   var sym_var_name = String.Format("RB_{0}_site{1}_{2}", this, site, idx);
                   var fresh_const = new SymbolicBool(PathConstraint.ctx.MkBoolConst(sym_var_name).ToBDD());
                   return new PBool(fresh_const);
           }, list, cnt);
    }

    protected void Assert(ValueSummary<PBool> cond, ValueSummary<string> msg)
    {
        var vs_cond_26 = (cond.InvokeUnary<PBool>(_ => !_)).Cond();
        {
            if (vs_cond_26.CondTrue())
            {
                throw new SystemException(msg);
            }
        }

        vs_cond_26.MergeBranch();
    }

    protected void Assert(ValueSummary<PBool> cond)
    {
        var vs_cond_27 = (cond.InvokeUnary<PBool>(_ => !_)).Cond();
        {
            if (vs_cond_27.CondTrue())
            {
                throw new SystemException("Assertion failure");
            }
        }

        vs_cond_27.MergeBranch();
    }

    public void RunStateMachine(ValueSummary<int> state_idx, ValueSummary<PInteger> e, ValueSummary<IPType> payload)
    {
        ValueSummary<int> state = this.states.InvokeMethod<int, int>((_, a0) => _[a0], state_idx);
        var vs_cond_29 = (this.IsGotoTransition.GetIndex(state, e)).Cond();
        {
            if (vs_cond_29.CondTrue())
            {
                var vs_cond_28 = PathConstraint.BeginLoop();
                for (ValueSummary<int> i = this.states.GetField<int>(_ => _.Count).InvokeBinary<int, int>((l, r) => l - r, 1); vs_cond_28.Loop(i.InvokeBinary<int, bool>((l, r) => l > r, state_idx)); i.Decrement())
                {
                    this.PopState();
                }

                vs_cond_28.MergeBranch();
            }
        }

        vs_cond_29.MergeBranch();
        this.retcode.Assign<int>(Constants.EXECUTE_FINISHED);
        ValueSummary<TransitionFunction> transition_fn = this.Transitions.GetIndex(state, e);
#if LOG_TRANSITIONS
        Console.WriteLine("{0} takes {1}", this, transition_fn);
#endif
        transition_fn.Invoke(payload);
    }

    protected void Transition_Ignore(ValueSummary<IPType> payload)
    {
        return;
    }

    public ValueSummary<PMachine> DeepCopy()
    {
        return this;
    }

    public ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<PMachine> other)
    {
        return other.InvokeBinary<object, SymbolicBool>((l, r) => l == r, this);
    }

    public override string ToString()
    {
        return string.Format("{0}{1}", this.GetType().Name, _mid);
    }
}