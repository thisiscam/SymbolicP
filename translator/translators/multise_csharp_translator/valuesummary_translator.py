import subprocess, os

transformer_proj_path = os.path.realpath(os.path.join(os.path.dirname(__file__), "MultiSETransformer"))
transformer_exe = None
for root, dirs, files in os.walk(os.path.join(transformer_proj_path, "bin"), topdown=False):
    for name in files:
        if name == "MultiSETransformer.exe":
        	transformer_exe = os.path.join(root, name)
        	break

if not transformer_exe:
	raise Exception("MultiSETransformer.exe not found, please build MultiSETransformer first")

if os.name == "nt":
	transformer_cmd = [transformer_exe]
else:
	transformer_cmd = ["mono", transformer_exe]

def valuesummary_transform(include_files, output_path, transform_files, no_copy_srcs=None, define_macros=None):
	if not isinstance(include_files, str):
		include_files = ",".join(include_files)
	cmd = transformer_cmd + [include_files, output_path, ",".join(transform_files)]
	if no_copy_srcs:
		cmd += [",".join(no_copy_srcs)]
	if define_macros:
		cmd += [",".join(define_macros)]
	# print " ".join(cmd)
	subprocess.check_call(cmd)
