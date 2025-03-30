using ObjectAssertions.Abstractions;
using System;
using Xunit;
using static ObjectAssertions.Sample.ObjectToAssert;

namespace ObjectAssertions.Sample
{
    public record BaseObjectWithObsoleteMember
    {
        [Obsolete("Obsolete from base class")]
        public string ObsoleteFromBaseClass { get; set; } = "I'm obsolete, and I live in base class";
    }

    // This is data model in your program
    public record ObjectWithObsoleteMembersToAssert : BaseObjectWithObsoleteMember
    {

        [Obsolete]
        public string ObsoleteValue { get; set; } = "I'm obsolete";

        [Obsolete("Obsolete with message has different warning")]
        public string ObsoleteValueWithText { get; set; } = "I'm obsolete";
        
        [Obsolete(null, true)]
        public string ObsoleteValueWithError { get; set; } = "I'm obsolete, I generate compile errors";

        [Obsolete("Obsolete with error and message", true)]
        public string ObsoleteValueWithTextAndError { get; set; } = "I'm obsolete, with message and errors";
    }

    // This is test utility for generating assertions
    public partial class ObsoleteAssertions : IAssertsAllPropertiesOf<ObjectWithObsoleteMembersToAssert>
    {

    }

    // This is your tests class
    public class ObsoleteAssertionTests
    {
        [Fact]
        public void AssertionClass_Works_WhenClassHasObsoleteMembers()
        {
            var testObject = new ObjectWithObsoleteMembersToAssert();

#pragma warning disable CS0618
#pragma warning disable CS0612
            var assertions = new ObsoleteAssertions(testObject)
            {
                ObsoleteValue = o => Assert.Equal("I'm obsolete", o),
                ObsoleteValueWithText = o => Assert.Equal("I'm obsolete", o),
                ObsoleteValueWithError = o => Assert.Equal("I'm obsolete, I generate compile errors", o),
                ObsoleteValueWithTextAndError = o => Assert.Equal("I'm obsolete, with message and errors", o),
                ObsoleteFromBaseClass = o => Assert.Equal("I'm obsolete, and I live in base class", o)
            };
#pragma warning restore CS0618
#pragma warning restore CS0612


            assertions.Assert();
        }

    }
}