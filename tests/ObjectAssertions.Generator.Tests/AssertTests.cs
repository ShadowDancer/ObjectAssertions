using System;
using System.Collections.Generic;
using Xunit;

namespace ObjectAssertions.Generator.Tests;

public class AssertTests
{
    [Fact]
    public void Assert_ExecutesAssertionsInCorrectOrder()
    {
        var executionOrder = new List<string>();
        var testObject = new TestClass();

        var assertions = new TestClassAssertions(testObject)
        {
            IntProperty = i => executionOrder.Add("IntProperty"),
            StringProperty = s => executionOrder.Add("StringProperty"),
            BoolProperty = b => executionOrder.Add("BoolProperty"),
            DoubleProperty = d => executionOrder.Add("DoubleProperty")
        };

        assertions.Assert();

        var expectedOrder = new[] { "IntProperty", "StringProperty", "BoolProperty", "DoubleProperty" };
        Assert.Equal(expectedOrder, executionOrder);
    }

    [Fact]
    public void Assert_PropagatesExceptionFromAssertion()
    {
        var testObject = new TestClass();

        var assertions = new TestClassAssertions(testObject)
        {
            IntProperty = i => throw new InvalidOperationException("Test exception"),
            StringProperty = s => { },
            BoolProperty = b => { },
            DoubleProperty = d => { }
        };

        Assert.Throws<InvalidOperationException>(() => assertions.Assert());
    }
}
