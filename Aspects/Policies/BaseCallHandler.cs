using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
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
        readonly MethodInfo _continueWithGeneric;

        /// <summary>
        /// Gets a value indicating whether the current class overrides <see cref="ContinueWith"/>
        /// </summary>
        protected bool IsContinueWithOverridden { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCallHandler{T}"/> class.
        /// </summary>
        protected BaseCallHandler()
        {
            _continueWithGeneric = GetType()
                                        .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                                        .Where(
                                            m =>
                                            {
                                                if (!m.IsGenericMethod        ||
                                                    m.Name != "ContinueWith"  ||
                                                    m.GetGenericArguments().Count() != 1)
                                                    return false;

                                                var parameters = m.GetParameters();

                                                return parameters.Count()          == 3                          &&
                                                       parameters[0].ParameterType == typeof(IMethodInvocation)  &&
                                                       parameters[1].ParameterType == typeof(IMethodReturn)      &&
                                                       parameters[2].ParameterType == typeof(T);
                                            })
                                        .Single();

            IsContinueWithOverridden = _continueWithGeneric.GetBaseDefinition().DeclaringType != _continueWithGeneric.DeclaringType;
        }

        /// <summary>
        /// Prepares per call data specific to the handler.
        /// </summary>
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
            Contract.Requires<ArgumentNullException>(input   != null, nameof(input));
            Contract.Requires<ArgumentNullException>(getNext != null, nameof(getNext));
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            var continueWith = GetContinueWith(input);
            var methodReturn = getNext().Invoke(input, getNext);

            if (continueWith != null)
                // attach the ContinueWith to the Task found in methodReturn.ReturnValue and put the new task in the result
                return input.CreateMethodReturn(
                                continueWith.Invoke(this, new object[] { input, methodReturn, callData }));
            else
                return methodReturn;
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
            Contract.Requires<ArgumentNullException>(input        != null, nameof(input));
            Contract.Requires<ArgumentNullException>(methodReturn != null, nameof(methodReturn));
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            return methodReturn;    // returns the final result or the task
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
        protected virtual async Task<TResult> ContinueWith<TResult>(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            T callData)
        {
            Contract.Requires<ArgumentNullException>(input        != null, nameof(input));
            Contract.Requires<ArgumentNullException>(methodReturn != null, nameof(methodReturn));
            Contract.Ensures(Contract.Result<Task<TResult>>() != null);

            var taskResult = methodReturn.ReturnValue as Task<TResult>;

            if (taskResult != null)
                return await taskResult;
            else
            {
                // in case the target method does not return Task<Result>, it must be just Task (see GetContinueWith), - we'll return Task<bool>, so return the default value false.
                await (Task)methodReturn.ReturnValue;
                return default(TResult);
            }
        }

        /// <summary>
        /// Creates and caches a new continue-with method for each method called on the target.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>MethodInfo.</returns>
        MethodInfo GetContinueWith(
            IMethodInvocation input)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));

            if (!input.IsAsyncCall() || !IsContinueWithOverridden)  // return directly the Task from the actual call. The caller will await it, i.e. there is no continue with here.
                return null;

            var resultType = input.ResultType();
            var returnedTaskResultType = resultType.IsGenericType
                                            ? resultType.GetGenericArguments()[0]
                                            : typeof(bool);

            return _continueWithGeneric.MakeGenericMethod(returnedTaskResultType);
        }

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

            try
            {
                if (input == null)
                    throw new ArgumentNullException(nameof(input));
                if (getNext == null)
                    throw new ArgumentNullException(nameof(getNext));

                var callData = Prepare(input);

                var methodReturn = PreInvoke(input, callData);

                if (methodReturn == null)
                    methodReturn = DoInvoke(input, getNext, callData);

                return PostInvoke(input, methodReturn, callData);
            }
            catch (Exception x)
            {
                return input.CreateExceptionMethodReturn(
                                new InvalidOperationException("Call handlers cannot throw exceptions. Caught in "+GetType().FullName, x));
            }
        }
        #endregion
    }
}
