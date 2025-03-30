using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ObjectAssertions.Abstractions;
using ObjectAssertions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ObjectAssertions.Generator
{
    internal class GenerationOrchestrator
    {
        private GeneratorExecutionContext _context;

        public GenerationOrchestrator(GeneratorExecutionContext context)
        {
            this._context = context;
        }

        public void Generate()
        {
            var compilation = _context.Compilation;

            IEnumerable<SyntaxNode> allNodes =
                compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
            IEnumerable<TypeDeclarationSyntax> allClasses = allNodes
                .Where(d => d.IsKind(SyntaxKind.ClassDeclaration) || d.IsKind(SyntaxKind.RecordDeclaration))
                .OfType<TypeDeclarationSyntax>();

            var generatedSource = allClasses
                .Select(classDeclaration => TryGenerateAssertions(compilation, classDeclaration))
                .Where(n => n != null)
                .Select(n => n!.Value)
                .ToImmutableArray();

            foreach ((string className, string source) in generatedSource) _context.AddSource(className, source);
        }

        private (string className, string source)? TryGenerateAssertions(Compilation compilation, TypeDeclarationSyntax classDeclaration)
        {
            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);

            var config = IsAssertionsClass(classDeclaration, semanticModel);
            if (config == null)
            {
                return null;
            }

            var source = ClassGenerator.GenerateSource(classDeclaration, semanticModel, config);

            if (semanticModel.GetDeclaredSymbol(classDeclaration) is not ITypeSymbol type)
            {
                return (Guid.NewGuid().ToString(), source);
            }

            string fullName = type.ContainingNamespace.ToDisplayString() + "." + type.Name;
            return (fullName, source);
        }

        private ObjectAssertionsConfiguration? IsAssertionsClass(TypeDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            var markerInterface = semanticModel.Compilation.GetTypeByMetadataName(typeof(IAssertsAllPropertiesOf<>).FullName)?.ConstructUnboundGenericType();
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

                var unboudnGeneric = namedTypeSymbol.ConstructUnboundGenericType();
                if (!SymbolEqualityComparer.Default.Equals(markerInterface, unboudnGeneric))
                {
                    return null;
                }

                return namedTypeSymbol;
            })
            .Where(n => n != null).Cast<INamedTypeSymbol>().ToList() ?? new List<INamedTypeSymbol>();

            if(assertAllPropertiesOfInterfaces.Count == 0)
            {
                return null;
            }

            if (assertAllPropertiesOfInterfaces.Count > 1)
            {
                var nonPartialDiagnostic =
                    Diagnostic.Create(Diagnostics.MultipleInterfaceDeclarations, classDeclaration.GetLocation(), classDeclaration.Identifier.Text);
                _context.ReportDiagnostic(nonPartialDiagnostic);
                return null;
            }

            var assertAllPropertiesOfInterface = assertAllPropertiesOfInterfaces[0];
            if (assertAllPropertiesOfInterface.TypeArguments[0] is not INamedTypeSymbol namedTypeSymbol)
            {
                var nonPartialDiagnostic =
                    Diagnostic.Create(Diagnostics.UnknownTypeName, classDeclaration.GetLocation(), classDeclaration.Identifier.Text);
                _context.ReportDiagnostic(nonPartialDiagnostic);
                return null;
            }

            var isPartial = classDeclaration.Modifiers.Any(n => n.IsKeyword() && n.ToString() == "partial");
            if (!isPartial)
            {
                var nonPartialDiagnostic =
                    Diagnostic.Create(Diagnostics.NonPartialAssertions, classDeclaration.GetLocation(), classDeclaration.Identifier.Text);
                _context.ReportDiagnostic(nonPartialDiagnostic);
                return null;
            }

            var containingClasses = GetContainingTypes(classDeclaration);
            bool anyParentIsNotPartial = false;
            foreach (var typeSymbol in containingClasses)
            {
                bool isParentPartial = typeSymbol.Modifiers.Any(n => n.ToString() == "partial");
                if (!isParentPartial)
                {
                    var nonPartialDiagnostic =
                        Diagnostic.Create(Diagnostics.AssertionsInNonPartialClass, typeSymbol.GetLocation(),
                            classDeclaration.Identifier.Text, typeSymbol.Identifier.Text);
                    _context.ReportDiagnostic(nonPartialDiagnostic);
                    anyParentIsNotPartial = true;
                }
            }

            if (anyParentIsNotPartial)
            {
                return null;
            }

            return ConfigurationCollector.Collect(_context, semanticModel, classDeclaration, assertAllPropertiesOfInterface);
        }

        private IReadOnlyCollection<TypeDeclarationSyntax> GetContainingTypes(TypeDeclarationSyntax classDeclaration)
        {
            SyntaxNode? current = classDeclaration;
            List<TypeDeclarationSyntax> parentTypes = new();

            while (true)
            {
                current = current.Parent;
                if (current == null)
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
