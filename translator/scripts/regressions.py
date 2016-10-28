import fnmatch
import os
from .build import translate_and_execute

regression_tests_dir = "tests/RegressionTests"

all_regressions = []
for root, dirnames, filenames in os.walk(regression_tests_dir):
    dirnames[:] = [d for d in dirnames if d != "Integration"]
    p_srcs = fnmatch.filter(filenames, '*.p')
    if len(p_srcs) == 1:
        all_regressions.append(os.path.join(root, p_srcs[0]))
    elif len(p_srcs) > 1:
        main_test_file_name = os.path.basename(root) + ".p"
        if main_test_file_name in p_srcs:
            all_regressions.append(os.path.join(root, main_test_file_name))
        else:
            found_main = False
            for p_src in p_srcs:
                p_src_path = os.path.join(root, p_src)
                if 'main machine ' in open(p_src_path).read():
                    all_regressions.append(p_src_path)
                    found_main = True
                    break
            if not found_main:
                print "Cannot determine main p file to compile, ignored sources under {0}".format(root)

correct_regressions = filter(lambda path: "Correct" in path, all_regressions)
dynamic_error_regressions = filter(lambda path: "DynamicError" in path, all_regressions)

workspace_dir = "workspace"

if not os.path.exists(workspace_dir):
    os.makedirs(workspace_dir)

for correct in correct_regressions:
    try:
        print "Compling and Running {0}".format(correct)
        translate_and_execute(workspace_dir, correct, "Release")
        print "Finished {0}".format(correct)
    except ValueError as ve:
        print "Translator terminated: {0}, ignored {1}".format(ve, correct)
