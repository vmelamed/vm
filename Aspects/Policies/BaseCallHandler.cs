using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// Generalizes the way call handlers work by dividing the process in four distinct phases:
    /// <list type="number">
    /// <item><see cref="Prepare"/>: gather and prepare per call local data of type <typeparamref name="T"/> that will be passed to the following phases.</item>
    /// <item><see cref="PreInvoke"/>: process the input and the context before the control is passed down the aspects pipeline.
    /// For various reasons it may cut the pipeline short, e.g. due to an invalid parameter</item>
    /// <item><see cref="DoInvoke"/>: synchronously or asynchronously pass the control down the aspects pipeline.</item>
    /// <item><see cref="PostInvoke"/>: process the output from the call so far and optionally modify the output.</item>
    /// </list>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="ICallHandler" />
    public abstract class BaseCallHandler<T> : ICallHandler
    {
        #region ICallHandler implementation
        /// <summary>
        /// Order in which the handler will be executed
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Implement this method to execute your handler processing.
        /// </summary>
        /// <param name="input">Inputs to the current call to the target.</param>
        /// <param name="getNext">Delegate to execute to get the next delegate in the handler
        /// chain.</param>
        /// <returns>Return value from the target.</returns>
        /// <exception cref="System.ArgumentNullException">thrown when <paramref name="input" /> or <paramref name="getNext" /> are <see langword="null" />.</exception>
        public virtual IMethodReturn Invoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext)
        {
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (getNext == null)
                throw new ArgumentNullException(nameof(getNext));

            var callData = Prepare(input);

            var methodReturn = PreInvoke(input, callData);

            if (methodReturn != null)
                return methodReturn;

            methodReturn = DoInvoke(input, getNext, callData);

            var methodTask = methodReturn.ReturnValue as Task;

            if (methodTask != null)
                methodTask.Wait();

            return PostInvoke(input, methodReturn, callData);
        }
        #endregion

        /// <summary>
        /// Prepares per call data specific to the handler.
        /// </summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <param name="input">The input.</param>
        /// <returns>T.</returns>
        protected virtual T Prepare(IMethodInvocation input)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));

            return default(T);
        }

        /// <summary>
        /// Process the input and the context before the control is passed down the aspects pipeline.
        /// For various reasons it may cut the pipeline short by returning non-<see langword="null"/>, e.g. due to an invalid parameter.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="callData">The per-call data.</param>
        /// <returns>IMethodReturn.</returns>
        protected virtual IMethodReturn PreInvoke(
            IMethodInvocation input,
            T callData)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));

            return null;
        }

        /// <summary>
        /// Synchronously or asynchronously passes the control down the aspects pipeline.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="getNext">Get next aspect in the pipeline.</param>
        /// <param name="callData">The per-call data.</param>
        /// <returns>IMethodReturn.</returns>
        protected virtual IMethodReturn DoInvoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext,
            T callData)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));
            Contract.Requires<ArgumentNullException>(getNext != null, nameof(getNext));
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            return getNext().Invoke(input, getNext);
        }

        /// <summary>
        /// Process the output from the call so far and optionally modify the output.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The method return.</param>
        /// <param name="callData">The per-call data.</param>
        /// <returns>IMethodReturn.</returns>
        protected virtual IMethodReturn PostInvoke(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            T callData)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));
            Contract.Requires<ArgumentNullException>(methodReturn != null, nameof(methodReturn));
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            return methodReturn;
        }
    }
}
