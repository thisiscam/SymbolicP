from .ast_to_pprogram import *
from .translator_base import TranslatorBase
from quik import FileLoader
import glob, shutil, uuid

class PProgramToCSharpTranslator(TranslatorBase):
    type_to_csharp_type_name_map = {
        None: "void",
        PTypeBool: "PBool",
        PTypeInt: "PInteger",
        PTypeMachine: "PMachine",
        PTypeEventUnknown: "PInteger",
        PTypeAny: "IPType"
    }

    type_to_default_exp_map = {
        None: "null",
        PTypeBool: "new PBool(false)",
        PTypeInt: "new PInteger(0)",
        PTypeMachine: "null",
        PTypeEventUnknown: "EVENT_NULL"
    }

    runtime_dir = os.environ.get("RUNTIME_DIR", os.path.realpath(os.path.dirname(__file__) + "/../runtimes/basic_csharp"))

    def __init__(self, *args):
        super(PProgramToCSharpTranslator, self).__init__(*args)

    def translate_type(self, T):
        t = None
        if T in self.type_to_csharp_type_name_map:
            return self.type_to_csharp_type_name_map[T]
        elif isinstance(T, PTypeEvent):
            return self.type_to_csharp_type_name_map[PTypeEventUnknown]
        elif isinstance(T, PTypeSeq):
            t = "PList<{0}>".format(self.translate_type(T.T))
        elif isinstance(T, PTypeMap):
            t = "PMap<{0},{1}>".format(self.translate_type(T.T1), self.translate_type(T.T2))
        elif isinstance(T, PTypeTuple):
            t = "PTuple<{0}>".format(','.join([self.translate_type(x) for x in T.Ts]))
        elif isinstance(T, PTypeNamedTuple):
            t = "PTuple<{0}>".format(','.join([self.translate_type(x) for x in T.NTs.values()]))
        self.type_to_csharp_type_name_map[T] = t
        return t

    def build_transition_map(self, machine):
        rtransitions_map = defaultdict(list) 
        for state in machine.state_decls.values():
            for on_e, (fn_name, to_state, is_named, is_push) in state.transitions.items():
                rtransitions_map[(state, machine.state_decls.get(to_state), fn_name, is_named, is_push)].append(on_e)
        return rtransitions_map

    def translate_state(self, machine, state, full_qualified=True):
        if full_qualified:
            return "Constants.Machine{0}_S_{1}".format(machine.name, state.name)
        else:
            return "Machine{0}_S_{1}".format(machine.name, state.name)

    def translate_event(self, event, full_qualified=True):
        if event == "null":
            event = "EVENT_NULL"
        if full_qualified:
            return "Constants.{0}".format(event)
        else:
            return event

    def translate_default_exp(self, T):
        r = None
        if T in self.type_to_default_exp_map:
            r = self.type_to_default_exp_map[T]
        elif isinstance(T, PTypeEvent):
            r = self.type_to_default_exp_map[PTypeEventUnknown]
        elif isinstance(T, PTypeSeq) or isinstance(T, PTypeMap):
            r = "new {0}()".format(self.translate_type(T))
        elif isinstance(T, PTypeTuple):
            r = "new {0}({1})".format(self.translate_type(T), ','.join([self.translate_default_exp(x) for x in T.Ts]))
        elif isinstance(T, PTypeNamedTuple):
            r = "new {0}({1})".format(self.translate_type(T), ','.join([self.translate_default_exp(x) for x in T.NTs.values()]))
        return r

    def out_fn_call(self, machine, fn_name, last_stmt=True, ret_type=None):
        self.out(fn_name)
        self.out("(")
        assert(len(machine.fun_decls[fn_name].params) <= 1)
        possible_payload_type = list(machine.fun_decls[fn_name].params.values())
        if possible_payload_type:
            if not possible_payload_type[0] == self.current_payload_type:
                self.out("(")
                self.out(self.translate_type(possible_payload_type[0]))
                self.out(")")
            self.out("payload")
        self.out(");")
        if not last_stmt and machine.fun_decls[fn_name].can_raise_event:
            self.out_check_raise_exit(ret_type)
        self.out("\n")

    def out_enter_state(self, machine, state, last_stmt=True, ret_type=None, is_push=False):
        if is_push:
            self.out("this.states.Insert(0, {0});\n".format(self.translate_state(machine, state)))
        else:
            self.out("this.states[0] = {0};\n".format(self.translate_state(machine, state)))
        if state.entry_fn:
            self.out_fn_call(machine, state.entry_fn, last_stmt, ret_type)

    def out_exit_state(self, machine, state, last_stmt=True, ret_type=None):
        if state.exit_fn:
            self.out_fn_call(machine, state.exit_fn, last_stmt, ret_type)

    raise_return_type_to_default_exp_map = {
        None: "",
        PTypeBool: "false",
        PTypeInt: "-1",
        PTypeMachine: "null",
        PTypeEventUnknown: "-1"
    }
    def out_check_raise_exit(self, ret_type):
        if isinstance(ret_type, PTypeEvent):
            ret_type = PTypeEventUnknown
        default_exp = self.raise_return_type_to_default_exp_map.get(ret_type, "null")
        self.out(" if(retcode == Constants.RAISED_EVENT) {{ return {0}; }}\n".format(default_exp))

    def out_fn_body(self, machine, fn_name):
        fn = machine.fun_decls[fn_name]
        self.tmp_var_cnt = 0
        self.current_visited_fn = fn
        fn.stmt_block.accept(self)

    def out_arg_cast_for_function(self, machine, fn_name):
        fn = machine.fun_decls[fn_name]
        if len(fn.params) > 0:
            p, t = fn.params.items()[0]
            t_str = self.translate_type(t)
            self.out("{0} {1} = ({2})_payload;\n".format(t_str, p, t_str))
            self.current_payload_type = t
        else:
            self.current_payload_type = PTypeAny

    def out_fn_decl(self, machine, fn_name):
        fn_node = machine.fun_decls[fn_name]
        ret_type = fn_node.ret_type
        if fn_node.is_transition_handler:
            self.out("private {0} {1} ({2} _payload) {{\n".format(self.translate_type(ret_type), fn_name, self.translate_type(PTypeAny)))
            self.out_arg_cast_for_function(machine, fn_name)
        else:
            self.out("private {0} {1} ({2}) {{\n".format(self.translate_type(ret_type), fn_name, 
                    ",".join(("{0} {1}".format(self.translate_type(t), i)) for i,t in fn_node.params.items())))
            self.current_payload_type = None
        if fn_node.from_to_state:
            from_state, to_state = fn_node.from_to_state
            self.out_exit_state(machine, machine.state_decls[from_state], last_stmt=False, ret_type=ret_type)
            self.out_fn_body(machine, fn_name)
            self.out_enter_state(machine, machine.state_decls[to_state], last_stmt=True)
        else:    
            self.out_fn_body(machine, fn_name)
        self.out("}\n")

    def get_csproj_template_parameters(self, paramters):
        return paramters

    def create_csproj(self):
        runtime_srcs =  [os.path.basename(src) for src in glob.glob("{0}/*.cs".format(self.runtime_dir))]
        generated_srcs = [os.path.basename(src) for src in glob.glob("{0}/*.cs".format(self.out_dir))]
        loader = FileLoader(self.runtime_dir)
        csproj_template = loader.load_template("template.csproj.in")
        self.out_csproj_path = os.path.join(self.out_dir, "{0}.csproj".format(self.project_name))
        with open(self.out_csproj_path, "w+") as csprojf:
            csprojf.write(csproj_template.render(
                self.get_csproj_template_parameters(
                    {
                        'generated_srcs': generated_srcs, 
                        'runtime_srcs': runtime_srcs, 
                        'project_name': self.project_name, 
                        'runtime_dir': os.path.abspath(self.runtime_dir),
                        'lib_dir': os.path.abspath(os.path.dirname(__file__) + "/../libraries"),
                        'guid': "{" + str(uuid.uuid1()).upper() + "}"
                    }
            )))

    def make_output_dir(self):
        shutil.rmtree(self.out_dir, ignore_errors=True, onerror=None)
        os.makedirs(self.out_dir)            

    def create_proj_macros(self):
        # create ProjectMacros
        with open(os.path.join(self.out_dir, "ProjectConstants.cs"), 'w+') as macrosf:
            self.stream = macrosf
            self.out("public partial class Constants {\n")
            for i, e in enumerate(self.pprogram.events):
                if e != "EVENT_NULL":
                    self.out("public const int {0} = {1};\n".format(e, i))
            for machine in self.pprogram.machines:
                self.out("\n")
                for i, s in enumerate(machine.state_decls.values()):
                    self.out("public const int {0} = {1};\n".format(self.translate_state(machine, s, full_qualified=False), i))
            self.out("}")

    def generate_foreach_machine(self):
        # generated .cs files for each machine
        for machine in self.pprogram.machines:
            self.generate_for_machine(machine)
            # if main machine, create machine starter
            if machine.is_main:
                self.generate_for_main_machine(machine)

    def get_machine_csclassname(self, machine):
        return "Machine" + machine.name

    def out_machine_body(self):
        machine = self.current_visited_machine
        classname = self.get_machine_csclassname(machine)
        # Defered set 
        self.out("\nprivate readonly static bool[,] _DeferedSet = {\n")
        for state in machine.state_decls.values():
            self.out("{");
            for e in self.pprogram.events:
                self.out("true ," if e in state.defered_events else "false,")
            self.out("},\n")
        self.out("};\n")
        # IsGotoTransition
        self.out("\nprivate readonly static bool[,] _IsGotoTransition = {\n")
        for state in machine.state_decls.values():
            self.out("{")
            for e in self.pprogram.events:
                if e in state.transitions and state.transitions[e].to_state:
                    self.out("true, ")
                else:
                    self.out("false,")
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
        self.out("this.DeferedSet = _DeferedSet;\nthis.IsGotoTransition = _IsGotoTransition;\n")
        self.out("this.Transitions = new TransitionFunction[{0},{1}];\n".format(len(machine.state_decls), len(self.pprogram.events)))
        rtransitions_map = self.build_transition_map(machine)
        for (state, to_state, fn_name, is_named, is_push), on_es in rtransitions_map.items():
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
                self.out("this.Transitions[{0},{1}] = {2};\n".format(self.translate_state(machine, state), self.translate_event(on_e), transition_fn_name))
        for state in machine.state_decls.values():
            for ignored_event in state.ignored_events:
                self.out("this.Transitions[{0},{1}] = Transition_Ignore;\n".format(self.translate_state(machine, state), self.translate_event(ignored_event)))
        # Exit Functions
        self.out("this.ExitFunctions = new ExitFunction[{0}];\n".format(len(machine.state_decls)))
        for state in machine.state_decls.values():
            if state.exit_fn:
                self.out("this.ExitFunctions[{state}] = {exit_fn};\n".format(state=self.translate_state(machine,state), exit_fn=state.exit_fn))
        # StartMachine
        if machine.is_spec:
            self.out_enter_state(machine, list(filter(lambda s: s.is_start, machine.state_decls.values()))[0], is_push=True)
        else:
            self.out("}\n")
            self.out("public override void StartMachine (Scheduler scheduler, {0} payload) {{\n".format(self.translate_type(PTypeAny)))
            self.out("this.scheduler = scheduler;\n")
            self.current_payload_type = PTypeAny
            self.out_enter_state(machine, list(filter(lambda s: s.is_start, machine.state_decls.values()))[0], is_push=True)
        self.out("}\n")

        # Transition functions
        for (from_state, to_state, with_fn_name, is_named, is_push), on_es in rtransitions_map.items():
            if to_state or len(machine.fun_decls[with_fn_name].params) != 1:
                if is_named or not with_fn_name:
                    transition_fn_name = "{0}_on_{1}".format(from_state.name, "_".join(on_es))
                    self.out("private void {0} ({1} payload) {{\n".format(transition_fn_name, self.translate_type(PTypeAny)))
                    self.current_payload_type = PTypeAny
                    if is_push:
                        if with_fn_name:
                            self.out_fn_call(machine, with_fn_name, last_stmt=False)
                        self.out_enter_state(machine, to_state, last_stmt=True, is_push=True)
                    else:
                        if to_state:
                            self.out_exit_state(machine, from_state, last_stmt=with_fn_name and to_state)
                        if with_fn_name:
                            self.out_fn_call(machine, with_fn_name, last_stmt=to_state)
                        if to_state:
                            self.out_enter_state(machine, to_state, last_stmt=True)
                    self.out("}\n")
        # Named and unnamed user defined functions
        for fn_name in machine.fun_decls:
            self.out_fn_decl(machine, fn_name)

    def generate_for_machine(self, machine):
        self.current_visited_machine = machine
        classname = self.get_machine_csclassname(machine)
        with open(os.path.join(self.out_dir, classname + ".cs"), 'w+') as csharpsrcf:
            self.stream = csharpsrcf
            self.out("using System;\nusing System.Collections.Generic;\nusing System.Diagnostics;\n\n")
            base_clase = "MonitorPMachine" if machine.is_spec else "PMachine"
            self.out("class {0} : {1} {{\n".format(classname, base_clase))
            self.out_machine_body()
            self.out("}")

    def generate_for_main_machine(self, machine):
        classname = self.get_machine_csclassname(machine)
        with open(os.path.join(self.out_dir, "MachineController.cs"), "w+") as ms:
            self.stream = ms
            self.out("class MachineController {\n\n")
            spec_machines = filter(lambda m: m.is_spec, self.pprogram.machines)
            for spec_machine in spec_machines:
                self.out("static MonitorPMachine {m_name} = new Machine{m_type}();\n".format(
                    m_name="monitor_" + spec_machine.name.lower(), 
                    m_type=spec_machine.name)
                )
            self.out("\n")
            self.out("public static PMachine CreateMainMachine() {{\nreturn new {0}();\n}}\n\n".format(classname))
            if len(self.pprogram.observes_map) > 0:
                self.out("/* Observers */\n")
                self.out("public static void AnnounceEvent({0} e, {1} payload) {{\n".format(self.translate_type(PTypeEventUnknown), self.translate_type(PTypeAny)))
                for observed_event, observing_machines in self.pprogram.observes_map.items():
                    self.out("if(e == {0}) {{\n".format(self.translate_event(observed_event)))
                    for m in observing_machines:
                        self.out("{0}.ServeEvent({1}, payload);\n".format("monitor_" + m.name.lower(), self.translate_event(observed_event)))
                    self.out("return;\n")
                    self.out("}\n")
                self.out("}\n")
            self.out("}\n")

    pipeline = [make_output_dir, create_proj_macros, generate_foreach_machine, create_csproj]

    def translate(self):
        for procedure in self.pipeline:
            procedure(self)
    
    def allocate_local_var(self):
        var = "tmp{0}".format(self.tmp_var_cnt)
        self.tmp_var_cnt += 1
        return var

    # Visit a parse tree produced by pParser#local_var_decl.
    def visitLocal_var_decl(self, ctx, **kwargs):
        t = ctx.getChild(3).accept(self, **kwargs)
        var_list = ctx.getChild(1).accept(self, **kwargs)
        self.current_visited_fn.local_decls.update({i : t for i in var_list})
        self.out("{0} {1};\n".format(self.translate_type(t), ",".join("{0}={1}".format(var,self.translate_default_exp(t)) for var in var_list)))


    # Visit a parse tree produced by pParser#local_var_decl_list.
    def visitLocal_var_decl_list(self, ctx, **kwargs):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#local_var_list.
    def visitLocal_var_list(self, ctx, **kwargs):
        if ctx.getChildCount() == 1:
            return [ctx.getChild(0).getText()]
        else:
            ret = ctx.getChild(0).accept(self, **kwargs)
            ret.append(ctx.getChild(2).getText())
            return ret

    # Visit a parse tree produced by pParser#stmt_block.
    def visitStmt_block(self, ctx, **kwargs):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_semicolon.
    def visitStmt_semicolon(self, ctx, **kwargs):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_rbrace.
    def visitStmt_rbrace(self, ctx, **kwargs):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_pop.
    def visitStmt_pop(self, ctx, **kwargs):
        self.out("this.PopState(); retcode = Constants.RAISED_EVENT; return;\n")

    # Visit a parse tree produced by pParser#stmt_stmt_list.
    def visitStmt_stmt_list(self, ctx, **kwargs):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by pParser#stmt_assert.
    def visitStmt_assert(self, ctx, **kwargs):
        c1 = ctx.getChild(1).accept(self, **kwargs)
        self.out("Assert({0});\n".format(c1))


    # Visit a parse tree produced by pParser#stmt_assert_str.
    def visitStmt_assert_str(self, ctx, **kwargs):
        c1 = ctx.getChild(1).accept(self, **kwargs)
        c3 = ctx.getChild(3).getText()
        self.out("Assert({0},{1});\n".format(c1, c3))


    # Visit a parse tree produced by pParser#stmt_print.
    def visitStmt_print(self, ctx, **kwargs):
        c1 = ctx.getChild(1).accept(self, **kwargs)
        self.out("Console.Write({0})\n;".format(c1))


    # Visit a parse tree produced by pParser#stmt_return.
    def visitStmt_return(self, ctx, **kwargs):
        self.out("return;\n")


    # Visit a parse tree produced by pParser#stmt_return_exp.
    def visitStmt_return_exp(self, ctx, **kwargs):
        kwargs["do_copy"] = True
        c1 = ctx.getChild(1).accept(self, **kwargs)
        self.out("return {0};\n".format(c1))


    # Visit a parse tree produced by pParser#stmt_assign.
    def visitStmt_assign(self, ctx, **_kwargs):
        c0 = ctx.getChild(0).accept(self, **_kwargs)
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = True
        c2 = ctx.getChild(2).accept(self, **kwargs)
        self.out("{0} = {1};\n".format(c0, c2))


    # Visit a parse tree produced by pParser#stmt_remove.
    def visitStmt_remove(self, ctx, **kwargs):
        target_type = ctx.getChild(0).exp_type
        c0 = ctx.getChild(0).accept(self, **kwargs)
        c2 = ctx.getChild(2).accept(self, **kwargs)
        self.out(c0)
        if isinstance(target_type, PTypeSeq):
            self.out(".RemoveAt(")
        else:
            self.out(".Remove(")
        self.out(c2)
        self.out(");\n")

    # Visit a parse tree produced by pParser#stmt_insert.
    def visitStmt_insert(self, ctx, **kwargs):
        c0 = ctx.getChild(0).accept(self, **kwargs)
        c2 = ctx.getChild(2).accept(self, **kwargs)
        self.out(c0)
        self.out(".Insert(")
        self.out(c2)
        self.out(");\n")

    # Visit a parse tree produced by pParser#stmt_while.
    def visitStmt_while(self, ctx, **kwargs):
        c2 = ctx.getChild(2).accept(self, **kwargs)
        self.out("while(")
        self.out(c2)
        self.out(") {\n")
        ctx.getChild(4).accept(self, **kwargs)
        self.out("}\n")


    # Visit a parse tree produced by pParser#stmt_if_then_else.
    def visitStmt_if_then_else(self, ctx, **kwargs):
        c2 = ctx.getChild(2).accept(self, **kwargs)
        self.out("if(")
        self.out(c2)
        self.out(") {\n")
        ctx.getChild(4).accept(self, **kwargs)
        self.out("} else {\n")
        ctx.getChild(6).accept(self, **kwargs)
        self.out("}\n")


    # Visit a parse tree produced by pParser#stmt_if_then.
    def visitStmt_if_then(self, ctx, **kwargs):
        c2 = ctx.getChild(2).accept(self, **kwargs)
        self.out("if(")
        self.out(c2)
        self.out(") {\n")
        ctx.getChild(4).accept(self, **kwargs)
        self.out("}\n")

    # Visit a parse tree produced by pParser#stmt_new.
    def visitStmt_new(self, ctx, **kwargs):
        self.out("NewMachine(new Machine{}(), null);\n".format(ctx.getChild(1).getText()))

    # Visit a parse tree produced by pParser#stmt_new_with_arguments.
    def visitStmt_new_with_arguments(self, ctx, **kwargs):
        c3 = ctx.getChild(3).accept(self, **kwargs)
        self.out("NewMachine(new Machine{0}(),{1});\n".format(ctx.getChild(1).getText(), c3))

    # Visit a parse tree produced by pParser#stmt_call.
    def visitStmt_call(self, ctx, **kwargs):
        fn_name = ctx.getChild(0).getText()
        self.out(fn_name)
        self.out("();")
        if self.current_visited_machine.fun_decls[fn_name].can_raise_event:
            self.out_check_raise_exit(self.current_visited_fn.ret_type)
        self.out("\n")

    # Visit a parse tree produced by pParser#stmt_call_with_arguments.
    def visitStmt_call_with_arguments(self, ctx, **kwargs):
        fn_name = ctx.getChild(0).getText()
        self.out(fn_name)
        c2 = ctx.getChild(2).accept(self, **kwargs)
        self.out("(")
        self.out(c2)
        self.out(");")
        if self.current_visited_machine.fun_decls[fn_name].can_raise_event:
            self.out_check_raise_exit(self.current_visited_fn.ret_type)
        self.out("\n")

    # Visit a parse tree produced by pParser#stmt_raise.
    def visitStmt_raise(self, ctx, **kwargs):
        c1 = ctx.getChild(1).accept(self, **kwargs)
        self.out("this.RaiseEvent({0}, null); retcode = Constants.RAISED_EVENT; return;\n".format(c1))

    # Visit a parse tree produced by pParser#stmt_raise_with_arguments.
    def visitStmt_raise_with_arguments(self, ctx, **kwargs):
        c1 = ctx.getChild(1).accept(self, **kwargs)
        c3 = ctx.getChild(3).accept(self, **kwargs)
        self.out("this.RaiseEvent({0}, {1}); retcode = Constants.RAISED_EVENT; return;\n".format(c1, c3))


    # Visit a parse tree produced by pParser#stmt_send.
    def visitStmt_send(self, ctx, **kwargs):
        c2 = ctx.getChild(2).accept(self, **kwargs)
        c4 = ctx.getChild(4).accept(self, **kwargs)
        self.out("this.SendMsg({0},{1},null);\n".format(c2, c4))
        event_type = ctx.getChild(4).exp_type
        if (len(self.pprogram.observes_map) != 0 and not event_type.is_static_event()) \
            or event_type.name in self.pprogram.observes_map:
            self.out("MachineController.AnnounceEvent({0}, null);\n".format(c4))


    # Visit a parse tree produced by pParser#stmt_send_with_arguments.
    def visitStmt_send_with_arguments(self, ctx, **_kwargs):
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = True
        c2 = ctx.getChild(2).accept(self, **kwargs)
        c4 = ctx.getChild(4).accept(self, **kwargs)
        c6 = ctx.getChild(6).accept(self, **kwargs)
        self.out("this.SendMsg({0},{1},{2});\n".format(c2, c4, c6))
        event_type = ctx.getChild(4).exp_type
        if (len(self.pprogram.observes_map) != 0 and not event_type.is_static_event()) \
            or event_type.name in self.pprogram.observes_map:
            self.out("MachineController.AnnounceEvent({0},{1});\n".format(c4, c6))

    # Visit a parse tree produced by pParser#stmt_monitor.
    def visitStmt_monitor(self, ctx, **kwargs):
        c1 = ctx.getChild(1).accept(self, **kwargs)
        self.out("MachineController.AnnounceEvent({0}, null);\n".format(c1))


    # Visit a parse tree produced by pParser#stmt_monitor_with_arguments.
    def visitStmt_monitor_with_arguments(self, ctx, **_kwargs):
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = True
        c1 = ctx.getChild(1).accept(self, **kwargs)
        c3 = ctx.getChild(3).accept(self, **kwargs)
        self.out("MachineController.AnnounceEvent({0},{1});\n".format(c1, c3))

    def visitBinary_Exp(self, ctx, **kwargs):
        if ctx.getChildCount() > 1:
            c0 = ctx.getChild(0).accept(self, **kwargs)
            c2 = ctx.getChild(2).accept(self, **kwargs)
            return "{0} {1} {2}".format(c0, ctx.getChild(1).getText(), c2)
        else:
            return ctx.getChild(0).accept(self, **kwargs)

    def exp_emit_do_copy(self, ctx, out, **kwargs):
        if kwargs.get("do_copy", False) and ctx.exp_type.clonable:
            return out + ".DeepCopy()"
        else:
            return out

    visitExp = visitBinary_Exp
    visitExp_7 = visitBinary_Exp

    def visitExp_6(self, ctx, **kwargs):            
        if ctx.getChildCount() > 1 and not (
                ctx.getChild(0).exp_type == PTypeBool 
            or ctx.getChild(0).exp_type == PTypeInt):
            c0 = ctx.getChild(0).accept(self, **kwargs)
            c2 = ctx.getChild(2).accept(self, **kwargs)
            r = "{0}.PTypeEquals({1})".format(c0, c2)
            if ctx.getChild(1).getText() == "!=":
                r = "!" + r
            return "new PBool({0})".format(r)
        else:
            return self.visitBinary_Exp(ctx, **kwargs)

    def visitExp_5(self, ctx, **kwargs):            
        if ctx.getChildCount() > 1 and ctx.getChild(1).getText() == "in":
            c2 = ctx.getChild(2).accept(self, **kwargs)
            c0 = ctx.getChild(0).accept(self, **kwargs)
            return "{0}.ContainsKey({1})".format(c2, c0)
        else:
            return self.visitBinary_Exp(ctx, **kwargs)
    
    def visitExp_4(self, ctx, **kwargs):
        if ctx.getChildCount() > 1:
            ret_type = self.translate_type(ctx.exp_type)
            c0 = ctx.getChild(0).accept(self, **kwargs)
            if ctx.getChild(0).exp_type == PTypeAny:
                c0 = self.exp_emit_do_copy(ctx, c0)
            return "(({0}) {1})".format(ret_type, c0)
        else:
            return ctx.getChild(0).accept(self, **kwargs)

    visitExp_3 = visitBinary_Exp
    visitExp_2 = visitBinary_Exp

    # Visit a parse tree produced by pParser#exp_1.
    def visitExp_1(self, ctx, **kwargs):
        if ctx.getChildCount() > 1:
            op = ctx.getChild(0).getText()
            c1 = ctx.getChild(1).accept(self, **kwargs)
            return "{op}{c1}".format(op=op, c1=c1)
        else:
            return ctx.getChild(0).accept(self, **kwargs)


    # Visit a parse tree produced by pParser#exp_getidx.
    def visitExp_getidx(self, ctx, **_kwargs):
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = False
        c0 = ctx.getChild(0).accept(self, **kwargs)
        idx = int(ctx.getChild(2).getText())
        return self.exp_emit_do_copy(ctx, "{0}.Item{1}".format(c0, idx + 1), **_kwargs)

    # Visit a parse tree produced by pParser#exp_sizeof.
    def visitExp_sizeof(self, ctx, **_kwargs):
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = False
        c2 = ctx.getChild(2).accept(self, **kwargs)
        return "{0}.Count".format(c2)

    # Visit a parse tree produced by pParser#exp_call.
    def visitExp_call(self, ctx, **kwargs):
        fn_name = ctx.getChild(0).getText()
        if self.current_visited_machine.fun_decls[fn_name].can_raise_event:
            exp_ref = self.allocate_local_var()
            self.out("{T} {ref}={fn_name}();".format(T=self.translate_type(ctx.exp_type), ref=exp_ref, fn_name=fn_name))
            self.out_check_raise_exit(self.current_visited_fn.ret_type)
            return exp_ref
        else:
            return "{fn_name}()".format(fn_name=fn_name)

    # Visit a parse tree produced by pParser#exp_new.
    def visitExp_new(self, ctx, **kwargs):
        return "NewMachine(new Machine{}(), null)".format(ctx.getChild(1).getText())

    # Visit a parse tree produced by pParser#exp_call_with_arguments.
    def visitExp_call_with_arguments(self, ctx, **_kwargs):
        fn_name = ctx.getChild(0).getText()
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = True
        c2 = ctx.getChild(2).accept(self, **kwargs)
        if self.current_visited_machine.fun_decls[fn_name].can_raise_event:
            exp_ref = self.allocate_local_var()
            self.out("{T} {ref}={fn_name}({args});".format(T=self.translate_type(ctx.exp_type), 
                                                        ref=exp_ref, 
                                                        fn_name=fn_name,
                                                        args=c2)
                    )
            self.out_check_raise_exit(self.current_visited_fn.ret_type)
            return exp_ref
        else:
            return "{fn_name}({args})".format(fn_name=fn_name, args=c2)

    # Visit a parse tree produced by pParser#exp_nondet.
    def visitExp_nondet(self, ctx, **kwargs):
        return "RandomBool()"

    # Visit a parse tree produced by pParser#exp_this.
    def visitExp_this(self, ctx, **kwargs):
        return "this"

    # Visit a parse tree produced by pParser#exp_id.
    def visitExp_id(self, ctx, **kwargs):
        identifier = ctx.getChild(0).getText()
        if type(ctx.exp_type) is PTypeEvent:
            if identifier in self.current_visited_fn.local_decls or identifier in self.current_visited_fn.params or identifier in self.current_visited_machine.var_decls:
                return identifier
            else:
                return self.translate_event(identifier)
        else:
            return self.exp_emit_do_copy(ctx, identifier, **kwargs)

    # Visit a parse tree produced by pParser#exp_getattr.
    def visitExp_getattr(self, ctx, **_kwargs):
        named_tuple_type = ctx.getChild(0).exp_type
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = False
        c0 = ctx.getChild(0).accept(self, **kwargs)
        attr = ctx.getChild(2).getText()
        idx = named_tuple_type.NTs.keys().index(attr) + 1
        return self.exp_emit_do_copy(ctx, "{0}.Item{1}".format(c0, idx), **_kwargs)

    # Visit a parse tree produced by pParser#exp_named_tuple_1_elem.
    def visitExp_named_tuple_1_elem(self, ctx, **_kwargs):
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = True
        c3 = ctx.getChild(3).accept(self, **kwargs)
        nmd_tuple_type = ctx.exp_type
        return "new {0}({1})".format(self.translate_type(nmd_tuple_type), c3)

    # Visit a parse tree produced by pParser#exp_keys.
    def visitExp_keys(self, ctx, **_kwargs):
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = False
        c2 = ctx.getChild(2).accept(self, **kwargs)
        return self.exp_emit_do_copy(ctx, "{0}.Keys()".format(c2), **_kwargs)

    # Visit a parse tree produced by pParser#exp_grouped.
    def visitExp_grouped(self, ctx, **kwargs):
        c1 = ctx.getChild(1).accept(self, **kwargs)
        return "({0})".format(c1)
        
    # Visit a parse tree produced by pParser#exp_named_tuple_n_elems.
    def visitExp_named_tuple_n_elems(self, ctx, **_kwargs):
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = True
        c3 = ctx.getChild(3).accept(self, **kwargs)
        c5 = ctx.getChild(5).accept(self, **kwargs)
        nmd_tuple_type = ctx.exp_type
        return "new {0}({1}, {2})".format(self.translate_type(nmd_tuple_type), c3, c5)

    # Visit a parse tree produced by pParser#exp_true.
    def visitExp_true(self, ctx, **kwargs):
        return "true"

    # Visit a parse tree produced by pParser#exp_values.
    def visitExp_values(self, ctx, **_kwargs):
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = False
        c2 = ctx.getChild(2).accept(self, **kwargs)
        return self.exp_emit_do_copy(ctx, "{0}.Values()".format(c2), **_kwargs)

    # Visit a parse tree produced by pParser#exp_default.
    def visitExp_default(self, ctx, **kwargs):
        return self.translate_default_exp(ctx.exp_type)

    # Visit a parse tree produced by pParser#exp_null.
    def visitExp_null(self, ctx, **kwargs):
        return "null"

    # Visit a parse tree produced by pParser#exp_new_with_arguments.
    def visitExp_new_with_arguments(self, ctx, **_kwargs):
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = True
        c3 = ctx.getChild(3).accept(self, **kwargs)
        return "NewMachine(new Machine{0}(), {1})".format(ctx.getChild(1).getText(), c3)

    # Visit a parse tree produced by pParser#exp_false.
    def visitExp_false(self, ctx, **kwargs):
        return "false"

    # Visit a parse tree produced by pParser#exp_getitem.
    def visitExp_getitem(self, ctx, **_kwargs):
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = False
        c0 = ctx.getChild(0).accept(self, **kwargs)
        c2 = ctx.getChild(2).accept(self, **kwargs)
        return self.exp_emit_do_copy(ctx, "{0}[{1}]".format(c0, c2), **_kwargs)

    # Visit a parse tree produced by pParser#exp_fairnondet.
    def visitExp_fairnondet(self, ctx, **kwargs):
        return "RandomBool()"

    # Visit a parse tree produced by pParser#exp_tuple_1_elem.
    def visitExp_tuple_1_elem(self, ctx, **_kwargs):
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = True
        c1 = ctx.getChild(1).accept(self, **kwargs)
        tuple_type = ctx.exp_type
        return "new {0}({1})".format(self.translate_type(tuple_type), c1)

    # Visit a parse tree produced by pParser#exp_int.
    def visitExp_int(self, ctx, **kwargs):
        return "new PInteger({0})".format(ctx.getChild(0).getText())

    # Visit a parse tree produced by pParser#exp_tuple_n_elems.
    def visitExp_tuple_n_elems(self, ctx, **_kwargs):
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = True
        c1 = ctx.getChild(1).accept(self, **kwargs)
        c3 = ctx.getChild(3).accept(self, **kwargs)
        tuple_type = ctx.exp_type
        return "new {0}({1},{2})".format(self.translate_type(tuple_type), c1, c3)

    # Visit a parse tree produced by pParser#single_expr_arg_list.
    def visitSingle_expr_arg_list(self, ctx, **kwargs):
        if ctx.getChildCount() == 1:
            return ctx.getChild(0).accept(self, **kwargs)
        else:
            c0 = ctx.getChild(0).accept(self, **kwargs)
            c3 = ctx.getChild(3).accept(self, **kwargs)
            if isinstance(ctx.exp_type, list):
                return "{0},{1}".format(c0, c3) 
            else:
                return "new {0}({1},{2})".format(self.translate_type(ctx.exp_type), c0, c3)

    # Visit a parse tree produced by pParser#expr_arg_list.
    def visitExpr_arg_list(self, ctx, **kwargs):
        if ctx.getChildCount() == 2:
            return ctx.getChild(0).accept(self, **kwargs)
        else:
            c0 = ctx.getChild(0).accept(self, **kwargs)
            c3 = ctx.getChild(3).accept(self, **kwargs)
            return "{0},{1}".format(c0, c3)

    # Visit a parse tree produced by pParser#nmd_expr_arg_list.
    def visitNmd_expr_arg_list(self, ctx, **kwargs):
        if ctx.getChildCount() == 3:
            return ctx.getChild(2).accept(self, **kwargs)
        else:
            c2 = ctx.getChild(2).accept(self, **kwargs)
            c4 = ctx.getChild(4).accept(self, **kwargs)
            return "{0},{1}".format(c2, c4)

Translator = PProgramToCSharpTranslator

