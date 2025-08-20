using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using ObjectAssertions.Generator.Utils;
using System.Linq;
using System.Collections.Generic;
using ObjectAssertions.Configuration;

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
            bool isObsolete = ObsoleteMemberHandler.IsObsolete(fieldSymbol, out string obsoleteMessage);
            var property = GenerateProperty(semanticModel, (INamedTypeSymbol)fieldSymbol.Type, fieldSymbol.Name);

            if (isObsolete)
            {
                var obsoleteAttribute = ObsoleteMemberHandler.GenerateObsoleteAttribute(obsoleteMessage);
                return property.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(obsoleteAttribute)));
            }

            return property;
        }

        internal static MemberDeclarationSyntax GenerateFromProperty(SemanticModel semanticModel, IPropertySymbol propertySymbol)
        {
            bool isObsolete = ObsoleteMemberHandler.IsObsolete(propertySymbol, out string obsoleteMessage);
            var property = GenerateProperty(semanticModel, propertySymbol.Type, propertySymbol.Name);

            if (isObsolete)
            {
                var obsoleteAttribute = ObsoleteMemberHandler.GenerateObsoleteAttribute(obsoleteMessage);
                return property.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(obsoleteAttribute)));
            }

            return property;
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

        internal static MethodDeclarationSyntax GenerateAssertMethod(SemanticModel semanticModel, string methodName, string assertionFieldName, IEnumerable<MemberInfo> members)
        {
            var statements = members.Select(m => SyntaxFactory.ParseStatement(m.Symbol.Name + "(" + assertionFieldName + "." + m.Symbol.Name + ");"));
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseName("void"), methodName)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(SyntaxFactory.Block(statements));
        }

        internal static MethodDeclarationSyntax GenerateCollectAssertionsMethod(SemanticModel semanticModel, string methodName, string assertionFieldName, IReadOnlyCollection<MemberInfo> members)
        {
            var arrayOfActionType = SyntaxFactory.ArrayType(SyntaxFactory.ParseName("System.Action[]"));

            var initializer = SyntaxFactory
                        .InitializerExpression(SyntaxKind.ArrayInitializerExpression);

            if (members.Count > 0)
            {
                var memberExpressions = members.Select(m => SyntaxFactory.ParseExpression("() => " + m.Symbol.Name + "(" + assertionFieldName + "." + m.Symbol.Name + ")"));
                var commas = Enumerable.Repeat(SyntaxFactory.Token(SyntaxKind.CommaToken), members.Count - 1);
                initializer = initializer
                    .WithExpressions(SyntaxFactory.SeparatedList(memberExpressions, commas));
            }

            var arrayInitializationSyntax = SyntaxFactory
                .ArrayCreationExpression(SyntaxFactory.Token(SyntaxKind.NewKeyword), arrayOfActionType, initializer);

            var returnArraySyntax = SyntaxFactory.ReturnStatement(arrayInitializationSyntax);

            return SyntaxFactory.MethodDeclaration(arrayOfActionType, methodName)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(SyntaxFactory.Block(returnArraySyntax));
        }

        internal static MemberDeclarationSyntax GenerateFromMemberInfo(SemanticModel semanticModel, MemberInfo memberInfo)
        {
            var symbol = memberInfo.Symbol;
            MemberDeclarationSyntax property;

            if (symbol is IFieldSymbol fieldSymbol)
            {
                property = GenerateProperty(semanticModel, (INamedTypeSymbol)fieldSymbol.Type, fieldSymbol.Name);
            }
            else if (symbol is IPropertySymbol propertySymbol)
            {
                property = GenerateProperty(semanticModel, propertySymbol.Type, propertySymbol.Name);
            }
            else
            {
                throw new NotSupportedException($"Unsupported symbol type: {symbol.GetType().Name}");
            }

            if (memberInfo.ObsoleteInfo != null)
            {
                var obsoleteAttribute = ObsoleteMemberHandler.GenerateObsoleteAttribute(
                    memberInfo.ObsoleteInfo.Message);

                return property.AddAttributeLists(
                    SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(obsoleteAttribute)));
            }

            return property;
        }
    }
}
