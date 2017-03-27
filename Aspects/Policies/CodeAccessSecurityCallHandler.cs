using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Permissions;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// The handler invokes the method <c>Demand()</c> on all code access security attributes applied to the invoked method.
    /// </summary>
    public class CodeAccessSecurityCallHandler : BaseCallHandler<bool>
    {
        /// <summary>
        /// Process the input and the context before the control is passed down the aspects pipeline.
        /// For various reasons it may cut the pipeline short by returning non-<see langword="null" />, e.g. due to an invalid parameter.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="_">ignored</param>
        /// <returns>IMethodReturn.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "protocol")]
        protected override IMethodReturn PreInvoke(
            IMethodInvocation input,
            bool _)
        {
            foreach (var a in input
                                .MethodBase
                                .GetCustomAttributes(true)
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

            return null;
        }
    }
}
