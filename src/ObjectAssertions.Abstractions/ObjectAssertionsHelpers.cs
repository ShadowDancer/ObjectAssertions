using System;

namespace ObjectAssertions.Abstractions
{
    /// <summary>
    /// Contains helper classes designed to be used with assertion
    /// </summary>
    public class ObjectAssertionsHelpers
    {
        /// <summary>
        /// Ignores assertion. Can be used in object initializer of class implementing <see cref="IAssertsAllPropertiesOf{TObject}"/> interface.
        /// This method does nothing.
        /// </summary>
        /// <typeparam name="T">Type of property which is ignored</typeparam>
        /// <param name="reason">Explains why property check was ignored</param>
        /// <returns>Noop delegate</returns>
        public static Action<T> Ignore<T>(string reason)
        {
            return t => { };
        }
    }
}
