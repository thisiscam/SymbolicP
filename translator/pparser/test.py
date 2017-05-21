from antlr4 import *
from pLexer import pLexer
from pParser import pParser
import sys

def main():
    lexer = pLexer(FileStream(sys.argv[1]))
    stream = CommonTokenStream(lexer)
    parser = pParser(stream)
    tree = parser.program()

if __name__ == '__main__':
    main()
