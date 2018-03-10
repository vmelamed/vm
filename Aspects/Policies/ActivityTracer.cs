using System;

using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// Struct ActivityTracer encapsulates a tracer.
    /// </summary>
    public struct ActivityTracer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTracer"/> struct.
        /// </summary>
        /// <param name="tracer">The tracer.</param>
        /// <exception cref="ArgumentNullException">tracer</exception>
        public ActivityTracer(
            Tracer tracer)
        {
            Tracer = tracer  ??  throw new ArgumentNullException(nameof(tracer));
        }

        /// <summary>
        /// Gets or sets the tracer.
        /// </summary>
        public Tracer Tracer { get; set; }
    }
}
