using System.Collections.Generic;
using Xunit;

namespace ObjectAssertions.Generator.Tests
{
    public class GeneratorTests
    {
        [Fact]
        public void TestClassAssertions_ExecutesAllAssertions()
        {
            var executedAssertions = new List<string>();
            var testObject = new TestClass();

            var assertions = new TestClassAssertions(testObject)
            {
                IntProperty = i => executedAssertions.Add("IntProperty"),
                StringProperty = s => executedAssertions.Add("StringProperty"),
                BoolProperty = b => executedAssertions.Add("BoolProperty"),
                DoubleProperty = d => executedAssertions.Add("DoubleProperty")
            };

            assertions.Assert();

            var expected = new[] { "IntProperty", "StringProperty", "BoolProperty", "DoubleProperty" };
            Assert.Equal(expected.Length, executedAssertions.Count);
            foreach (var prop in expected)
            {
                Assert.Contains(prop, executedAssertions);
            }
        }

        [Fact]
        public void NestedTestClassAssertions_ExecutesAllAssertions()
        {
            var executedAssertions = new List<string>();
            var testObject = new NestedTestClass();

            var assertions = new NestedTestClassAssertions(testObject)
            {
                Name = n => executedAssertions.Add("Name"),
                Nested = n => executedAssertions.Add("Nested")
            };

            assertions.Assert();

            var expected = new[] { "Name", "Nested" };
            Assert.Equal(expected.Length, executedAssertions.Count);
            foreach (var prop in expected)
            {
                Assert.Contains(prop, executedAssertions);
            }
        }
    }
}
