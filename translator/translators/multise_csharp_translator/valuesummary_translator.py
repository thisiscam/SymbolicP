import subprocess, os

transformer_proj_path = os.path.realpath(os.path.join(os.path.dirname(__file__), "MultiSETransformer"))
transformer_exe = os.path.join(transformer_proj_path, "MultiSETransformer.exe")
transformer_lib_path = os.path.join(transformer_proj_path, "Libs")

if os.name == "nt":
	transformer_cmd = [transformer_exe]
	ld_library_path_var = "PATH"
else:
	transformer_cmd = ["mono", transformer_exe]
	ld_library_path_var = "MONO_PATH"

def valuesummary_transform(include_files_path, output_path, transform_files):
	cmd = transformer_cmd + [include_files_path, output_path, ",".join(transform_files)]
	env = os.environ.copy()
	if ld_library_path_var in env:
		env[ld_library_path_var] += os.pathsep + transformer_lib_path
	else:
		env[ld_library_path_var] = transformer_lib_path
	subprocess.call(cmd, env=env)
