from __future__ import print_function
import os, sys
from build import translate_and_execute

benchmark_base_dir = "benchmarks"
max_round_robin_bound = 10

def main():
	if not os.path.exists(benchmark_base_dir):
	    os.makedirs(benchmark_base_dir)
	def benchmark_exception_cases(proj_path, exe_extra_args=[]):
		proj_name = os.path.splitext(os.path.basename(proj_path))[0]
		# benchmark random scheduler
		print(
			"""
#####{space}#####
#    {name}    #
#####{space}#####
			""".format(space=len(proj_name)*"#", name=proj_name))
		sys.stdout.flush()
		ret = translate_and_execute(benchmark_base_dir, proj_path, 
							configuration="Release", 
							build_to_stdout=False,
							exe_extra_args=exe_extra_args,
							time_exe=True,
							check_return=False)
		print("{0}: {1} seconds".format(proj_path, ret["elapsedTime"]))
		sys.stdout.flush()
		# benchmark round robin scheduler
		for d in range(0, max_round_robin_bound):
			ret = translate_and_execute(benchmark_base_dir, proj_path, 
							configuration="ReleaseRR", 
							build_to_stdout=False,
							exe_extra_args=exe_extra_args + ["-d", str(d)],
							time_exe=True,
							check_return=False)
			if ret["returncode"] != 0:
				print("[RR] {0}: delay_bound={1}, {2} seconds, bug found!".format(proj_path, d, ret["elapsedTime"]))
				sys.stdout.flush()
				break
			else:
				print("[RR] {0}: delay_bound={1}, {2} seconds, no bug found".format(proj_path, d, ret["elapsedTime"]))
				sys.stdout.flush()



	benchmark_exception_cases("tests/Benchmarks/2pc_1/2pc_1.p")
	benchmark_exception_cases("tests/Benchmarks/2pc_6/2pc_6.p")
	
	benchmark_exception_cases("tests/Benchmarks/chainRep_1/chain_1.p")
	benchmark_exception_cases("tests/Benchmarks/chainRep_2/chain_2.p")

	benchmark_exception_cases("tests/Benchmarks/MultiPaxos_3/Multi_Paxos_3.p")

	benchmark_exception_cases("tests/Benchmarks/Paxos_1/Paxos_1.p")
	benchmark_exception_cases("tests/Benchmarks/Paxos_2/Paxos_2.p")

if __name__ == '__main__':
	main()
