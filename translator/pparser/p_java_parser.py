from tempfile import TemporaryFile
import subprocess, os

class AntlrSExpCtx(object):
    def __init__(self, name, childs):
        self.name = name
        self.childs = childs

        self.visitor_invoke_name = "visit" + self.name[0].upper() + self.name[1:]

    def getChildCount(self):
        return len(self.childs)

    def getChild(self, i):
        return self.childs[i]

    def accept(self, visitor, *args, **kwargs):
        if hasattr(visitor, self.visitor_invoke_name):
            return getattr(visitor, self.visitor_invoke_name)(self, *args, **kwargs)
        else:
            print("Warning: {0} not invoked".format(self.visitor_invoke_name))

    def __repr__(self):
        return "Antlr_{0}Ctx{1}".format(self.name, self.childs)

class Token(object):
    def __init__(self, txt):
        self.text = txt
    def getText(self):
        return self.text
    def accept(self, visitor):
        return
    def __repr__(self):
        return "`{0}`".format(self.text)

def peek1(s):
    c = s.read(1)
    s.seek(s.tell() - 1)
    return c

def to_tree(s):
    return parse_exp(s)

def parse_exp(s):
    c = peek1(s)
    if c == "(":
        c = s.read(1)
        if peek1(s) in (")", " "):
            return Token(c)
        else:
            name = parse_tid(s)
            childs = []
            while peek1(s) == " ":
                s.read(1)
                childs.append(parse_exp(s))
            if s.read(1) != ")":
                raise ValueError("Invalid s-expression")
            return AntlrSExpCtx(name, childs)
    else:
        return Token(parse_id(s))

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


class pJavaParser(object):
	def parse(self, filename):
		jar_path = os.path.dirname(__file__)
		cmd = ["java", "-jar", os.path.join(jar_path, "pparser.jar"), filename]
		with TemporaryFile() as outfile:
			subprocess.call(cmd, stdout=outfile)
			outfile.seek(0)
			return to_tree(outfile)
