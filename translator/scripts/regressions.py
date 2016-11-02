import os
from .build import translate_and_execute, find_all_p_files

regression_tests_dir = "tests/RegressionTests"

all_regressions = find_all_p_files(regression_tests_dir)
all_regressions = filter(lambda r: "Integration" not in r, all_regressions)

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
