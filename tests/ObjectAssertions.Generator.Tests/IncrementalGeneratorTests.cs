using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ObjectAssertions.Abstractions;
using System.Linq;
using System.Reflection;
using Xunit;

namespace ObjectAssertions.Generator.Tests
{
    public class IncrementalGeneratorTests
    {
        [Fact]
        public void UnrelatedFileEdit_DoesNotChangeGeneratedSource()
        {
            const string assertionCode = @"
using ObjectAssertions.Abstractions;

namespace TestNamespace
{
    public class Target { public int Value { get; set; } }

    public partial class TargetAssertions : IAssertsAllPropertiesOf<Target>
    {
    }
}";

            const string unrelatedCode = @"
namespace TestNamespace
{
    public class Unrelated { public string Name { get; set; } }
}";

            var assertionTree = CSharpSyntaxTree.ParseText(assertionCode, path: "Assertions.cs");
            var unrelatedTree = CSharpSyntaxTree.ParseText(unrelatedCode, path: "Unrelated.cs");

            var compilation = CreateCompilation(assertionTree);
            var driver = CSharpGeneratorDriver.Create(new ObjectAssertionsSourceGenerator());

            var initialResult = driver.RunGenerators(compilation).GetRunResult();
            var initialGenerated = initialResult.Results[0].GeneratedSources.Single().SourceText.ToString();

            compilation = compilation.AddSyntaxTrees(unrelatedTree);
            var updatedResult = driver.RunGenerators(compilation).GetRunResult();
            var updatedGenerated = updatedResult.Results[0].GeneratedSources.Single().SourceText.ToString();

            Assert.Equal(initialGenerated, updatedGenerated);
        }

        [Fact]
        public void RunGeneratorsAndUpdateCompilation_ProducesExpectedSource()
        {
            const string testCode = @"
using ObjectAssertions.Abstractions;

namespace TestNamespace
{
    public class Target { public int Value { get; set; } }

    public partial class TargetAssertions : IAssertsAllPropertiesOf<Target>
    {
    }
}";

            var compilation = CreateCompilation(CSharpSyntaxTree.ParseText(testCode));
            var driver = CSharpGeneratorDriver.Create(new ObjectAssertionsSourceGenerator());

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

            Assert.Empty(diagnostics);
            Assert.Contains(
                outputCompilation.SyntaxTrees,
                tree => tree.FilePath.EndsWith("TestNamespace.TargetAssertions.g.cs"));
        }

        private static CSharpCompilation CreateCompilation(params SyntaxTree[] syntaxTrees)
        {
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IAssertsAllPropertiesOf<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ObjectAssertionsSourceGenerator).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
            };

            return CSharpCompilation.Create(
                "IncrementalGeneratorTests",
                syntaxTrees,
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }
    }
}
