from __future__ import print_function
import os, sys, argparse

from pparser.p_java_parser import pJavaParser
from ordered_set import OrderedSet
from collections import defaultdict, OrderedDict
from translators.basic_csharp_translator import PProgramToCSharpTranslator

def translate(options):
	pparser = pJavaParser()
	ast = pparser.parse(options.input_file)
	translator = PProgramToCSharpTranslator(ast, options.out_dir)
	translator.translate()


def process_options(options):
	if not options.out_dir:
		options.out_dir = os.path.splitext(os.path.basename(options.input_file))[0]

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('-o', '--out-dir', type=str, dest="out_dir",
    					help="ouput directory, defaults to input file's name")
    parser.add_argument('input_file')
    options = parser.parse_args()
    process_options(options)
    translate(options)