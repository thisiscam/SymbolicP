from .ast_to_pprogram import *
from collections import defaultdict

class PProgramCallGraphASTAnnotator(PTypeTranslatorVisitor):
    def __init__(self, pprogram, *args):
        super(PProgramCallGraphASTAnnotator, self).__init__(*args)
        self.pprogram = pprogram
        self.call_graph = defaultdict(list)
        self.raise_event_fns = []

    def annotate_with_call_graph(self):
        for machine in self.pprogram.machines:
            self.current_visited_machine = machine
            for fn_name in machine.fun_decls:
                self.build_call_graph_for_fn(machine, fn_name)

        # annotate function decls that can raise an event
        while len(self.raise_event_fns) != 0:
            f = self.raise_event_fns.pop()
            if not f.can_raise_event:
                f.can_raise_event = True
                self.raise_event_fns += self.call_graph[f]

    def build_call_graph_for_fn(self, machine, fn_name):
        fn = machine.fun_decls[fn_name]
        fn.can_raise_event = False
        self.current_visited_fn = fn
        fn.stmt_block.accept(self)

    # Visit a parse tree produced by pParser#stmt_call.
    def visitStmt_call(self, ctx):
        self.call_graph[self.current_visited_machine.fun_decls[ctx.getChild(0).getText()]].append(self.current_visited_fn)
        self.visitChildren(ctx)
    
    # Visit a parse tree produced by pParser#stmt_call_with_arguments.
    def visitStmt_call_with_arguments(self, ctx):
        self.call_graph[self.current_visited_machine.fun_decls[ctx.getChild(0).getText()]].append(self.current_visited_fn)
        self.visitChildren(ctx)
    
    # Visit a parse tree produced by pParser#exp_call.
    def visitExp_call(self, ctx):
        self.call_graph[self.current_visited_machine.fun_decls[ctx.getChild(0).getText()]].append(self.current_visited_fn)
        self.visitChildren(ctx)

    # Visit a parse tree produced by pParser#exp_call_with_arguments.
    def visitExp_call_with_arguments(self, ctx):
        self.call_graph[self.current_visited_machine.fun_decls[ctx.getChild(0).getText()]].append(self.current_visited_fn)
        self.visitChildren(ctx)

    # Visit a parse tree produced by pParser#stmt_raise.
    def visitStmt_raise(self, ctx):
        self.raise_event_fns.append(self.current_visited_fn)

    # Visit a parse tree produced by pParser#stmt_raise_with_arguments.
    def visitStmt_raise_with_arguments(self, ctx):
        self.raise_event_fns.append(self.current_visited_fn)

    # Visit a parse tree produced by pParser#stmt_pop.
    def visitStmt_pop(self, ctx):
        self.raise_event_fns.append(self.current_visited_fn)

