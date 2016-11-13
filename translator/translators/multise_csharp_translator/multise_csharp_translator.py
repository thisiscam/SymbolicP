import os, glob
from ..ast_to_pprogram import *
from ..symbolic_csharp_translator import PProgramToSymbolicCSharpTranslator
from .valuesummary_translator import valuesummary_transform
from quik import FileLoader
from lxml import etree as ET
import uuid
import shutil

class PProgramToMultSECSharpTranslator(PProgramToSymbolicCSharpTranslator):
    buddysharp_dir = os.environ.get("BUDDYSHARP_DIR", os.path.realpath(os.path.join(os.path.dirname(__file__), "../../libraries/BuDDySharp")))
    sylvansharp_dir = os.environ.get("SYLVANSHARP_DIR", os.path.realpath(os.path.join(os.path.dirname(__file__), "../../libraries/SylvanSharp")))
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
                    "DefaultArray.cs",
                    "RandomScheduler.cs",
                    "RoundRobinScheduler.cs"
                }
        all_srcs = runtime_srcs + generated_srcs
        transform_srcs = filter(lambda s: os.path.basename(s) not in ignore_srcs, all_srcs)
        valuesummary_transform(all_srcs, self.out_dir, transform_srcs, no_copy_srcs)

    @staticmethod
    def find_project(csproj_path):
        csproj_xml = ET.parse(csproj_path)
        guid = csproj_xml.xpath("//*[local-name() = 'ProjectGuid']")[0].text
        project = {
                    "name": os.path.splitext(os.path.basename(csproj_path))[0],
                    "path": os.path.realpath(csproj_path),
                    "guid": guid
                }
        return project


    def find_referenced_projects(self):
        reference_project_paths = [ 
              os.path.join(self.runtime_dir, "BDDToZ3Wrap/BDDToZ3Wrap.csproj"),
              os.path.join(self.buddysharp_dir, "BuDDySharp/BuDDySharp/BuDDySharp.csproj"),
              os.path.join(self.sylvansharp_dir, "SylvanSharp/SylvanSharp/SylvanSharp.csproj")
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
        with open("{0}/{1}.sln".format(self.out_dir, self.project_name), "w+") as csprojf:
            csprojf.write(csproj_template.render(
                {
                    "sln_guid": "{" + str(uuid.uuid1()).upper() + "}",
                    "include_projects": projects,
                    "main_project": main_project
                }
            ))

    def copy_package_config(self):
        pkgs_config_name =  "packages.config"
        shutil.copy(os.path.join(self.runtime_dir, pkgs_config_name), self.out_dir)

    pipeline = [
                PProgramToSymbolicCSharpTranslator.make_output_dir, 
                PProgramToSymbolicCSharpTranslator.create_proj_macros, 
                PProgramToSymbolicCSharpTranslator.generate_foreach_machine,
                multise_transform,
                find_referenced_projects,
                PProgramToSymbolicCSharpTranslator.create_csproj,
                create_cssln,
                copy_package_config
            ]

    # Visit a parse tree produced by pParser#exp_new.
    def visitExp_new(self, ctx, **kwargs):
        return "NewMachine(Machine{0}.Allocate(), null)".format(ctx.getChild(1).getText())

    # Visit a parse tree produced by pParser#exp_new_with_arguments.
    def visitExp_new_with_arguments(self, ctx, **_kwargs):
        kwargs = _kwargs.copy()
        kwargs["do_copy"] = True
        c3 = ctx.getChild(3).accept(self, **kwargs)
        return "NewMachine(Machine{0}.Allocate(), {1})".format(ctx.getChild(1).getText(), c3)

    # Visit a parse tree produced by pParser#stmt_new.
    def visitStmt_new(self, ctx, **kwargs):
        self.out("NewMachine(Machine{0}.Allocate(), null);\n".format(ctx.getChild(1).getText()))

    # Visit a parse tree produced by pParser#stmt_new_with_arguments.
    def visitStmt_new_with_arguments(self, ctx, **kwargs):
        c3 = ctx.getChild(3).accept(self, **kwargs)
        self.out("NewMachine(Machine{0}.Allocate(),{1});\n".format(ctx.getChild(1).getText(), c3))

    def visitExp_nondet(self, ctx, **kwargs):
        self.rand_bool_cnt += 1
        return "RandomBool({cnt}, _rnd{cnt}, _allRnds{cnt})".format(cnt=self.rand_bool_cnt)

    def visitExp_fairnondet(self, ctx, **kwargs):
        self.rand_bool_cnt += 1
        return "RandomBool({cnt}, _rnd{cnt}, _allRnds{cnt})".format(cnt=self.rand_bool_cnt)

    def out_random_cnts(self):
        self.out("#region multisenorewrite\n")
        for i in range(self.rand_bool_cnt):
            self.out("System.Collections.Generic.List<PBool> _allRnds{cnt} = new System.Collections.Generic.List<PBool>();\n".format(cnt=i+1))
            self.out("ValueSummary<int> _rnd{cnt} = 0;\n".format(cnt=i+1))
        self.out("#endregion\n")

    def out_machine_body(self):
        self.rand_bool_cnt = 0
        super(PProgramToSymbolicCSharpTranslator, self).out_machine_body()
        self.out_random_cnts()
        if not self.current_visited_machine.is_spec:
            machine_name = self.get_machine_csclassname(self.current_visited_machine)
            self.out(
                """
                #region multisenorewrite
                private static System.Collections.Generic.List<PMachine> _allAllocs = new System.Collections.Generic.List<PMachine>();
                private static ValueSummary<int> _allAllocsCounter = 0;
                #endregion
                public static PMachine Allocate()
                {{
                #region multisenorewrite
                    return PathConstraint.Allocate<PMachine>(_ => new {0}(), _allAllocs, _allAllocsCounter);
                #endregion
                }}
                """.format(machine_name))

Translator = PProgramToMultSECSharpTranslator
