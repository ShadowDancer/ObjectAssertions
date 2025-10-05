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
}
