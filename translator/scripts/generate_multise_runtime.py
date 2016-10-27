import glob, os, shutil
from translators.multise_csharp_translator import valuesummary_transform

runtimes_dir = os.path.join(os.path.dirname(__file__), "../runtimes/")
symbolic_runtime_dir = os.path.join(runtimes_dir, "symbolic_csharp")
multise_runtime_dir = os.path.join(runtimes_dir, "multise_csharp")

def main():
    all_srcs = glob.glob(os.path.join(symbolic_runtime_dir, "*.cs"))
    ignore_srcs = {
                    "SymbolicBool.cs", 
                    "SymbolicInteger.cs", 
                    "SymbolicEngine.cs", 
                    "PathConstraint.cs",
                    "PInteger.cs",
                    "Program.cs",
                    # "Scheduler.cs",
                    "PBool.cs",
                    "DefaultArray.cs"
                }
    define_macros = {"RANDOM_SCHEDULER", "ROUND_ROBIN_SCHEDULER"}
    transform_srcs = filter(lambda s: os.path.basename(s) not in ignore_srcs, all_srcs)
    nocopy_srcs = ignore_srcs
    nocopy_srcs = filter(lambda s: os.path.basename(s) in nocopy_srcs, all_srcs)
    print "Transforming runtime..."
    valuesummary_transform(symbolic_runtime_dir, multise_runtime_dir, transform_srcs, nocopy_srcs, define_macros)

if __name__ == '__main__':
    main()
