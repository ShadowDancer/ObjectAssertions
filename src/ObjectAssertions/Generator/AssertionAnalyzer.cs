using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ObjectAssertions.Configuration;
using ObjectAssertions.Generator.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace ObjectAssertions.Generator
{
    internal static class AssertionAnalyzer
    {
        public static GenerationResult? TryAnalyze(
            TypeDeclarationSyntax classDeclaration,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var markerInterface = semanticModel.Compilation
                .GetTypeByMetadataName("ObjectAssertions.Abstractions.IAssertsAllPropertiesOf`1")
                ?.ConstructUnboundGenericType();

            if (markerInterface is null)
            {
                return null;
            }

            var assertAllPropertiesOfInterfaces = classDeclaration.BaseList?.Types.Select(baseType =>
            {
                if (baseType is not SimpleBaseTypeSyntax simpleBaseTypeSyntax)
                {
                    return null;
                }

                if (simpleBaseTypeSyntax.ChildNodes().Single() is not GenericNameSyntax genericNameSyntax)
                {
                    return null;
                }

                var genericTypeInfo = semanticModel.GetTypeInfo(genericNameSyntax);
                if (genericTypeInfo.Type is not INamedTypeSymbol namedTypeSymbol)
                {
                    return null;
                }

                if (!namedTypeSymbol.IsGenericType)
                {
                    return null;
                }

                var unboundGeneric = namedTypeSymbol.ConstructUnboundGenericType();
                if (!SymbolEqualityComparer.Default.Equals(markerInterface, unboundGeneric))
                {
                    return null;
                }

                return namedTypeSymbol;
            })
            .Where(n => n != null).Cast<INamedTypeSymbol>().ToList() ?? new List<INamedTypeSymbol>();

            if (assertAllPropertiesOfInterfaces.Count == 0)
            {
                return null;
            }

            var hintName = GetHintName(classDeclaration, semanticModel);
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            if (assertAllPropertiesOfInterfaces.Count > 1)
            {
                diagnostics.Add(Diagnostic.Create(
                    Diagnostics.MultipleInterfaceDeclarations,
                    classDeclaration.GetLocation(),
                    classDeclaration.Identifier.Text));
                return new GenerationResult(hintName, null, diagnostics.ToImmutable());
            }

            var assertAllPropertiesOfInterface = assertAllPropertiesOfInterfaces[0];
            if (assertAllPropertiesOfInterface.TypeArguments[0] is not INamedTypeSymbol namedTypeSymbol)
            {
                diagnostics.Add(Diagnostic.Create(
                    Diagnostics.UnknownTypeName,
                    classDeclaration.GetLocation(),
                    classDeclaration.Identifier.Text));
                return new GenerationResult(hintName, null, diagnostics.ToImmutable());
            }

            if (namedTypeSymbol.IsAbstract)
            {
                diagnostics.Add(Diagnostic.Create(
                    Diagnostics.AbstractClassesAreNotSupported,
                    classDeclaration.GetLocation(),
                    classDeclaration.Identifier.Text));
                return new GenerationResult(hintName, null, diagnostics.ToImmutable());
            }

            var isPartial = classDeclaration.Modifiers.Any(n => n.IsKeyword() && n.ToString() == "partial");
            if (!isPartial)
            {
                diagnostics.Add(Diagnostic.Create(
                    Diagnostics.NonPartialAssertions,
                    classDeclaration.GetLocation(),
                    classDeclaration.Identifier.Text));
                return new GenerationResult(hintName, null, diagnostics.ToImmutable());
            }

            var containingClasses = GetContainingTypes(classDeclaration);
            bool anyParentIsNotPartial = false;
            foreach (var typeSymbol in containingClasses)
            {
                bool isParentPartial = typeSymbol.Modifiers.Any(n => n.ToString() == "partial");
                if (!isParentPartial)
                {
                    diagnostics.Add(Diagnostic.Create(
                        Diagnostics.AssertionsInNonPartialClass,
                        typeSymbol.GetLocation(),
                        classDeclaration.Identifier.Text,
                        typeSymbol.Identifier.Text));
                    anyParentIsNotPartial = true;
                }
            }

            if (anyParentIsNotPartial)
            {
                return new GenerationResult(hintName, null, diagnostics.ToImmutable());
            }

            var config = ConfigurationCollector.Collect(semanticModel, classDeclaration, assertAllPropertiesOfInterface);
            if (config is null)
            {
                return new GenerationResult(hintName, null, diagnostics.ToImmutable());
            }

            var source = ClassGenerator.GenerateSource(classDeclaration, semanticModel, config);
            return new GenerationResult(hintName, source, diagnostics.ToImmutable());
        }

        private static string GetHintName(TypeDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            if (semanticModel.GetDeclaredSymbol(classDeclaration) is ITypeSymbol type)
            {
                return type.ContainingNamespace.ToDisplayString() + "." + type.Name;
            }

            return $"{classDeclaration.SyntaxTree.FilePath}_{classDeclaration.Span.Start}";
        }

        private static IReadOnlyCollection<TypeDeclarationSyntax> GetContainingTypes(TypeDeclarationSyntax classDeclaration)
        {
            SyntaxNode? current = classDeclaration;
            List<TypeDeclarationSyntax> parentTypes = new();

            while (true)
            {
                current = current.Parent;
                if (current is null)
                {
                    break;
                }

                if (current is TypeDeclarationSyntax typeSyntax)
                {
                    parentTypes.Add(typeSyntax);
                }
            }

            return parentTypes;
        }
    }
}
