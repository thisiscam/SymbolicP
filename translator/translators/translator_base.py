from .ast_to_pprogram import *

class TranslatorBase(PTypeTranslatorVisitor):

    def __init__(self, ast, out_dir):
        super(TranslatorBase, self).__init__()
        visitor = AntlrTreeToPProgramVisitor()
        self.pprogram = ast.accept(visitor)
        self.out_dir = out_dir

    def warning(self, msg, ctx):
        warnings.warn("{}@line{}~{}".format(msg, 
                    *ctx.getChild(0).getSourceInterval())
                )

    def acquire_buffer(self):
        r = self.stream
        self.stream = StringIO()
        return r

    def release_buffer(self, old_stream):
        old_stream.write(self.stream.getvalue())
        self.stream.close()
        self.stream = old_stream

    def out(self, s):
    	self.stream.write(s)

