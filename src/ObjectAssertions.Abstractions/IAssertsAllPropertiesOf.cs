using System;

namespace ObjectAssertions.Abstractions
{
    /// <summary>
    /// This is marker interface used to designate type for which assertions will be generated.
    /// Property with required keyword will be generated for each property of <see cref="TObject"/>
    /// This property will contain code to execute when calling Assert.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    public interface IAssertsAllPropertiesOf<TObject>
    {

    }
}
