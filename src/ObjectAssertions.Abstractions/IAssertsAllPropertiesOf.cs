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
        /// <summary>
        /// Run all assertions. This method will execute all delegates passed to object initializer, one by one.
        /// </summary>
        public void Assert();

        /// <summary>
        /// Return all assertion delegates. 
        /// This method will collect all delegates passed to object initializer, and return them as array (without executing them).
        /// It will return delegate without param, as correct property will be already setup inside delegate. Executing delegate will execute assertion.
        /// </summary>
        public System.Action [] CollectAssertions();
    }
}
