import java.io.FileInputStream;

import org.antlr.v4.runtime.ANTLRInputStream;
import org.antlr.v4.runtime.CommonTokenStream;
import org.antlr.v4.runtime.tree.ParseTree;
import org.antlr.v4.runtime.CommonToken;
import org.antlr.v4.runtime.Parser;
import org.antlr.v4.runtime.RuleContext;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.atn.ATN;
import org.antlr.v4.runtime.tree.*;
import org.antlr.v4.runtime.misc.Utils;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.Collections;
import java.util.List;


public class PToLispFormatConverter  {
    public static void main(String[] args) throws Exception {
        ANTLRInputStream input = new ANTLRInputStream(new FileInputStream(args[0]));
        pLexer lexer = new pLexer(input);
        CommonTokenStream tokens = new CommonTokenStream(lexer);
        pParser parser = new pParser(tokens);
        ParseTree tree = parser.program();
        System.out.println(toStringTree(tree, parser));
    }


	/** Print out a whole tree in LISP form. {@link #getNodeText} is used on the
	 *  node payloads to get the text for the nodes.  Detect
	 *  parse trees and extract data appropriately.
	 */
	public static String toStringTree(Tree t) {
		return toStringTree(t, (List<String>)null);
	}

	/** Print out a whole tree in LISP form. {@link #getNodeText} is used on the
	 *  node payloads to get the text for the nodes.  Detect
	 *  parse trees and extract data appropriately.
	 */
	public static String toStringTree(Tree t, Parser recog) {
		String[] ruleNames = recog != null ? recog.getRuleNames() : null;
		List<String> ruleNamesList = ruleNames != null ? Arrays.asList(ruleNames) : null;
		return toStringTree(t, ruleNamesList);
	}

	/** Print out a whole tree in LISP form. {@link #getNodeText} is used on the
	 *  node payloads to get the text for the nodes.
	 */
	public static String toStringTree(final Tree t, final List<String> ruleNames) {
		String s = Utils.escapeWhitespace(getNodeText(t, ruleNames), false);
		if (!(t instanceof RuleContext)) {
			return s;
		}
		StringBuilder buf = new StringBuilder();
		buf.append("(");
		s = Utils.escapeWhitespace(getNodeText(t, ruleNames), false);
		buf.append(s);
		if ( t.getChildCount()==0 ) { 
		} else {
			buf.append(' ');
			for (int i = 0; i<t.getChildCount(); i++) {
				if ( i>0 ) buf.append(' ');
				buf.append(toStringTree(t.getChild(i), ruleNames));
			}
		}
		buf.append(")");
		return buf.toString();
	}

	public static String getNodeText(Tree t, Parser recog) {
		String[] ruleNames = recog != null ? recog.getRuleNames() : null;
		List<String> ruleNamesList = ruleNames != null ? Arrays.asList(ruleNames) : null;
		return getNodeText(t, ruleNamesList);
	}

	public static String getNodeText(Tree t, List<String> ruleNames) {
		if ( ruleNames!=null ) {
			if ( t instanceof RuleContext ) {
				int ruleIndex = ((RuleContext)t).getRuleContext().getRuleIndex();
				String ruleName = ruleNames.get(ruleIndex);
				int altNumber = ((RuleContext) t).getAltNumber();
				if ( altNumber!=ATN.INVALID_ALT_NUMBER ) {
					return ruleName+":"+altNumber;
				}
				String name = t.getClass().getSimpleName();
				return name.substring(0, name.length() - "Content".length());
			}
			else if ( t instanceof ErrorNode) {
				return t.toString();
			}
			else if ( t instanceof TerminalNode) {
				Token symbol = ((TerminalNode)t).getSymbol();
				if (symbol != null) {
					String s = symbol.getText();
					return s;
				}
			}
		}
		// no recog for rule names
		Object payload = t.getPayload();
		if ( payload instanceof Token ) {
			return ((Token)payload).getText();
		}
		return t.getPayload().toString();
	}

}