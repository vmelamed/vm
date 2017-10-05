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
    public class ActivityTracerCallHandler : BaseCallHandler<Tracer>
    {
        static object _sync = new object();
        static TraceManager traceManager;

        /// <summary>
        /// Prepares per call data specific to the handler.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>T.</returns>
        protected override Tracer Prepare(
            IMethodInvocation input)
        {
            if (traceManager == null)
                lock (_sync)
                    if (traceManager == null)
                        traceManager = new TraceManager(Facility.LogWriter);

            return traceManager.StartTrace(LogWriterFacades.General);
        }

        /// <summary>
        /// Process the output from the call so far and optionally modify the output.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The method return.</param>
        /// <param name="tracer">The tracer.</param>
        /// <returns>IMethodReturn.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "2#")]
        protected override IMethodReturn PostInvoke(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            Tracer tracer)
        {
            if (!input.IsAsyncCall())
            {
                traceManager.Dispose();
                traceManager = null;
            }

            return base.PostInvoke(input, methodReturn, tracer);
        }

        /// <summary>
        /// Gives the aspect a chance to do some final work after the main task is truly complete.
        /// The overriding implementations should begin by calling the base class' implementation first.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The method return.</param>
        /// <param name="tracer">The call data.</param>
        /// <returns>Task{TResult}.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "2#")]
        protected override async Task<TResult> ContinueWith<TResult>(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            Tracer tracer)
        {
            try
            {
                return await base.ContinueWith<TResult>(input, methodReturn, tracer);
            }
            finally
            {
                traceManager.Dispose();
                traceManager = null;
            }
        }
    }
}
