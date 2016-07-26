using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp; 
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MultiSETransformer
{
	public class ValueSummaryRewriter : CSharpSyntaxRewriter
	{
		public const int NUM_PASSES = 5;

		private int pass = 0;
		private bool noRewrite = false, _inPropertyDecl = false, _inConstructorDecl = false;
		private SemanticModel model;
		private HashSet<string> transformSources;

		public ValueSummaryRewriter(int pass, SemanticModel semanticModel, HashSet<string> transformSources)
		{
			this.pass = pass;
			this.model = semanticModel;
			this.transformSources = transformSources;
		}

		//		public override SyntaxNode VisitLocalDeclarationStatement(
		//			LocalDeclarationStatementSyntax node)
		//		{
		//			return node;
		//		}

		public override SyntaxNode VisitRegionDirectiveTrivia (RegionDirectiveTriviaSyntax node)
		{
			if (node.DirectiveNameToken.Text == "multisenorewrite") {
				this.noRewrite = true;
			}
			return base.VisitRegionDirectiveTrivia (node);
		}

		public override SyntaxNode VisitEndRegionDirectiveTrivia (EndRegionDirectiveTriviaSyntax node)
		{
			if (node.DirectiveNameToken.Text == "multisenorewrite") {
				this.noRewrite = false;
			}
			return base.VisitEndRegionDirectiveTrivia (node);
		}

		public override SyntaxNode VisitVariableDeclaration (VariableDeclarationSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 4:
				{
					node = base.VisitVariableDeclaration (node) as VariableDeclarationSyntax;
					return node.WithType (SyntaxFactory.GenericName (SyntaxFactory.Identifier (@"ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.Type)));
				}
			default:
				{
					return base.VisitVariableDeclaration (node);
				}
			}
		}

		public override SyntaxNode VisitFieldDeclaration (FieldDeclarationSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 4:
				{
					if (node.Modifiers.Any (SyntaxKind.ConstKeyword) || node.Modifiers.Any (SyntaxKind.StaticKeyword)) {
						return node;
					}
					goto default;
				}
			default:
				{
					return base.VisitFieldDeclaration (node);
				}
			}
		}

		public override SyntaxNode VisitLocalDeclarationStatement (LocalDeclarationStatementSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 4:
				{
					if (node.Modifiers.Any (SyntaxKind.ConstKeyword) || node.Modifiers.Any (SyntaxKind.StaticKeyword)) {
						return node;
					}
					goto default;
				}
			default:
				{
					return base.VisitLocalDeclarationStatement (node);
				}
			}
		}

		public override SyntaxNode VisitParameter (ParameterSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 4:
				{
					node = base.VisitParameter (node) as ParameterSyntax;
					return node.WithType (SyntaxFactory.GenericName (SyntaxFactory.Identifier (@"ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.Type)));
				}
			default:
				{
					return base.VisitParameter (node);
				}
			}
		}

		public override SyntaxNode VisitArrayType (ArrayTypeSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 4:
				{
					node = base.VisitArrayType (node) as ArrayTypeSyntax;
					return node.WithElementType (SyntaxFactory.GenericName (SyntaxFactory.Identifier (@"ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.ElementType)));
				}
			default:
				{
					return base.VisitArrayType (node);
				}
			}
		}

		public override SyntaxNode VisitBinaryExpression (BinaryExpressionSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 1:
				{
					var leftType = model.GetTypeInfo (node.Left).ConvertedType;
					var rightType = model.GetTypeInfo (node.Right).ConvertedType;
					if (leftType.Name == "String" || rightType.Name == "String") {
						goto default;
					} 
					var left = node.Left.Accept(this) as ExpressionSyntax;
					var right = node.Right.Accept (this) as ExpressionSyntax;
					var lambdaParameters = SyntaxFactory.ParameterList().AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("l"))).AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("r")));
					var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression(parameterList: lambdaParameters, body: node.WithLeft(SyntaxFactory.IdentifierName("l")).WithRight(SyntaxFactory.IdentifierName("r")));
					var typeArguments = SyntaxFactory.TypeArgumentList ().AddArguments (SyntaxFactory.ParseTypeName (leftType.ToDisplayString ()), SyntaxFactory.ParseTypeName(model.GetTypeInfo(node).ConvertedType.ToDisplayString()));
					return SyntaxFactory.InvocationExpression (
						expression: SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, left, SyntaxFactory.GenericName (SyntaxFactory.Identifier("InvokeBinary"), typeArguments)),
						argumentList: SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(lambdaExpression)).AddArguments(SyntaxFactory.Argument(right)));
				}
			default:
				{
					return base.VisitBinaryExpression (node);
				}
			}
		}

		public override SyntaxNode VisitPrefixUnaryExpression (PrefixUnaryExpressionSyntax node)
		{
			if (noRewrite) {
				return node;
			}
			switch (pass) {
			case 1:
				{
					if (node.Operand.IsKind (SyntaxKind.NumericLiteralExpression) || node.Operand.IsKind (SyntaxKind.TrueLiteralExpression) || node.Operand.IsKind (SyntaxKind.FalseLiteralExpression) || node.Operand.IsKind (SyntaxKind.StringLiteralExpression)) {
						return base.VisitPrefixUnaryExpression (node);
					} else {
						var left = node.Operand.Accept(this) as ExpressionSyntax;
						var lambdaParameters = SyntaxFactory.ParameterList().AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_")));
						var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression(parameterList: lambdaParameters, body: node.WithOperand(SyntaxFactory.IdentifierName("_")));
						return SyntaxFactory.InvocationExpression (
							expression: SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, left, SyntaxFactory.IdentifierName ("InvokeMethod")),
							argumentList: SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(lambdaExpression)));
					}
				}
			default:
				{
					return base.VisitPrefixUnaryExpression (node);
				}
			}
		}

		public override SyntaxNode VisitPostfixUnaryExpression (PostfixUnaryExpressionSyntax node)
		{
			if (noRewrite) {
				return node;
			}
			switch (pass) {
			case 1:
				{
					if (node.Operand.IsKind (SyntaxKind.NumericLiteralExpression) || node.Operand.IsKind (SyntaxKind.TrueLiteralExpression) || node.Operand.IsKind (SyntaxKind.FalseLiteralExpression) || node.Operand.IsKind (SyntaxKind.StringLiteralExpression)) {
						return base.VisitPostfixUnaryExpression (node);
					} else {
						var left = node.Operand.Accept(this) as ExpressionSyntax;
						var lambdaParameters = SyntaxFactory.ParameterList().AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_")));
						var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression(parameterList: lambdaParameters, body: node.WithOperand(SyntaxFactory.IdentifierName("_")));
						return SyntaxFactory.InvocationExpression (
							expression: SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, left, SyntaxFactory.IdentifierName ("InvokeMethod")),
							argumentList: SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(lambdaExpression)));
					}
				}
			default:
				{
					return base.VisitPostfixUnaryExpression (node);
				}
			}
		}

		public override SyntaxNode VisitElementAccessExpression (ElementAccessExpressionSyntax node)
		{

			if (noRewrite)
				return node;
			switch (pass) {
			case 4:
				{
					var targetExpression = node.Expression.Accept(this) as ExpressionSyntax;
					var lambdaParameters = SyntaxFactory.ParameterList().AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_")));
					var lambdaBodyInvocationArugments = SyntaxFactory.BracketedArgumentList ();
					for (int i=0; i < node.ArgumentList.Arguments.Count; i++) {
						var p = SyntaxFactory.Parameter(SyntaxFactory.Identifier("a" + i.ToString()));
						var a = SyntaxFactory.Argument(SyntaxFactory.IdentifierName("a" + i.ToString()));
						lambdaParameters = lambdaParameters.AddParameters (p);
						lambdaBodyInvocationArugments = lambdaBodyInvocationArugments.AddArguments (a);
					}
					var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression(parameterList: lambdaParameters, body: node.WithExpression(SyntaxFactory.IdentifierName ("_")).WithArgumentList(lambdaBodyInvocationArugments));
					return SyntaxFactory.InvocationExpression (
						expression: SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, targetExpression, SyntaxFactory.IdentifierName ("GetIndex")),
						argumentList: SyntaxFactory.ArgumentList(node.ArgumentList.Arguments.Insert(0, SyntaxFactory.Argument(lambdaExpression)))
					);
				}
			default:
				{
					return base.VisitElementAccessExpression (node);
				}
			}		
		}

		public override SyntaxNode VisitObjectCreationExpression (ObjectCreationExpressionSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 0:
				{
					var originalNode = node;
					node = base.VisitObjectCreationExpression (node) as ObjectCreationExpressionSyntax;
//					var lambdaParameters = SyntaxFactory.ParameterList();
//					var lambdaBodyInvocationArugments = SyntaxFactory.ArgumentList ();
//					for (int i=0; i < node.ArgumentList.Arguments.Count; i++) {
//						var p = SyntaxFactory.Parameter(SyntaxFactory.Identifier("a" + i.ToString()));
//						var a = SyntaxFactory.Argument(SyntaxFactory.IdentifierName("a" + i.ToString()));
//						lambdaParameters = lambdaParameters.AddParameters (p);
//						lambdaBodyInvocationArugments = lambdaBodyInvocationArugments.AddArguments (a);
//					}
//					var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression(parameterList: lambdaParameters, body: node.WithArgumentList(lambdaBodyInvocationArugments));
					return node.WithType (SyntaxFactory.GenericName (SyntaxFactory.Identifier (@"ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(SyntaxFactory.ParseTypeName(model.GetTypeInfo(originalNode).ConvertedType.ToDisplayString())))).WithArgumentList(SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(originalNode)));
				}
			default:
				{
					return base.VisitObjectCreationExpression (node);
				}
			}
		}

		public override SyntaxNode VisitPropertyDeclaration (PropertyDeclarationSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 4:
				{
					node = base.VisitPropertyDeclaration (node) as PropertyDeclarationSyntax;
					return node.WithType (SyntaxFactory.GenericName (SyntaxFactory.Identifier (@"ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.Type)));
				}
			default:
				{
					var ret = base.VisitPropertyDeclaration (node);
					return ret;
				}
			}	
		}

		public override SyntaxNode VisitAccessorDeclaration (AccessorDeclarationSyntax node)
		{
			_inPropertyDecl = true;
			var ret = base.VisitAccessorDeclaration (node);
			_inPropertyDecl = false;
			return ret;
		}

		public override SyntaxNode VisitConstructorDeclaration (ConstructorDeclarationSyntax node)
		{
			_inConstructorDecl = true;
			var ret = base.VisitConstructorDeclaration (node);
			_inConstructorDecl = false;
			return ret;
		}

		public override SyntaxNode VisitIdentifierName (IdentifierNameSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 0:
				{
					var symbol = model.GetSymbolInfo (node).Symbol;
					if (symbol == null && model.GetSymbolInfo(node).CandidateSymbols.Count() > 0) {
						symbol = model.GetSymbolInfo (node).CandidateSymbols.First ();
					}
					if (symbol != null) {
						if (node.Parent.Kind() != SyntaxKind.SimpleMemberAccessExpression && !symbol.IsStatic && (symbol.Kind == SymbolKind.Field || symbol.Kind == SymbolKind.Property || symbol.Kind == SymbolKind.Method)) {
							return SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression() , SyntaxFactory.IdentifierName (node.Identifier));
						}
					}
					goto default;
				}
			default:
				{
					return base.VisitIdentifierName (node);
				}
			}	
		}

		public override SyntaxNode VisitMethodDeclaration (MethodDeclarationSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 2:
				{
					if(!node.Modifiers.Any(SyntaxKind.StaticKeyword)) {
						var containing_symbol = model.GetDeclaredSymbol (node).ContainingType;
						var type = SyntaxFactory.ParseTypeName (containing_symbol.ToString ());
						node = base.VisitMethodDeclaration(node.WithParameterList(SyntaxFactory.ParameterList(node.ParameterList.Parameters.Insert(0, SyntaxFactory.Parameter(
							SyntaxFactory.Identifier(@"self")).WithType(type))))) as MethodDeclarationSyntax;
					}
					if ((node.ReturnType as PredefinedTypeSyntax) == null || (node.ReturnType as PredefinedTypeSyntax).Keyword.Kind() != SyntaxKind.VoidKeyword) {
						node = node.WithReturnType(SyntaxFactory.GenericName (SyntaxFactory.Identifier (@"ValueSummary"), SyntaxFactory.TypeArgumentList ().AddArguments (node.ReturnType)));
					}
					return node;
				}
			default:
				{
					return base.VisitMethodDeclaration (node);
				}
			}
		}

		public override SyntaxNode VisitDelegateDeclaration (DelegateDeclarationSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 2:
				{
					var containing_symbol = model.GetDeclaredSymbol (node).ContainingSymbol;
					node = base.VisitDelegateDeclaration(node.WithParameterList(SyntaxFactory.ParameterList(node.ParameterList.Parameters.Insert(0, SyntaxFactory.Parameter(
						SyntaxFactory.Identifier(@"self")).WithType(SyntaxFactory.ParseTypeName(containing_symbol.Name)))))) as DelegateDeclarationSyntax;
					if ((node.ReturnType as PredefinedTypeSyntax) == null || (node.ReturnType as PredefinedTypeSyntax).Keyword.Kind() != SyntaxKind.VoidKeyword) {
						node = node.WithReturnType(SyntaxFactory.GenericName (SyntaxFactory.Identifier (@"ValueSummary"), SyntaxFactory.TypeArgumentList ().AddArguments (node.ReturnType)));
					}
					return node;
				}
			default:
				{
					return base.VisitDelegateDeclaration (node);
				}
			}
		}

		public override SyntaxNode VisitMemberAccessExpression (MemberAccessExpressionSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 3:
				{
					if ((_inPropertyDecl || _inConstructorDecl) && node.Expression.IsKind (SyntaxKind.ThisExpression)) {
						return base.VisitMemberAccessExpression (node);
					}
					var symbol = model.GetSymbolInfo (node).Symbol;
					if (symbol == null || symbol.Locations.Any(loc => loc.IsInMetadata) || symbol.Locations.Any(loc => loc.IsInSource && !this.transformSources.Contains(loc.SourceTree.FilePath))) {
						return base.VisitMemberAccessExpression (node);
					} else if (symbol.DeclaringSyntaxReferences.Length > 0) {
						var decl = model.GetSymbolInfo (node).Symbol.DeclaringSyntaxReferences.First ().GetSyntax();
						if (decl.IsKind (SyntaxKind.VariableDeclarator) && decl.Ancestors ().First ().IsKind (SyntaxKind.VariableDeclaration)) {
							var varDecls = decl.Ancestors ().Where ((x, i) => x.IsKind (SyntaxKind.FieldDeclaration) || x.IsKind (SyntaxKind.LocalDeclarationStatement));
							if (varDecls.Count () > 0) {
								var varDecl = varDecls.First ();
								if (varDecl.IsKind (SyntaxKind.FieldDeclaration)) {
									if ((varDecl as FieldDeclarationSyntax).Modifiers.Any (SyntaxKind.ConstKeyword) || (varDecl as FieldDeclarationSyntax).Modifiers.Any (SyntaxKind.StaticKeyword)) {
										return node.WithExpression (node.Expression.Accept (this) as ExpressionSyntax);									
									}
								} else if (varDecl.IsKind (SyntaxKind.LocalDeclarationStatement)) {
									if ((varDecl as FieldDeclarationSyntax).Modifiers.Any (SyntaxKind.ConstKeyword) || (varDecl as FieldDeclarationSyntax).Modifiers.Any (SyntaxKind.StaticKeyword)) {
										return node.WithExpression (node.Expression.Accept (this) as ExpressionSyntax);									
									}
								}
							}
						} 
					}
					var type = SyntaxFactory.ParseTypeName(model.GetTypeInfo(node).Type.ToDisplayString().Replace("*", ""));
					node = base.VisitMemberAccessExpression (node) as MemberAccessExpressionSyntax;
					return SyntaxFactory.InvocationExpression (
						expression: SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, node.Expression, SyntaxFactory.GenericName(SyntaxFactory.Identifier("GetField"), SyntaxFactory.TypeArgumentList().AddArguments(type))),
						argumentList: SyntaxFactory.ArgumentList ().AddArguments (SyntaxFactory.Argument (SyntaxFactory.SimpleLambdaExpression (SyntaxFactory.Parameter (SyntaxFactory.Identifier ("_")), node.WithExpression (SyntaxFactory.IdentifierName ("_")))))
					);
				}
			default:
				{
					return base.VisitMemberAccessExpression (node);
				}
			}
		}

		public override SyntaxNode VisitInvocationExpression (InvocationExpressionSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 1:
				{
					if (node.Expression.IsKind (SyntaxKind.SimpleMemberAccessExpression)) {
						var accessExpression = node.Expression as MemberAccessExpressionSyntax;
						var target = accessExpression.Expression;
						var method = model.GetSymbolInfo (node).Symbol as IMethodSymbol;
						if (method == null) {
							return node;
						}
						if (method.Locations.Any(loc => loc.IsInMetadata) || method.Locations.Any(loc => loc.IsInSource && !this.transformSources.Contains(loc.SourceTree.FilePath))) {
							return node.WithArgumentList(node.ArgumentList.Accept(this) as ArgumentListSyntax);
						} else {
							var targetExpression = target.Accept (this) as ExpressionSyntax;
							var typeArguments = SyntaxFactory.TypeArgumentList ();
							var lambdaParameters = SyntaxFactory.ParameterList ().AddParameters (SyntaxFactory.Parameter (SyntaxFactory.Identifier ("_"))).AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("s")));
							var lambdaBodyInvocationArugments = SyntaxFactory.ArgumentList ().AddArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("s")));
							for (int i = 0; i < node.ArgumentList.Arguments.Count; i++) {
								var argExpression = node.ArgumentList.Arguments.ElementAt(i).Expression;
								var t = SyntaxFactory.ParseTypeName(model.GetTypeInfo (argExpression).ConvertedType.ToDisplayString ());
								var p = SyntaxFactory.Parameter (SyntaxFactory.Identifier ("a" + i.ToString ()));
								var a = SyntaxFactory.Argument (SyntaxFactory.IdentifierName ("a" + i.ToString ()));
								typeArguments = typeArguments.AddArguments (t);
								lambdaParameters = lambdaParameters.AddParameters (p);
								lambdaBodyInvocationArugments = lambdaBodyInvocationArugments.AddArguments (a);
							}
							var retType = SyntaxFactory.ParseTypeName (model.GetTypeInfo (node).ConvertedType.ToDisplayString ());
							if(!((retType as PredefinedTypeSyntax) != null && (retType as PredefinedTypeSyntax).Keyword.IsKind(SyntaxKind.VoidKeyword))) {
								typeArguments.AddArguments (retType);
							}
							var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression (parameterList: lambdaParameters, body: node.WithExpression (accessExpression.WithExpression (SyntaxFactory.IdentifierName ("_"))).WithArgumentList (lambdaBodyInvocationArugments));
							string invoke_name = "";
							if (!method.IsVirtual && !method.IsOverride && method.ReceiverType.TypeKind != TypeKind.Interface) {
								if ((_inPropertyDecl || _inConstructorDecl) && accessExpression.Expression.IsKind (SyntaxKind.ThisExpression)) {
									return base.VisitInvocationExpression (node);
								}
								invoke_name = "InvokeMethod";
							} else {
								invoke_name = "InvokeDynamic";
							}
							SimpleNameSyntax invoke_name_and_type_params = node.ArgumentList.Arguments.Count > 0 ? (SimpleNameSyntax)SyntaxFactory.GenericName (SyntaxFactory.Identifier(invoke_name), typeArguments) : (SimpleNameSyntax)SyntaxFactory.IdentifierName(invoke_name);
							return SyntaxFactory.InvocationExpression (
								expression: SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, targetExpression, invoke_name_and_type_params),
								argumentList: node.ArgumentList.WithArguments ((node.ArgumentList.Accept (this) as ArgumentListSyntax).Arguments.Insert (0, SyntaxFactory.Argument (lambdaExpression)))
							);
						}
					}
					var retType2 = SyntaxFactory.ParseTypeName(model.GetTypeInfo(node).ConvertedType.ToDisplayString());
					SimpleNameSyntax invoke_name2 = (retType2 as PredefinedTypeSyntax) != null && (retType2 as PredefinedTypeSyntax).Keyword.IsKind(SyntaxKind.VoidKeyword) ? (SimpleNameSyntax)SyntaxFactory.IdentifierName("Invoke") :  (SimpleNameSyntax)SyntaxFactory.GenericName (SyntaxFactory.Identifier("Invoke"), SyntaxFactory.TypeArgumentList().AddArguments(retType2));
					node = base.VisitInvocationExpression (node) as InvocationExpressionSyntax;
					return SyntaxFactory.InvocationExpression (
						expression: SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, node.Expression, invoke_name2),
						argumentList: node.ArgumentList
					);
				}
			case 2:
			case 3:
			case 4:
				{
					if (node.Expression.IsKind (SyntaxKind.SimpleMemberAccessExpression)) {
						var accessExpression = node.Expression as MemberAccessExpressionSyntax;
						if (accessExpression.Name.Identifier.Text == "InvokeMethod" || accessExpression.Name.Identifier.Text == "InvokeDynamic" || accessExpression.Name.Identifier.Text == "InvokeBinary" || accessExpression.Name.Identifier.Text == "GetIndex") {
							accessExpression = accessExpression.WithExpression (accessExpression.Expression.Accept (this) as ExpressionSyntax);
							var args = SyntaxFactory.ArgumentList ();
							args = args.AddArguments (node.ArgumentList.Arguments.First ());
							foreach (var arg in node.ArgumentList.Arguments.Skip (1)) { // Skip first argument, which is lambda expression
								args = args.AddArguments (arg.Accept (this) as ArgumentSyntax);
							}
							return node.WithExpression (accessExpression).WithArgumentList (args);
						} else if (accessExpression.Name.Identifier.Text == "Invoke"  || accessExpression.Name.Identifier.Text == "Assign") {
							accessExpression = accessExpression.WithExpression (accessExpression.Expression.Accept (this) as ExpressionSyntax);
							var args = node.ArgumentList.Accept (this) as ArgumentListSyntax;
							return node.WithExpression (accessExpression).WithArgumentList (args);
						} else if (accessExpression.Name.Identifier.Text == "GetField") {
							return node.WithExpression (accessExpression.WithExpression(accessExpression.Expression.Accept(this) as ExpressionSyntax));
						}
						goto default;
					}
				}
			default:
				{
					return base.VisitInvocationExpression (node);
				}
			}
		}

		public override SyntaxNode VisitThisExpression (ThisExpressionSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 4:
				{
					if (_inPropertyDecl || _inConstructorDecl) {
						return node;
					}
					return SyntaxFactory.IdentifierName (@"self");
				}
			default:
				{
					return base.VisitThisExpression (node);
				}
			}
		}

		public override SyntaxNode VisitThrowStatement (ThrowStatementSyntax node)
		{
			return node;
		}

		public override SyntaxNode VisitAssignmentExpression (AssignmentExpressionSyntax node)
		{
			if (noRewrite)
				return node;
			switch (pass) {
			case 3:
				{
					if (_inConstructorDecl) {
						return base.VisitAssignmentExpression (node);
					}
					return SyntaxFactory.InvocationExpression (SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, node.Left.Accept (this) as ExpressionSyntax, SyntaxFactory.IdentifierName ("Assign")), SyntaxFactory.ArgumentList ().AddArguments (SyntaxFactory.Argument( node.Right.Accept(this) as ExpressionSyntax)));
				}
			default:
				{
					return base.VisitAssignmentExpression (node);
				}
			}			
		}
	}
}