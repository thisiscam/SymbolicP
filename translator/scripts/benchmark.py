from build import translate_and_execute
if __name__ == '__main__':
	if not os.path.exists("benchmark"):
	    os.makedirs("benchmark")

	translate_and_execute("benchmark", os.path.join("tests", "PingPong.p"), "Release")
	translate_and_execute("benchmark", os.path.join("tests", "Dummy2PC.p"), "Release")
	translate_and_execute("benchmark", os.path.join("tests", "LeaderElection.p"), "Release")

	translate_and_execute("benchmark", os.path.join("tests", "PingPong.p"), "ReleaseVSNoMerging")
	# translate_and_execute("benchmark", os.path.join("tests", "Dummy2PC.p"), "ReleaseVSNoMerging")
	translate_and_execute("benchmark", os.path.join("tests", "LeaderElection.p"), "ReleaseVSNoMerging")

	translate_and_execute("benchmark", os.path.join("tests", "two-phase-commit.p"), "Release")

