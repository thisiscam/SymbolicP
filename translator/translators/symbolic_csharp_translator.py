import os
from .basic_csharp_translator import PProgramToCSharpTranslator

class PProgramToSymbolicCSharpTranslator(PProgramToCSharpTranslator):
    def __init__(self, *args):
        super(PProgramToCSharpTranslator, self).__init__(*args)
        self.runtime_dir = os.environ.get("RUNTIME_DIR", os.path.realpath(os.path.dirname(__file__) + "/../runtimes/symbolic_csharp"))

Translator = PProgramToSymbolicCSharpTranslator
