using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace vm.Aspects.Policies
{
#pragma warning disable CS3003 // Constraint type is not CLS-compliant
    /// <summary>
    /// Class CallData encapsulates some audit data to be output about the current call.
    /// </summary>
    public class CallTraceData
    {
        /// <summary>
        /// Gets or sets a value indicating whether to trace the current call.
        /// </summary>
        public bool Trace { get; set; }

        /// <summary>
        /// Gets or sets the call stack.
        /// </summary>
        public string CallStack { get; set; }

        /// <summary>
        /// Gets or sets the call timer that measures the actual call duration.
        /// </summary>
        public Stopwatch CallTimer { get; set; }

        /// <summary>
        /// Gets or sets the caller's principal identity name.
        /// </summary>
        public string IdentityName { get; set; }

        /// <summary>
        /// Gets or sets the return value.
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// Gets or sets the output values.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IParameterCollection OutputValues { get; set; }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the parallel task of logging data available before the call.
        /// </summary>
        public Task LogBeforeCall { get; set; }
    }
#pragma warning restore CS3003 // Constraint type is not CLS-compliant
}
