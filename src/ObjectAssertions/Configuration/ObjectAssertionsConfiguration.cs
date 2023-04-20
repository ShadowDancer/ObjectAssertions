using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ObjectAssertions.Generator.Utils;
using System.Collections.Immutable;

namespace ObjectAssertions.Configuration
{
    internal class ObjectAssertionsConfiguration
    {
        public ObjectAssertionsConfiguration(TypeDeclarationSyntax assertionClassDeclaration, INamedTypeSymbol assertedType, string assertionFieldName, IImmutableList<ISymbol> members)
        {
            AssertionClassDeclaration = assertionClassDeclaration;
            AssertedType = assertedType;
            AssertionFieldName = assertionFieldName;
            Members = members;
            AssertionClassName = assertionClassDeclaration.GetName();
        }

        public string AssertionClassName { get; }
        public TypeDeclarationSyntax AssertionClassDeclaration { get; }
        public INamedTypeSymbol AssertedType { get; }
        public string AssertionFieldName { get; }
        public IImmutableList<ISymbol> Members { get; }
    }
}
