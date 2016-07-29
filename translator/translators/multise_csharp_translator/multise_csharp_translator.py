import os, glob
from ..ast_to_pprogram import *
from ..symbolic_csharp_translator import PProgramToSymbolicCSharpTranslator
from .valuesummary_translator import valuesummary_transform

class PProgramToMultSECSharpTranslator(PProgramToSymbolicCSharpTranslator):
    runtime_dir = os.environ.get("RUNTIME_DIR", os.path.realpath(os.path.dirname(__file__) + "/../../runtimes/multise_csharp"))
    
    def __init__(self, *args):
        super(PProgramToSymbolicCSharpTranslator, self).__init__(*args)

    def multise_transform(self):
        runtime_srcs =  [os.path.realpath(src) for src in glob.glob("{0}/*.cs".format(PProgramToSymbolicCSharpTranslator.runtime_dir))]
        generated_srcs = [os.path.realpath(src) for src in glob.glob("{0}/*.cs".format(self.out_dir))]
        no_copy_srcs = runtime_srcs
        ignore_srcs = {
                    "SymbolicBool.cs", 
                    "SymbolicInteger.cs", 
                    "SymbolicEngine.cs", 
                    "PathConstraint.cs",
                    "PInteger.cs",
                    "Program.cs",
                    "Scheduler.cs",
                    "PBool.cs",
                }
        all_srcs = runtime_srcs + generated_srcs
        transform_srcs = filter(lambda s: os.path.basename(s) not in ignore_srcs, all_srcs)
        valuesummary_transform(all_srcs, self.out_dir, transform_srcs, no_copy_srcs)

    pipline = [
                PProgramToSymbolicCSharpTranslator.make_output_dir, 
                PProgramToSymbolicCSharpTranslator.create_proj_macros, 
                PProgramToSymbolicCSharpTranslator.generate_foreach_machine,
                multise_transform,
                PProgramToSymbolicCSharpTranslator.create_csproj
            ]

    # def translate_type(self, T, outer=True):
    #     t = None
    #     if isinstance(T, str):
    #         t = T
    #     elif T in self.type_to_csharp_type_name_map:
    #         t = self.type_to_csharp_type_name_map[T]
    #     elif isinstance(T, PTypeSeq):
    #         t = "PList<{0}>".format(self.translate_type(T.T, outer=False))
    #         self.type_to_csharp_type_name_map[T] = t
    #     elif isinstance(T, PTypeMap):
    #         t = "PMap<{0},{1}>".format(self.translate_type(T.T1, outer=False), self.translate_type(T.T2, outer=False))
    #         self.type_to_csharp_type_name_map[T] = t
    #     elif isinstance(T, PTypeTuple):
    #         t = "PTuple<{0}>".format(','.join([self.translate_type(x, outer=False) for x in T.Ts],))
    #         self.type_to_csharp_type_name_map[T] = t
    #     elif isinstance(T, PTypeNamedTuple):
    #         t = "PTuple<{0}>".format(','.join([self.translate_type(x, outer=False) for x in T.NTs.values()]))
    #         self.type_to_csharp_type_name_map[T] = t
    #     if T and outer:
    #         return "ValueSummary<{0}>".format(t)
    #     else:
    #         return t

    # def out_fn_decl(self, machine, fn_name):
    #     fn_node = machine.fun_decls[fn_name]
    #     ret_type = fn_node.ret_type
    #     if fn_node.is_transition_handler:
    #         self.out("private {0} {1} ({2} self, {3} _payload) {{\n".format(self.translate_type(ret_type), fn_name, 
    #             self.translate_type("Machine" + machine.name), self.translate_type(PTypeAny)))
    #         self.out_arg_cast_for_function(machine, fn_name)
    #     else:
    #         self.out("private {0} {1} ({2} self{3}) {{\n".format(self.translate_type(ret_type), fn_name, 
    #                 self.translate_type("Machine" + machine.name),
    #                 "".join((", {} {}".format(self.translate_type(t), i)) for i,t in fn_node.params.items())))
    #     if fn_node.from_to_state:
    #         from_state, to_state = fn_node.from_to_state
    #         self.out_exit_state(machine, machine.state_decls[from_state], last_stmt=False, ret_type=ret_type)
    #         self.out_fn_body(machine, fn_name)
    #         self.out_enter_state(machine, machine.state_decls[to_state], last_stmt=True)
    #     else:    
    #         self.out_fn_body(machine, fn_name)
    #     self.out("}\n")

    # # Visit a parse tree produced by pParser#exp_this.
    # def visitExp_this(self, ctx, **kwargs):
    #     return "this"

    # # Visit a parse tree produced by pParser#exp_id.
    # def visitExp_id(self, ctx, **kwargs):
    #     identifier = ctx.getChild(0).getText()
    #     if identifier in self.current_visited_fn.local_decls:
    #         return self.exp_emit_do_copy(ctx, ctx.getChild(0).getText(), **kwargs)
    #     elif identifier in self.current_visited_fn.params:
    #         return self.exp_emit_do_copy(ctx, ctx.getChild(0).getText(), **kwargs)
    #     elif identifier in self.current_visited_machine.var_decls:
    #         return self.exp_emit_do_copy(ctx, "self.GetField({0})".format(ctx.getChild(0).getText()), **kwargs)
    #     elif identifier in self.pprogram.events:
    #         return self.translate_event(ctx.getChild(0).getText())

Translator = PProgramToMultSECSharpTranslator
