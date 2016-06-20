from .ast_to_pprogram import *

class PProgramTypeAnnotator(PTypeTranslatorVisitor):
    def __init__(self, pprogram, *args):
        super(PProgramTypeAnnotator, self).__init__(*args)
        self.pprogram = pprogram

    def annotate_types(self):
        for machine in self.pprogram.machines:
            self.current_visited_machine = machine
            for fn_name in machine.fun_decls:
                self.annotate_fn_body(machine, fn_name)

    def annotate_fn_body(self, machine, fn_name):
        fn = machine.fun_decls[fn_name]
        self.current_visited_fn = fn
        fn.stmt_block.accept(self)

    def visitBinary_Exp(ret_type):
        def wrapped(self, ctx):
            if ctx.getChildCount() > 1:
                self.visitChildren(ctx)
                ctx.exp_type = ret_type
            else:
                ctx.exp_type = ctx.getChild(0).accept(self)
            return ctx.exp_type
        return wrapped

    visitExp = visitBinary_Exp(PTypeBool)
    visitExp_7 = visitBinary_Exp(PTypeBool)
    visitExp_6 = visitBinary_Exp(PTypeBool)
    visitExp_5 = visitBinary_Exp(PTypeBool)

    
    def visitExp_4(self, ctx):
        if ctx.getChildCount() > 1:
            ret_type = ctx.getChild(2).accept(self)
            ctx.getChild(0).accept(self)
            ctx.exp_type = ret_type
        else:
            ctx.exp_type = ctx.getChild(0).accept(self)
        return ctx.exp_type

    visitExp_3 = visitBinary_Exp(PTypeInt)
    visitExp_2 = visitBinary_Exp(PTypeInt)

    # Visit a parse tree produced by pParser#exp_1.
    def visitExp_1(self, ctx):
        if ctx.getChildCount() > 1:
            op = ctx.getChild(0).getText()
            ctx.getChild(1).accept(self)
            ctx.exp_type = PTypeInt if op == "-" else PTypeBool
        else:
            ctx.exp_type = ctx.getChild(0).accept(self)
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_getidx.
    def visitExp_getidx(self, ctx):
        tuple_type = ctx.getChild(0).accept(self)
        idx = int(ctx.getChild(2).getText())
        ctx.exp_type = tuple_type.Ts[idx]
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_sizeof.
    def visitExp_sizeof(self, ctx):
        ctx.getChild(2).accept(self)
        ctx.exp_type = PTypeInt
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_call.
    def visitExp_call(self, ctx):
        fn_name = ctx.getChild(0).getText()
        ctx.exp_type = self.current_visited_machine.fun_decls[fn_name].ret_type
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_new.
    def visitExp_new(self, ctx):
        ctx.exp_type = PTypeMachine
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_call_with_arguments.
    def visitExp_call_with_arguments(self, ctx):
        fn_name = ctx.getChild(0).getText()
        ctx.getChild(2).accept(self)
        ctx.exp_type = self.current_visited_machine[fn_name].ret_type
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_nondet.
    def visitExp_nondet(self, ctx):
        ctx.exp_type = PTypeBool
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_this.
    def visitExp_this(self, ctx):
        ctx.exp_type = PTypeMachine
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_id.
    def visitExp_id(self, ctx):
        identifier = ctx.getChild(0).getText()
        if identifier in self.current_visited_fn.local_decls:
            ctx.exp_type = self.current_visited_fn.local_decls[identifier]
        elif identifier in self.current_visited_fn.params:
            ctx.exp_type = self.current_visited_fn.params[identifier]
        elif identifier in self.current_visited_machine.var_decls:
            ctx.exp_type = self.current_visited_machine.var_decls[identifier]
        elif identifier in self.pprogram.events:
            ctx.exp_type = PTypeEvent
        else:
            self.warning("Cannot find identifier {0}".format(identifier), ctx)
            ctx.exp_type = PTypeAny
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_getattr.
    def visitExp_getattr(self, ctx):
        named_tuple_type = ctx.getChild(0).accept(self)
        attr = ctx.getChild(2).getText()
        idx = named_tuple_type.NTs.keys().index(attr) + 1
        ctx.exp_type = named_tuple_type.NTs[attr]
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_named_tuple_1_elem.
    def visitExp_named_tuple_1_elem(self, ctx):
        n1 = ctx.getChild(1).getText()
        t1 = ctx.getChild(3).accept(self)
        nmd_tuple_type = PTypeNamedTuple([(n1, t1)])
        ctx.exp_type = nmd_tuple_type
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_keys.
    def visitExp_keys(self, ctx):
        map_type = ctx.getChild(2).accept(self)
        ctx.exp_type = map_type.T1
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_grouped.
    def visitExp_grouped(self, ctx):
        ctx.exp_type = ctx.getChild(1).accept(self)
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_named_tuple_n_elems.
    def visitExp_named_tuple_n_elems(self, ctx):
        n1 = ctx.getChild(1).getText()
        t1 = ctx.getChild(3).accept(self)
        trest = ctx.getChild(5).accept(self)
        nmd_tuple_type = PTypeNamedTuple([(n1, t1)] + trest)
        ctx.exp_type = nmd_tuple_type
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_true.
    def visitExp_true(self, ctx):
        ctx.exp_type = PTypeBool
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_values.
    def visitExp_values(self, ctx):
        map_type = ctx.getChild(2).accept(self)
        ctx.exp_type = map_type.T1
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_default.
    def visitExp_default(self, ctx):
        ctx.exp_type = ctx.getChild(2).accept(self)
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_null.
    def visitExp_null(self, ctx):
        ctx.exp_type = PTypeAny
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_new_with_arguments.
    def visitExp_new_with_arguments(self, ctx):
        self.visitChildren(ctx)
        ctx.exp_type = PTypeMachine
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_false.
    def visitExp_false(self, ctx):
        ctx.exp_type =  PTypeBool
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_halt.
    def visitExp_halt(self, ctx):
        ctx.exp_type =  PTypeEvent
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_getitem.
    def visitExp_getitem(self, ctx):
        map_or_seq_type = ctx.getChild(0).accept(self)
        ctx.getChild(2).accept(self)
        if isinstance(map_or_seq_type, PTypeMap):
            ctx.exp_type =  map_or_seq_type.T2
        elif isinstance(map_or_seq_type, PTypeSeq):
            ctx.exp_type =  map_or_seq_type.T
        else:
            self.warning("Failed to infer type", ctx)
            ctx.exp_type = PTypeAny
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_fairnondet.
    def visitExp_fairnondet(self, ctx):
        ctx.exp_type = PTypeBool
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_tuple_1_elem.
    def visitExp_tuple_1_elem(self, ctx):
        t = ctx.getChild(1).accept(self)
        tuple_type = PTypeTuple([t])
        ctx.exp_type = tuple_type
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_int.
    def visitExp_int(self, ctx):
        ctx.exp_type = PTypeInt
        return ctx.exp_type

    # Visit a parse tree produced by pParser#exp_tuple_n_elems.
    def visitExp_tuple_n_elems(self, ctx):
        t = ctx.getChild(1).accept(self)
        trest = ctx.getChild(3).accept(self)
        tuple_type = PTypeTuple([t] + trest)
        ctx.exp_type = tuple_type
        return ctx.exp_type

    # Visit a parse tree produced by pParser#single_expr_arg_list.
    def visitSingle_expr_arg_list(self, ctx):
        if ctx.getChildCount() == 1:
            ctx.exp_type = ctx.getChild(0).accept(self)
        else:
            t1 = ctx.getChild(0).accept(self)
            trest = ctx.getChild(3).accept(self)
            if isinstance(trest, PTypeTuple):
                ctx.getChild(3).exp_type = list(trest.Ts)
                ctx.exp_type = PTypeTuple([t1] + trest.Ts)
            else:
                ctx.exp_type = PTypeTuple([t1, trest])
        return ctx.exp_type

    # Visit a parse tree produced by pParser#expr_arg_list.
    def visitExpr_arg_list(self, ctx):
        if ctx.getChildCount() == 2:
            ctx.exp_type = [ctx.getChild(0).accept(self)]
        else:
            t1 = ctx.getChild(0).accept(self)
            trest = ctx.getChild(3).accept(self)
            ctx.exp_type = [t1] + trest
        return ctx.exp_type

    # Visit a parse tree produced by pParser#nmd_expr_arg_list.
    def visitNmd_expr_arg_list(self, ctx):
        if ctx.getChildCount() == 3:
            ctx.exp_type = [(ctx.getChild(0).getText(), ctx.getChild(2).accept(self))]
        else:
            id1 = ctx.getChild(0).getText()
            t1 = ctx.getChild(2).accept(self)
            trest = ctx.getChild(4).accept(self)
            ctx.exp_type = [(id1, t1)] + trest
        return ctx.exp_type
