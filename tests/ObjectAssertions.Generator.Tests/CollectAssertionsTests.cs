using System.Collections.Generic;
using Xunit;

namespace ObjectAssertions.Generator.Tests;

public class CollectAssertionsTests
{
    [Fact]
    public void CollectAssertions_BasicScenario()
    {
        var executedAssertions = new List<string>();
        var testObject = new TestClass();

        var actions = new TestClassAssertions(testObject)
        {
            IntProperty = i => executedAssertions.Add("IntProperty"),
            StringProperty = s => executedAssertions.Add("StringProperty"),
            BoolProperty = b => executedAssertions.Add("BoolProperty"),
            DoubleProperty = d => executedAssertions.Add("DoubleProperty")
        }.CollectAssertions();

        foreach (var action in actions)
        {
            action();
        }

        var expected = new[] { "IntProperty", "StringProperty", "BoolProperty", "DoubleProperty" };
        Assert.Equal(expected.Length, executedAssertions.Count);
        Assert.Equal(expected, executedAssertions);
    }

    [Fact]
    public void CollectAssertions_WorksWithObsoleteProperties()
    {
        var executedAssertions = new List<string>();
        var testObject = new TestClassWithObsolete();

#pragma warning disable CS0618
        var actions = new TestClassWithObsoleteAssertions(testObject)
#pragma warning restore CS0618
        {
#pragma warning disable CS0612
            ObsoleteProperty = o => executedAssertions.Add("ObsoleteProperty"),
#pragma warning restore CS0612
            NormalProperty = n => executedAssertions.Add("NormalProperty")
        }
            
            .CollectAssertions();

        foreach (var action in actions)
        {
            action();
        }

        var expected = new[] { "ObsoleteProperty", "NormalProperty" };
        Assert.Equal(expected.Length, executedAssertions.Count);
        Assert.Equal(expected, executedAssertions);
    }

    [Fact]
    public void CollectAssertions_ReturnsCorrectActionCount()
    {
        var testObject = new TestClass();

        var actions = new TestClassAssertions(testObject)
        {
            IntProperty = i => { },
            StringProperty = s => { },
            BoolProperty = b => { },
            DoubleProperty = d => { }
        }.CollectAssertions();

        Assert.Equal(4, actions.Length);
        Assert.All(actions, action => Assert.NotNull(action));
    }

    [Fact]
    public void CollectAssertions_ActionsExecuteInCorrectOrder()
    {
        var executionOrder = new List<string>();
        var testObject = new TestClass();

        var actions = new TestClassAssertions(testObject)
        {
            IntProperty = i => executionOrder.Add("IntProperty"),
            StringProperty = s => executionOrder.Add("StringProperty"),
            BoolProperty = b => executionOrder.Add("BoolProperty"),
            DoubleProperty = d => executionOrder.Add("DoubleProperty")
        }.CollectAssertions();

        // Execute actions in order
        foreach (var action in actions)
        {
            action();
        }

        var expectedOrder = new[] { "IntProperty", "StringProperty", "BoolProperty", "DoubleProperty" };
        Assert.Equal(expectedOrder, executionOrder);
    }

    [Fact]
    public void CollectAssertions_ReturnsNonNullActions()
    {
        var testObject = new TestClass();

        var actions = new TestClassAssertions(testObject)
        {
            IntProperty = i => { },
            StringProperty = s => { },
            BoolProperty = b => { },
            DoubleProperty = d => { }
        }.CollectAssertions();

        Assert.NotNull(actions);
        Assert.All(actions, Assert.NotNull);
    }
}
