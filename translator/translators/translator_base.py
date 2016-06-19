from __future__ import print_function
from .ast_to_pprogram import *
from StringIO import StringIO

class TranslatorBase(PTypeTranslatorVisitor):

    def __init__(self, ast, out_dir):
        super(TranslatorBase, self).__init__()
        visitor = AntlrTreeToPProgramVisitor()
        self.pprogram = ast.accept(visitor)
        self.out_dir = out_dir

    def warning(self, msg, ctx):
        print("Warning: {}".format(msg), file=sys.stderr)

    def acquire_buffer(self):
        r = self.stream
        self.stream = StringIO()
        return r

    def release_buffer(self, old_stream):
        tmp_buffer = self.stream
        self.stream = old_stream
        return tmp_buffer

    def dump_buffer(self, tmp_buffer):
        self.stream.write(tmp_buffer.getvalue())
        tmp_buffer.close()

    def out(self, s):
    	self.stream.write(s)

