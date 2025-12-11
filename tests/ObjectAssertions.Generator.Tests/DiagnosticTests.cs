using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ObjectAssertions.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace ObjectAssertions.Generator.Tests
{
    public class DiagnosticTests
    {
        [Fact]
        public void NonPartialAssertionsClass_ReportsError()
        {
            var testCode = @"
using ObjectAssertions.Abstractions;

namespace TestNamespace
{
    public class NonPartialAssertions : IAssertsAllPropertiesOf<string>
    {
    }
}";

            var diagnostics = GetDiagnostics(testCode);
            Assert.Contains(diagnostics, d => d.Id == "OBJASS0002" && d.Severity == DiagnosticSeverity.Error);
        }

        [Fact]
        public void AssertionsInNonPartialContainingClass_ReportsError()
        {
            var testCode = @"
using ObjectAssertions.Abstractions;

namespace TestNamespace
{
    public class NonPartialContainer
    {
        public partial class NestedAssertions : IAssertsAllPropertiesOf<string>
        {
        }
    }
}";

            var diagnostics = GetDiagnostics(testCode);
            Assert.Contains(diagnostics, d => d.Id == "OBJASS0003" && d.Severity == DiagnosticSeverity.Error);
        }

        [Fact]
        public void MultipleInterfaceDeclarations_ReportsError()
        {
            var testCode = @"
using ObjectAssertions.Abstractions;

namespace TestNamespace
{
    public partial class MultipleInterfaces : IAssertsAllPropertiesOf<string>, IAssertsAllPropertiesOf<int>
    {
    }
}";

            var diagnostics = GetDiagnostics(testCode);
            Assert.Contains(diagnostics, d => d.Id == "OBJASS0004" && d.Severity == DiagnosticSeverity.Error);
        }

        private static IEnumerable<Diagnostic> GetDiagnostics(string testCode)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IAssertsAllPropertiesOf<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ObjectAssertionsSourceGenerator).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
            };

            var compilation = CSharpCompilation.Create(
                "TestCompilation",
                [syntaxTree],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var generator = new ObjectAssertionsSourceGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            var runResult = driver.RunGenerators(compilation);
            var generatorDiagnostics = runResult.GetRunResult().Diagnostics;
            var compilationDiagnostics = compilation.GetDiagnostics();

            return generatorDiagnostics.Concat(compilationDiagnostics);
        }
    }
}
