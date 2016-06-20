from __future__ import print_function
from .ast_to_pprogram import *
from .type_annotator import *
from .call_graph_annotator import *
from StringIO import StringIO

class TranslatorBase(PTypeTranslatorVisitor):

    def __init__(self, ast, out_dir):
        super(TranslatorBase, self).__init__()
        visitor = AntlrTreeToPProgramVisitor()
        self.pprogram = ast.accept(visitor)
        PProgramTypeAnnotator(self.pprogram).annotate_types()
        PProgramCallGraphASTAnnotator(self.pprogram).annotate_with_call_graph()
        self.out_dir = out_dir

    def warning(self, msg, ctx):
        print("Warning: {}".format(msg), file=sys.stderr)

    def out(self, s):
    	self.stream.write(s)

