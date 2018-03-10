using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Practices.Unity.InterceptionExtension;

using vm.Aspects.Threading;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// Generalizes the way call handlers work by dividing the process in four distinct phases:
    /// <list type="number">
    /// <item><see cref="Prepare"/>: gather and prepare per-call local data of custom type <typeparamref name="T"/> that will be passed to the following phases.</item>
    /// <item><see cref="PreInvoke"/>: process the input and the context before the control is passed down the aspects pipeline.
    /// For various reasons it may cut the pipeline short, e.g. due to an invalid parameter</item>
    /// <item><see cref="DoInvoke"/>: synchronously or asynchronously pass the control down the aspects pipeline.</item>
    /// <item><see cref="PostInvoke"/>: process the output from the call so far and optionally modify the output.</item>
    /// <item><see cref="ContinueWith"/>: process the final result from asynchronous TPL style calls and optionally modify the final result.</item>
    /// </list>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="ICallHandler" />
    public abstract class BaseCallHandler<T> : NongenericBaseCallHandler, ICallHandler
    {
        readonly MethodIsOverridden _continueWith;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCallHandler{T}"/> class.
        /// </summary>
        protected BaseCallHandler()
        {
            // Get the method info for an overridden ContinueWith, if exists in the inheriting call handler:
            //
            // protected virtual async Task<TResult> ContinueWith<TResult>(IMethodInvocation input, IMethodReturn methodReturn, T callData);
            //
            using (SyncContinueWithGenericMethods.UpgradableReaderLock())
                if (!ContinueWithGenericMethods.TryGetValue(GetType(), out MethodIsOverridden methodIsOverridden))
                {
                    var continueWithGeneric = GetType()
                                                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                                                .Where(
                                                    m =>
                                                    {
                                                        if (m.IsGenericMethod               &&
                                                            m.Name == nameof(ContinueWith)  &&
                                                            m.GetGenericArguments().Count() == 1)
                                                        {
                                                            var parameters = m.GetParameters();

                                                            return parameters.Count()          == 3                          &&
                                                                   parameters[0].ParameterType == typeof(IMethodInvocation)  &&
                                                                   parameters[1].ParameterType == typeof(IMethodReturn)      &&
                                                                   parameters[2].ParameterType == typeof(T);
                                                        }
                                                        else
                                                            return false;
                                                    })
                                                .Single();

                    using (SyncContinueWithGenericMethods.WriterLock())
                        ContinueWithGenericMethods[GetType()] = new MethodIsOverridden(continueWithGeneric);
                }
        }

        #region ICallHandler implementation
        /// <summary>
        /// Order in which the handler will be executed
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Implement this method to execute your call-handler process. Here it is implemented like a call template:
        /// <list type="number">
        /// <item><see cref="Prepare"/> builds the "call data" context for the subsequent phases of this call</item>
        /// </list>
        /// </summary>
        /// <param name="input">Inputs to the current call to the target.</param>
        /// <param name="getNext">Delegate to execute to get the next delegate in the handler
        /// chain.</param>
        /// <returns>Return value from the target.</returns>
        /// <exception cref="ArgumentNullException">thrown when <paramref name="input" /> or <paramref name="getNext" /> are <see langword="null" />.</exception>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "protocol")]
        public virtual IMethodReturn Invoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext)
        {
            try
            {
                if (input == null)
                    throw new ArgumentNullException(nameof(input));
                if (getNext == null)
                    throw new ArgumentNullException(nameof(getNext));

                // 1. Prepare/builds the "call data" context for the subsequent phases of this call
                var callData = Prepare(input);

                // 2. PreInvoke does things that need to be done before invoking the next aspect in the pipeline (e.g. parameter validation or generate a call audit in the logs)
                var methodReturn = PreInvoke(input, callData);

                // 3. If the PreInvoke was successful, call DoInvoke to invoke the next aspect or final object in the pipeline.
                if (methodReturn == null)
                    methodReturn = DoInvoke(input, getNext, callData);

                // 4. PostInvoke does things that need to be done after the actual work is done, e.g. audit the output in the logs, or end the transaction, or process the exception from the invoke, etc.
                return PostInvoke(input, methodReturn, callData);
            }
            catch (Exception x)
            {
                // we should not have exceptions thrown outside of the aspect - log and pass the exception on as a return result and swallow it.
                return input.CreateExceptionMethodReturn(
                                new InvalidOperationException(
                                        $"Call handlers cannot throw exceptions - this is a bug that needs fixing. The exception was caught in {GetType().FullName}.Invoke while handling the call to {input.MethodBase.DumpString()} See also the inner exception for more details.", x));
            }
        }
        #endregion

        /// <summary>
        /// builds the "call data" context for the subsequent phases of this call.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>T.</returns>
        protected virtual T Prepare(
            IMethodInvocation input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // create a call data context data for the current handler

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
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // Process the input and the context before the control is passed down the aspects pipeline.
            // For various reasons it may cut the pipeline short by returning non-<see langword="null"/>, e.g. due to an invalid parameter.

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
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (getNext == null)
                throw new ArgumentNullException(nameof(getNext));

            var continueWith = GetContinueWith(input);
            var methodReturn = getNext().Invoke(input, getNext);

            if (continueWith == null)
                return methodReturn;

            try
            {
                // ContinueWith will return a new task that will invoke asynchronously the target method and then will continue with its own post processing
                return input.CreateMethodReturn(
                                continueWith.Invoke(this, new object[] { input, methodReturn, callData }));
            }
            catch (Exception x)
            {
                // the above fails (e.g. synchronously) - put the exception in the result data
                return input.CreateExceptionMethodReturn(x);
            }
        }

        /// <summary>
        /// Process the output from the call so far and optionally modifies the output.
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
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (methodReturn == null)
                throw new ArgumentNullException(nameof(methodReturn));

            // Process the output from the call so far (async call may not be finished yet) and optionally modify the output.

            return methodReturn;
        }

        /// <summary>
        /// Gives the aspect a chance to do some final work after the main task is truly complete.
        /// The overriding implementations should begin by calling the base class' implementation first.
        /// Here we can throw exceptions, which will be the true result of a failed operation.
        /// At the caller the exception should arrive wrapped in a <see cref="AggregateException"/>.
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
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (methodReturn == null)
                throw new ArgumentNullException(nameof(methodReturn));

            // if the call already failed - throw the exception - DoInvoke should catch it
            if (methodReturn.Exception != null)
                throw methodReturn.Exception;

            // if the method returns Task<T> await the task's completion and return the result
            if (methodReturn.ReturnValue is Task<TResult> taskResult)
                return await DoContinueWith(
                                input,
                                methodReturn,
                                callData,
                                await taskResult);

            // if the method is plain Task - await the task's completion and return Task<bool> equal to Task.FromResult(false);
            if (methodReturn.ReturnValue is Task task)
            {
                await task;
                await DoContinueWith(
                                input,
                                methodReturn,
                                callData);
            }

            // - we'll return Task<bool>, so return the default value false.
            return default(TResult);
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
        protected virtual Task<TResult> DoContinueWith<TResult>(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            T callData,
            TResult result)
        {
            // do the final handling

            return Task.FromResult(result);
        }

        /// <summary>
        /// Performs any action that needs to take place when the target method completes
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The method return.</param>
        /// <param name="callData">The call data.</param>
        /// <returns>Task.</returns>
        protected virtual Task DoContinueWith(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            T callData)
        {
            // do the final handling

            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates and caches a new, invokable continue-with method out of ContinueWith&lt;T&gt; for each method called on the target.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>MethodInfo.</returns>
        MethodInfo GetContinueWith(
            IMethodInvocation input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var handlerForMethod = new HandlerForMethod(GetType(), input.MethodBase);

            using (SyncMethodToContinueWith.UpgradableReaderLock())
            {

                // if we have it cached already - return it
                if (MethodToContinueWith.TryGetValue(handlerForMethod, out MethodInfo methodInfo))
                    return methodInfo;

                if (input.IsAsyncCall() && _continueWith.IsOverridden)
                {
                    var resultType       = input.ResultType();
                    var resultTypeOfTask = resultType.IsGenericType
                                                        ? resultType.GetGenericArguments()[0]
                                                        : typeof(bool);

                    methodInfo = _continueWith.MethodInfo.MakeGenericMethod(resultTypeOfTask);
                }
                else
                    methodInfo = null;  // effectively return directly the Task from the actual call. The caller will await it, i.e. there is no continue-with in the current call handler.

                using (SyncMethodToContinueWith.WriterLock())
                    return MethodToContinueWith[handlerForMethod] = methodInfo;
            }
        }
    }
}
