import os
from ..ast_to_pprogram import *
from ..symbolic_csharp_translator import PProgramToSymbolicCSharpTranslator

class PProgramToMultSECSharpTranslator(PProgramToSymbolicCSharpTranslator):
    def __init__(self, *args):
        super(PProgramToSymbolicCSharpTranslator, self).__init__(*args)
        self.runtime_dir = os.environ.get("RUNTIME_DIR", os.path.realpath(os.path.dirname(__file__) + "/../../runtimes/multise_csharp"))

    def translate_type(self, T, outer=True):
        t = None
        if isinstance(T, str):
            t = T
        elif T in self.type_to_csharp_type_name_map:
            t = self.type_to_csharp_type_name_map[T]
        elif isinstance(T, PTypeSeq):
            t = "PList<{0}>".format(self.translate_type(T.T, outer=False))
            self.type_to_csharp_type_name_map[T] = t
        elif isinstance(T, PTypeMap):
            t = "PMap<{0},{1}>".format(self.translate_type(T.T1, outer=False), self.translate_type(T.T2, outer=False))
            self.type_to_csharp_type_name_map[T] = t
        elif isinstance(T, PTypeTuple):
            t = "PTuple<{0}>".format(','.join([self.translate_type(x, outer=False) for x in T.Ts],))
            self.type_to_csharp_type_name_map[T] = t
        elif isinstance(T, PTypeNamedTuple):
            t = "PTuple<{0}>".format(','.join([self.translate_type(x, outer=False) for x in T.NTs.values()]))
            self.type_to_csharp_type_name_map[T] = t
        if T and outer:
            return "ValueSummary<{0}>".format(t)
        else:
            return t

    def out_fn_decl(self, machine, fn_name):
        fn_node = machine.fun_decls[fn_name]
        ret_type = fn_node.ret_type
        if fn_node.is_transition_handler:
            self.out("private {0} {1} ({2} self, {3} _payload) {{\n".format(self.translate_type(ret_type), fn_name, 
                self.translate_type("Machine" + machine.name), self.translate_type(PTypeAny)))
            self.out_arg_cast_for_function(machine, fn_name)
        else:
            self.out("private {0} {1} ({2} self{3}) {{\n".format(self.translate_type(ret_type), fn_name, 
                    self.translate_type("Machine" + machine.name),
                    "".join((", {} {}".format(self.translate_type(t), i)) for i,t in fn_node.params.items())))
        if fn_node.from_to_state:
            from_state, to_state = fn_node.from_to_state
            self.out_exit_state(machine, machine.state_decls[from_state], last_stmt=False, ret_type=ret_type)
            self.out_fn_body(machine, fn_name)
            self.out_enter_state(machine, machine.state_decls[to_state], last_stmt=True)
        else:    
            self.out_fn_body(machine, fn_name)
        self.out("}\n")

    # Visit a parse tree produced by pParser#exp_this.
    def visitExp_this(self, ctx, **kwargs):
        return "this"

    # Visit a parse tree produced by pParser#exp_id.
    def visitExp_id(self, ctx, **kwargs):
        identifier = ctx.getChild(0).getText()
        if identifier in self.current_visited_fn.local_decls:
            return self.exp_emit_do_copy(ctx, ctx.getChild(0).getText(), **kwargs)
        elif identifier in self.current_visited_fn.params:
            return self.exp_emit_do_copy(ctx, ctx.getChild(0).getText(), **kwargs)
        elif identifier in self.current_visited_machine.var_decls:
            return self.exp_emit_do_copy(ctx, "self.GetField({0})".format(ctx.getChild(0).getText()), **kwargs)
        elif identifier in self.pprogram.events:
            return self.translate_event(ctx.getChild(0).getText())

Translator = PProgramToMultSECSharpTranslator
