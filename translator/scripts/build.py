from __future__ import print_function
from scripts.translate import main as translate_p
import os, subprocess, functools, time, sys
import fnmatch

MONO_CMD = os.environ.get("MONO_CMD", "mono")
NUGET_CMD = os.environ.get("NUGET_CMD", "nuget")

if os.name == "nt":
	builder = "msbuild"
else:
	builder = "xbuild"

FNULL = open(os.devnull, 'w')

def timeit(func):
    @functools.wraps(func)
    def newfunc(*args, **kwargs):
        startTime = time.time()
        ret = func(*args, **kwargs)
        elapsedTime = time.time() - startTime
        return (ret, elapsedTime)
    return newfunc

def translate_and_execute(out_dir, p_file, configuration,
						  build_to_stdout=True,
						  running_to_stdout=True, 
						  exe_extra_args=[], time_exe=False, check_return=True):
	project_name = os.path.splitext(os.path.basename(p_file))[0]
	project_dir = os.path.join(out_dir, project_name)
	translate_p(["-o", project_dir, "-t", "multise_csharp", p_file])
	print("Building {0} Configuration: {1}".format(project_name, configuration))
	sys.stdout.flush()
	stdout = sys.stdout if build_to_stdout else FNULL 
	subprocess.check_call(
						[
							NUGET_CMD, 
							"restore"
						],
						cwd=project_dir,
						stdout=stdout
					)
	subprocess.check_call(
						[
							builder, 
							'/property:Configuration={0}'.format(configuration), 
							'/property:Platform=x64',
							'{0}.sln'.format(project_name)
						],
						cwd=project_dir,
						stdout=stdout
					)
	stdout.flush()
	print("Running {0}".format(project_name))
	sys.stdout.flush()
	stdout = sys.stdout if running_to_stdout else FNULL
	if check_return:
		invoke_cmd = subprocess.check_call
	else:
		invoke_cmd = subprocess.call
	def run_exe_f():
		ret = invoke_cmd(
					[MONO_CMD, '{0}.exe'.format(project_name)] + exe_extra_args,
					cwd=os.path.join(project_dir, "bin/{0}/".format(configuration)), 
					stdout=stdout)
		stdout.flush()
		return ret
	if time_exe:
		ret, elapsedTime = timeit(run_exe_f)()
		return {"returncode": ret, "elapsedTime": elapsedTime}
	else:
		return {"returncode": run_exe_f()}

def find_all_p_files(base_dir):
	all_tests = []
	for root, dirnames, filenames in os.walk(base_dir):
	    p_srcs = fnmatch.filter(filenames, '*.p')
	    if len(p_srcs) == 1:
	        all_tests.append(os.path.join(root, p_srcs[0]))
	    elif len(p_srcs) > 1:
	        main_test_file_name = os.path.basename(root) + ".p"
	        if main_test_file_name in p_srcs:
	            all_tests.append(os.path.join(root, main_test_file_name))
	        else:
	            found_main = False
	            for p_src in p_srcs:
	                p_src_path = os.path.join(root, p_src)
	                if 'main machine ' in open(p_src_path).read():
	                    all_tests.append(p_src_path)
	                    found_main = True
	                    break
	            if not found_main:
	                print("Cannot determine main p file to compile, ignored sources under {0}".format(root))
	return all_tests
