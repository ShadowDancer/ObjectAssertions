using System.Collections.Generic;
using Xunit;

namespace ObjectAssertions.Generator.Tests
{
    public class RecordTests
    {
        [Fact]
        public void ClassContainingRecordAssertions_ExecutesAllAssertions()
        {
            var executedAssertions = new List<string>();
            var testObject = new ClassContainingRecord();

            var assertions = new ClassContainingRecordAssertions(testObject)
            {
                Name = n => executedAssertions.Add("Name"),
                NestedRecord = nr => executedAssertions.Add("NestedRecord")
            };

            assertions.Assert();

            var expected = new[] { "Name", "NestedRecord" };
            Assert.Equal(expected.Length, executedAssertions.Count);
            foreach (var prop in expected)
            {
                Assert.Contains(prop, executedAssertions);
            }
        }

        [Fact]
        public void RecordContainingClassAssertions_ExecutesAllAssertions()
        {
            var executedAssertions = new List<string>();
            var testObject = new RecordContainingClass();

            var assertions = new RecordContainingClassAssertions(testObject)
            {
                Name = n => executedAssertions.Add("Name"),
                NestedClass = nc => executedAssertions.Add("NestedClass")
            };

            assertions.Assert();

            var expected = new[] { "Name", "NestedClass" };
            Assert.Equal(expected.Length, executedAssertions.Count);
            foreach (var prop in expected)
            {
                Assert.Contains(prop, executedAssertions);
            }
        }

        [Fact]
        public void RecordWithPrimaryConstructorAssertions_ExecutesAllAssertions()
        {
            var executedAssertions = new List<string>();
            var testObject = new RecordWithPrimaryConstructor(Value: 100, Name: "test");

            var assertions = new RecordWithPrimaryConstructorAssertions(testObject)
            {
                Value = v => executedAssertions.Add("Value"),
                Name = n => executedAssertions.Add("Name")
            };

            assertions.Assert();

            var expected = new[] { "Value", "Name" };
            Assert.Equal(expected.Length, executedAssertions.Count);
            foreach (var prop in expected)
            {
                Assert.Contains(prop, executedAssertions);
            }
        }

        [Fact]
        public void RecordAssertions_WithKeyword_ModifiesAndExecutesAllAssertions()
        {
            var executedAssertions = new List<string>();
            var testObject = new TestRecord { IntProperty = 100, StringProperty = "modified" };
            var originalAssertions = new TestRecordAssertions(testObject)
            {
                IntProperty = i => executedAssertions.Add("IntProperty"),
                StringProperty = s => executedAssertions.Add("StringProperty"),
                BoolProperty = b => executedAssertions.Add("BoolProperty"),
                DoubleProperty = d => executedAssertions.Add("DoubleProperty")
            };

            var modifiedAssertions = originalAssertions with
            {
                IntProperty = i => 
                {
                    executedAssertions.Add("Modified IntProperty");
                    Assert.Equal(100, i);
                }
            };
            modifiedAssertions.Assert();

            var expected = new[] { "Modified IntProperty", "StringProperty", "BoolProperty", "DoubleProperty" };
            Assert.Equal(expected.Length, executedAssertions.Count);
            foreach (var prop in expected)
            {
                Assert.Contains(prop, executedAssertions);
            }
        }
    }
}
