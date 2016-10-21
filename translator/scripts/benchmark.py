from scripts.translate import main as translate_p
import os, subprocess

MONO_CMD = os.environ.get("MONO_CMD", "mono")

FNULL = open(os.devnull, 'w')

def translate_and_execute(out_dir, p_file, configuration):
	project_name = os.path.splitext(os.path.basename(p_file))[0]
	project_dir = os.path.join(out_dir, project_name)
	translate_p(["-o", project_dir, "-t", "multise_csharp_translator", p_file])
	print "Building {0} Configuration: {1}".format(project_name, configuration)
	subprocess.check_call(['xbuild', 
						'/property:Configuration={0}'.format(configuration), 
						'/property:Platform=x64',
						'{0}.sln'.format(project_name)],
						stdout=FNULL,
						cwd=project_dir
					)
	print "Running {0}".format(project_name)
	subprocess.check_call([MONO_CMD, '{0}.exe'.format(project_name)],
						cwd=os.path.join(project_dir, "bin/{0}/".format(configuration)))

if not os.path.exists("benchmark"):
    os.makedirs("benchmark")

translate_and_execute("benchmark", os.path.join("tests", "PingPong.p"), "Release")
translate_and_execute("benchmark", os.path.join("tests", "Dummy2PC.p"), "Release")
translate_and_execute("benchmark", os.path.join("tests", "LeaderElection.p"), "Release")

translate_and_execute("benchmark", os.path.join("tests", "PingPong.p"), "ReleaseVSNoMerging")
# translate_and_execute("benchmark", os.path.join("tests", "Dummy2PC.p"), "ReleaseVSNoMerging")
translate_and_execute("benchmark", os.path.join("tests", "LeaderElection.p"), "ReleaseVSNoMerging")

translate_and_execute("benchmark", os.path.join("tests", "two-phase-commit.p"), "Release")

