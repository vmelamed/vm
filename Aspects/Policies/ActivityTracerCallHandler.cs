using System;
using System.Threading.Tasks;

using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.Unity.InterceptionExtension;

using vm.Aspects.Facilities;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// Marks the current call context with a GUID for use by the log entries.
    /// </summary>
    /// <seealso cref="BaseCallHandler{Guid}" />
    public class ActivityTracerCallHandler : BaseCallHandler<ActivityTracer>
    {
        static object _syncTraceManager = new object();
        static TraceManager _traceManager;

        /// <summary>
        /// Prepares per call data specific to the handler.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>T.</returns>
        protected override ActivityTracer Prepare(
            IMethodInvocation input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            if (_traceManager is null)
                lock (_syncTraceManager)
                    if (_traceManager is null)
                        _traceManager = new TraceManager(Facility.LogWriter);

            return new ActivityTracer(_traceManager.StartTrace(LogWriterFacades.General));
        }

        /// <summary>
        /// Process the output from the call so far and optionally modifies the output.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The method return.</param>
        /// <param name="callData">The per-call data.</param>
        /// <returns>IMethodReturn.</returns>
        protected override IMethodReturn PostInvoke(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            ActivityTracer callData)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (methodReturn is null)
                throw new ArgumentNullException(nameof(methodReturn));

            if (!(methodReturn.ReturnValue is Task))
                Cleanup(callData);

            return methodReturn;
        }

        /// <summary>
        /// Performs any action that needs to take place when the target method completes
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The method return.</param>
        /// <param name="callData">The call data.</param>
        /// <returns>Task.</returns>
        protected override Task DoContinueWith(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            ActivityTracer callData)
        {
            Cleanup(callData);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs any action that needs to take place when the target method completes
        /// </summary>
        /// <typeparam name="TResult">The type of the method's result.</typeparam>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The method return data.</param>
        /// <param name="callData">The call data.</param>
        /// <param name="result">The result from the method.</param>
        /// <returns>Task&lt;TResult&gt;.</returns>
        protected override Task<TResult> DoContinueWith<TResult>(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            ActivityTracer callData,
            TResult result)
        {
            Cleanup(callData);
            return Task.FromResult(result);
        }

        static void Cleanup(ActivityTracer callData)
        {
            callData.Tracer?.Dispose();
            callData.Tracer = null;
        }
    }
}
