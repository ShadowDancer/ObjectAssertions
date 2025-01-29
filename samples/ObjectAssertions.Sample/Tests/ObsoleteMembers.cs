using ObjectAssertions.Abstractions;
using System;
using Xunit;
using static ObjectAssertions.Sample.ObjectToAssert;

namespace ObjectAssertions.Sample
{
    // This is data model in your program
    public record ObjectWithObsoleteMembersToAssert
    {

        [Obsolete]
        public string ObsoleteValue { get; set; } = "I'm obsolete";

        [Obsolete("Obsolete with message has different warning")]
        public string ObsoleteValueWithText { get; set; } = "I'm obsolete";
    }

    // This is test utility for generating assertions
    public partial class ObsoleteAssertions : IAssertsAllPropertiesOf<ObjectWithObsoleteMembersToAssert>
    {

    }

    // This is your tests class
    public class ObsoleteAssertionTests
    {
        [Fact]
        public void HowTo()
        {
            var testObject = new ObjectWithObsoleteMembersToAssert();


            var assertions = new ObsoleteAssertions(testObject)
            {
                ObsoleteValue = o => Assert.Equal("I'm obsolete", o), // Obsolete warnings are ignored in generated code
                ObsoleteValueWithText = o => Assert.Equal("I'm obsolete", o) // Obsolete warnings are ignored in generated code
            };


            assertions.Assert();
        }

    }
}