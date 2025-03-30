using ObjectAssertions.Abstractions;
using System;
using Xunit;

namespace ObjectAssertions.Sample
{
    public record ObjectForFailingAssertionTests
    {
        public string Text { get; set; } = "Hello";
    }

    public partial class ObjectForFailingAssertionTestsAssertions : IAssertsAllPropertiesOf<ObjectForFailingAssertionTests>
    {
    }
    
    /// <summary>
    /// Ensure that thrown exception is originating from user code
    /// </summary>
    public class CustomAssertionException : Exception
    {
        public CustomAssertionException() : base() { }
    }

    public class FailingAssertionsTests
    {
        [Fact]
        public void Assert_ThrowsException_WhenAssertionFails()
        {
            var assertions = new ObjectForFailingAssertionTestsAssertions(new ObjectForFailingAssertionTests())
            {
                Text = t => throw new CustomAssertionException()
            };

            var exception = Assert.Throws<CustomAssertionException>(() => 
            {
                assertions.Assert();
            });
        }

        [Fact]
        public void CollectAssertions_ReturnsAssertions_ThatCanFail()
        {
            var assertions = new ObjectForFailingAssertionTestsAssertions(new ObjectForFailingAssertionTests())
            {
                Text = t => throw new CustomAssertionException()
            };

            var collectedAssertions = assertions.CollectAssertions();
            
            Assert.Single(collectedAssertions);
            var textException = Assert.Throws<CustomAssertionException>(() => 
            {
                collectedAssertions[0]();
            });
        }
    }
} 