using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ObjectAssertions.Generator
{
    internal static class AssertionSyntaxHelper
    {
        public static bool IsCandidate(SyntaxNode node)
        {
            if (node is not TypeDeclarationSyntax typeDeclaration)
            {
                return false;
            }

            if (!typeDeclaration.IsKind(SyntaxKind.ClassDeclaration)
                && !typeDeclaration.IsKind(SyntaxKind.RecordDeclaration))
            {
                return false;
            }

            if (typeDeclaration.BaseList is null)
            {
                return false;
            }

            foreach (var baseType in typeDeclaration.BaseList.Types)
            {
                if (baseType.ToString().Contains("IAssertsAllPropertiesOf"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
