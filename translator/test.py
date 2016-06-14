from __future__ import print_function
import os, sys
from parser.p_java_parser import pJavaParser
from ordered_set import OrderedSet
from collections import defaultdict, OrderedDict
from translator.basic_csharp_translator import PProgramToCSharpTranslator


if __name__ == '__main__':
    ast = pJavaParser().parse("tests/pingpong.p")
    translator = PProgramToCSharpTranslator(ast, "pingpong")
    translator.translate()
