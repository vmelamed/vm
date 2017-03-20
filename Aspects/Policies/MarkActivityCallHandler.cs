using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using vm.Aspects.Facilities;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// Marks the current call context with a GUID for use by the log entries.
    /// </summary>
    /// <seealso cref="BaseCallHandler{Guid}" />
    public class MarkActivityCallHandler : BaseCallHandler<Guid>
    {
        /// <summary>
        /// Prepares per call data specific to the handler.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>T.</returns>
        protected override Guid Prepare(
            IMethodInvocation input)
        {
            var activityId = Facility.GuidGenerator.NewGuid();

            CallContext.LogicalSetData(LogWriterFacades.ActivityIdSlotName, activityId);

            return activityId;
        }

        /// <summary>
        /// Process the output from the call so far and optionally modify the output.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The method return.</param>
        /// <param name="callData">The per-call data.</param>
        /// <returns>IMethodReturn.</returns>
        protected override IMethodReturn PostInvoke(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            Guid callData)
        {
            try
            {
                return base.PostInvoke(input, methodReturn, callData);
            }
            finally
            {
                CallContext.LogicalSetData(LogWriterFacades.ActivityIdSlotName, default(Guid));
            }
        }

        /// <summary>
        /// Gives the aspect a chance to do some final work after the main task is truly complete.
        /// The overriding implementations should begin by calling the base class' implementation first.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The method return.</param>
        /// <param name="callData">The call data.</param>
        /// <returns>Task{TResult}.</returns>
        protected override async Task<TResult> ContinueWith<TResult>(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            Guid callData)
        {
            try
            {
                return await base.ContinueWith<TResult>(input, methodReturn, callData);
            }
            finally
            {
                CallContext.LogicalSetData(LogWriterFacades.ActivityIdSlotName, default(Guid));
            }
        }
    }
}
