from __future__ import print_function
import os, sys, argparse, importlib

from pparser.p_java_parser import pJavaParser
from ordered_set import OrderedSet
from collections import defaultdict, OrderedDict

def translate(options):
    pparser = pJavaParser(options.search_dirs.split(","))
    ast = pparser.parse(options.input_file)
    translator = options.translator(ast, options.out_dir)
    translator.translate()


def process_options(options):
    if not options.out_dir:
        options.out_dir = os.path.splitext(os.path.basename(options.input_file))[0]
    options.translator = __import__("translators." + options.translator, globals(), locals(), ['Translator'], -1).Translator

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('-o', '--out-dir', type=str, dest="out_dir",
                        help="ouput directory, defaults to input file's name")
    parser.add_argument('-t', '--translator', type=str, dest="translator",
                        default="basic_csharp_translator",
                        help="which translator to use")
    parser.add_argument('-I', '--include', type=str, dest="search_dirs", 
                        default="",
                        help="directories for the translator to search for P source files")
    parser.add_argument('input_file')
    options = parser.parse_args()
    process_options(options)
    translate(options)