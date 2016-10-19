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

        public static readonly string KW_PMACHINE_STATES = "states";
        public static readonly string KW_PMACHINE_SCHEDULER = "scheduler";
        public static readonly string KW_PMACHINE_DEFERED_SET = "DeferedSet";
        public static readonly string KW_PMACHINE_IS_GOTO_TRANSITION = "IsGotoTransition";
        public static readonly string KW_PMACHINE_TRANSITIONS = "Transitions";
        public static readonly string KW_PMACHINE_EXIT_FUNCTIONS = "ExitFunctions";
        public static readonly string KW_PMACHINE_SENDQUEUE = "sendQueue";
        public static readonly string KW_PMONITOR = "monitor_";

        public static readonly HashSet<string> special_kw_fields = new HashSet<string> {
//			KW_PMACHINE_STATES,
			KW_PMACHINE_SCHEDULER,
            KW_PMACHINE_DEFERED_SET,
            KW_PMACHINE_IS_GOTO_TRANSITION,
            KW_PMACHINE_TRANSITIONS,
            KW_PMACHINE_EXIT_FUNCTIONS,
            KW_PMACHINE_SENDQUEUE,
            KW_PMONITOR
        };

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

        private SyntaxList<StatementSyntax> RemoveRange(SyntaxList<StatementSyntax> l, int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                l = l.RemoveAt(start);
            }
            return l;
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            switch (pass)
            {
                case 0: 
                    { 
                        var ret = base.VisitBlock(node) as BlockSyntax;
                        ret = ret.WithStatements(SyntaxFactory.List<StatementSyntax>(ret.Statements.Select((StatementSyntax stmt, int arg2) => 
                                {
                                    if (stmt.IsKind(SyntaxKind.IfStatement) || stmt.IsKind(SyntaxKind.ForStatement) || stmt.IsKind(SyntaxKind.WhileStatement))
                                    {
                                        var cond_name = String.Format("vs_cond_{0}", cond_cnt++);
                                        return stmt.WithAdditionalAnnotations(new SyntaxAnnotation("cond_var_name", cond_name));
                                    }
                                    else 
                                    {
                                        return stmt; 
                                    }
                                })));
                        return ret;
                    }
                case 1:
                    {
                        var ret = base.VisitBlock(node) as BlockSyntax;
                        if (inserted_logic_vars.ContainsKey(node))
                        {
                            foreach (var new_var_defn in inserted_logic_vars[node])
                            {
                                ret = ret.WithStatements(
                                    ret.Statements.Insert(
                                        0,
                                        SyntaxFactory.ParseStatement(
                                            String.Format("{0} {1};", new_var_defn.Item2, new_var_defn.Item1)
                                        )
                                    )
                                );
                            }
                        }
                        for (int i = ret.Statements.Count - 1; i >= 0; i--)
                        {
                            var stmt = ret.Statements[i];
                            if (stmt.IsKind(SyntaxKind.IfStatement) || stmt.IsKind(SyntaxKind.ForStatement) || stmt.IsKind(SyntaxKind.WhileStatement))
                            {
                                var cond_name = stmt.GetAnnotations("cond_var_name").First().Data;
                                var contains_ret = stmt.DescendantNodes().Any((arg) => arg.IsKind(SyntaxKind.ReturnStatement));
                                if (contains_ret) 
                                {
                                    var annotated_stmt = stmt.WithAdditionalAnnotations(new SyntaxAnnotation("has_merged_path", "true"));
                                    ret = ret.WithStatements(ret.Statements.Replace(stmt, annotated_stmt));
                                }

                                var enclosing_branch = node.Ancestors().FirstOrDefault((arg) => arg.IsKind(SyntaxKind.IfStatement) || arg.IsKind(SyntaxKind.ForStatement) || arg.IsKind(SyntaxKind.WhileStatement));
                                string merge_branch_inst = null;
                                if (enclosing_branch != null && contains_ret)
                                {
                                    var enclosing_branch_var_name = enclosing_branch.GetAnnotations("cond_var_name").First().Data;
                                    merge_branch_inst = String.Format("{0}.MergeBranch({1})", cond_name, enclosing_branch_var_name);
                                }
                                else 
                                {
                                    merge_branch_inst = String.Format("{0}.MergeBranch()", cond_name);
                                }
                                StatementSyntax merge_stmt = null;
                                if(!contains_ret) {
                                    merge_stmt = SyntaxFactory.ParseStatement(merge_branch_inst + ";");
                                    ret = ret.WithStatements(ret.Statements.Insert(i + 1, merge_stmt));
                                } 
                                else if (i == ret.Statements.Count - 1)
                                {
                                    merge_stmt = SyntaxFactory.ParseStatement(merge_branch_inst + ";");
                                    ret = ret.WithStatements(RemoveRange(ret.Statements, i + 1, ret.Statements.Count).Add(merge_stmt));
                                }
                                else 
                                { 
                                    var block = SyntaxFactory.Block(ret.Statements.Skip(i + 1));
                                    merge_stmt = SyntaxFactory.IfStatement(
                                            condition: SyntaxFactory.ParseExpression(merge_branch_inst),
                                            statement: block
                                    );
                                    ret = ret.WithStatements(RemoveRange(ret.Statements, i + 1, ret.Statements.Count).Add(merge_stmt));
                                }
                            }
                        }
                        return ret;
                    }
                case 4:
                    {
                        var ret = node;
                        for (int i = 0; i < ret.Statements.Count; i++)
                        {
                            if (ret.Statements[i].IsKind(SyntaxKind.IfStatement))
                            {
                                if (ret.Statements[i].HasAnnotations("cond_var_name"))
                                {
                                    var stmt = ret.Statements[i] as IfStatementSyntax;
                                    var cond_name = stmt.GetAnnotations("cond_var_name").First().Data;
                                    var if_prelude = SyntaxFactory.ParseStatement(String.Format("var {0} = ({1}).Cond();", cond_name, stmt.Condition.Accept(this).ToFullString()));
                                    ret = ret.WithStatements(ret.Statements.Insert(i, if_prelude));
                                    i++;
                                }
                            }
                            else if (ret.Statements[i].IsKind(SyntaxKind.ForStatement) || ret.Statements[i].IsKind(SyntaxKind.WhileStatement))
                            {
                                if (ret.Statements[i].HasAnnotations("cond_var_name"))
                                {
                                    var cond_name = ret.Statements[i].GetAnnotations("cond_var_name").First().Data;
                                    var loop_prelude = SyntaxFactory.ParseStatement(String.Format("var {0} = PathConstraint.BeginLoop();", cond_name));
                                    ret = ret.WithStatements(ret.Statements.Insert(i, loop_prelude));
                                    i++;
                                }
                            }
                        }
                        return base.VisitBlock(ret) as BlockSyntax;
                    }
                default:
                    {
                        return base.VisitBlock(node);
                    }
            }
        }

        public override SyntaxNode VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node)
        {
            if (node.DirectiveNameToken.Text == "multisenorewrite")
            {
                this.noRewrite = true;
            }
            return base.VisitRegionDirectiveTrivia(node);
        }

        public override SyntaxNode VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax node)
        {
            if (node.DirectiveNameToken.Text == "multisenorewrite")
            {
                this.noRewrite = false;
            }
            return base.VisitEndRegionDirectiveTrivia(node);
        }

        public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 4:
                    {
                        node = base.VisitVariableDeclaration(node) as VariableDeclarationSyntax;
                        node = node.WithVariables(SyntaxFactory.SeparatedList(node.Variables.Select(v =>
                        {
                            if (v.Identifier.Text.StartsWith("vs_lgc_tmp_") || v.Identifier.Text.StartsWith("vs_ret_") || v.Identifier.Text.StartsWith("vs_cond_"))
                            {
                                return v;
                            }
                            else if (v.Initializer == null)
                            {
                                return v.WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.GenericName(SyntaxFactory.Identifier("ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.Type)))
                                .WithNewKeyword(SyntaxFactory.Token(SyntaxKind.NewKeyword).WithTrailingTrivia(SyntaxFactory.ParseTrailingTrivia(" ")))
                                .WithArgumentList(SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(SyntaxFactory.DefaultExpression(node.Type))))));
                            }
                            else if (v.Initializer.Value.IsKind(SyntaxKind.NumericLiteralExpression) || v.Initializer.Value.IsKind(SyntaxKind.NullLiteralExpression))
                            {
                                return v;
                            }
                            else if (v.Initializer.IsKind(SyntaxKind.IdentifierName))
                            {
                                return v.WithInitializer(SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.GenericName(SyntaxFactory.Identifier("ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.Type)), SyntaxFactory.IdentifierName("InitializeFrom")),
                                        SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(new ArgumentSyntax[] { SyntaxFactory.Argument(v.Initializer.Value) }))
                                )));
                            }
                            else {
                                return v;
                            }
                        })));
                        if (!node.Type.IsVar)
                        {
                            node = node.WithType(SyntaxFactory.GenericName(SyntaxFactory.Identifier("ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.Type))); ;
                        }
                        return node;
                    }
                default:
                    {
                        return base.VisitVariableDeclaration(node);
                    }
            }
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (noRewrite)
                return node;

            if (node.Declaration.Variables.Any((arg => IsSpecialKW(arg.Identifier.Text))))
            {
                return node;
            }

            switch (pass)
            {
                case 4:
                    {
                        if (node.Modifiers.Any(SyntaxKind.ConstKeyword) || node.Modifiers.Any(SyntaxKind.StaticKeyword))
                        {
                            return node;
                        }
                        goto default;
                    }
                default:
                    {
                        return base.VisitFieldDeclaration(node);
                    }
            }
        }

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 4:
                    {
                        if (node.Modifiers.Any(SyntaxKind.ConstKeyword) || node.Modifiers.Any(SyntaxKind.StaticKeyword))
                        {
                            return node;
                        }
                        goto default;
                    }
                default:
                    {
                        return base.VisitLocalDeclarationStatement(node);
                    }
            }
        }

        public override SyntaxNode VisitParameter(ParameterSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 4:
                    {
                        node = base.VisitParameter(node) as ParameterSyntax;
                        return node.WithType(SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.Type)));
                    }
                default:
                    {
                        return base.VisitParameter(node);
                    }
            }
        }

        public override SyntaxNode VisitArrayType(ArrayTypeSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 4:
                    {
                        node = base.VisitArrayType(node) as ArrayTypeSyntax;
                        return node.WithElementType(SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.ElementType)));
                    }
                default:
                    {
                        return base.VisitArrayType(node);
                    }
            }
        }

        private static Dictionary<string, string> binOperatorMap = new Dictionary<string, string> {
            {"+", "Add"},
            {"-", "Sub"},
            {"*", "Mul"},
            {"/", "Div"},
            {"%", "Mod"},
            {"==", "EE"},
            {"!=", "NE"},
            {">", "GT"},
            {"<", "LT"},
            {">=", "GE"},
            {"<=", "LE"},
        };

        private string SymbolicTypeAvoidCastHelper(ExpressionSyntax node)
        {
            var t0 = model.GetTypeInfo(node).Type.ToDisplayString();
            var t1 = model.GetTypeInfo(node).ConvertedType.ToDisplayString();
            string t;
            if ((t0 == "bool" && t1 == "SymbolicBool") || (t0 == "int" && t1 == "SymbolicInteger"))
            {
                t = t1;
            }
            else {
                t = t0;
            }
            return t;
        }

        private int vs_logic_var_count = 0;
        private Dictionary<BlockSyntax, List<Tuple<string, string>>> inserted_logic_vars = new Dictionary<BlockSyntax, List<Tuple<string, string>>>();
        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 1:
                    {
                        if (node.IsKind(SyntaxKind.LogicalAndExpression))
                        {
                            var new_var_name = String.Format("vs_lgc_tmp_{0}", vs_logic_var_count++);
                            var enclosing_block = node.Ancestors().Where((n, i) => n.IsKind(SyntaxKind.Block)).First() as BlockSyntax;
                            if (!inserted_logic_vars.ContainsKey(enclosing_block))
                            {
                                inserted_logic_vars.Add(enclosing_block, new List<Tuple<string, string>>());
                            }
                            inserted_logic_vars[enclosing_block].Add(Tuple.Create(new_var_name, model.GetTypeInfo(node).Type.ToDisplayString()));
                            return SyntaxFactory.ParenthesizedExpression(
                                    SyntaxFactory.ConditionalExpression(
                                    SyntaxFactory.ParenthesizedExpression(
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName(new_var_name),
                                            node.Left.Accept(this) as ExpressionSyntax)
                                    ),
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.BitwiseAndExpression,
                                        SyntaxFactory.IdentifierName(new_var_name),
                                        node.Right.Accept(this) as ExpressionSyntax),
                                    SyntaxFactory.IdentifierName(new_var_name)
                                ));
                        }
                        else if (node.IsKind(SyntaxKind.LogicalOrExpression))
                        {
                            var new_var_name = String.Format("vs_lgc_tmp_{0}", vs_logic_var_count++);
                            var enclosing_block = node.Ancestors().Where((n, i) => n.IsKind(SyntaxKind.Block)).First() as BlockSyntax;
                            if (!inserted_logic_vars.ContainsKey(enclosing_block))
                            {
                                inserted_logic_vars.Add(enclosing_block, new List<Tuple<string, string>>());
                            }
                            inserted_logic_vars[enclosing_block].Add(Tuple.Create(new_var_name, model.GetTypeInfo(node).Type.ToDisplayString()));
                            return SyntaxFactory.ParenthesizedExpression(
                                SyntaxFactory.ConditionalExpression(
                                    SyntaxFactory.ParenthesizedExpression(
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName(new_var_name),
                                            node.Left.Accept(this) as ExpressionSyntax)
                                    ),
                                    SyntaxFactory.IdentifierName(new_var_name),
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.BitwiseOrExpression,
                                        SyntaxFactory.IdentifierName(new_var_name),
                                        node.Right.Accept(this) as ExpressionSyntax)
                                ));
                        }
                        else {
                            goto default;
                        }
                    }
                case 2:
                    {
                        var leftType = model.GetTypeInfo(node.Left).ConvertedType;
                        var rightType = model.GetTypeInfo(node.Right).ConvertedType;
                        if (node.Left.IsKind(SyntaxKind.ThisExpression) || leftType.Name == "String" || rightType.Name == "String")
                        {
                            goto default;
                        }
                        var left = node.Left.Accept(this) as ExpressionSyntax;
                        var right = node.Right.Accept(this) as ExpressionSyntax;
                        var lambdaParameters = SyntaxFactory.ParameterList().AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("l"))).AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("r")));
                        var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression(parameterList: lambdaParameters, body: node.WithLeft(SyntaxFactory.IdentifierName("l")).WithRight(SyntaxFactory.IdentifierName("r")));
                        var t = SymbolicTypeAvoidCastHelper(node);
                        var typeArguments = SyntaxFactory.TypeArgumentList().AddArguments(SyntaxFactory.ParseTypeName(rightType.ToDisplayString()), SyntaxFactory.ParseTypeName(t));
                        return SyntaxFactory.InvocationExpression(
                            expression: SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, left, SyntaxFactory.GenericName(SyntaxFactory.Identifier("InvokeBinary"), typeArguments)),
                            argumentList: SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(lambdaExpression)).AddArguments(SyntaxFactory.Argument(right)));
                    }
                default:
                    {
                        return base.VisitBinaryExpression(node);
                    }
            }
        }

        public override SyntaxNode VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            if (noRewrite)
            {
                return node;
            }
            switch (pass)
            {
                case 2:
                    {
                        if (node.Operand.IsKind(SyntaxKind.NumericLiteralExpression) || node.Operand.IsKind(SyntaxKind.TrueLiteralExpression) || node.Operand.IsKind(SyntaxKind.FalseLiteralExpression) || node.Operand.IsKind(SyntaxKind.StringLiteralExpression))
                        {
                            return base.VisitPrefixUnaryExpression(node);
                        }
                        else if (node.OperatorToken.IsKind(SyntaxKind.PlusPlusToken))
                        {
                            return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node.Operand, SyntaxFactory.IdentifierName("Increment")));
                        }
                        else if (node.OperatorToken.IsKind(SyntaxKind.MinusMinusToken))
                        {
                            return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node.Operand, SyntaxFactory.IdentifierName("Decrement")));
                        }
                        else {
                            var operand = node.Operand.Accept(this) as ExpressionSyntax;
                            var lambdaExpression = SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_")), node.WithOperand(SyntaxFactory.IdentifierName("_")));
                            var t = SymbolicTypeAvoidCastHelper(node);
                            var typeArguments = SyntaxFactory.TypeArgumentList().AddArguments(SyntaxFactory.ParseTypeName(t));
                            return SyntaxFactory.InvocationExpression(
                                expression: SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, operand, SyntaxFactory.GenericName(SyntaxFactory.Identifier("InvokeUnary"), typeArguments)),
                                argumentList: SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(lambdaExpression)));
                        }
                    }
                default:
                    {
                        return base.VisitPrefixUnaryExpression(node);
                    }
            }
        }

        public override SyntaxNode VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            if (noRewrite)
            {
                return node;
            }
            switch (pass)
            {
                case 2:
                    {
                        if (node.Operand.IsKind(SyntaxKind.NumericLiteralExpression) || node.Operand.IsKind(SyntaxKind.TrueLiteralExpression) || node.Operand.IsKind(SyntaxKind.FalseLiteralExpression) || node.Operand.IsKind(SyntaxKind.StringLiteralExpression))
                        {
                            return base.VisitPostfixUnaryExpression(node);
                        }
                        else if (node.OperatorToken.IsKind(SyntaxKind.PlusPlusToken))
                        {
                            return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node.Operand, SyntaxFactory.IdentifierName("Increment")));
                        }
                        else if (node.OperatorToken.IsKind(SyntaxKind.MinusMinusToken))
                        {
                            return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node.Operand, SyntaxFactory.IdentifierName("Decrement")));
                        }
                        else {
                            var operand = node.Operand.Accept(this) as ExpressionSyntax;
                            var lambdaExpression = SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_")), node.WithOperand(SyntaxFactory.IdentifierName("_")));
                            var typeArguments = SyntaxFactory.TypeArgumentList().AddArguments(SyntaxFactory.ParseTypeName(model.GetTypeInfo(node).ConvertedType.ToDisplayString()));
                            return SyntaxFactory.InvocationExpression(
                                expression: SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, operand, SyntaxFactory.GenericName(SyntaxFactory.Identifier("InvokeUnary"), typeArguments)),
                                argumentList: SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(lambdaExpression)));
                        }
                    }
                default:
                    {
                        return base.VisitPostfixUnaryExpression(node);
                    }
            }
        }

        public override SyntaxNode VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {

            if (noRewrite)
                return node;
            switch (pass)
            {
                case 2:
                    {
                        if (node.Expression.IsKind(SyntaxKind.ThisExpression))
                        {
                            return node.WithArgumentList(node.ArgumentList.Accept(this) as BracketedArgumentListSyntax);
                        }
                        else if (node.Expression.ChildNodes().Any((n) =>
                        {
                            var s = model.GetSymbolInfo(n);
                            return s.Symbol != null && s.Symbol.Kind == SymbolKind.Field && IsSpecialKW(s.Symbol.Name);
                        }) && node.Expression.ChildNodes().Any((n) => n.IsKind(SyntaxKind.ThisExpression)))
                        {
                            return SyntaxFactory.InvocationExpression(
                                expression: SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    node.Expression.Accept(this) as ExpressionSyntax,
                                    SyntaxFactory.IdentifierName("GetIndex")),
                                argumentList: SyntaxFactory.ArgumentList((node.ArgumentList.Accept(this) as BracketedArgumentListSyntax).Arguments)
                            );
                        }
                        TypeSyntax elementType = null;
                        bool need_cast = false;
                        var converted_type = ConvertedType(node, ref need_cast, ref elementType);
                        var targetExpression = node.Expression.Accept(this) as ExpressionSyntax;
                        var lambdaParameters = SyntaxFactory.ParameterList().AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_")));
                        var invocationTypeParams = SyntaxFactory.TypeArgumentList();
                        var lambdaBodyInvocationArugments = SyntaxFactory.BracketedArgumentList();
                        for (int i = 0; i < node.ArgumentList.Arguments.Count; i++)
                        {
                            var argExpression = node.ArgumentList.Arguments.ElementAt(i).Expression;
                            var t = SyntaxFactory.ParseTypeName(model.GetTypeInfo(argExpression).Type.ToDisplayString());
                            var p = SyntaxFactory.Parameter(SyntaxFactory.Identifier("a" + i.ToString()));
                            var a = SyntaxFactory.Argument(SyntaxFactory.IdentifierName("a" + i.ToString()));
                            lambdaParameters = lambdaParameters.AddParameters(p);
                            lambdaBodyInvocationArugments = lambdaBodyInvocationArugments.AddArguments(a);
                            invocationTypeParams = invocationTypeParams.AddArguments(t);
                        }
                        invocationTypeParams = invocationTypeParams.AddArguments(elementType);
                        var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression(parameterList: lambdaParameters, body: node.WithExpression(SyntaxFactory.IdentifierName("_")).WithArgumentList(lambdaBodyInvocationArugments));
                        ExpressionSyntax ret;
                        if (model.GetTypeInfo(node.Expression).Type.TypeKind != TypeKind.Array)
                        {
                            ret = SyntaxFactory.InvocationExpression(
                                expression: SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    targetExpression,
                                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("InvokeMethod"), invocationTypeParams)),
                                argumentList: SyntaxFactory.ArgumentList((node.ArgumentList.Accept(this) as BracketedArgumentListSyntax).Arguments.Insert(0, SyntaxFactory.Argument(lambdaExpression)))
                            );
                        }
                        else {
                            ret = SyntaxFactory.InvocationExpression(
                                expression: SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    targetExpression,
                                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("GetIndex"), invocationTypeParams.WithArguments(invocationTypeParams.Arguments.RemoveAt(0)))),
                                argumentList: SyntaxFactory.ArgumentList((node.ArgumentList.Accept(this) as BracketedArgumentListSyntax).Arguments)
                            );
                        }
                        if (need_cast)
                        {
                            return CastTypeNode(ret, converted_type);
                        }
                        else {
                            return ret;
                        }
                    }
                default:
                    {
                        return base.VisitElementAccessExpression(node);
                    }
            }
        }

        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 2:
                    {
                        var originalNode = node;
                        bool need_cast = false;
                        TypeSyntax retType = null;
                        var converted_type = ConvertedType(node, ref need_cast, ref retType);
                        if (!need_cast)
                        {
                            converted_type = retType;
                        }
                        node = base.VisitObjectCreationExpression(node) as ObjectCreationExpressionSyntax;
                        return node.WithType(SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(converted_type))).WithArgumentList(SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(node)));
                    }
                default:
                    {
                        return base.VisitObjectCreationExpression(node);
                    }
            }
        }

        public override SyntaxNode VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 2:
                    {
                        node = base.VisitArrayCreationExpression(node) as ArrayCreationExpressionSyntax;
                        return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.GenericName(SyntaxFactory.Identifier("ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.Type.ElementType)), SyntaxFactory.IdentifierName("NewVSArray")), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(node.Type.RankSpecifiers.First().Sizes.Select((arg1, arg2) => SyntaxFactory.Argument(arg1)))));
                    }
                default:
                    {
                        return base.VisitArrayCreationExpression(node);
                    }
            }
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 4:
                    {
                        node = base.VisitPropertyDeclaration(node) as PropertyDeclarationSyntax;
                        return node.WithType(SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.Type)));
                    }
                default:
                    {
                        var ret = base.VisitPropertyDeclaration(node);
                        return ret;
                    }
            }
        }

        public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            _inPropertyDecl = true;
            var return_cnt = node.DescendantNodes().Count((arg) => arg.IsKind(SyntaxKind.ReturnStatement));
            handle_return = !(return_cnt == 0 || (return_cnt == 1 && (node.Body.IsKind(SyntaxKind.ReturnStatement) || (node.Body.IsKind(SyntaxKind.Block) && (node.Body as BlockSyntax).Statements.Last().IsKind(SyntaxKind.ReturnStatement)))));
            var ret = base.VisitAccessorDeclaration(node);
            _inPropertyDecl = false;
            return ret;
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            _inConstructorDecl = true;
            var ret = base.VisitConstructorDeclaration(node);
            _inConstructorDecl = false;
            return ret;
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 1:
                    {
                        var symbol = model.GetSymbolInfo(node).Symbol;
                        if (symbol == null && model.GetSymbolInfo(node).CandidateSymbols.Count() > 0)
                        {
                            symbol = model.GetSymbolInfo(node).CandidateSymbols.First();
                        }
                        if (symbol != null)
                        {
                            if (node.Parent.Kind() != SyntaxKind.SimpleMemberAccessExpression && !symbol.IsStatic && (symbol.Kind == SymbolKind.Field || symbol.Kind == SymbolKind.Property || symbol.Kind == SymbolKind.Method))
                            {
                                return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName(node.Identifier));
                            }
                        }
                        goto default;
                    }
                case 2:
                {
                    var need_cast = false;
                    TypeSyntax nodeType = null, type = null;
                    if(node.Parent.IsKind(SyntaxKind.Argument) && node.Parent.Parent.IsKind(SyntaxKind.ArgumentList)) {
                        type = ConvertedType(node, ref need_cast, ref nodeType);
                        if (need_cast)
                           return CastTypeNode(base.VisitIdentifierName(node) as IdentifierNameSyntax, type);
                    }
                    goto default;
                }
                default:
                    {
                        return base.VisitIdentifierName(node);
                    }
            }
        }

        private int method_cnt = 0;
        private bool handle_return = false; // TODO remove this reduntant state variable
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
           if (noRewrite)
                return node;
           switch (pass)
            {
                case 0:
                    {
                        var return_cnt = node.DescendantNodes().Count((arg) => arg.IsKind(SyntaxKind.ReturnStatement));
                        handle_return = !(return_cnt == 0 || (return_cnt == 1 && (node.Body.IsKind(SyntaxKind.ReturnStatement) || (node.Body.IsKind(SyntaxKind.Block) && (node.Body as BlockSyntax).Statements.Last().IsKind(SyntaxKind.ReturnStatement))))); 
                        return base.VisitMethodDeclaration(node)
                                .WithAdditionalAnnotations(
                                    new SyntaxAnnotation("method_id", (method_cnt++).ToString()),
                                    new SyntaxAnnotation("handle_return", handle_return ? "t" : "f")
                            );        
                    }
                //case 1:
                //    {
                //        handle_return = node.GetAnnotations("handle_return").First().Data.Equals("t");
                //        goto default;
                //    }
                case 3:
                    {
                        var not_returns_void = (node.ReturnType as PredefinedTypeSyntax) == null || (node.ReturnType as PredefinedTypeSyntax).Keyword.Kind() != SyntaxKind.VoidKeyword;
                        
                        handle_return = node.GetAnnotations("handle_return").First().Data.Equals("t");
                        node = base.VisitMethodDeclaration(node) as MethodDeclarationSyntax;
                        
                        var ret_var_name = String.Format("vs_ret_{0}", node.GetAnnotations("method_id").First().Data);
                        
                        if (not_returns_void)
                        {
                            node = node.WithReturnType(SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.ReturnType)));
                        }

                        if (node.Body != null && handle_return)
                        {
                            var push_frame = SyntaxFactory.ParseStatement("PathConstraint.PushFrame();");
                            var pop_frame = SyntaxFactory.ParseStatement("PathConstraint.PopFrame();");
                            if (not_returns_void)
                            {
                                var ret_var_decl = SyntaxFactory.ParseStatement(String.Format("var {0} = new {1}();", ret_var_name, node.ReturnType.ToFullString()));
                                var ret_var_stmt = SyntaxFactory.ParseStatement(String.Format("return {0};", ret_var_name));
                                node = node.WithBody(node.Body.WithStatements(node.Body.Statements.Insert(0, ret_var_decl).Insert(0, push_frame).Add(pop_frame).Add(ret_var_stmt)));
                            }
                            else
                            {
                                node = node.WithBody(node.Body.WithStatements(node.Body.Statements.Insert(0, push_frame).Add(pop_frame)));
                            }
                        }
                        return node;
                    }
                default:
                    {
                        return base.VisitMethodDeclaration(node);
                    }
            }
        }

        public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 3:
                    {
                        node = base.VisitDelegateDeclaration(node) as DelegateDeclarationSyntax;
                        if ((node.ReturnType as PredefinedTypeSyntax) == null || (node.ReturnType as PredefinedTypeSyntax).Keyword.Kind() != SyntaxKind.VoidKeyword)
                        {
                            node = node.WithReturnType(SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.ReturnType)));
                        }
                        return node;
                    }
                default:
                    {
                        return base.VisitDelegateDeclaration(node);
                    }
            }
        }

        public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 3:
                    {
                        node = base.VisitIndexerDeclaration(node) as IndexerDeclarationSyntax;
                        if ((node.Type as PredefinedTypeSyntax) == null || (node.Type as PredefinedTypeSyntax).Keyword.Kind() != SyntaxKind.VoidKeyword)
                        {
                            node = node.WithType(SyntaxFactory.GenericName(SyntaxFactory.Identifier(@"ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(node.Type)));
                        }
                        return node;
                    }
                default:
                    {
                        return base.VisitIndexerDeclaration(node);
                    }
            }
        }

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (noRewrite)
                return node;

            bool need_cast = false;
            TypeSyntax nodeType = null, type = null;

            switch (pass)
            {
                case 2:
                    {
                        if (node.Expression.IsKind(SyntaxKind.ThisExpression))
                        {
                            type = ConvertedType(node, ref need_cast, ref nodeType);
                            if (need_cast)
                                return CastTypeNode(base.VisitMemberAccessExpression(node) as MemberAccessExpressionSyntax, type);
                            return base.VisitMemberAccessExpression(node);
                        }
                        var symbol = model.GetSymbolInfo(node).Symbol;
                        if (symbol == null || symbol.Locations.Any(loc => loc.IsInMetadata) || symbol.Locations.Any(loc => loc.IsInSource && !this.transformSources.Contains(loc.SourceTree.FilePath)))
                        {
                            return base.VisitMemberAccessExpression(node);
                        }
                        else if (symbol.DeclaringSyntaxReferences.Length > 0)
                        {
                            var decl = model.GetSymbolInfo(node).Symbol.DeclaringSyntaxReferences.First().GetSyntax();
                            if (decl.IsKind(SyntaxKind.VariableDeclarator) && decl.Ancestors().First().IsKind(SyntaxKind.VariableDeclaration))
                            {
                                var varDecls = decl.Ancestors().Where((x, i) => x.IsKind(SyntaxKind.FieldDeclaration) || x.IsKind(SyntaxKind.LocalDeclarationStatement));
                                if (varDecls.Count() > 0)
                                {
                                    var varDecl = varDecls.First();
                                    if (varDecl.IsKind(SyntaxKind.FieldDeclaration))
                                    {
                                        if ((varDecl as FieldDeclarationSyntax).Modifiers.Any(SyntaxKind.ConstKeyword) || (varDecl as FieldDeclarationSyntax).Modifiers.Any(SyntaxKind.StaticKeyword))
                                        {
                                            var t = model.GetTypeInfo(node);
                                            node = node.WithExpression(node.Expression.Accept(this) as ExpressionSyntax);
                                            if (!t.Type.Equals(t.ConvertedType))
                                            {
                                                return SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName(t.ConvertedType.ToDisplayString()), node);
                                            }
                                            else {
                                                return node;
                                            }
                                        }
                                    }
                                    else if (varDecl.IsKind(SyntaxKind.LocalDeclarationStatement))
                                    {
                                        if ((varDecl as FieldDeclarationSyntax).Modifiers.Any(SyntaxKind.ConstKeyword) || (varDecl as FieldDeclarationSyntax).Modifiers.Any(SyntaxKind.StaticKeyword))
                                        {
                                            var t = model.GetTypeInfo(node);
                                            node = node.WithExpression(node.Expression.Accept(this) as ExpressionSyntax);
                                            if (!t.Type.Equals(t.ConvertedType))
                                            {
                                                return SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName(t.ConvertedType.ToDisplayString()), node);
                                            }
                                            else {
                                                return node;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        string invoke_name = "GetField";
                        if (node.ChildNodes().Any((n) =>
                        {
                            var s = model.GetSymbolInfo(n);
                            return s.Symbol != null && s.Symbol.Kind == SymbolKind.Field && IsSpecialKW(s.Symbol.Name);
                        }))
                        {
                            invoke_name = "GetConstField";
                        }
                        type = ConvertedType(node, ref need_cast, ref nodeType);
                        node = base.VisitMemberAccessExpression(node) as MemberAccessExpressionSyntax;
                        if (need_cast)
                        {
                            var ret = SyntaxFactory.InvocationExpression(
                                expression: SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node.Expression, SyntaxFactory.GenericName(SyntaxFactory.Identifier(invoke_name), SyntaxFactory.TypeArgumentList().AddArguments(nodeType))),
                                argumentList: SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_")), node.WithExpression(SyntaxFactory.IdentifierName("_")))))
                            );
                            return CastTypeNode(ret, type);
                        }
                        else {
                            return SyntaxFactory.InvocationExpression(
                                expression: SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node.Expression, SyntaxFactory.GenericName(SyntaxFactory.Identifier(invoke_name), SyntaxFactory.TypeArgumentList().AddArguments(type))),
                                argumentList: SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_")), node.WithExpression(SyntaxFactory.IdentifierName("_")))))
                            );
                        }
                    }
                default:
                    {
                        return base.VisitMemberAccessExpression(node);
                    }
            }
        }

        private SyntaxNode CastTypeNode(ExpressionSyntax exp, TypeSyntax toType)
        {
            var lambdaExpression = SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_")), SyntaxFactory.CastExpression(toType, SyntaxFactory.IdentifierName("_")));
            return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, exp, SyntaxFactory.GenericName(SyntaxFactory.Identifier("Cast"), SyntaxFactory.TypeArgumentList().AddArguments(toType))), SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(lambdaExpression)));
        }

        private TypeSyntax ConvertedType(SyntaxNode node, ref bool need_cast, ref TypeSyntax nodeType)
        {
            var t = model.GetTypeInfo(node);
            nodeType = WrapType(SyntaxFactory.ParseTypeName(t.Type.ToDisplayString().Replace('*', ' ')));
            need_cast = false;
            if (node.Parent.IsKind(SyntaxKind.Argument))
            {
                int arg_idx = -1;
                if (node.Parent.Parent.IsKind(SyntaxKind.BracketedArgumentList))
                {
                    arg_idx = (node.Parent.Parent as BracketedArgumentListSyntax).Arguments.IndexOf(arg => arg == node.Parent);
                }
                else if (node.Parent.Parent.IsKind(SyntaxKind.ArgumentList))
                {
                    arg_idx = (node.Parent.Parent as ArgumentListSyntax).Arguments.IndexOf(arg => arg == node.Parent);
                }
                else {
                    throw new SystemException("Should not reach here");
                }
                var symbol = model.GetSymbolInfo(node.Parent.Parent.Parent).Symbol;
                string str = null;
                if (symbol == null)
                {
                    return nodeType;
                }
                switch (symbol.Kind)
                {
                    case SymbolKind.Property:
                        {
                            str = (symbol as IPropertySymbol).OriginalDefinition.Parameters.ElementAt(arg_idx).Type.ToDisplayString().Replace('*', ' ');
                            break;
                        }
                    case SymbolKind.Method:
                        {
                            str = (symbol as IMethodSymbol).OriginalDefinition.Parameters.ElementAt(arg_idx).Type.ToDisplayString().Replace('*', ' ');
                            break;
                        }
                    default:
                        {
                            throw new SystemException("Should not reach here");
                        }
                }
                if (str.StartsWith("PMachine") || str.StartsWith("IPType") || str.StartsWith("int") || str.StartsWith("PInteger") || str.StartsWith("SymbolicInteger") || str.StartsWith("bool") || str.StartsWith("SymbolicBool") || str.StartsWith("PBool"))
                {
                    if (!t.Type.ToDisplayString().Equals(str))
                    {
                        need_cast = true;
                        return WrapType(SyntaxFactory.ParseTypeName(str));
                    }
                }
            }
            if (t.Type != t.ConvertedType)
            {
                need_cast = true;
            }
            return WrapType(SyntaxFactory.ParseTypeName(t.ConvertedType.ToDisplayString().Replace('*', ' ')));
        }

        private TypeSyntax WrapType(TypeSyntax t)
        {
            TypeSyntax ret;
            if (t.IsKind(SyntaxKind.ArrayType))
            {
                var t1 = t as ArrayTypeSyntax;
                ret = t1.WithElementType(SyntaxFactory.GenericName(SyntaxFactory.Identifier("ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(WrapType(t1.ElementType))));
            }
            else {
                ret = t;
            }
            return ret;
        }

        private bool IsSpecialKW(string name)
        {
            return special_kw_fields.Any(kw => name.Contains(kw));
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 2:
                    {
                        if (node.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                        {
                            var accessExpression = node.Expression as MemberAccessExpressionSyntax;
                            var target = accessExpression.Expression;
                            var accessSymbol = model.GetSymbolInfo(accessExpression.Expression);
                            var method = model.GetSymbolInfo(node).Symbol as IMethodSymbol;
                            if (method == null)
                            {
                                return node;
                            }
                            else if (accessExpression.Expression.IsKind(SyntaxKind.ThisExpression) || method.Locations.Any(loc => loc.IsInMetadata) || method.Locations.Any(loc => loc.IsInSource && !this.transformSources.Contains(loc.SourceTree.FilePath)))
                            {
                                if (accessExpression.Name.Identifier.Text.Contains("ToString"))
                                {
                                    return base.VisitInvocationExpression(node);
                                }
                                return node.WithArgumentList(node.ArgumentList.Accept(this) as ArgumentListSyntax);
                            }
                            else if (accessSymbol.Symbol != null && accessSymbol.Symbol.Kind == SymbolKind.Field && IsSpecialKW(accessSymbol.Symbol.Name))
                            {
                                return node.WithArgumentList(node.ArgumentList.Accept(this) as ArgumentListSyntax);
                            }
                            else if (method.OriginalDefinition.IsStatic)
                            {
                                return node.WithArgumentList(node.ArgumentList.Accept(this) as ArgumentListSyntax);
                            }
                            else {
                                var targetExpression = target.Accept(this) as ExpressionSyntax;
                                var typeArguments = SyntaxFactory.TypeArgumentList();
                                var lambdaParameters = SyntaxFactory.ParameterList().AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_")));
                                var lambdaBodyInvocationArugments = SyntaxFactory.ArgumentList();
                                for (int i = 0; i < node.ArgumentList.Arguments.Count; i++)
                                {
                                    var argExpression = node.ArgumentList.Arguments.ElementAt(i).Expression;
                                    var _t = model.GetTypeInfo(argExpression).ConvertedType;
                                    var t = SyntaxFactory.ParseTypeName(_t.ToDisplayString());
                                    var p = SyntaxFactory.Parameter(SyntaxFactory.Identifier("a" + i.ToString()));
                                    var a = SyntaxFactory.Argument(SyntaxFactory.IdentifierName("a" + i.ToString()));
                                    typeArguments = typeArguments.AddArguments(t);
                                    lambdaParameters = lambdaParameters.AddParameters(p);
                                    lambdaBodyInvocationArugments = lambdaBodyInvocationArugments.AddArguments(a);
                                }
                                TypeSyntax retType = null;
                                bool need_cast = false;
                                var converted_type = ConvertedType(node, ref need_cast, ref retType);
                                if (!((retType as PredefinedTypeSyntax) != null && (retType as PredefinedTypeSyntax).Keyword.IsKind(SyntaxKind.VoidKeyword)))
                                {
                                    typeArguments = typeArguments.AddArguments(retType);
                                }
                                var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression(parameterList: lambdaParameters, body: node.WithExpression(accessExpression.WithExpression(SyntaxFactory.IdentifierName("_"))).WithArgumentList(lambdaBodyInvocationArugments));
                                string invoke_name = "";
                                if (!method.IsVirtual && !method.IsOverride && method.ReceiverType.TypeKind != TypeKind.Interface)
                                {
                                    if ((_inPropertyDecl || _inConstructorDecl) && accessExpression.Expression.IsKind(SyntaxKind.ThisExpression))
                                    {
                                        return base.VisitInvocationExpression(node);
                                    }
                                }
                                invoke_name = "InvokeMethod";
                                SimpleNameSyntax invoke_name_and_type_params = node.ArgumentList.Arguments.Count > 0 ? (SimpleNameSyntax)SyntaxFactory.GenericName(SyntaxFactory.Identifier(invoke_name), typeArguments) : (SimpleNameSyntax)SyntaxFactory.IdentifierName(invoke_name);

                                var ret = SyntaxFactory.InvocationExpression(
                                    expression: SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, targetExpression, invoke_name_and_type_params),
                                    argumentList: node.ArgumentList.WithArguments((node.ArgumentList.Accept(this) as ArgumentListSyntax).Arguments.Insert(0, SyntaxFactory.Argument(lambdaExpression)))
                                );

                                if (need_cast)
                                {
                                    return CastTypeNode(ret as ExpressionSyntax, converted_type);
                                }
                                else {
                                    return ret;
                                }
                            }
                        }
                        var retType2 = SyntaxFactory.ParseTypeName(model.GetTypeInfo(node).ConvertedType.ToDisplayString());
                        SimpleNameSyntax invoke_name2 = (retType2 as PredefinedTypeSyntax) != null && (retType2 as PredefinedTypeSyntax).Keyword.IsKind(SyntaxKind.VoidKeyword) ? (SimpleNameSyntax)SyntaxFactory.IdentifierName("Invoke") : (SimpleNameSyntax)SyntaxFactory.GenericName(SyntaxFactory.Identifier("Invoke"), SyntaxFactory.TypeArgumentList().AddArguments(retType2));
                        node = base.VisitInvocationExpression(node) as InvocationExpressionSyntax;
                        return SyntaxFactory.InvocationExpression(
                            expression: SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node.Expression, invoke_name2),
                            argumentList: node.ArgumentList
                        );
                    }
                case 3:
                case 4:
                    {
                        if (node.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                        {
                            var accessExpression = node.Expression as MemberAccessExpressionSyntax;
                            if (accessExpression.Name.Identifier.Text == "InvokeMethod" || accessExpression.Name.Identifier.Text == "InvokeBinary" || accessExpression.Name.Identifier.Text == "Cast" || accessExpression.Name.Identifier.Text == "InvokeUnary" || accessExpression.Name.Identifier.Text == "GetIndex" || accessExpression.Name.Identifier.Text == "SetIndex" || accessExpression.Name.Identifier.Text == "SetField" || accessExpression.Name.Identifier.Text == "AndAnd")
                            {
                                accessExpression = accessExpression.WithExpression(accessExpression.Expression.Accept(this) as ExpressionSyntax);
                                var args = SyntaxFactory.ArgumentList();
                                args = args.AddArguments(node.ArgumentList.Arguments.First());
                                foreach (var arg in node.ArgumentList.Arguments.Skip(1))
                                { // Skip first argument, which is lambda expression
                                    args = args.AddArguments(arg.Accept(this) as ArgumentSyntax);
                                }
                                return node.WithExpression(accessExpression).WithArgumentList(args);
                            }
                            else if (accessExpression.Name.Identifier.Text == "Invoke" || accessExpression.Name.Identifier.Text == "Assign")
                            {
                                accessExpression = accessExpression.WithExpression(accessExpression.Expression.Accept(this) as ExpressionSyntax);
                                var args = node.ArgumentList.Accept(this) as ArgumentListSyntax;
                                return node.WithExpression(accessExpression).WithArgumentList(args);
                            }
                            else if (accessExpression.Name.Identifier.Text == "GetField" || accessExpression.Name.Identifier.Text == "GetConstField")
                            {
                                return node.WithExpression(accessExpression.WithExpression(accessExpression.Expression.Accept(this) as ExpressionSyntax));
                            }
                        }
                        goto default;
                    }
                default:
                    {
                        return base.VisitInvocationExpression(node);
                    }
            }
        }

        public override SyntaxNode VisitThrowStatement(ThrowStatementSyntax node)
        {
            return node;
        }

        private SyntaxNode MakeTransformedSimpleAssignmentNode(AssignmentExpressionSyntax node)
        {
            var leftType = model.GetTypeInfo(node.Left).Type;
            var rightType = model.GetTypeInfo(node.Right).Type;
            SimpleNameSyntax invoke_name = leftType.TypeKind == TypeKind.Array ? (SimpleNameSyntax)SyntaxFactory.IdentifierName("Assign") : (SimpleNameSyntax)SyntaxFactory.GenericName(SyntaxFactory.Identifier("Assign"), SyntaxFactory.TypeArgumentList().AddArguments(SyntaxFactory.ParseTypeName(rightType.ToDisplayString())));
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    node.Left.Accept(this) as ExpressionSyntax,
                    invoke_name),
                SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(node.Right.Accept(this) as ExpressionSyntax)));
        }

        public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            if (noRewrite)
                return node;
            if (node.Left.ChildNodes().Any(
                n =>
                {
                    var s = model.GetSymbolInfo(n);
                    return s.Symbol != null && s.Symbol.Kind == SymbolKind.Field && IsSpecialKW(s.Symbol.Name);
                }
            ))
            {
                return node;
            }
            switch (pass)
            {
                case 2:
                    {
                        if (_inConstructorDecl)
                        {
                            return base.VisitAssignmentExpression(node);
                        }
                        if (node.Left.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                        {
                            var memberAccess = node.Left as MemberAccessExpressionSyntax;
                            if (memberAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                            {
                                return MakeTransformedSimpleAssignmentNode(node);
                            }
                            var type = WrapType(SyntaxFactory.ParseTypeName(model.GetTypeInfo(memberAccess).Type.ToDisplayString().Replace("*", "")));
                            memberAccess = base.VisitMemberAccessExpression(memberAccess) as MemberAccessExpressionSyntax;
                            return SyntaxFactory.InvocationExpression(
                                expression: SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, memberAccess.Expression, SyntaxFactory.GenericName(SyntaxFactory.Identifier("SetField"), SyntaxFactory.TypeArgumentList().AddArguments(type))),
                                argumentList: SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(SyntaxFactory.ParenthesizedLambdaExpression(SyntaxFactory.ParameterList().AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_"))), memberAccess.WithExpression(SyntaxFactory.IdentifierName("_")))), SyntaxFactory.Argument(node.Right.Accept(this) as ExpressionSyntax))
                            );
                        }
                        else if (node.Left.IsKind(SyntaxKind.ElementAccessExpression))
                        {
                            var elementAccess = node.Left as ElementAccessExpressionSyntax;
                            if (elementAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                            {
                                return node.WithRight(node.Right.Accept(this) as ExpressionSyntax);
                            }
                            var elementType = model.GetTypeInfo(elementAccess).Type;
                            var targetExpression = elementAccess.Expression.Accept(this) as ExpressionSyntax;
                            var lambdaParameters = SyntaxFactory.ParameterList().AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_")));
                            var invocationTypeParams = SyntaxFactory.TypeArgumentList();
                            var lambdaBodyInvocationArugments = SyntaxFactory.BracketedArgumentList();
                            for (int i = 0; i < elementAccess.ArgumentList.Arguments.Count; i++)
                            {
                                var argExpression = elementAccess.ArgumentList.Arguments.ElementAt(i).Expression;
                                var t = SyntaxFactory.ParseTypeName(model.GetTypeInfo(argExpression).Type.ToDisplayString());
                                var p = SyntaxFactory.Parameter(SyntaxFactory.Identifier("a" + i.ToString()));
                                var a = SyntaxFactory.Argument(SyntaxFactory.IdentifierName("a" + i.ToString()));
                                lambdaParameters = lambdaParameters.AddParameters(p);
                                lambdaBodyInvocationArugments = lambdaBodyInvocationArugments.AddArguments(a);
                                invocationTypeParams = invocationTypeParams.AddArguments(t);
                            }
                            lambdaParameters = lambdaParameters.AddParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("r")));
                            invocationTypeParams = invocationTypeParams.AddArguments(SyntaxFactory.ParseTypeName(elementType.ToDisplayString()));
                            var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression(parameterList: lambdaParameters, body: node.WithLeft(elementAccess.WithExpression(SyntaxFactory.IdentifierName("_")).WithArgumentList(lambdaBodyInvocationArugments)).WithRight(SyntaxFactory.IdentifierName("r")));
                            if (model.GetTypeInfo(elementAccess.Expression).Type.TypeKind != TypeKind.Array)
                            {
                                return SyntaxFactory.InvocationExpression(
                                    expression: SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        targetExpression,
                                        SyntaxFactory.GenericName(SyntaxFactory.Identifier("InvokeMethod"), invocationTypeParams)),
                                    argumentList: SyntaxFactory.ArgumentList((elementAccess.ArgumentList.Accept(this) as BracketedArgumentListSyntax).Arguments.Insert(0, SyntaxFactory.Argument(lambdaExpression)).Add(SyntaxFactory.Argument(node.Right.Accept(this) as ExpressionSyntax)))
                                );
                            }
                            else {
                                return SyntaxFactory.InvocationExpression(
                                    expression: SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        targetExpression,
                                        SyntaxFactory.GenericName(SyntaxFactory.Identifier("SetIndex"), invocationTypeParams.WithArguments(invocationTypeParams.Arguments.RemoveAt(0)))),
                                    argumentList: SyntaxFactory.ArgumentList((elementAccess.ArgumentList.Accept(this) as BracketedArgumentListSyntax).Arguments.Add(SyntaxFactory.Argument(node.Right.Accept(this) as ExpressionSyntax)))
                                );
                            }
                        }
                        else if (node.IsKind(SyntaxKind.SimpleAssignmentExpression) && (node.Left as IdentifierNameSyntax).Identifier.Text.StartsWith("vs_lgc_tmp_"))
                        {
                            var nodeType = model.GetTypeInfo(node).Type;
                            return SyntaxFactory.ParseExpression(String.Format("{0} = ValueSummary<{1}>.InitializeFrom({2})", node.Left.ToFullString(), nodeType.ToDisplayString(), node.Right.Accept(this).ToFullString()));
                        }
                        else {
                            return MakeTransformedSimpleAssignmentNode(node);
                        }
                    }
                default:
                    {
                        return base.VisitAssignmentExpression(node);
                    }
            }
        }

        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 2:
                    {
                        if (node.IsKind(SyntaxKind.NumericLiteralExpression) || node.IsKind(SyntaxKind.TrueLiteralExpression) || node.IsKind(SyntaxKind.FalseLiteralExpression))
                        {
                            var t = model.GetTypeInfo(node);
                            if (!t.Type.Equals(t.ConvertedType))
                            {
                                return SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName(t.ConvertedType.ToDisplayString()), node);
                            }
                        }
                        else if (node.IsKind(SyntaxKind.NullLiteralExpression))
                        {
                            var t = SyntaxFactory.ParseTypeName(model.GetTypeInfo(node).ConvertedType.ToDisplayString());
                            return SyntaxFactory.ObjectCreationExpression(
                                    GenericName(SyntaxFactory.Identifier("ValueSummary"), SyntaxFactory.TypeArgumentList().AddArguments(t)))
                            .WithNewKeyword(SyntaxFactory.Token(SyntaxKind.NewKeyword).WithTrailingTrivia(SyntaxFactory.ParseTrailingTrivia(" ")))
                            .WithArgumentList(SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)))
                            );
                        }
                        goto default;
                    }
                default:
                    {
                        return base.VisitLiteralExpression(node);
                    }
            }
        }

        public override SyntaxNode VisitCastExpression(CastExpressionSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 2:
                    {
                        var exp = node.Expression.Accept(this) as ExpressionSyntax;
                        var lambdaExpression = SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(SyntaxFactory.Identifier("_")), node.WithExpression(SyntaxFactory.IdentifierName("_")));
                        return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, exp, SyntaxFactory.GenericName(SyntaxFactory.Identifier("Cast"), SyntaxFactory.TypeArgumentList().AddArguments(node.Type))), SyntaxFactory.ArgumentList().AddArguments(SyntaxFactory.Argument(lambdaExpression)));
                    }
                default:
                    {
                        return base.VisitCastExpression(node);
                    }
            }
        }

        private static int cond_cnt = 0;
        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 4:
                    {
                        if (node.Condition.ToFullString().StartsWith("vs_cond_") || node.Condition.ToFullString().StartsWith("PathConstraint.MergedPcFeasible"))
                        {
                            goto default;
                        }
                        var cond_name = node.HasAnnotations("cond_var_name") ? node.GetAnnotations("cond_var_name").First().Data : String.Format("vs_cond_{0}", cond_cnt++);
                        var ret = SyntaxFactory.Block(
                            );
                        ret = ret.AddStatements(SyntaxFactory.IfStatement(
                                condition: node.CopyAnnotationsTo(SyntaxFactory.ParseExpression(String.Format("{0}.CondTrue()", cond_name))),
                                statement: node.Statement.Accept(this) as StatementSyntax
                        ));
                        if (node.Else != null)
                        {
                            ret = ret.AddStatements(SyntaxFactory.IfStatement(
                                condition: node.CopyAnnotationsTo(SyntaxFactory.ParseExpression(String.Format("{0}.CondFalse()", cond_name))),
                                statement: (node.Else.Accept(this) as ElseClauseSyntax).Statement
                            ));
                        }
                        return ret;
                    }
                default:
                    {
                        return base.VisitIfStatement(node);
                    }
            }
        }

        public override SyntaxNode VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 2:
                    {
                        var cond_name = String.Format("vs_cond_{0}", cond_cnt);
                        var var_name = String.Format("vs_cond_ret_{0}", cond_cnt++);
                        var ret_type = SyntaxFactory.ParseTypeName(model.GetTypeInfo(node).Type.ToDisplayString());
                        var block = SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement(String.Format("var {0} = ({1}).Cond();", cond_name, node.Condition.Accept(this).ToFullString())),
                                SyntaxFactory.ParseStatement(String.Format("var {0} = new ValueSummary<{1}>();", var_name, ret_type.ToFullString()))
                                );
                        var whenTrue = node.WhenTrue.Accept(this) as ExpressionSyntax;
                        block = block.AddStatements(SyntaxFactory.IfStatement(
                            condition: SyntaxFactory.ParseExpression(String.Format("{0}.CondTrue()", cond_name)),
                            statement: SyntaxFactory.ParseStatement(String.Format("{0}.Merge({1});", var_name, whenTrue.ToFullString()))
                        ));
                        var whenFalse = node.WhenFalse.Accept(this) as ExpressionSyntax;
                        block = block.AddStatements(SyntaxFactory.IfStatement(
                            condition: SyntaxFactory.ParseExpression(String.Format("{0}.CondFalse()", cond_name)),
                            statement: SyntaxFactory.ParseStatement(String.Format("{0}.Merge({1});", var_name, whenFalse.ToFullString()))
                        ));
                        block = block.AddStatements(
                                SyntaxFactory.ParseStatement(String.Format("{0}.MergeBranch();", cond_name)),
                            SyntaxFactory.ParseStatement(String.Format("return {0};", var_name))
                        );
                        var ret = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.ObjectCreationExpression(
                                    type: SyntaxFactory.IdentifierName(String.Format("Func<ValueSummary<{0}>>", ret_type.ToFullString())))
                            .WithNewKeyword(SyntaxFactory.Token(SyntaxKind.NewKeyword).WithTrailingTrivia(SyntaxFactory.ParseTrailingTrivia(" ")))
                            .WithArgumentList(
                                    SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                        new ArgumentSyntax[]{
                                        SyntaxFactory.Argument(SyntaxFactory.ParenthesizedLambdaExpression(parameterList: SyntaxFactory.ParameterList(), body: block))}))
                                ));
                        return ret;
                    }
                default:
                    {
                        return base.VisitConditionalExpression(node);
                    }
            }
        }

        public override SyntaxNode VisitForStatement(ForStatementSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 4:
                    {
                        var cond_name = node.HasAnnotations("cond_var_name") ? node.GetAnnotations("cond_var_name").First().Data : String.Format("vs_cond_{0}", cond_cnt++);
                        var cond = SyntaxFactory.InvocationExpression(
                                SyntaxFactory.ParseExpression(String.Format("{0}.Loop", cond_name)),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SeparatedList<ArgumentSyntax>(new ArgumentSyntax[] {
                                        SyntaxFactory.Argument(node.Condition.Accept(this) as ExpressionSyntax)
                                    }))
                        );                        
                        var loop = (base.VisitForStatement(node) as ForStatementSyntax).WithCondition(cond);
                        return loop;
                    }
                default:
                    {
                        return base.VisitForStatement(node);
                    }
            }
        }

        public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 4:
                    {
                        var cond_name = node.HasAnnotations("cond_var_name") ? node.GetAnnotations("cond_var_name").First().Data : String.Format("vs_cond_{0}", cond_cnt++);
                        var cond = SyntaxFactory.InvocationExpression(
                                SyntaxFactory.ParseExpression(String.Format("{0}.Loop", cond_name)),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SeparatedList<ArgumentSyntax>(new ArgumentSyntax[] {
                                        SyntaxFactory.Argument(node.Condition.Accept(this) as ExpressionSyntax)
                                    }))
                        );
                        var loop = (base.VisitWhileStatement(node) as WhileStatementSyntax).WithCondition(cond);
                        return loop;
                    }
                default:
                    {
                        return base.VisitWhileStatement(node);
                    }
            }
        }

        public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 4:
                    {
                        var cond = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node.Expression.Accept(this) as ExpressionSyntax, SyntaxFactory.IdentifierName("Switch")));
                        return (base.VisitSwitchStatement(node) as SwitchStatementSyntax).WithExpression(cond);
                    }
                default:
                    {
                        return base.VisitSwitchStatement(node);
                    }
            }
        }

        public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax node)
        {
            if (noRewrite)
                return node;
            switch (pass)
            {
                case 3:
                    {
                        if (handle_return)
                        {
                            if (node.Expression != null && !(node.Expression.ToFullString().StartsWith("vs_ret_") || node.Expression.ToFullString().StartsWith("vs_cond_")))
                            {
                                var ret_var_name = String.Format("vs_ret_{0}", node.Ancestors().First((arg) => arg.IsKind(SyntaxKind.MethodDeclaration)).GetAnnotations("method_id").First().Data);
                                var enclosing_stmt = node.Ancestors().FirstOrDefault((arg) => arg.HasAnnotations("cond_var_name"));
                                if(enclosing_stmt != null)
                                {
                                    var enclosing_cond_var_name = enclosing_stmt.GetAnnotations("cond_var_name").First().Data;
                                    return SyntaxFactory.ParseStatement(String.Format("PathConstraint.RecordReturnPath({0}, {1}, {2});", ret_var_name, node.Expression.Accept(this).ToFullString(), enclosing_cond_var_name));
                                }  
                                else 
                                {
                                    return SyntaxFactory.ParseStatement(String.Format("PathConstraint.RecordReturnPath({0}, {1});", ret_var_name, node.Expression.Accept(this).ToFullString()));
                                }         
                            }
                            else if (node.Expression == null)
                            {
                                var enclosing_stmt = node.Ancestors().FirstOrDefault((arg) => arg.HasAnnotations("cond_var_name"));
                                if(enclosing_stmt != null) {
                                    var enclosing_cond_var_name = enclosing_stmt.GetAnnotations("cond_var_name").First().Data;
                                    return SyntaxFactory.ParseStatement(String.Format("PathConstraint.RecordReturnPath({0});", enclosing_cond_var_name));
                                }
                                else 
                                {
                                    return SyntaxFactory.ParseStatement("PathConstraint.RecordReturnPath();");
                                }
                            }
                        }
                        goto default;
                    }
                default:
                    {
                        return base.VisitReturnStatement(node);
                    }
            }
        }
    }
}