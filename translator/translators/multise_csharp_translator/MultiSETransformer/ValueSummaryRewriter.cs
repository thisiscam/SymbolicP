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
		public const int NUM_PASSES = 3;

		private int pass = 0;
		private bool noRewrite = false, _inPropertyDecl = false;
		private SemanticModel model;

		public ValueSummaryRewriter(int pass, SemanticModel semanticModel)
		{
			this.pass = pass;
			this.model = semanticModel;
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
			case 2:
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
			case 2:
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
			case 2:
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
			case 2:
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
			case 2:
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
			case 2:
				{
					var left = node.Left.Accept(this) as ExpressionSyntax;
					var right = node.Right.Accept (this) as ExpressionSyntax;
					var lambdaParameters = SyntaxFactory.ParameterList().AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("l"))).AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("r")));
					var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression(parameterList: lambdaParameters, body: node.WithLeft(SyntaxFactory.IdentifierName("l")).WithRight(SyntaxFactory.IdentifierName("r")));
					return SyntaxFactory.InvocationExpression (
						expression: SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, left, SyntaxFactory.IdentifierName ("InvokeMethod")),
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
			case 2:
				{
					if (node.Operand.IsKind (SyntaxKind.NumericLiteralExpression) || node.Operand.IsKind (SyntaxKind.TrueLiteralExpression) || node.Operand.IsKind (SyntaxKind.FalseLiteralExpression) || node.Operand.IsKind (SyntaxKind.StringLiteralExpression)) {
						return base.VisitPrefixUnaryExpression (node);
					} else {
						var left = node.Operand as ExpressionSyntax;
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
			case 2:
				{
					if (node.Operand.IsKind (SyntaxKind.NumericLiteralExpression) || node.Operand.IsKind (SyntaxKind.TrueLiteralExpression) || node.Operand.IsKind (SyntaxKind.FalseLiteralExpression) || node.Operand.IsKind (SyntaxKind.StringLiteralExpression)) {
						return base.VisitPostfixUnaryExpression (node);
					} else {
						var left = node.Operand as ExpressionSyntax;
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
			case 2:
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
			case 2:
				{
					node = base.VisitObjectCreationExpression (node) as ObjectCreationExpressionSyntax;
					return node.WithType (SyntaxFactory.GenericName (SyntaxFactory.Identifier (@"ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.Type)));
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
			case 2:
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
						if (node.Parent.Kind() != SyntaxKind.SimpleMemberAccessExpression && (symbol.Kind == SymbolKind.Field || symbol.Kind == SymbolKind.Property)) {
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
					var containing_symbol = model.GetDeclaredSymbol (node).ContainingSymbol;
					node = base.VisitMethodDeclaration(node.WithParameterList(SyntaxFactory.ParameterList(node.ParameterList.Parameters.Insert(0, SyntaxFactory.Parameter(
						SyntaxFactory.Identifier(@"self")).WithType(SyntaxFactory.ParseTypeName(containing_symbol.Name)))))) as MethodDeclarationSyntax;
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
			case 1:
				{
					var symbol = model.GetSymbolInfo (node).Symbol;
					if (symbol != null && symbol.DeclaringSyntaxReferences.Length > 0) {
						var decl = model.GetSymbolInfo (node).Symbol.DeclaringSyntaxReferences.First ().GetSyntax();
						if(decl.IsKind(SyntaxKind.VariableDeclarator) && decl.Ancestors().First().IsKind(SyntaxKind.VariableDeclaration)){
							var varDecls = decl.Ancestors ().Where ((x, i) => x.IsKind(SyntaxKind.FieldDeclaration) || x.IsKind(SyntaxKind.LocalDeclarationStatement));
							if (varDecls.Count() > 0) {
								var varDecl = varDecls.First ();
								if (varDecl.IsKind(SyntaxKind.FieldDeclaration)) {
									if ((varDecl as FieldDeclarationSyntax).Modifiers.Any (SyntaxKind.ConstKeyword) || (varDecl as FieldDeclarationSyntax).Modifiers.Any (SyntaxKind.StaticKeyword)) {
										return node.WithExpression(node.Expression.Accept(this) as ExpressionSyntax);									
									}
								} else if (varDecl.IsKind(SyntaxKind.LocalDeclarationStatement)) {
									if ((varDecl as FieldDeclarationSyntax).Modifiers.Any (SyntaxKind.ConstKeyword) || (varDecl as FieldDeclarationSyntax).Modifiers.Any (SyntaxKind.StaticKeyword)) {
										return node.WithExpression(node.Expression.Accept(this) as ExpressionSyntax);									
									}
								}
							}
						}
					}
					if (_inPropertyDecl && node.Expression.IsKind (SyntaxKind.ThisExpression)) {
						return base.VisitMemberAccessExpression (node);
					}
					node = base.VisitMemberAccessExpression (node) as MemberAccessExpressionSyntax;
					return SyntaxFactory.InvocationExpression (
						expression: SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, node.Expression, SyntaxFactory.IdentifierName ("GetField")),
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
			case 0:
				{
					if (node.Expression.IsKind (SyntaxKind.SimpleMemberAccessExpression)) {
						var accessExpression = node.Expression as MemberAccessExpressionSyntax;
						var target = accessExpression.Expression;
						var method = model.GetSymbolInfo (node).Symbol as IMethodSymbol;
						if (method == null) {
							return node;
						} else if (!method.IsVirtual && !method.IsOverride && method.ReceiverType.TypeKind != TypeKind.Interface) {
							if (_inPropertyDecl && accessExpression.Expression.IsKind (SyntaxKind.ThisExpression)) {
								return base.VisitInvocationExpression (node);
							}
							var targetExpression = target.Accept(this) as ExpressionSyntax;
							var lambdaParameters = SyntaxFactory.ParameterList().AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_"))).AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("s")));
							var lambdaBodyInvocationArugments = SyntaxFactory.ArgumentList ().AddArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("s")));
							for (int i=0; i < node.ArgumentList.Arguments.Count; i++) {
								var p = SyntaxFactory.Parameter(SyntaxFactory.Identifier("a" + i.ToString()));
								var a = SyntaxFactory.Argument(SyntaxFactory.IdentifierName("a" + i.ToString()));
								lambdaParameters = lambdaParameters.AddParameters (p);
								lambdaBodyInvocationArugments = lambdaBodyInvocationArugments.AddArguments (a);
							}
							var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression(parameterList: lambdaParameters, body: node.WithExpression(accessExpression.WithExpression(SyntaxFactory.IdentifierName ("_"))).WithArgumentList(lambdaBodyInvocationArugments));
							return SyntaxFactory.InvocationExpression (
								expression: SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, targetExpression, SyntaxFactory.IdentifierName ("InvokeMethod")),
								argumentList: node.ArgumentList.WithArguments(node.ArgumentList.Arguments.Insert(0, SyntaxFactory.Argument(lambdaExpression)))
							);
						}
					}
					node = base.VisitInvocationExpression (node) as InvocationExpressionSyntax;
					return SyntaxFactory.InvocationExpression (
						expression: SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, node.Expression, SyntaxFactory.IdentifierName ("Invoke")),
						argumentList: node.ArgumentList
					);
				}
			case 1:
			case 2:
				{
					if (node.Expression.IsKind (SyntaxKind.SimpleMemberAccessExpression)) {
						var accessExpression = node.Expression as MemberAccessExpressionSyntax;
						if (accessExpression.Name.Identifier.Text == "InvokeMethod" || accessExpression.Name.Identifier.Text == "GetIndex") {
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
					}
					goto default;
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
			case 2:
				{
					if (_inPropertyDecl) {
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
			case 1:
				{
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