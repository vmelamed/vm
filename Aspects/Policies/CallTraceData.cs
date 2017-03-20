using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Diagnostics;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// Class CallData encapsulates some audit data to be output about the current call.
    /// </summary>
    public class CallTraceData
    {
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
        public IParameterCollection OutputValues { get; set; }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; set; }
    }
}
