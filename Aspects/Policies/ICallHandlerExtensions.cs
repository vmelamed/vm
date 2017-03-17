using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading.Tasks;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// IMethodReturn extension methods.
    /// </summary>
    public static class ICallHandlerExtensions
    {
        /// <summary>
        /// Gets the type of the returned value from the current method.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Type.</returns>
        public static Type ResultType(
            this IMethodInvocation input)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));

            return ((MethodInfo)input.MethodBase).ReturnType;
        }
        /// <summary>
        /// Determines whether the current method is asynchronous.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns><see langword="true" /> if the method is asynchronous; otherwise, <see langword="false" />.</returns>
        public static bool IsAsyncCall(
            this IMethodInvocation input)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));

            return typeof(Task).IsAssignableFrom(input.ResultType());
        }

        /// <summary>
        /// Determines whether the <paramref name="methodReturn"/> is a result from an asynchronous call.
        /// </summary>
        /// <param name="methodReturn">The result.</param>
        /// <returns><see langword="true" /> if <paramref name="methodReturn"/> is a result from an asynchronous call; otherwise, <see langword="false" />.</returns>
        public static bool IsAsyncCall(
            this IMethodReturn methodReturn)
        {
            Contract.Requires<ArgumentNullException>(methodReturn != null, nameof(methodReturn));

            return methodReturn.ReturnValue is Task;
        }
    }
}
