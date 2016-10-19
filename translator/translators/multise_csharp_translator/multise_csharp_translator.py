import os, glob
from ..ast_to_pprogram import *
from ..symbolic_csharp_translator import PProgramToSymbolicCSharpTranslator
from .valuesummary_translator import valuesummary_transform
from quik import FileLoader
from lxml import etree as ET
import uuid

class PProgramToMultSECSharpTranslator(PProgramToSymbolicCSharpTranslator):
    buddysharp_dir = os.environ.get("BUDDYSHARP_DIR", os.path.realpath(os.path.join(os.path.dirname(__file__), "../../libraries/BuDDySharp")))
    runtime_dir = os.environ.get("RUNTIME_DIR", os.path.realpath(os.path.join(os.path.dirname(__file__), "../../runtimes/multise_csharp")))
    
    def __init__(self, *args):
        super(PProgramToSymbolicCSharpTranslator, self).__init__(*args)

    def multise_transform(self):
        runtime_srcs =  [os.path.realpath(src) for src in glob.glob("{0}/*.cs".format(PProgramToSymbolicCSharpTranslator.runtime_dir))]
        generated_srcs = [os.path.realpath(src) for src in glob.glob("{0}/*.cs".format(self.out_dir))]
        no_copy_srcs = runtime_srcs
        ignore_srcs = {
                    "SymbolicBool.cs", 
                    "SymbolicInteger.cs", 
                    "SymbolicEngine.cs", 
                    "PathConstraint.cs",
                    "PInteger.cs",
                    "Program.cs",
                    "Scheduler.cs",
                    "PBool.cs",
                }
        all_srcs = runtime_srcs + generated_srcs
        transform_srcs = filter(lambda s: os.path.basename(s) not in ignore_srcs, all_srcs)
        valuesummary_transform(all_srcs, self.out_dir, transform_srcs, no_copy_srcs)

    @staticmethod
    def find_project(csproj_path):
        csproj_xml = ET.parse(csproj_path)
        guid = csproj_xml.xpath("//*[local-name() = 'ProjectGuid']")[0].text
        project = {
                    "name": os.path.basename(csproj_path),
                    "path": os.path.realpath(csproj_path),
                    "guid": guid
                }
        return project


    def find_referenced_projects(self):
        reference_project_paths = [ 
              os.path.join(self.runtime_dir, "BDDToZ3Wrap/BDDToZ3Wrap.csproj"),
              os.path.join(self.buddysharp_dir, "BuDDySharp/BuDDySharp/BuDDySharp.csproj")
            ]
        self.reference_projects = map(self.find_project, reference_project_paths)


    def get_csproj_template_parameters(self, parameters):
        parameters["include_projects"] = self.reference_projects
        return parameters

    def create_cssln(self):
        main_project = self.find_project(self.out_csproj_path)
        projects = [main_project] + self.reference_projects
        loader = FileLoader(self.runtime_dir)
        csproj_template = loader.load_template("template.sln.in")
        with open("{0}/{0}.sln".format(self.out_dir), "w+") as csprojf:
            csprojf.write(csproj_template.render(
                {
                    "sln_guid": "{" + str(uuid.uuid1()).upper() + "}",
                    "include_projects": projects,
                    "main_project": main_project
                }
            ))

    pipeline = [
                PProgramToSymbolicCSharpTranslator.make_output_dir, 
                PProgramToSymbolicCSharpTranslator.create_proj_macros, 
                PProgramToSymbolicCSharpTranslator.generate_foreach_machine,
                multise_transform,
                find_referenced_projects,
                PProgramToSymbolicCSharpTranslator.create_csproj,
                create_cssln
            ]

Translator = PProgramToMultSECSharpTranslator
