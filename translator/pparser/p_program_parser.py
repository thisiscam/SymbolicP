from tempfile import TemporaryFile
import subprocess, os
from .pVisitor import pVisitor
from antlr4 import *
from pLexer import pLexer
from pParser import pParser
from ordered_set import OrderedSet
from antlr4.tree.Tree import TerminalNode, TerminalNodeImpl

# class AntlrASTNode(object):
#     is_token = False

# class AntlrSExpCtx(AntlrASTNode):
#     def __init__(self, name, children):
#         self.name = name
#         self.children = children

#         self.visitor_invoke_name = "visit" + self.name

#     def getChildCount(self):
#         return len(self.children)

#     def getChild(self, i):
#         return self.children[i]

#     def accept(self, visitor, *args, **kwargs):
#         if hasattr(visitor, self.visitor_invoke_name):
#             return getattr(visitor, self.visitor_invoke_name)(self, *args, **kwargs)
#         else:
#             print("Warning: {0} not invoked".format(self.visitor_invoke_name))

#     def __repr__(self):
#         return "{0}{1}".format(self.name, self.children)

# class Token(AntlrASTNode):
#     is_token = True
#     def __init__(self, txt):
#         self.getText() = txt

#     def text(self):
#         return self.getText()

#     def accept(self, visitor):
#         return

#     def __repr__(self):
#         return "`{0}`".format(self.getText())

def creat_token(text):
    token = Token()
    token._text = text
    return TerminalNodeImpl(token) 

def peek1(s):
    c = s.read(1)
    s.seek(s.tell() - 1)
    return c

def to_tree(s):
    ret = parse_exp(s)
    return ret

def parse_exp(s):
    c = peek1(s)
    if c == "(":
        c = s.read(1)
        if peek1(s) in (")", " "):
            return creat_token(c)
        else:
            name = parse_tid(s) + "Context"
            children = []
            while peek1(s) == " ":
                s.read(1)
                children.append(parse_exp(s))
            if s.read(1) != ")":
                raise ValueError("Invalid s-expression")
            node_type = getattr(pParser, name)
            new_node = node_type.__new__(node_type)
            new_node.children = children
            return new_node
    else:
        return creat_token(parse_id(s))

def parse_id(s):
    if peek1(s) in ("(", ")"):
        return s.read(1)
    else:
        return parse_tid(s)

def parse_tid(s):
    r = ""
    while True:
        c = s.read(1)
        if c in (" ", "(", ")"):
            s.seek(s.tell() - 1)
            break
        r += c
    return r

if __name__ == "__main__":
    from StringIO import StringIO
    s = StringIO("(State_decl (Is_start_state_or_none start) (Is_hot_or_cold_or_none) state Init (State_annot_or_none) { (State_body (State_body_item_entry_unnamed entry (Payload_var_decl_or_none) { (Stmt_block (Local_var_decl_list) (Stmt_list (Stmt_assign (Exp (Exp_7 (Exp_6 (Exp_5 (Exp_4 (Exp_3 (Exp_2 (Exp_1 (Exp_id TimerV))))))))) = (Exp (Exp_7 (Exp_6 (Exp_5 (Exp_4 (Exp_3 (Exp_2 (Exp_1 (Exp_new_with_arguments new Timer ( (Single_expr_arg_list (Exp (Exp_7 (Exp_6 (Exp_5 (Exp_4 (Exp_3 (Exp_2 (Exp_1 (Exp_this this)))))))))) )))))))))) ;) (Stmt_list (Stmt_assign (Exp (Exp_7 (Exp_6 (Exp_5 (Exp_4 (Exp_3 (Exp_2 (Exp_1 (Exp_id DoorV))))))))) = (Exp (Exp_7 (Exp_6 (Exp_5 (Exp_4 (Exp_3 (Exp_2 (Exp_1 (Exp_new_with_arguments new Door ( (Single_expr_arg_list (Exp (Exp_7 (Exp_6 (Exp_5 (Exp_4 (Exp_3 (Exp_2 (Exp_1 (Exp_this this)))))))))) )))))))))) ;) (Stmt_list (Stmt_raise raise (Exp (Exp_7 (Exp_6 (Exp_5 (Exp_4 (Exp_3 (Exp_2 (Exp_1 (Exp_id eUnit))))))))) ;))))) }) (State_body (State_body_item_on_e_goto (On_event_list on (Event_list (Event_id eUnit))) goto (State_target DoorClosed) (Trig_annot_or_none) ;))) })")
    t = parse_exp(s)

class IncludeFileHandlerVisitor(pVisitor):
    def __init__(self, parser):
        self.parser = parser

    # Visit a parse tree produced by pParser#program.
    def visitProgram(self, ctx):
        c1 = ctx.getChild(-1).accept(self)
        if c1 == None:
            c1 = Token("EOF")
        ctx.children[-1] = c1
        return c1

    # Visit a parse tree produced by pParser#top_decl_list.
    def visitTop_decl_list(self, ctx):
        if ctx.getChildCount() == 1:
            c0 = ctx.getChild(0).accept(self)
            if c0 == None:
                return None
            elif isinstance(c0, pParser.Top_decl_listContext):
                return c0
            else:
                ctx.children[0] = c0
                return ctx
        else:
            c0 = ctx.getChild(0).accept(self)
            c1 = ctx.getChild(1).accept(self)
            if c1 == None:
                return c0
            elif c0 == None:
                return c1
            elif isinstance(c1, pParser.Top_decl_listContext):
                i = c1
                while i.getChildCount() > 1:
                    i = i.getChild(0)
                i.children.insert(0, c0)
                return c1
            else:
                ctx.children[1] = c1
                ctx.children[0] = c0
                return ctx

    # Visit a parse tree produced by pParser#top_decl.
    def visitTop_decl(self, ctx):
        import pdb; pdb.set_trace()
        if isinstance(ctx.getChild(0), pParser.Include_declContext):
            return ctx.getChild(0).accept(self)
        else:
            return ctx

    # Visit a parse tree produced by pParser#include_decl.
    def visitInclude_decl(self, ctx):
        include_file_name = ctx.getChild(1).getText()[1:-1]
        potential_file_path = self.parser.search_file(include_file_name)
        if potential_file_path:
            potential_file_path = os.path.abspath(potential_file_path)
            if potential_file_path in self.parser.included_files:
                return None
            self.parser.included_files.add(potential_file_path)
            ast = self.parser.parse(potential_file_path)
            if ast.getChildCount() == 1:
                if isinstance(ast.children[0], TerminalNode) or isinstance(ast.children[0], pParser.Annotation_setContext):
                    return None
                else:
                    return ast.getChild(-1) # return a top_decl_list for now, annotation set is currently ignored
        else:
            raise Exception("{0} not found".format(include_file_name))

class pProgramParser(object):
    def __init__(self, search_dirs=[], backend="java"):
        self.include_file_handler = IncludeFileHandlerVisitor(self)
        self.search_dirs = OrderedSet(["."] + search_dirs)
        self.included_files = set()
        self.backend = backend

    def search_and_parse(self, filename):
        filename = self.search_file(filename)
        if filename:
            return self.parse(filename)
        else:
            raise Exception("{0} not found".format(filename))

    def search_file(self, filename):
        for directory in self.search_dirs:
            potential_file_path = os.path.join(directory, filename)
            if os.path.exists(potential_file_path):
                return potential_file_path
        return None

    def parse(self, filename):
        self.included_files.add(os.path.abspath(filename))
        self.search_dirs.add(os.path.dirname(filename))
        jar_path = os.path.dirname(__file__)
        ast = None
        if self.backend == "java":
            cmd = ["java", "-jar", os.path.join(jar_path, "pparser.jar"), filename]
            with TemporaryFile() as outfile:
                subprocess.call(cmd, stdout=outfile)
                outfile.seek(0)
                ast = to_tree(outfile)
        elif self.backend == "python":
            lexer = pLexer(FileStream(filename))
            stream = CommonTokenStream(lexer)
            parser = pParser(stream)
            ast = parser.program()
        else:
            raise Exception("Invalid parser backend")
        ast.accept(self.include_file_handler)
        return ast

