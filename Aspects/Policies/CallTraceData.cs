using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;

using Microsoft.Practices.EnterpriseLibrary.Logging;

using Unity.Interception.PolicyInjection.Pipeline;

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
        /// Gets or sets the caller's principal identity.
        /// </summary>
        public IIdentity Identity { get; set; }

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
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the before call log entry.
        /// </summary>
        public LogEntry BeforeCallLogEntry { get; set; }

        /// <summary>
        /// Gets or sets the after call log entry.
        /// </summary>
        public LogEntry AfterCallLogEntry { get; set; }

        /// <summary>
        /// Gets or sets the parallel task of logging data available before the call.
        /// </summary>
        public Task LogBeforeCall { get; set; }

        /// <summary>
        /// Gets or sets the parallel task of logging data available before the call but intended to be dumped in the after the call.
        /// </summary>
        public Task BeginLogAfterCall { get; set; }

        /// <summary>
        /// Gets or sets the writer to be used for the after call dump.
        /// </summary>
        public StringWriter AfterCallWriter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the call has <see langword="out"/> or <see langword="ref"/> parameters.
        /// </summary>
        public bool HasOutParameters { get; set; }
    }
#pragma warning restore CS3003 // Constraint type is not CLS-compliant
}
