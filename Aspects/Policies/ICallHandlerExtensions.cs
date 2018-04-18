using System;
using System.Reflection;
using System.Threading.Tasks;

using Unity.Interception.PolicyInjection.Pipeline;

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
            if (input == null)
                throw new ArgumentNullException(nameof(input));

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
            if (input == null)
                throw new ArgumentNullException(nameof(input));

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
            if (methodReturn == null)
                throw new ArgumentNullException(nameof(methodReturn));

            return methodReturn.ReturnValue is Task;
        }
    }
}
