from __future__ import print_function
import os, sys
import copy
from ordered_set import OrderedSet
from collections import defaultdict, OrderedDict, namedtuple
from pparser.pVisitor import pVisitor

class PProgram(object):
    def __init__(self):
        self.events = OrderedSet()
        self.events.add("EVENT_NULL")
        self.machines = OrderedSet()
        self.observes_map = defaultdict(list)
        self.global_fn_decls = OrderedDict()

class PMachine(object):
    def __init__(self):
        self.is_main = False
        self.is_spec = False
        self.var_decls = {}
        self.fun_decls = OrderedDict()
        self.state_decls = OrderedDict()

class PFunctionWrapper(object): 
    def __init__(self):
        self.name = None
        self.params = OrderedDict()
        self.local_decls = {}
        self.ret_type = None
        self.stmt_block = None
        self.is_transition_handler = False
        self.from_to_state = None # extra info signaturing if this function should transit to state
    def __str__(self):
        return "PFunctionWrapper({name})".format(self.name)

class PMachineState(object):
    def __init__(self):
        self.name = None
        self.is_start = False
        self.entry_fn = None
        self.exit_fn = None
        self.transitions = {}
        self.defered_events = OrderedSet()
        self.ignored_events = OrderedSet()

TransitionAttributes = namedtuple('TransitionAttributes', 
                                  ['fn_name', 'to_state', 'is_named', 'is_push'])

class PTypeSingleton(object):
    def __init__(self, typename, clonable):
        self.typename = typename
        self.clonable = clonable
    def __hash__(self):
        return hash(self.typename)
    def __eq__(self, other):
        return isinstance(other, PTypeSingleton) and other.typename == self.typename
    def __repr__(self):
        return self.typename

PTypeMachine = PTypeSingleton("PTypeMachine", False)
PTypeBool = PTypeSingleton("PTypeBool", False)
PTypeInt = PTypeSingleton("PTypeInt", False)
PTypeAny = PTypeSingleton("PTypeAny", False)

class PTypeEvent(object):
    clonable = False
    events = {}
    def __init__(self, name):
        self.name = name
    def is_static_event(self):
        return self.name != "*unknown"
    @staticmethod
    def get_or_create(event_name):
        if event_name not in PTypeEvent.events:
            PTypeEvent.events[event_name] = PTypeEvent(event_name)
        return PTypeEvent.events[event_name]
    def __eq__(self, other):
        return isinstance(other, PTypeEvent)
PTypeEventUnknown = PTypeEvent("*unknown")
PTypeEventHalt = PTypeEvent("*halt")

class PTypeSeq(object):
    clonable = True
    def __init__(self, T):
        self.T = T
    def __eq__(self, other):
        return isinstance(other, PTypeSeq) and self.T == other.T
class PTypeMap(object):
    clonable = True
    def __init__(self, T1, T2):
        self.T1 = T1
        self.T2 = T2
    def __eq__(self, other):
        return isinstance(other, PTypeMap) and self.T1 == other.T1 and self.T2 == other.T2
class PTypeTuple(object):
    clonable = True
    def __init__(self, Ts):
        self.Ts = Ts
    def __eq__(self, other):
        return isinstance(other, PTypeTuple) and self.Ts == other.Ts
class PTypeNamedTuple(object):
    clonable = True
    def __init__(self, NTs):
        self.NTs = OrderedDict(NTs)
    def __eq__(self, other):
        return isinstance(other, PTypeNamedTuple) and self.NTs == other.NTs

class PTypeTranslatorVisitor(pVisitor):
    def warning(self, msg, ctx):
        print("Warning: {}".format(msg), file=sys.stderr)

    # Visit a parse tree produced by pParser#ptype_null.
    def visitPtype_null(self, ctx):
        raise ValueError("Null type not supported")

    # Visit a parse tree produced by pParser#stmt_recieve.
    def visitStmt_recieve(self, ctx, **kwargs):
        raise ValueError("Recieve not supported")

    # Visit a parse tree produced by pParser#exp_halt.
    def visitExp_halt(self, ctx, **kwargs):
        raise ValueError("Halt not supported")

    # Visit a parse tree produced by pParser#ptype_bool.
    def visitPtype_bool(self, ctx):
        return PTypeBool


    # Visit a parse tree produced by pParser#ptype_int_type.
    def visitPtype_int_type(self, ctx):
        return PTypeInt


    # Visit a parse tree produced by pParser#ptype_event.
    def visitPtype_event(self, ctx):
        return PTypeEventUnknown


    # Visit a parse tree produced by pParser#ptype_machine.
    def visitPtype_machine(self, ctx):
        return PTypeMachine


    # Visit a parse tree produced by pParser#ptype_any.
    def visitPtype_any(self, ctx):
        return PTypeAny


    # Visit a parse tree produced by pParser#ptype_typedef.
    def visitPtype_typedef(self, ctx):
        return self.typedefs[ctx.getChild(0).getText()]


    # Visit a parse tree produced by pParser#ptype_seq.
    def visitPtype_seq(self, ctx):
        return PTypeSeq(ctx.getChild(2).accept(self))


    # Visit a parse tree produced by pParser#ptype_map.
    def visitPtype_map(self, ctx):
        return PTypeMap(ctx.getChild(2).accept(self), ctx.getChild(4).accept(self))


    # Visit a parse tree produced by pParser#ptype_tuple.
    def visitPtype_tuple(self, ctx):
        return PTypeTuple(ctx.getChild(1).accept(self))


    # Visit a parse tree produced by pParser#ptype_named_tuple.
    def visitPtype_named_tuple(self, ctx):
        return PTypeNamedTuple(ctx.getChild(1).accept(self))

    # Visit a parse tree produced by pParser#nmd_tup_type_list.
    def visitNmd_tup_type_list(self, ctx):
        if ctx.getChildCount() == 4:
            return [(ctx.getChild(0).getText(), ctx.getChild(3).accept(self))]
        else:
            return [(ctx.getChild(0).getText(), ctx.getChild(3).accept(self))] + ctx.getChild(5).accept(self)

    # Visit a parse tree produced by pParser#tup_type_list.
    def visitTup_type_list(self, ctx):
        if ctx.getChildCount() == 1:
            return [ctx.getChild(0).accept(self)]
        else:
            return [ctx.getChild(0).accept(self)] + ctx.getChild(2).accept(self)


class AntlrTreeToPProgramVisitor(PTypeTranslatorVisitor):

    def __init__(self, enable_warning=False):
        self.current_pprogram = None
        self.current_visited_machine = None
        self.current_visited_state = None
        self.current_visited_event_list = None
        self.current_state_target = None

        self.enable_warning = enable_warning

    # Visit a parse tree produced by pParser#program.
    def visitProgram(self, ctx):
        new_program = PProgram()
        self.current_pprogram = new_program
        self.visitChildren(ctx)
        self.current_pprogram = None
        return new_program

    # Visit a parse tree produced by pParser#annotation_set.
    def visitAnnotation_set(self, ctx):
        raise ValueError("Annotation not supported")


    # Visit a parse tree produced by pParser#annotation_list.
    def visitAnnotation_list(self, ctx):
        raise ValueError("Annotation not supported")


    # Visit a parse tree produced by pParser#annotation.
    def visitAnnotation(self, ctx):
        raise ValueError("Annotation not supported")


    # Visit a parse tree produced by pParser#type_def_decl.
    def visitType_def_decl(self, ctx):
        raise ValueError("Typedef not supported")

    # Visit a parse tree produced by pParser#event_decl.
    def visitEvent_decl(self, ctx):
        self.current_pprogram.events.add(ctx.getChild(1).getText())
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#ev_card_or_none.
    def visitEv_card_or_none(self, ctx):
        if self.enable_warning and ctx.getChildCount() > 0:
            self.warning("Ignored '{0}'".format(ctx.getText()), ctx)
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#machine_decl.
    def visitMachine_decl(self, ctx):
        new_visiting_machine = PMachine()
        self.current_visited_machine = new_visiting_machine
        self.visitChildren(ctx)
        self.current_visited_machine = None
        self.current_pprogram.machines.add(new_visiting_machine)
        return new_visiting_machine


    # Visit a parse tree produced by pParser#machine_name_decl_regular.
    def visitMachine_name_decl_regular(self, ctx):
        ctx.getChild(0).accept(self)
        name = ctx.getChild(2).getText()
        self.current_visited_machine.name = name

    # Visit a parse tree produced by pParser#machine_name_decl_model.
    def visitMachine_name_decl_model(self, ctx):
        if self.enable_warning:
            self.warning("Ignored keyword 'model'", ctx)
        name = ctx.getChild(1).getText()
        self.current_visited_machine.name = name

    # Visit a parse tree produced by pParser#machine_name_decl_spec.
    def visitMachine_name_decl_spec(self, ctx):
        self.current_visited_machine.is_spec = True
        name = ctx.getChild(1).getText()
        self.current_visited_machine.name = name
        ctx.getChild(2).accept(self)

    # Visit a parse tree produced by pParser#observes_list.
    def visitObserves_list(self, ctx):
        new_event_list = []
        self.current_visited_event_list = new_event_list
        self.visitChildren(ctx)
        self.current_visited_event_list = None
        for e in new_event_list:
            self.current_pprogram.observes_map[e].append(self.current_visited_machine)

    # Visit a parse tree produced by pParser#is_main.
    def visitIs_main(self, ctx):
        self.current_visited_machine.is_main = ctx.getChildCount() != 0


    # Visit a parse tree produced by pParser#mach_card_or_none.
    def visitMach_card_or_none(self, ctx):
        if self.enable_warning and ctx.getChildCount() > 0:
            self.warning("Ignored '{0}'".format(ctx.getText()), ctx)
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#var_decl.
    def visitVar_decl(self, ctx):
        var_list = ctx.getChild(1).accept(self)
        var_type = ctx.getChild(3).accept(self)
        self.current_visited_machine.var_decls.update({i : var_type for i in var_list})
        return

    # Visit a parse tree produced by pParser#var_list.
    def visitVar_list(self, ctx):
        if ctx.getChildCount() == 1:
            return [ctx.getChild(0).getText()]
        else:
            return [ctx.getChild(0).getText()] + ctx.getChild(2).accept(self)


    # Visit a parse tree produced by pParser#fun_decl.
    def visitFun_decl(self, ctx):
        f = PFunctionWrapper()
        self.current_visited_fn = f 
        self.visitChildren(ctx)
        self.current_visited_fn = None
        if self.current_visited_machine:
            self.current_visited_machine.fun_decls[f.name] = f
        else:
            self.current_pprogram.global_fn_decls[f.name] = f
        return f


    # Visit a parse tree produced by pParser#fun_name_decl.
    def visitFun_name_decl(self, ctx):
        self.current_visited_fn.name = ctx.getChild(1).getText()


    # Visit a parse tree produced by pParser#is_model.
    def visitIs_model(self, ctx):
        if self.enable_warning and ctx.getChildCount() > 0:
            self.warning("Ignored '{0}'".format(ctx.getText()), ctx)
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#params_or_none.
    def visitParams_or_none(self, ctx):
        if ctx.getChildCount() == 2:
            return 
        return self.current_visited_fn.params.update(ctx.getChild(1).accept(self))

    # Visit a parse tree produced by pParser#payload_var_decl_or_none.
    def visitPayload_var_decl_or_none(self, ctx):
        if ctx.getChildCount() > 0:
            self.current_visited_fn.params[ctx.getChild(1).getText()] = ctx.getChild(3).accept(self)


    # Visit a parse tree produced by pParser#payload_var_decl_or_none_ref.
    def visitPayload_var_decl_or_none_ref(self, ctx):
        if ctx.getChildCount() > 0:
            self.current_visited_fn.params[ctx.getChild(1).getText()] = ctx.getChild(3).accept(self)

    # Visit a parse tree produced by pParser#ret_type_or_none.
    def visitRet_type_or_none(self, ctx):
        self.current_visited_fn.ret_type = ctx.getChild(1).accept(self) if ctx.getChildCount() > 0 else None
        return

    # Visit a parse tree produced by pParser#group.
    def visitGroup(self, ctx):
        raise ValueError("Group is not supported")


    # Visit a parse tree produced by pParser#group_body.
    def visitGroup_body(self, ctx):
        raise ValueError("Group is not supported")


    # Visit a parse tree produced by pParser#group_item.
    def visitGroup_item(self, ctx):
        raise ValueError("Group is not supported")


    # Visit a parse tree produced by pParser#group_name.
    def visitGroup_name(self, ctx):
        raise ValueError("Group is not supported")


    # Visit a parse tree produced by pParser#state_decl.
    def visitState_decl(self, ctx):
        new_visiting_state = PMachineState()
        new_visiting_state.name = ctx.getChild(3).getText()
        self.current_visited_state = new_visiting_state
        self.visitChildren(ctx)
        self.current_visited_state = None
        self.current_visited_machine.state_decls[new_visiting_state.name] = new_visiting_state
        return new_visiting_state

    # Visit a parse tree produced by pParser#is_start_state_or_none.
    def visitIs_start_state_or_none(self, ctx):
        self.current_visited_state.is_start = ctx.getChildCount() > 0

    # Visit a parse tree produced by pParser#is_hot_or_cold_or_none.
    def visitIs_hot_or_cold_or_none(self, ctx):
        if self.enable_warning and ctx.getChildCount() > 0:
            self.warning("Ignored '{0}'".format(ctx.getText()), ctx)
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#state_body_item_entry_unnamed.
    def visitState_body_item_entry_unnamed(self, ctx):
        f = PFunctionWrapper()
        f.name = self.current_visited_state.name + "_Entry" 
        self.current_visited_fn = f
        self.visitChildren(ctx)
        self.current_visited_fn = None
        self.current_visited_machine.fun_decls[f.name] = f
        self.current_visited_state.entry_fn = f.name
        return f


    # Visit a parse tree produced by pParser#state_body_item_entry_fn_named.
    def visitState_body_item_entry_fn_named(self, ctx):
        self.current_visited_state.entry_fn = ctx.getChild(1).getText()


    # Visit a parse tree produced by pParser#state_body_item_exit_unnamed.
    def visitState_body_item_exit_unnamed(self, ctx):
        f = PFunctionWrapper()
        f.name = self.current_visited_state.name + "_Exit" 
        self.current_visited_fn = f
        self.visitChildren(ctx)
        self.current_visited_fn = None
        self.current_visited_machine.fun_decls[f.name] = f
        self.current_visited_state.exit_fn = f.name
        return f


    # Visit a parse tree produced by pParser#state_body_item_exit_fn_named.
    def visitState_body_item_exit_fn_named(self, ctx):
        self.current_visited_state.exit_fn = ctx.getChild(1).getText()


    # Visit a parse tree produced by pParser#state_body_item_defer.
    def visitState_body_item_defer(self, ctx):
        new_event_list = []
        self.current_visited_event_list = new_event_list
        self.visitChildren(ctx)
        self.current_visited_state.defered_events.update(self.current_visited_event_list)
        self.current_visited_event_list = None

    # Visit a parse tree produced by pParser#state_body_item_ignore.
    def visitState_body_item_ignore(self, ctx):
        new_event_list = []
        self.current_visited_event_list = new_event_list
        self.visitChildren(ctx)
        self.current_visited_state.ignored_events.update(self.current_visited_event_list)
        self.current_visited_event_list = None


    # Visit a parse tree produced by pParser#state_body_item_on_e_do_fn_named.
    def visitState_body_item_on_e_do_fn_named(self, ctx):
        fn_name = ctx.getChild(2).getText()
        new_event_list = []
        self.current_visited_event_list = new_event_list
        self.visitChildren(ctx)
        self.current_visited_event_list = None
        self.current_visited_state.transitions.update({e : TransitionAttributes(fn_name=fn_name, 
                                                                                to_state=None, 
                                                                                is_named=True, 
                                                                                is_push=False) 
                                                        for e in new_event_list})        

    # Visit a parse tree produced by pParser#state_body_item_on_e_do_unamed.
    def visitState_body_item_on_e_do_unamed(self, ctx):
        f = PFunctionWrapper()
        f.is_transition_handler = True
        self.current_visited_fn = f
        new_event_list = []
        self.current_visited_event_list = new_event_list
        self.visitChildren(ctx)
        self.current_visited_fn = None
        self.current_visited_event_list = None
        f.name = "{0}_on_{1}".format(self.current_visited_state.name, "_".join(new_event_list))
        self.current_visited_machine.fun_decls[f.name] = f
        self.current_visited_state.transitions.update({e : TransitionAttributes(fn_name=f.name, 
                                                                                to_state=None, 
                                                                                is_named=False, 
                                                                                is_push=False)  
                                                        for e in new_event_list})


    # Visit a parse tree produced by pParser#state_body_item_push.
    def visitState_body_item_push(self, ctx):
        new_event_list = []
        self.current_visited_event_list = new_event_list
        self.visitChildren(ctx)
        self.current_visited_event_list = None
        self.current_visited_state.transitions.update({e : TransitionAttributes(fn_name=None, 
                                                                                to_state=self.current_state_target, 
                                                                                is_named=False, 
                                                                                is_push=True) 
                                                        for e in new_event_list})
        self.current_state_target = None


    # Visit a parse tree produced by pParser#state_body_item_on_e_goto.
    def visitState_body_item_on_e_goto(self, ctx):
        new_event_list = []
        self.current_visited_event_list = new_event_list
        self.visitChildren(ctx)
        self.current_visited_event_list = None
        self.current_visited_state.transitions.update({e : TransitionAttributes(fn_name=None, 
                                                                                to_state=self.current_state_target, 
                                                                                is_named=False, 
                                                                                is_push=False) 
                                                        for e in new_event_list})
        self.current_state_target = None

    # Visit a parse tree produced by pParser#state_body_item_on_e_goto_with_unnamed.
    def visitState_body_item_on_e_goto_with_unnamed(self, ctx):
        f = PFunctionWrapper()
        f.is_transition_handler = True
        new_event_list = []
        self.current_visited_fn = f
        self.current_visited_event_list = new_event_list
        self.visitChildren(ctx)
        self.current_visited_fn = None
        self.current_visited_event_list = None
        f.name = "{0}_on_{1}".format(self.current_visited_state.name, "_".join(new_event_list))
        f.from_to_state = (self.current_visited_state.name, self.current_state_target)
        self.current_visited_machine.fun_decls[f.name] = f
        self.current_visited_state.transitions.update({e : TransitionAttributes(fn_name=f.name, 
                                                                                to_state=self.current_state_target, 
                                                                                is_named=False,
                                                                                is_push=False) 
                                                        for e in new_event_list})


    # Visit a parse tree produced by pParser#state_body_item_on_e_goto_with_fn_named.
    def visitState_body_item_on_e_goto_with_fn_named(self, ctx):
        fn_name = ctx.getChild(2).getText()
        new_event_list = []
        self.current_visited_event_list = new_event_list
        self.visitChildren(ctx)
        self.current_visited_event_list = None
        self.current_visited_state.transitions.update({e : TransitionAttributes(fn_name=fn_name, 
                                                                                to_state=self.current_state_target, 
                                                                                is_named=True, 
                                                                                is_push=False) 
                                                        for e in new_event_list})
        return new_event_list


    # Visit a parse tree produced by pParser#event_id.
    def visitEvent_id(self, ctx):
        return self.current_visited_event_list.append(ctx.getChild(0).getText())


    # Visit a parse tree produced by pParser#non_default_event_id.
    def visitNon_default_event_id(self, ctx):
        return self.current_visited_event_list.append(ctx.getChild(0).getText())


    # Visit a parse tree produced by pParser#stmt_block.
    def visitStmt_block(self, ctx):
        self.current_visited_fn.stmt_block = ctx
        return

    # Visit a parse tree produced by pParser#state_target.
    def visitState_target(self, ctx):
        self.current_state_target = ctx.getChild(0).getText()
        if self.enable_warning and ctx.getChildCount() > 1:
            self.warning("'.' ignored", ctx)
        return

    # Visit a parse tree produced by pParser#stmt_pop.
    def visitStmt_pop(self, ctx):
        self.out("this.PopState(); retcode = RAISED_EVENT; return;\n")

