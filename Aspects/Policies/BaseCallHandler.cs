using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Unity.Interception.PolicyInjection.Pipeline;

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
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCallHandler{T}"/> class.
        /// </summary>
        protected BaseCallHandler()
        {
            // Get the method info for an overridden ContinueWith<TResult>, if exists in the inheriting call handler:
            using (SyncHandlerToGenericContinueWith.UpgradableReaderLock())
                if (!HandlerToGenericContinueWith.TryGetValue(GetType(), out MethodInfo continueWithGeneric))
                {
                    continueWithGeneric = GetType().GetMethod(
                                                            nameof(ContinueWith),
                                                            BindingFlags.Instance | BindingFlags.NonPublic,
                                                            null,
                                                            new[] { typeof(IMethodInvocation), typeof(IMethodReturn), typeof(T) },
                                                            null);

                    var isOverridden = continueWithGeneric.IsOverridden() ||
                                       GetType().GetMethods().Any(mi => mi.Name == nameof(DoContinueWith) && mi.IsOverridden());

                    using (SyncHandlerToGenericContinueWith.WriterLock())
                        HandlerToGenericContinueWith[GetType()] = isOverridden ? continueWithGeneric : null;
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

                // 4. PostInvoke does things that need to be done after the actual work is done,
                //    e.g. audit the output in the logs, or end the transaction, or process the exception from the invoke, etc.
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

            return default;
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
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is how it works.")]
        protected virtual IMethodReturn DoInvoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext,
            T callData)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (getNext == null)
                throw new ArgumentNullException(nameof(getNext));

            // call the target method
            var methodReturn = getNext().Invoke(input, getNext);
            var continueWith = GetContinueWith(input);

            if (continueWith == null)
                return methodReturn;

            try
            {
                // ContinueWith should return a new task that awaits the target method and then will await its own post processing
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

            // if the method returns Task<T> await the task's completion and return the result
            if (methodReturn.ReturnValue is Task<TResult> taskResult)
            {
                var result = await taskResult;

                await DoContinueWith(
                                input,
                                methodReturn,
                                callData,
                                result);

                return result;
            }

            // if the method is plain Task - await the task's completion and return a default value (false) that is ignored anyway;
            if (methodReturn.ReturnValue is Task task)
            {
                await task;

                await DoContinueWith(
                                input,
                                methodReturn,
                                callData);
            }

            // - we'll return Task<bool>, so return the default value false.
            return default;
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
        /// Creates and caches a new, closed/invokable continue-with method out of ContinueWith&lt;T&gt; for each method called on the target.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>MethodInfo.</returns>
        MethodInfo GetContinueWith(
            IMethodInvocation input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (!input.IsAsyncCall())
                return null;

            var returnType            = ((MethodInfo)input.MethodBase).ReturnType;
            var resultType            = returnType.IsGenericType
                                            ? returnType.GetGenericArguments()[0]
                                            : typeof(bool);
            var handlerTypeReturnType = new HandlerTypeReturnType(GetType(), returnType);

            // if we have it cached already - return it
            using (SyncMethodToContinueWith.ReaderLock())
                if (HandlerTypeReturnTypeToContinueWith.TryGetValue(handlerTypeReturnType, out MethodInfo cachedContinueWith))
                    return cachedContinueWith;

            // get the continueWith MethodInfo
            MethodInfo genericContinueWith;

            using (SyncHandlerToGenericContinueWith.ReaderLock())
                genericContinueWith = HandlerToGenericContinueWith[GetType()];        // if it is not overridden, it will be null!

            var continueWith = genericContinueWith?.MakeGenericMethod(resultType);

            using (SyncMethodToContinueWith.WriterLock())
                return HandlerTypeReturnTypeToContinueWith[handlerTypeReturnType] = continueWith;
        }
    }
}
