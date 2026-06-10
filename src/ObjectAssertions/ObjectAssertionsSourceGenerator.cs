using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ObjectAssertions.Generator;
using ObjectAssertions.Generator.Models;

namespace ObjectAssertions
{
    [Generator]
    public class ObjectAssertionsSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var candidates = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => AssertionSyntaxHelper.IsCandidate(node),
                    transform: static (ctx, _) => (TypeDeclarationSyntax)ctx.Node)
                .Combine(context.CompilationProvider)
                .Select(static (pair, cancellationToken) =>
                {
                    var typeDeclaration = pair.Left;
                    var compilation = pair.Right;
                    var semanticModel = compilation.GetSemanticModel(typeDeclaration.SyntaxTree);
                    return AssertionAnalyzer.TryAnalyze(typeDeclaration, semanticModel, cancellationToken);
                })
                .Where(static result => result is not null);

            context.RegisterSourceOutput(candidates, static (sourceProductionContext, result) =>
            {
#if DEBUG
                System.Diagnostics.Debugger.Launch();
#endif
                foreach (var diagnostic in result!.Value.Diagnostics)
                {
                    sourceProductionContext.ReportDiagnostic(diagnostic);
                }

                if (result.Value.Source is not null)
                {
                    sourceProductionContext.AddSource($"{result.Value.HintName}.g.cs", result.Value.Source);
                }
            });
        }
    }
}
