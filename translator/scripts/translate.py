from __future__ import print_function
import os, sys, argparse, importlib

from pparser.p_java_parser import pJavaParser
from ordered_set import OrderedSet
from collections import defaultdict, OrderedDict

import pkgutil

viable_translators = []
translator_suffix = "_translator"
for _, name, _ in pkgutil.iter_modules([os.path.realpath(os.path.join(__file__, '../../translators'))]):
    if name.endswith(translator_suffix):
        viable_translators.append(name[:-len(translator_suffix)])

def translate(options):
    pparser = pJavaParser(options.search_dirs)
    ast = pparser.parse(options.input_file)
    translator = options.translator(ast, 
                    os.path.splitext(os.path.basename(options.input_file))[0],
                    options.out_dir)
    translator.translate()

def process_options(options):
    if not options.out_dir:
        options.out_dir = os.path.splitext(os.path.basename(options.input_file))[0]
    options.translator = __import__("translators." + options.translator + translator_suffix, 
                                globals(), locals(), ['Translator'], -1).Translator

def main(argv):
    parser = argparse.ArgumentParser("Translates P code into cs code")
    parser.add_argument('-o', '--out-dir', type=str, dest="out_dir",
                        help="ouput directory, defaults to input file's name")
    parser.add_argument('-t', '--translator', type=str, dest="translator",
                        default="basic_csharp_translator",
                        choices=viable_translators,
                        help="which translator to use")
    parser.add_argument('-I', '--include', dest="search_dirs", 
                        default=[],
                        action="append",
                        help="include directory for the translator to search for P source files")
    parser.add_argument('input_file')
    options = parser.parse_args(argv)
    process_options(options)
    translate(options)

if __name__ == '__main__':
    main(sys.argv[1:])

