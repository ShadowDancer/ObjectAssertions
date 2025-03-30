using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ObjectAssertions.Generator.Utils;
using System.Collections.Immutable;
using System.Linq;

namespace ObjectAssertions.Configuration
{
    internal class ObsoleteInfo
    {
        public ObsoleteInfo(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }

    internal class MemberInfo
    {
        public MemberInfo(ISymbol symbol, ObsoleteInfo? obsoleteInfo)
        {
            Symbol = symbol;
            ObsoleteInfo = obsoleteInfo;
        }

        public ISymbol Symbol { get; }
        public ObsoleteInfo? ObsoleteInfo { get; }
    }

    internal class ObjectAssertionsConfiguration
    {
        public ObjectAssertionsConfiguration(TypeDeclarationSyntax assertionClassDeclaration, INamedTypeSymbol assertedType, string assertionFieldName, IImmutableList<ISymbol> members)
        {
            AssertionClassDeclaration = assertionClassDeclaration;
            AssertedType = assertedType;
            AssertionFieldName = assertionFieldName;
            Members = members.Select(message => 
            {
                bool isObsolete = ObsoleteMemberHandler.IsObsolete(message, out var obsoleteMessage);
                return new MemberInfo(
                    message, 
                    isObsolete ? new ObsoleteInfo(obsoleteMessage) : null
                );
            }).ToImmutableList();
            AssertionClassName = assertionClassDeclaration.GetName();
        }

        public string AssertionClassName { get; }
        public TypeDeclarationSyntax AssertionClassDeclaration { get; }
        public INamedTypeSymbol AssertedType { get; }
        public string AssertionFieldName { get; }
        public IImmutableList<MemberInfo> Members { get; }
    }
}
