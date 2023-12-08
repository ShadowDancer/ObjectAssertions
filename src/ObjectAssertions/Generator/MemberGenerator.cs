using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using ObjectAssertions.Generator.Utils;
using System.Linq;
using System.Collections.Generic;

namespace ObjectAssertions.Generator
{
    internal class MemberGenerator
    {


        internal static ConstructorDeclarationSyntax GenerateConstructor(SemanticModel semanticModel, string typeName, INamedTypeSymbol parameterType, string fieldName)
        {
            const string constructorParamName = "objectToAssert";
            var underlyingType = SyntaxFactory.ParseTypeName(parameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

            var fieldAssigmentSyntax = SyntaxFactory.ParseStatement(fieldName + " = " + constructorParamName + ";");

            return SyntaxFactory.ConstructorDeclaration(typeName)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(constructorParamName)).WithType(underlyingType))
                .WithBody(SyntaxFactory.Block(fieldAssigmentSyntax));
        }

        internal static MemberDeclarationSyntax GenerateFromField(SemanticModel semanticModel, IFieldSymbol fieldSymbol)
        {
            return GenerateProperty(semanticModel, (INamedTypeSymbol)fieldSymbol.Type, fieldSymbol.Name);
        }

        internal static MemberDeclarationSyntax GenerateFromProperty(SemanticModel semanticModel, IPropertySymbol propertySymbol)
        {
            return GenerateProperty(semanticModel, propertySymbol.Type, propertySymbol.Name);
        }

        internal static MemberDeclarationSyntax GenerateBackingField(SemanticModel semanticModel, INamedTypeSymbol propertyType, string name)
        {
            var typeSyntax = TypeExtensions.GetTypeSyntax(propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            var variableDeclarationSyntax = SyntaxFactory.VariableDeclaration(typeSyntax)
                .AddVariables(SyntaxFactory.VariableDeclarator(name));

            return SyntaxFactory.FieldDeclaration(new SyntaxList<AttributeListSyntax>(),
                new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword))
                , variableDeclarationSyntax);
        }


        private static MemberDeclarationSyntax GenerateProperty(SemanticModel semanticModel, ITypeSymbol propertyType, string name)
        {
            var action = semanticModel.Compilation.GetTypeByMetadataName("System.Action`1");

            var typeWithActionSyntax = TypeExtensions.GetTypeSyntax(typeof(Action<>).Namespace + "." + nameof(Action).ToString(), propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

            return SyntaxFactory.PropertyDeclaration(new SyntaxList<AttributeListSyntax>(),
                new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.RequiredKeyword))
                , typeWithActionSyntax, null, SyntaxFactory.Identifier(name), SyntaxFactory.AccessorList(SyntaxFactory.List(new[]
            {
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            })));
        }

        internal static MethodDeclarationSyntax GenerateAssertMethod(SemanticModel semanticModel, string methodName, string assertionFieldName, IEnumerable<string> members)
        {
            var statmentes = members.Select(n => SyntaxFactory.ParseStatement(n + "(" + assertionFieldName + "." + n + ");"));
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseName("void"), methodName)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(SyntaxFactory.Block(statmentes));
        }

        internal static MethodDeclarationSyntax GenerateCollectAssertionsMethod(SemanticModel semanticModel, string methodName, string assertionFieldName, IReadOnlyCollection<string> members)
        {
            var memberExpressions = members.Select(n => SyntaxFactory.ParseExpression("() => " + n + "(" + assertionFieldName + "." + n + ")"));

            var arrayOfActionType = SyntaxFactory.ArrayType(SyntaxFactory.ParseName("System.Action[]"));
            var commas = Enumerable.Repeat(SyntaxFactory.Token(SyntaxKind.CommaToken), members.Count - 1);

            var arrayInitializationSyntax = SyntaxFactory
                .ArrayCreationExpression(SyntaxFactory.Token(SyntaxKind.NewKeyword), arrayOfActionType, SyntaxFactory
                    .InitializerExpression(SyntaxKind.ArrayInitializerExpression)
                    .WithExpressions(SyntaxFactory.SeparatedList(memberExpressions, commas)));

            var returnArraySyntax = SyntaxFactory.ReturnStatement(arrayInitializationSyntax);

            return SyntaxFactory.MethodDeclaration(arrayOfActionType, methodName)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(SyntaxFactory.Block(returnArraySyntax));
        }
    }
}
