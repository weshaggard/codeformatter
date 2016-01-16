// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.DotNet.CodeFormatting.Rules
{
    [LocalSemanticRule(UseKeywordsOverTypes.Name, UseKeywordsOverTypes.Description, LocalSemanticRuleOrder.ExplicitVisibilityRule)]
    internal sealed partial class UseKeywordsOverTypes : ILocalSemanticFormattingRule
    {
        internal const string Name = "UseKeywordsOverTypes";
        internal const string Description = "Ensure use of keywords instead of types";

        public bool SupportsLanguage(string languageName)
        {
            return languageName == LanguageNames.CSharp;
        }

        public Task<SyntaxNode> ProcessAsync(Document document, SyntaxNode syntaxRoot, CancellationToken cancellationToken)
        {
            SyntaxNode result;
            switch (document.Project.Language)
            {
                case LanguageNames.CSharp:
                    {
                        var rewriter = new CSharpVisibilityRewriter(document, cancellationToken);
                        result = rewriter.Visit(syntaxRoot);
                        break;
                    }
                default:
                    throw new NotSupportedException();
            }

            return Task.FromResult(result);
        }

        private sealed class CSharpVisibilityRewriter : CSharpSyntaxRewriter
        {
            private readonly Document _document;
            private readonly CancellationToken _cancellationToken;
            private SemanticModel _semanticModel;

            internal CSharpVisibilityRewriter(Document document, CancellationToken cancellationToken)
            {
                _document = document;
                _cancellationToken = cancellationToken;
                _semanticModel = _document.GetSemanticModelAsync(_cancellationToken).Result;
            }

            public override SyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
            {
                return base.VisitConversionOperatorDeclaration(node.WithType(ReplaceWithKeyword(node.Type, node)));
            }

            public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
            {
                return base.VisitDelegateDeclaration(node.WithReturnType(ReplaceWithKeyword(node.ReturnType, node)));
            }

            public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
            {
                return base.VisitIndexerDeclaration(node.WithType(ReplaceWithKeyword(node.Type, node)));
            }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                return base.VisitMethodDeclaration(node.WithReturnType(ReplaceWithKeyword(node.ReturnType, node)));
            }

            public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
            {
                return base.VisitOperatorDeclaration(node.WithReturnType(ReplaceWithKeyword(node.ReturnType, node)));
            }

            public override SyntaxNode VisitParameter(ParameterSyntax node)
            {
                return base.VisitParameter(node.WithType(ReplaceWithKeyword(node.Type, node)));
            }

            public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                return base.VisitPropertyDeclaration(node.WithType(ReplaceWithKeyword(node.Type, node)));
            }

            public override SyntaxNode VisitTypeOfExpression(TypeOfExpressionSyntax node)
            {
                return base.VisitTypeOfExpression(node.WithType(ReplaceWithKeyword(node.Type, node)));
            }

            public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
            {
                IFieldSymbol symbol = (IFieldSymbol)_semanticModel.GetDeclaredSymbol(node.Variables.First(), _cancellationToken);
                ITypeSymbol ts = symbol.Type;

                var st = ts.SpecialType;

                var s2 = _semanticModel.GetSymbolInfo(node.Type);
                return base.VisitVariableDeclaration(node.WithType(ReplaceWithKeyword(node.Type, node)));
            }

            private TypeSyntax ReplaceWithKeyword(TypeSyntax typeExpression, CSharpSyntaxNode node)
            {
                if (!(typeExpression is IdentifierNameSyntax) &&
                  !(typeExpression is QualifiedNameSyntax))
                {
                    return typeExpression;
                }

                if (typeExpression.IsVar)
                {
                    return typeExpression;
                }

                _semanticModel = _document.GetSemanticModelAsync(_cancellationToken).Result;
                INamedTypeSymbol tsymbol = _semanticModel.GetSymbolInfo((IdentifierNameSyntax)typeExpression, _cancellationToken).Symbol as INamedTypeSymbol;

                var s2 = _semanticModel.GetTypeInfo(typeExpression);//.GetDeclaredSymbol(node, _cancellationToken);
                //SyntaxFactory.ParseTypeName(typeExpression.ToDi)
                ISymbol symbol = _semanticModel.GetDeclaredSymbol(typeExpression, _cancellationToken);
                if (symbol == null)
                {
                    return typeExpression;
                }

                if (symbol.ContainingNamespace.Name != "System")
                {
                    return typeExpression;
                }

                SyntaxKind kind = SyntaxKind.None;

                switch (symbol.MetadataName)
                {
                    case "Boolean": { kind = SyntaxKind.BoolKeyword; break; }
                    case "Byte":    { kind = SyntaxKind.ByteKeyword; break; }
                    case "Char":    { kind = SyntaxKind.CharKeyword; break; }
                    case "Double":  { kind = SyntaxKind.DoubleKeyword; break; }
                    case "Decimal": { kind = SyntaxKind.DecimalKeyword; break; }
                    case "Float":   { kind = SyntaxKind.FloatKeyword; break; }
                    case "Int16":   { kind = SyntaxKind.ShortKeyword; break; }
                    case "Int32":   { kind = SyntaxKind.IntKeyword; break; }
                    case "Int64":   { kind = SyntaxKind.LongKeyword; break; }
                    case "Object":  { kind = SyntaxKind.ObjectKeyword; break; }
                    case "SByte":   { kind = SyntaxKind.SByteKeyword; break; }
                    case "String":  { kind = SyntaxKind.StringKeyword; break; }
                    case "UInt16":  { kind = SyntaxKind.UShortKeyword; break; }
                    case "UInt32":  { kind = SyntaxKind.UIntKeyword; break; }
                    case "UInt64":  { kind = SyntaxKind.ULongKeyword; break; }
                    case "Void":    { kind = SyntaxKind.VoidKeyword; break; }
                }

                if (kind != SyntaxKind.None)
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(kind));

                return typeExpression;
            }
        }
    }
}

