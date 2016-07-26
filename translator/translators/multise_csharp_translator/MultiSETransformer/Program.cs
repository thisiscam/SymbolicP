using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MultiSETransformer
{
	internal class Program
	{
		private static void Main(string [] args)
		{
			var outputPath = new DirectoryInfo(args [1]);
			List<string> files = null;
			if (!args [0].Contains (",") && !args[0].EndsWith(".cs")) {
				var compileSourcesPath = Path.GetFullPath(args [0]);
				files = Directory.GetFiles (compileSourcesPath, "*.cs").Select ((src_path, i) => Path.GetFullPath (src_path)).ToList();
				files.Add(Path.Combine(outputPath.FullName, "ValueSummary.cs"));
			} else {
				files = args [0].Split (new []{ ',' }, 1000).Select((src_path, i) => Path.GetFullPath(src_path)).ToList();
			}

			outputPath.Create ();
			var transformSources = new HashSet<string>(args [2].Split (new []{ ',' }, 1000).Select((src_path, i) => Path.GetFullPath(src_path)));

			var noCopySrcs = args.Count() > 3 ? args [3].Split (new []{ ',' }, 1000).Select((src_path, i) => Path.GetFullPath(src_path)) : new string[0];

			Compilation test = CreateTestCompilation(files.ToArray());
			
			for (int pass = 0; pass < ValueSummaryRewriter.NUM_PASSES; pass++) {
				foreach (SyntaxTree sourceTree in test.SyntaxTrees) 
				{
					if (transformSources.Contains(sourceTree.FilePath)) {
						SyntaxTree tree = sourceTree;
						SemanticModel model;
						SyntaxNode newSource = null;
						model = test.GetSemanticModel (tree);
						ValueSummaryRewriter rewriter = new ValueSummaryRewriter (pass, model, transformSources);
						newSource = rewriter.Visit (sourceTree.GetRoot ()).NormalizeWhitespace ();
						tree = tree.WithRootAndOptions (newSource, tree.Options);
						test = test.ReplaceSyntaxTree (sourceTree, tree);					
					}
				}
			}
			foreach (SyntaxTree sourceTree in test.SyntaxTrees) {
				if (noCopySrcs.Contains (sourceTree.FilePath)) {
					continue;
				}
				var newFile = Path.Combine (outputPath.FullName, Path.GetFileName(sourceTree.FilePath));
				File.WriteAllText(newFile, sourceTree.GetRoot().ToFullString());
			}
		}

		private static Compilation CreateTestCompilation(string[] files)
		{
			var sourceTrees = files.Select (
				src_path => {
					String programText = File.ReadAllText (src_path);
					return CSharpSyntaxTree.ParseText (programText).WithFilePath (src_path);
			});

			MetadataReference mscorlib =
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

			MetadataReference[] references = { mscorlib };

			return CSharpCompilation.Create("TransformationCS",
				sourceTrees,
				references,
				new CSharpCompilationOptions(
					OutputKind.ConsoleApplication));
		}
	}
}