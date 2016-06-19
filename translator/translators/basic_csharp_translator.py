from .ast_to_pprogram import *
from .translator_base import TranslatorBase

class PProgramToCSharpTranslator(TranslatorBase):
    type_to_csharp_type_name_map = {
        None: "void",
        PTypeBool: "bool",
        PTypeInt: "int",
        PTypeMachine: "PMachine",
        PTypeEvent: "int"
    }

    type_to_default_exp_map = {
        None: "null",
        PTypeBool: "false",
        PTypeInt: "0",
        PTypeMachine: "null",
        PTypeEvent: "EVENT_NULL"
    }

    def __init__(self, *args):
        super(PProgramToCSharpTranslator, self).__init__(*args)

    def translate_type(self, T):
        t = None
        if T in self.type_to_csharp_type_name_map:
            return self.type_to_csharp_type_name_map[T]
        elif isinstance(T, PTypeSeq):
            t = "List<{0}>".format(self.translate_type(T.T))
        elif isinstance(T, PTypeMap):
            t = "Dictionary<{0},{1}>".format(self.translate_type(T.T1), self.translate_type(T.T2))
        elif isinstance(T, PTypeTuple):
            t = "Tuple<{0}>".format(','.join([self.translate_type(x) for x in T.Ts]))
        elif isinstance(T, PTypeNamedTuple):
            t = "Tuple<{0}>".format(','.join([self.translate_type(x) for x in T.NTs.values()]))
        self.type_to_csharp_type_name_map[T] = t
        return t

    def build_transition_map(self, machine):
        rtransitions_map = defaultdict(list) 
        for state in machine.state_decls.values():
            for on_e, (fn_name, to_state, is_named) in state.transitions.items():
                rtransitions_map[(state, machine.state_decls.get(to_state), fn_name, is_named)].append(on_e)
        return rtransitions_map

    def translate_state(self, machine, state):
        return "Machine{0}_S_{1}".format(machine.name, state.name)

    def translate_default_exp(self, T):
        r = None
        if T in self.type_to_default_exp_map:
            r = self.type_to_default_exp_map[T]
        elif isinstance(T, PTypeSeq) or isinstance(T, PTypeMap):
            r = "new {0}()".format(self.translate_type(T))
        elif isinstance(T, PTypeTuple):
            r = "new {0}({1})".format(self.translate_type(T), ','.join([self.translate_default_exp(x) for x in T.Ts]))
        elif isinstance(T, PTypeNamedTuple):
            r = "new {0}({1})".format(self.translate_type(T), ','.join([self.translate_default_exp(x) for x in T.NTs.values()]))
        return r

    def out_fn_call(self, machine, fn_name):
        self.out(fn_name)
        self.out("(")
        assert(len(machine.fun_decls[fn_name].params) <= 1)
        possible_payload_type = list(machine.fun_decls[fn_name].params.values())
        if possible_payload_type:
            self.out("(")
            self.out(self.translate_type(possible_payload_type[0]))
            self.out(")")
            self.out("payload")
        self.out(");\n")

    def out_enter_state(self, machine, state):
        self.out("this.state = {0};\n".format(self.translate_state(machine, state)))
        if state.entry_fn:
            self.out_fn_call(machine, state.entry_fn)

    def out_exit_state(self, machine, state):
        if state.exit_fn:
            self.out_fn_call(state.exit_fn)

    def out_fn_body(self, machine, fn_name):
        fn = machine.fun_decls[fn_name]
        self.current_visited_fn = fn
        fn.stmt_block.accept(self)

    def out_arg_cast_for_function(self, machine, fn_name):
        fn = machine.fun_decls[fn_name]
        if len(fn.params) > 0:
            p, t = fn.params.items()[0]
            t_str = self.translate_type(t)
            self.out("{0} {1} = ({2})_payload;\n".format(t_str, p, t_str))

    def out_fn_decl(self, machine, fn_name):
        fn_node = machine.fun_decls[fn_name]
        ret_type = fn_node.ret_type
        if fn_node.is_transition_handler:
            self.out("private {0} {1} (object _payload) {{\n".format(self.translate_type(ret_type), fn_name))
            self.out_arg_cast_for_function(machine, fn_name)
        else:
            self.out("private {0} {1} ({2}) {{\n".format(self.translate_type(ret_type), fn_name, 
                    ",".join(("{} {}".format(self.translate_type(t), i)) for i,t in fn_node.params.items())))
        self.out_fn_body(machine, fn_name)
        self.out("}\n")

    def translate(self):
        if not os.path.exists(self.out_dir):
            os.makedirs(self.out_dir)
        # create ProjectMacros
        with open(os.path.join(self.out_dir, "ProjectMacros.h"), 'w+') as macrosf:
            self.stream = macrosf
            self.out('#include "CommonMacros.h"\n\n')
            for i, e in enumerate(self.pprogram.events):
                self.out("#define {0} {1}\n".format(e, i))
            for machine in self.pprogram.machines:
                self.out("\n")
                for i, s in enumerate(machine.state_decls.values()):
                    self.out("#define {0} {1}\n".format(self.translate_state(machine, s), i))
        # generated .cs files for each machine
        for machine in self.pprogram.machines:
            self.current_visited_machine = machine
            classname = "Machine" + machine.name
            with open(os.path.join(self.out_dir, classname + ".cs"), 'w+') as csharpsrcf:
                self.stream = csharpsrcf
                self.out('#include "ProjectMacros.h"\n\n')
                self.out("using System;\nusing System.Collections.Generic;\nusing System.Diagnostics;\n\n")
                base_clase = "MonitorPMachine" if machine.is_spec else "PMachine"
                self.out("class {0} : {1} {{\n".format(classname, base_clase))
                # Defered set 
                self.out("\nprivate readonly static bool[,] _DeferedSet = {\n")
                for state in machine.state_decls.values():
                    self.out("{");
                    for e in self.pprogram.events:
                        self.out("true ," if e in state.defered_events else "false,")
                    self.out("},\n")
                self.out("};\n")
                # P local vars
                self.out("/* P local vars */\n")
                for i, t in machine.var_decls.items():
                    self.out("{type} {var_name}={init};\n".format(
                                    type=self.translate_type(t), 
                                    var_name=i,
                                    init=self.translate_default_exp(t)
                                ))
                self.out("\n\n")
                # Constructor(including transition function map)
                self.out("public {0} () {{\n".format(classname))
                self.out("this.DeferedSet = _DeferedSet;\n")
                self.out("this.Transitions = new TransitionFunction[{0},{1}];\n".format(len(machine.state_decls), len(self.pprogram.events)))
                rtransitions_map = self.build_transition_map(machine)
                for (state, to_state, fn_name, is_named), on_es in rtransitions_map.items():
                    transition_fn_name = None
                    if to_state:
                        if is_named or not fn_name:
                            transition_fn_name = "{0}_on_{1}".format(state.name, "_".join(on_es))
                        else:
                            transition_fn_name = fn_name
                    else:
                        if is_named and len(machine.fun_decls[fn_name].params) == 1:
                            transition_fn_name = fn_name
                        else:
                            transition_fn_name = "{0}_on_{1}".format(state.name, "_".join(on_es))
                    for on_e in on_es:
                        self.out("this.Transitions[{0},{1}] = {2};\n".format(self.translate_state(machine, state), on_e, transition_fn_name))
                for state in machine.state_decls.values():
                    for ignored_event in state.ignored_events:
                        self.out("this.Transitions[{0},{1}] = Transition_Ignore;\n".format(self.translate_state(machine, state), ignored_event))
                # StartMachine
                if machine.is_spec:
                    self.out_enter_state(machine, list(filter(lambda s: s.is_start, machine.state_decls.values()))[0])
                else:
                    self.out("}\n")
                    self.out("public override void StartMachine (Scheduler s, object payload) {\n")
                    self.out("base.StartMachine(s, payload);\n")
                    self.out_enter_state(machine, list(filter(lambda s: s.is_start, machine.state_decls.values()))[0])
                self.out("}\n")

                # Transition functions
                for (from_state, to_state, with_fn_name, is_named), on_es in rtransitions_map.items():
                    if to_state or len(machine.fun_decls[with_fn_name].params) != 1:
                        if is_named or not with_fn_name:
                            transition_fn_name = "{0}_on_{1}".format(from_state.name, "_".join(on_es))
                            self.out("private void {0} (object payload) {{\n".format(transition_fn_name))
                            if to_state:
                                self.out_exit_state(machine, from_state)
                            if with_fn_name:
                                self.out_fn_call(machine, with_fn_name)
                            if to_state:
                                self.out_enter_state(machine, to_state)
                            self.out("}\n")
                # Named and unnamed user defined functions
                for fn_name in machine.fun_decls:
                    self.out_fn_decl(machine, fn_name)
                self.out("}")

                # if main machine, create machine starter
                if machine.is_main:
                    with open(os.path.join(self.out_dir, "MachineController.cs"), "w+") as ms:
                        self.stream = ms
                        self.out('#include "ProjectMacros.h"\n\n')
                        self.out("class MachineController {\n\n")
                        spec_machines = filter(lambda m: m.is_spec, self.pprogram.machines)
                        for spec_machine in spec_machines:
                            self.out("static MonitorPMachine {m_name} = new Machine{m_type}();\n".format(
                                m_name=spec_machine.name.lower(), 
                                m_type=spec_machine.name)
                            )
                        self.out("\n")
                        self.out("public static PMachine CreateMainMachine() {{\nreturn new {0}();\n}}\n\n".format(classname))
                        self.out("/* Observers */\n")
                        self.out("public static void AnnounceEvent(int e, object payload) {\n")
                        self.out("switch(e) {")
                        for observed_event, observing_machines in self.pprogram.observes_map.items():
                            self.out("case {0}: {{\n".format(observed_event))
                            for m in observing_machines:
                                self.out("{0}.ServeEvent({1}, payload);\n".format(m.name.lower(), observed_event))
                            self.out("break;\n")
                            self.out("}\n")
                        self.out("}\n")
                        self.out("}\n")
                        self.out("}\n")
                                                
    # Visit a parse tree produced by pParser#local_var_decl.
    def visitLocal_var_decl(self, ctx):
        t = ctx.getChild(3).accept(self)
        var_list = ctx.getChild(1).accept(self)
        self.current_visited_fn.local_decls.update({i : t for i in var_list})
        self.out("{0} {1};", self.translate_type(t), ",".join("{0}={1}".format(var,self.translate_default_exp(t)) for var in var_list))


    # Visit a parse tree produced by pParser#local_var_decl_list.
    def visitLocal_var_decl_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#local_var_list.
    def visitLocal_var_list(self, ctx):
        if ctx.getChildCount() == 1:
            return [ctx.getChild(0).getText()]
        else:
            return self.ctx.getChild(2).accept(self).append(ctx.getChild(0).getText())


    # Visit a parse tree produced by pParser#stmt_block.
    def visitStmt_block(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_semicolon.
    def visitStmt_semicolon(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_rbrace.
    def visitStmt_rbrace(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_pop.
    def visitStmt_pop(self, ctx):
        raise ValueError("Pop not supported")


    # Visit a parse tree produced by pParser#stmt_stmt_list.
    def visitStmt_stmt_list(self, ctx):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_assert.
    def visitStmt_assert(self, ctx):
        self.out("Debug.Assert(")
        ctx.getChild(1).accept(self)
        self.out(");\n")


    # Visit a parse tree produced by pParser#stmt_assert_str.
    def visitStmt_assert_str(self, ctx):
        self.out("Debug.Assert(")
        ctx.getChild(1).accept(self)
        self.out(",")
        self.out(ctx.getChild(3).getText())
        self.out(");\n")


    # Visit a parse tree produced by pParser#stmt_print.
    def visitStmt_print(self, ctx):
        self.out("Console.Write(")
        ctx.getChild(1).accept(self)
        self.out(");\n")


    # Visit a parse tree produced by pParser#stmt_return.
    def visitStmt_return(self, ctx):
        self.out("return;\n")


    # Visit a parse tree produced by pParser#stmt_return_exp.
    def visitStmt_return_exp(self, ctx):
        self.out("return ")
        ctx.getChild(1).accept(self)
        self.out(";\n")


    # Visit a parse tree produced by pParser#stmt_assign.
    def visitStmt_assign(self, ctx):
        ctx.getChild(0).accept(self)
        self.out(" = ")
        ctx.getChild(2).accept(self)
        self.out(";\n")


    # Visit a parse tree produced by pParser#stmt_remove.
    def visitStmt_remove(self, ctx):
        target_type = ctx.getChild(0).accept(self)
        if isinstance(target_type, PTypeSeq):
            self.out(".RemoveAt(")
        else:
            self.out(".Remove(")
        ctx.getChild(2).accept(self)
        self.out(");")

    # Visit a parse tree produced by pParser#stmt_insert.
    def visitStmt_insert(self, ctx):
        ctx.getChild(0).accept(self)
        self.out(".Insert(")
        ctx.getChild(2).accept(self)
        self.out(");")

    # Visit a parse tree produced by pParser#stmt_while.
    def visitStmt_while(self, ctx):
        self.out("while(")
        ctx.getChild(2).accept(self)
        self.out(") {\n")
        ctx.getChild(4).accept(self)
        self.out("}\n")


    # Visit a parse tree produced by pParser#stmt_if_then_else.
    def visitStmt_if_then_else(self, ctx):
        self.out("if(")
        ctx.getChild(2).accept(self)
        self.out(") {\n")
        ctx.getChild(4).accept(self)
        self.out("} else {\n")
        ctx.getChild(6).accept(self)
        self.out("}\n")


    # Visit a parse tree produced by pParser#stmt_if_then.
    def visitStmt_if_then(self, ctx):
        self.out("if(")
        ctx.getChild(2).accept(self)
        self.out(") {\n")
        ctx.getChild(4).accept(self)
        self.out("}\n")

    # Visit a parse tree produced by pParser#stmt_new.
    def visitStmt_new(self, ctx):
        self.out("NewMachine(new Machine{}(), null);\n".format(ctx.getChild(1).getText()))


    # Visit a parse tree produced by pParser#stmt_new_with_arguments.
    def visitStmt_new_with_arguments(self, ctx):
        self.out("NewMachine(new Machine{}(), ".format(ctx.getChild(1).getText()))
        ctx.getChild(3).accept(self)
        self.out(");\n")

    # Visit a parse tree produced by pParser#stmt_call.
    def visitStmt_call(self, ctx):
        self.out(ctx.getChild(0).getText())
        self.out("();\n")


    # Visit a parse tree produced by pParser#stmt_call_with_arguments.
    def visitStmt_call_with_arguments(self, ctx):
        self.out(ctx.getChild(0).getText())
        self.out("(")
        ctx.getChild(2).accept(self)
        self.out(");\n")

    # Visit a parse tree produced by pParser#stmt_raise.
    def visitStmt_raise(self, ctx):
        self.out("this.ServeEvent(")
        ctx.getChild(1).accept(self)
        self.out(", null); retcode = RAISED_EVENT; return;\n")


    # Visit a parse tree produced by pParser#stmt_raise_with_arguments.
    def visitStmt_raise_with_arguments(self, ctx):
        self.out("this.ServeEvent(")
        ctx.getChild(1).accept(self)
        self.out(", ")
        ctx.getChild(3).accept(self)
        self.out("); retcode = RAISED_EVENT; return;\n")


    # Visit a parse tree produced by pParser#stmt_send.
    def visitStmt_send(self, ctx):
        self.out("this.SendMsg(")
        ctx.getChild(2).accept(self)
        self.out(", ")
        ctx.getChild(4).accept(self)
        self.out(", null);\n")


    # Visit a parse tree produced by pParser#stmt_send_with_arguments.
    def visitStmt_send_with_arguments(self, ctx):
        self.out("this.SendMsg(")
        ctx.getChild(2).accept(self)
        self.out(", ")
        ctx.getChild(4).accept(self)
        self.out(", ")
        ctx.getChild(6).accept(self)
        self.out(");\n")


    # Visit a parse tree produced by pParser#stmt_monitor.
    def visitStmt_monitor(self, ctx):
        self.out("MachineController.AnnounceEvent(")
        ctx.getChild(1).accept(self)
        self.out(", null);\n")


    # Visit a parse tree produced by pParser#stmt_monitor_with_arguments.
    def visitStmt_monitor_with_arguments(self, ctx):
        self.out("MachineController.AnnounceEvent(")
        ctx.getChild(1).accept(self)
        self.out(", ")
        ctx.getChild(3).accept(self)
        self.out(");\n")


    # Visit a parse tree produced by pParser#stmt_recieve.
    def visitStmt_recieve(self, ctx):
        raise ValueError("Recieve not supported")

    def visitBinary_Exp(ret_type):
        def visitBinary_Exp_Wrapped(self, ctx):
            if ctx.getChildCount() > 1:
                ctx.getChild(0).accept(self)
                self.out(" {0} ".format(ctx.getChild(1).getText()))
                ctx.getChild(2).accept(self)
                return ret_type
            else:
                return ctx.getChild(0).accept(self)
        return visitBinary_Exp_Wrapped

    visitExp = visitBinary_Exp(PTypeBool)
    visitExp_7 = visitBinary_Exp(PTypeBool)
    visitExp_6 = visitBinary_Exp(PTypeBool)

    def visitExp_5(self, ctx):            
        if ctx.getChildCount() > 1:
            if ctx.getChild(1).getText() == "in":
                ctx.getChild(2).accept(self)
                self.out(".ContainsKey(".format(ctx.getChild(1).getText()))
                ctx.getChild(0).accept(self)
                self.out(")")
            else:
                ctx.getChild(0).accept(self)
                self.out(" {0} ".format(ctx.getChild(1).getText()))
                ctx.getChild(2).accept(self)
            return PTypeBool
        else:
            return ctx.getChild(0).accept(self)
    
    def visitExp_4(self, ctx):
        if ctx.getChildCount() > 1:
            self.out("((")
            ret_type = ctx.getChild(2).accept(self)
            self.out(")")
            ctx.getChild(0).accept(self)
            self.out(")")
            return ret_type
        else:
            return ctx.getChild(0).accept(self)

    visitExp_3 = visitBinary_Exp(PTypeInt)
    visitExp_2 = visitBinary_Exp(PTypeInt)

    # Visit a parse tree produced by pParser#exp_1.
    def visitExp_1(self, ctx):
        if ctx.getChildCount() > 1:
            op = ctx.getChild(0).getText()
            self.out(op)
            ctx.getChild(1).accept(self)
            return PTypeInt if op == "-" else PTypeBool
        else:
            return ctx.getChild(0).accept(self)


    # Visit a parse tree produced by pParser#exp_getidx.
    def visitExp_getidx(self, ctx):
        tuple_type = ctx.getChild(0).accept(self)
        idx = int(ctx.getChild(2).getText())
        self.out(".Item{0}".format(idx + 1))
        return tuple_type.Ts[idx]

    # Visit a parse tree produced by pParser#exp_sizeof.
    def visitExp_sizeof(self, ctx):
        ctx.getChild(2).accept(self)
        self.out(".Count")
        return PTypeInt

    # Visit a parse tree produced by pParser#exp_call.
    def visitExp_call(self, ctx):
        fn_name = ctx.getChild(0).getText()
        self.out("{0}()".format(fn_name))
        return self.current_visited_machine.fun_decls[fn_name].ret_type

    # Visit a parse tree produced by pParser#exp_new.
    def visitExp_new(self, ctx):
        self.out("NewMachine(new Machine{}(), null)".format(ctx.getChild(1).getText()))
        return PTypeMachine

    # Visit a parse tree produced by pParser#exp_call_with_arguments.
    def visitExp_call_with_arguments(self, ctx):
        fn_name = ctx.getChild(0).getText()
        self.out("{0}(".format(fn_name))
        ctx.getChild(2).accept(self)
        self.out(")")
        return self.current_visited_machine[fn_name].ret_type

    # Visit a parse tree produced by pParser#exp_nondet.
    def visitExp_nondet(self, ctx):
        self.out("RandomBool()")
        return PTypeBool

    # Visit a parse tree produced by pParser#exp_this.
    def visitExp_this(self, ctx):
        self.out("this")
        return PTypeMachine

    # Visit a parse tree produced by pParser#exp_id.
    def visitExp_id(self, ctx):
        identifier = ctx.getChild(0).getText()
        self.out(identifier)
        if identifier in self.current_visited_fn.local_decls:
            return self.current_visited_fn.local_decls[identifier]
        elif identifier in self.current_visited_fn.params:
            return self.current_visited_fn.params[identifier]
        elif identifier in self.current_visited_machine.var_decls:
            return self.current_visited_machine.var_decls[identifier]
        elif identifier in self.pprogram.events:
            return PTypeEvent
        else:
            self.warning("Cannot find identifier {0}".format(identifier), ctx)
            return PTypeAny

    # Visit a parse tree produced by pParser#exp_getattr.
    def visitExp_getattr(self, ctx):
        named_tuple_type = ctx.getChild(0).accept(self)
        attr = ctx.getChild(2).getText()
        idx = named_tuple_type.NTs.keys().index(attr) + 1
        self.out(".Item{0}".format(idx))
        return named_tuple_type.NTs[attr]

    # Visit a parse tree produced by pParser#exp_named_tuple_1_elem.
    def visitExp_named_tuple_1_elem(self, ctx):
        n1 = ctx.getChild(1).getText()
        old_s = self.acquire_buffer()
        t1 = ctx.getChild(3).accept(self)
        tmp_buffer = self.release_buffer(old_s)
        nmd_tuple_type = PTypeNamedTuple([(n1, t1)])
        self.out("new ")
        self.out(self.translate_type(nmd_tuple_type))
        self.out("(")
        self.dump_buffer(tmp_buffer)
        self.out(")")       
        return nmd_tuple_type

    # Visit a parse tree produced by pParser#exp_keys.
    def visitExp_keys(self, ctx):
        map_type = ctx.getChild(2).accept(self)
        self.out(".Keys()")
        return map_type.T1

    # Visit a parse tree produced by pParser#exp_grouped.
    def visitExp_grouped(self, ctx):
        self.out("(")
        ret_type = ctx.getChild(1).accept(self)
        self.out(")")
        return ret_type

    # Visit a parse tree produced by pParser#exp_named_tuple_n_elems.
    def visitExp_named_tuple_n_elems(self, ctx):
        n1 = ctx.getChild(1).getText()
        old_s = self.acquire_buffer()
        t1 = ctx.getChild(3).accept(self)
        self.out(",")
        trest = ctx.getChild(5).accept(self)
        tmp_buffer = self.release_buffer(old_s)
        nmd_tuple_type = PTypeNamedTuple([(n1, t1)] + trest)
        self.out("new ")
        self.out(self.translate_type(nmd_tuple_type))
        self.out("(")
        self.dump_buffer(tmp_buffer)
        self.out(")")       
        return nmd_tuple_type

    # Visit a parse tree produced by pParser#exp_true.
    def visitExp_true(self, ctx):
        self.out("true")
        return PTypeBool

    # Visit a parse tree produced by pParser#exp_values.
    def visitExp_values(self, ctx):
        map_type = ctx.getChild(2).accept(self)
        self.out(".Values()")
        return map_type.T1

    # Visit a parse tree produced by pParser#exp_default.
    def visitExp_default(self, ctx):
        raise ValueError("Default not implemented")
        t = ctx.getChild(2).accept(self)
        self.out("null")
        return t

    # Visit a parse tree produced by pParser#exp_null.
    def visitExp_null(self, ctx):
        self.out("null")
        return PTypeAny

    # Visit a parse tree produced by pParser#exp_new_with_arguments.
    def visitExp_new_with_arguments(self, ctx):
        self.out("NewMachine(new Machine{0}(), ".format(ctx.getChild(1).getText()))
        ctx.getChild(3).accept(self)
        self.out(")")
        return PTypeMachine

    # Visit a parse tree produced by pParser#exp_false.
    def visitExp_false(self, ctx):
        self.out("false")
        return PTypeBool

    # Visit a parse tree produced by pParser#exp_halt.
    def visitExp_halt(self, ctx):
        self.out("this.Halt()")
        return PTypeEvent

    # Visit a parse tree produced by pParser#exp_getitem.
    def visitExp_getitem(self, ctx):
        map_or_seq_type = ctx.getChild(0).accept(self)
        self.out("[")
        ctx.getChild(2).accept(self)
        self.out("]")
        if isinstance(map_or_seq_type, PTypeMap):
            return map_or_seq_type.T2
        elif isinstance(map_or_seq_type, PTypeSeq):
            return map_or_seq_type.T
        else:
            self.warning("Failed to infer type", ctx)
            return PTypeAny

    # Visit a parse tree produced by pParser#exp_fairnondet.
    def visitExp_fairnondet(self, ctx):
        self.out("RandomBool()")
        return PTypeBool

    # Visit a parse tree produced by pParser#exp_tuple_1_elem.
    def visitExp_tuple_1_elem(self, ctx):
        old_s = self.acquire_buffer()
        t = ctx.getChild(1).accept(self)
        tmp_buffer = self.release_buffer(old_s)
        tuple_type = PTypeTuple([t])
        self.out("new ")
        self.out(self.translate_type(tuple_type))
        self.out("(")
        self.dump_buffer(tmp_buffer)
        self.out(")")
        return tuple_type

    # Visit a parse tree produced by pParser#exp_int.
    def visitExp_int(self, ctx):
        self.out(ctx.getChild(0).getText())
        return PTypeInt

    # Visit a parse tree produced by pParser#exp_tuple_n_elems.
    def visitExp_tuple_n_elems(self, ctx):
        old_s = self.acquire_buffer()
        t = ctx.getChild(1).accept(self)
        self.out(",")
        trest = ctx.getChild(3).accept(self)
        tmp_s = self.release_buffer(old_s)
        self.out("new ")
        tuple_type = PTypeTuple([t] + trest)
        self.out(self.translate_type(tuple_type))
        self.out("(")
        self.dump_buffer(tmp_s)
        self.out(")")
        return tuple_type

    # Visit a parse tree produced by pParser#single_expr_arg_list.
    def visitSingle_expr_arg_list(self, ctx):
        if ctx.getChildCount() == 1:
            return ctx.getChild(0).accept(self)
        else:
            old_s = self.acquire_buffer()
            t1 = ctx.getChild(0).accept(self)
            self.out(",")
            trest = ctx.getChild(3).accept(self)
            tmp_s = self.release_buffer(old_s)
            self.out("new ")
            if isinstance(trest, PTypeTuple):
                trest.Ts = [t1] + trest.Ts
                tuple_type = trest
            else:
                tuple_type = PTypeTuple([t1, trest])
            self.out(self.translate_type(tuple_type))
            self.out("(")
            self.dump_buffer(tmp_s)
            self.out(")")

    # Visit a parse tree produced by pParser#expr_arg_list.
    def visitExpr_arg_list(self, ctx):
        if ctx.getChildCount() == 2:
            return [ctx.getChild(0).accept(self)]
        else:
            t1 = ctx.getChild(0).accept(self)
            self.out(",")
            trest = ctx.getChild(3).accept(self)
            return [t1] + trest

    # Visit a parse tree produced by pParser#nmd_expr_arg_list.
    def visitNmd_expr_arg_list(self, ctx):
        if ctx.getChildCount() == 3:
            return [(ctx.getChild(0).getText(), ctx.getChild(2).accept(self))]
        else:
            id1 = ctx.getChild(0).getText()
            t1 = ctx.getChild(2).accept(self)
            self.out(",")
            trest = ctx.getChild(4).accept(self)
            return [(id1, t1)] + trest