using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ObjectAssertions.Abstractions;
using System.Linq;
using System.Reflection;
using Xunit;

namespace ObjectAssertions.Generator.Tests
{
    public class ObsoleteDiagnosticTests
    {
        [Fact]
        public void ObsoleteClass_GeneratesObsoleteWarning()
        {
            var testCode = @"
using ObjectAssertions.Abstractions;
using System;

namespace TestNamespace
{
    [Obsolete]
    public class TestClassWithObsolete
    {
        public string ObsoleteProperty { get; set; } = ""obsolete"";
        public int NormalProperty { get; set; } = 1;
    }

#pragma warning disable CS0612
    public partial class TestClassWithObsoleteAssertions : IAssertsAllPropertiesOf<TestClassWithObsolete>
#pragma warning restore CS0612
    {
    }

    public class TestUsage
    {
        public void UseAssertions()
        {
            var obj = new TestClassWithObsolete();
            var assertions = new TestClassWithObsoleteAssertions(obj)
            {
                ObsoleteProperty = o => { },
                NormalProperty = o => { }
            };
            assertions.Assert();
        }
    }
}";
            var expectedDiagnosticStartsAt = testCode.IndexOf("var assertions = new", StringComparison.Ordinal);
            Assert.NotEqual(expectedDiagnosticStartsAt, -1);

            var diagnostics = GetCompilationDiagnostics(testCode);
            Assert.Single(diagnostics, d => d.Id == "CS0612" && d.Severity == DiagnosticSeverity.Warning);
        }
        
        [Fact]
        public void ObsoleteClassWithMessage_GeneratesObsoleteWarning()
        {
            var testCode = @"
using ObjectAssertions.Abstractions;
using System;

namespace TestNamespace
{
    [Obsolete(""This class is obsolete"")]
    public class TestClassWithObsolete
    {
        public string ObsoleteProperty { get; set; } = ""obsolete"";
        public int NormalProperty { get; set; } = 1;
    }

#pragma warning disable CS0618
    public partial class TestClassWithObsoleteAssertions : IAssertsAllPropertiesOf<TestClassWithObsolete>
#pragma warning restore CS0618
    {
    }

    public class TestUsage
    {
        public void UseAssertions()
        {
            var obj = new TestClassWithObsolete();
            var assertions = new TestClassWithObsoleteAssertions(obj)
            {
                ObsoleteProperty = o => { },
                NormalProperty = o => { }
            };
            assertions.Assert();
        }
    }
}";
            var expectedDiagnosticStartsAt = testCode.IndexOf("var assertions = new", StringComparison.Ordinal);
            Assert.NotEqual(expectedDiagnosticStartsAt, -1);

            var diagnostics = GetCompilationDiagnostics(testCode);
            Assert.Equal("CS0618", diagnostics[0].Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostics[0].Severity);
        }
 
        [Fact]
        public void ObsoleteProperty_GeneratesObsoleteWarning()
        {
            var testCode = @"
using ObjectAssertions.Abstractions;
using System;

namespace TestNamespace
{
    public class TestClassWithObsolete
    {
        [Obsolete]
        public string ObsoleteProperty { get; set; } = ""obsolete"";
        public int NormalProperty { get; set; } = 1;
    }

    public partial class TestClassWithObsoleteAssertions : IAssertsAllPropertiesOf<TestClassWithObsolete>
    {
    }

    public class TestUsage
    {
        public void UseAssertions()
        {
            var obj = new TestClassWithObsolete();
            var assertions = new TestClassWithObsoleteAssertions(obj)
            {
                ObsoleteProperty = o => { },
                NormalProperty = o => { }
            };
            assertions.Assert();
        }
    }
}";
            var expectedDiagnosticStartsAt = testCode.IndexOf("var assertions = new", StringComparison.Ordinal);
            Assert.NotEqual(expectedDiagnosticStartsAt, -1);

            var diagnostics = GetCompilationDiagnostics(testCode);
            
            // One for assertion assignment, one for assert method
            Assert.Equal(2, diagnostics.Length);
            var classDiagnostic = diagnostics[0];
            Assert.Equal("CS0618", classDiagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, classDiagnostic.Severity);
            Assert.True(classDiagnostic.Location.SourceSpan.Start >= expectedDiagnosticStartsAt, $"Diagnostic at unexpected location: {classDiagnostic.Location.SourceSpan.Start}");

            var propertyDiagnostic = diagnostics[1];
            Assert.Equal("CS0612", propertyDiagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Warning, propertyDiagnostic.Severity);
            Assert.True(propertyDiagnostic.Location.SourceSpan.Start >= expectedDiagnosticStartsAt, $"Diagnostic at unexpected location: {classDiagnostic.Location.SourceSpan.Start}");
        }

        [Fact]
        public void ObsoletePropertyWithMessage_GeneratesObsoleteWarning()
        {
            var testCode = @"
using ObjectAssertions.Abstractions;
using System;

namespace TestNamespace
{
    public class TestClassWithObsolete
    {
#pragma warning disable CS0618
        [Obsolete(""This property is obsolete"")]
        public string ObsoleteProperty { get; set; } = ""obsolete"";
#pragma warning restore CS0618
        public int NormalProperty { get; set; } = 1;
    }

    public partial class TestClassWithObsoleteAssertions : IAssertsAllPropertiesOf<TestClassWithObsolete>
    {
    }

    public class TestUsage
    {
        public void UseAssertions()
        {
            var obj = new TestClassWithObsolete();
            var assertions = new TestClassWithObsoleteAssertions(obj)
            {
                ObsoleteProperty = o => { },
                NormalProperty = o => { }
            };
            assertions.Assert();
        }
    }
}";
            var expectedDiagnosticStartsAt = testCode.IndexOf("var assertions = new", StringComparison.Ordinal);
            Assert.NotEqual(expectedDiagnosticStartsAt, -1);
            
            var diagnostics = GetCompilationDiagnostics(testCode);
            
            // One for assertion assignment, one for assert method
            Assert.Equal(2, diagnostics.Length);
            foreach (var d in diagnostics)
            {
                Assert.Equal("CS0618", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                
                Assert.True(d.Location.SourceSpan.Start >= expectedDiagnosticStartsAt, $"Diagnostic at unexpected location: {d.Location.SourceSpan.Start}");
            }
        }

        private static Diagnostic[] GetCompilationDiagnostics(string testCode)
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

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var _);

            return outputCompilation.GetDiagnostics().ToArray();
        }
    }
}
