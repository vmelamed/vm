using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Permissions;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// The handler invokes the method <c>Demand()</c> on all code access security attributes applied to the invoked method.
    /// </summary>
    public class CodeAccessSecurityCallHandler : ICallHandler
    {
        #region ICallHandler Members

        /// <summary>
        /// Invokes the method <c>Demand()</c> on all code access security attributes applied to the invoked method.
        /// </summary>
        /// <param name="input">Inputs to the current call to the target.</param>
        /// <param name="getNext">Delegate to execute to get the next delegate in the handler
        /// chain.</param>
        /// <returns>Return value from the target.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// input
        /// or
        /// getNext
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification="This is the ICallHandler protocol.")]
        public IMethodReturn Invoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext)
        {
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            if (input == null)
                throw new ArgumentNullException("input");
            if (getNext == null)
                throw new ArgumentNullException("getNext");

            foreach (var a in input.MethodBase.GetCustomAttributes(true)
                                              .OfType<CodeAccessSecurityAttribute>())
                if (!a.Unrestricted && a.Action == SecurityAction.Demand)
                    try
                    {
                        a.CreatePermission()
                         .Demand();
                    }
                    catch (Exception x)
                    {
                        return input.CreateExceptionMethodReturn(x);
                    }

            return getNext().Invoke(input, getNext);
        }

        /// <summary>
        /// Order in which the handler will be executed
        /// </summary>
        public int Order { get; set; }

        #endregion
    }
}
