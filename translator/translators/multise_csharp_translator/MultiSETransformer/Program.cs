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
			var compileSourcesPath = args [0];
			var outputPath = new DirectoryInfo(args [1]);
			outputPath.Create ();
			var transformSources = new HashSet<string>(args [2].Split (new []{ ',' }, 1000));

			Compilation test = CreateTestCompilation(compileSourcesPath);
			
			for (int pass = 0; pass < ValueSummaryRewriter.NUM_PASSES; pass++) {
				foreach (SyntaxTree sourceTree in test.SyntaxTrees) 
				{
					if (transformSources.Contains(new FileInfo (sourceTree.FilePath).Name)) {
						SyntaxTree tree = sourceTree;
						SemanticModel model;
						SyntaxNode newSource = null;
						model = test.GetSemanticModel (tree);
						ValueSummaryRewriter rewriter = new ValueSummaryRewriter (pass, model);
						newSource = rewriter.Visit (sourceTree.GetRoot ()).NormalizeWhitespace ();
						tree = tree.WithRootAndOptions (newSource, tree.Options);
						test = test.ReplaceSyntaxTree (sourceTree, tree);					
					}
				}
			}
			foreach (SyntaxTree sourceTree in test.SyntaxTrees) {
				var newFile = Path.Combine (outputPath.FullName, Path.GetFileName(sourceTree.FilePath));
				File.WriteAllText(newFile, sourceTree.GetRoot().ToFullString());
				Console.WriteLine(sourceTree.GetRoot().ToFullString());
			}
		}

		private static Compilation CreateTestCompilation(string path)
		{
			var files = Directory.GetFiles (path, "*.cs");
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