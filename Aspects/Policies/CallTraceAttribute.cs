using System;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// Controls whether the <see cref="CallTraceCallHandler"/> will trace the calls on the current class or method.
    /// The attribute applied to a method has precedence over the attribute applied to the method's class (if present).
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = false, Inherited = true)]
    public sealed class CallTraceAttribute : Attribute
    {
        /// <summary>
        /// Gets a value indicating whether to trace the calls on the current class or method.
        /// </summary>
        public bool Trace { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallTraceAttribute"/> class.
        /// </summary>
        /// <param name="trace">if set to <c>true</c> the <see cref="CallTraceCallHandler"/> will trace the calls on the current class or method; otherwise the trace will be suppressed.</param>
        public CallTraceAttribute(
            bool trace = true)
        {
            Trace = trace;
        }
    }
}
