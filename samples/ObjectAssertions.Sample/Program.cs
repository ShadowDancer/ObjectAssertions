using ObjectAssertions.Abstractions;
using Xunit;
using static ObjectAssertions.Sample.ObjectToAssert;

namespace ObjectAssertions.Sample
{
    // This is data model in your program
    public record ObjectToAssert
    {
        public int IntegerNumber { get; set; } = 30;
        public double DoubleNumber { get; set; } = 42d;
        public bool BooleanValue { get; set; } = true;
        public char CharacterValue { get; set; } = 'z';
        public string StringValue { get; set; } = "Hello world";
        public string[] StringArray { get; set; } = new string[] { "One", "Two", "Three" };
        public ExampleEnum EnumValue { get; set; } = ExampleEnum.Option3;
        public int? NullableNumber { get; set; } = 5;
        public NestedObject NestedObjectValue { get; set; } = new NestedObject();


        public enum ExampleEnum { Option1, Option2, Option3 };
        public record NestedObject(int NestedInt = 5, string NestedString = "ubuaa");
    }

    // This is test utility for generating assertions
    public partial class ObjectAssertions : IAssertsAllPropertiesOf<ObjectToAssert>
    {

    }

    public partial class NestedObjectAssertions : IAssertsAllPropertiesOf<NestedObject>
    {

    }

    // This is your tests class
    public class Example
    {
        [Fact]
        public void HowTo()
        {
            var testObject = new ObjectToAssert();


            var assertions = new ObjectAssertions(testObject)
            {
                BooleanValue = Assert.True,
                CharacterValue = c => Assert.Equal('z', c),
                DoubleNumber = d => Assert.Equal(42, d, 2),
                EnumValue = e => Assert.Equal(ExampleEnum.Option3, e),
                StringArray = s => Assert.Equal(3, s.Length),
                IntegerNumber = i => Assert.Equal(30, i),
                NullableNumber = n => Assert.Equal(5, n),
                StringValue = s => Assert.Equal("Hello world", s),
                NestedObjectValue = n => new NestedObjectAssertions(n)
                {
                    NestedInt = ObjectAssertionsHelpers.Ignore<int>("Out of test scope"),
                    NestedString = ns => Assert.Equal("ubuaa", ns)
                }.Assert()
            };


            assertions.Assert();
        }

    }
}