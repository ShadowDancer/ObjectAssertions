using System;
using ObjectAssertions.Abstractions;

namespace ObjectAssertions.Generator.Tests
{
    public class TestClass
    {
        public int IntProperty { get; set; } = 42;
        public string StringProperty { get; set; } = "test";
        public bool BoolProperty { get; set; } = true;
        public double DoubleProperty { get; set; } = 3.14;
    }

    public partial class TestClassAssertions : IAssertsAllPropertiesOf<TestClass>
    {
    }

    public class NestedTestClass
    {
        public string Name { get; set; } = "nested";
        public TestClass Nested { get; set; } = new();
    }

    public partial class NestedTestClassAssertions : IAssertsAllPropertiesOf<NestedTestClass>
    {
    }

    public class TestClassWithObsolete
    {
        [Obsolete]
        public string ObsoleteProperty { get; set; } = "obsolete";
        public int NormalProperty { get; set; } = 1;
    }

    public partial class TestClassWithObsoleteAssertions : IAssertsAllPropertiesOf<TestClassWithObsolete>
    {
    }

    public record TestRecord
    {
        public int IntProperty { get; set; } = 42;
        public string StringProperty { get; set; } = "test";
        public bool BoolProperty { get; set; } = true;
        public double DoubleProperty { get; set; } = 3.14;
    }

    public partial record TestRecordAssertions : IAssertsAllPropertiesOf<TestRecord>
    {
    }

    public record NestedTestRecord
    {
        public string Name { get; set; } = "nested";
        public TestRecord Nested { get; set; } = new();
    }

    public partial record NestedTestRecordAssertions : IAssertsAllPropertiesOf<NestedTestRecord>
    {
    }

    public class ClassContainingRecord
    {
        public string Name { get; set; } = "parent";
        public TestRecord NestedRecord { get; set; } = new();
    }

    public partial class ClassContainingRecordAssertions : IAssertsAllPropertiesOf<ClassContainingRecord>
    {
    }

    public record RecordContainingClass
    {
        public string Name { get; set; } = "parent";
        public TestClass NestedClass { get; set; } = new();
    }

    public partial record RecordContainingClassAssertions : IAssertsAllPropertiesOf<RecordContainingClass>
    {
    }

    public record RecordWithPrimaryConstructor(int Value, string Name)
    {
        public int Value { get; init; } = Value;
        public string Name { get; init; } = Name;
    }

    public partial record RecordWithPrimaryConstructorAssertions : IAssertsAllPropertiesOf<RecordWithPrimaryConstructor>
    {
    }
}
