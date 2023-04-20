using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace ObjectAssertions.Configuration
{
    internal class ConfigurationCollector
    {
        private GeneratorExecutionContext _context;
        private SemanticModel _semanticModel;
        private TypeDeclarationSyntax _assertionClassDeclaration;
        private readonly INamedTypeSymbol _assertAllPropertiesOfInterface;

        public ConfigurationCollector(GeneratorExecutionContext context, SemanticModel semanticModel, TypeDeclarationSyntax assertionClassDeclaration, INamedTypeSymbol assertAllPropertiesOfInterface)
        {
            _context = context;
            _semanticModel = semanticModel;
            _assertionClassDeclaration = assertionClassDeclaration;
            _assertAllPropertiesOfInterface = assertAllPropertiesOfInterface;
        }

        public static ObjectAssertionsConfiguration? Collect(GeneratorExecutionContext context, SemanticModel semanticModel,
    TypeDeclarationSyntax assertionClassDeclaration, INamedTypeSymbol assertAllPropertiesOfInterface)
        {
            return new ConfigurationCollector(context, semanticModel, assertionClassDeclaration, assertAllPropertiesOfInterface).Collect();
        }

        private ObjectAssertionsConfiguration? Collect()
        {
            var assertedType = (INamedTypeSymbol)_assertAllPropertiesOfInterface.TypeArguments[0];

            var publicMembers = assertedType.GetMembers().Where(n => n.DeclaredAccessibility == Accessibility.Public);

            var properties = publicMembers.OfType<IPropertySymbol>();

            var fields = publicMembers.OfType<IFieldSymbol>()
                .Where(n => n.AssociatedSymbol == null && n.DeclaredAccessibility == Accessibility.Public)
                .ToList();

            var members = new List<ISymbol>();
            members.AddRange(properties);
            members.AddRange(fields);

            string assertionFieldName = "_objectToAssert";

            while (members.Any(n => n.Name == assertionFieldName))
            {
                assertionFieldName = "_" + assertionFieldName;
            }

            return new ObjectAssertionsConfiguration(_assertionClassDeclaration, assertedType, assertionFieldName, members.ToImmutableArray());
        }


    }
}
