using ObjectAssertions.Abstractions;
using ObjectAssertions.Sample;
using System;
using Xunit;
using static ObjectAssertions.Sample.ObjectToAssert;

namespace ObjectAssertions.Sample
{
    public abstract class AbstractClass
    {
        public abstract string Property { get; }
    }

    public class ImplementationClass : AbstractClass
    {
        public override string Property => "value";
    }


    public class ObjectAssertionsBugRepro
    {
        [Fact]
        public void ImplementationClass_NotRequires_AssertionOfAbstractProperty()
        {
            var derivedClass = new ImplementationClass();

            var assertions = new DerivedClassAssertions(derivedClass)
            {
                Property = a => Assert.NotNull(a)
            };

            assertions.Assert();
        }
    }



    public partial class DerivedClassAssertions : IAssertsAllPropertiesOf<ImplementationClass>
    {
    }
}