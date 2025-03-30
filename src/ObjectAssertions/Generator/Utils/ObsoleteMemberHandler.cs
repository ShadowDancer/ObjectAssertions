using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectAssertions.Generator.Utils
{
    public static class ObsoleteMemberHandler
    {
        /// <summary>
        /// Check if symbol is obsolete
        /// </summary>
        /// <param name="symbol">symbol to check</param>
        /// <returns>True if symbol is obos</returns>
        public  static bool IsObsolete(ISymbol symbol, out string obsoleteMessage)
        {
            obsoleteMessage = string.Empty;

            var obsoleteAttribute = symbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute");

            if (obsoleteAttribute == null)
            {
                return false;
            }

            if (obsoleteAttribute.ConstructorArguments.Length > 0 && 
                obsoleteAttribute.ConstructorArguments[0].Value is { } message)
            {
                obsoleteMessage = message.ToString();
            }

            return true;
        }

        public static AttributeSyntax GenerateObsoleteAttribute(string message)
        {
            // Create the attribute name (Obsolete) with fully qualified name
            var attributeName = SyntaxFactory.ParseName("System.ObsoleteAttribute");
            
            // Build the attribute arguments
            var arguments = new List<AttributeArgumentSyntax>();
            
            // Add message argument (null if message is empty)
            if (string.IsNullOrEmpty(message))
            {
                var nullLiteralExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
                arguments.Add(SyntaxFactory.AttributeArgument(nullLiteralExpression));
            }
            else
            {
                var messageArgument = SyntaxFactory.AttributeArgument(
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(message)));
                
                arguments.Add(messageArgument);
            }
            
            // Create the attribute argument list
            var attributeArgumentList = SyntaxFactory.AttributeArgumentList(
                SyntaxFactory.SeparatedList(arguments));
            
            // Return the complete attribute syntax
            return SyntaxFactory.Attribute(attributeName, attributeArgumentList);
        }
    }
}
