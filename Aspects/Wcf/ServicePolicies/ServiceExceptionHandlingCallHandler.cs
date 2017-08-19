using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity.InterceptionExtension;
using vm.Aspects.Facilities;
using vm.Aspects.Policies;
using vm.Aspects.Threading;
using vm.Aspects.Wcf.FaultContracts;

namespace vm.Aspects.Wcf.ServicePolicies
{
    /// <summary>
    /// Class ServiceExceptionHandlingCallHandler is an exception handling call policy which converts all exceptions thrown by the target method to
    /// their corresponding faults and then throws <see cref="FaultException{TFault}"/>.
    /// If particular exception does not have a mapped <see cref="Fault"/> or the fault is not in the list of fault contracts for the method the
    /// policy will throe plain <see cref="FaultException"/> with elaborate message text.
    /// </summary>
    /// <seealso cref="ICallHandler" />
    public sealed class ServiceExceptionHandlingCallHandler : BaseCallHandler<bool>
    {
        /// <summary>
        /// Gets or sets the name of the exception handling policy used by this handler.
        /// The default value is <see cref="ServiceFaultFromExceptionHandlingPolicies.PolicyName"/>
        /// </summary>
        public string ExceptionHandlingPolicyName { get; set; } = ServiceFaultFromExceptionHandlingPolicies.PolicyName;

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
        protected override async Task<TResult> ContinueWith<TResult>(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            bool callData)
        {
            try
            {
                return await base.ContinueWith<TResult>(input, methodReturn, callData);
            }
            catch (Exception x)
            {
                // process it (e.g.transform it to a fault)
                var exceptionToThrow = TransformException(input, x);

                // throw the new (fault) exception
                if (exceptionToThrow != null)
                    throw exceptionToThrow;

                // if swallowed 
                return default(TResult);
            }
        }

        /// <summary>
        /// Process the output from the call so far and optionally modify the output.
        /// Here we cannot throw exceptions - we need to wrap the in a <see cref="IMethodInvocation"/>.
        /// For async calls the exceptions will be discovered in the continue-with which is awaited by the caller.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The method return.</param>
        /// <param name="callData">The per-call data.</param>
        /// <returns>IMethodReturn.</returns>
        protected override IMethodReturn PostInvoke(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            bool callData)
        {
            // if success - return the result
            if (methodReturn.Exception == null)
                return methodReturn;

            // if exception - process it (e.g.transform it to a fault)
            var exceptionToThrow = TransformException(input, methodReturn.Exception);

            // if new (fault) exception to be thrown - create a new method return with the new exception.
            if (exceptionToThrow != null)
                return input.CreateExceptionMethodReturn(exceptionToThrow);

            // if swallowed return default(TResult)
            return input.CreateMethodReturn(((MethodInfo)input.MethodBase).ReturnType.Default());
        }

        static ReaderWriterLockSlim _sync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        static IDictionary<MethodBase, ICollection<Type>> _faultContracts = new Dictionary<MethodBase, ICollection<Type>>();

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "vm.Aspects.Wcf.FaultContracts.Fault.set_Message(System.String)", Justification = "For programmers' eyes only.")]
        FaultException TransformException(
    IMethodInvocation input,
    Exception exception)
        {
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));

            Exception outException = null;

            try
            {
                if (!Facility.ExceptionManager.HandleException(exception, ExceptionHandlingPolicyName, out outException))
                    return null;
            }
            catch (Exception x)
            {
                Facility.LogWriter.LogError(
                    $@"
Facility.ExceptionManager.HandleException throws:
{x.DumpString(1)}
when processing
{exception.DumpString(1)}");

                if (ExceptionHandlingPolicyName == ExceptionPolicyProvider.LogAndSwallowPolicyName)
                    return null;

                if (outException == null)
                    outException = exception;
            }

            var faultException = outException as FaultException;

            // try to get the fault, if any
            var fault = (faultException != null  &&
                         faultException.GetType().IsGenericType)
                            ? faultException
                                .GetType()
                                .GetProperty(nameof(FaultException<Fault>.Detail))
                                ?.GetValue(faultException) as Fault
                            : null;

            // can we return the faultException as we got it
            if (fault != null  &&  IsFaultSupported(input, fault.GetType()))
                return faultException;

            // if the base fault is supported one with all fields copied either to properties or to the Data collection.
            // Send it with status code 500 to indicate that the fault must be added to the protocol
            if (IsFaultSupported(input, typeof(Fault)))
            {
                fault = Fault.FaultFactory<Exception>(exception);
                return new WebFaultException<Fault>(fault, HttpStatusCode.InternalServerError);
            }

            // otherwise construct base WebFaultException with some debug data in the Data property that will be dumped in the logs
            var exceptionToReturn = new WebFaultException(HttpStatusCode.InternalServerError);

            if (fault != null)
                return exceptionToReturn
                            .PopulateData(
                                    new SortedDictionary<string, string>
                                    {
                                        ["HandlingInstanceId"] = fault.HandlingInstanceId.ToString(),
                                        ["Fault"]              = fault.DumpString(),
                                    });

            return exceptionToReturn
                        .PopulateData(
                                new SortedDictionary<string, string>
                                {
                                    ["Exception"]          = exception.DumpString(),
                                });
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        static bool IsFaultSupported(
            IMethodInvocation input,
            Type faultType)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));

            if (faultType == null)
                return false;

            ICollection<Type> contracts;

            using (_sync.UpgradableReaderLock())
            {
                var method = input.MethodBase;

                if (_faultContracts.TryGetValue(method, out contracts))
                    return contracts.Contains(faultType);

                if (method.GetCustomAttribute<OperationContractAttribute>() == null)
                    method = input
                                .Target
                                .GetType()
                                .GetInterfaces()
                                .Where(i => i.GetCustomAttribute<ServiceContractAttribute>() != null)
                                .SelectMany(i => i.GetMethods())
                                .FirstOrDefault(m => m.GetCustomAttribute<OperationContractAttribute>() != null  &&
                                                     m.Name == input.MethodBase.Name)
                                ;

                if (method == null)
                {
                    Facility.LogWriter.TraceError($"Could not find operation contract {input.MethodBase.Name}.");
                    return false;
                }

                contracts = new HashSet<Type>(method.GetCustomAttributes<FaultContractAttribute>().Select(a => a.DetailType));

                using (_sync.WriterLock())
                    _faultContracts[input.MethodBase] = contracts;
            }

            if (contracts.Count() == 0)
            {
                Facility.LogWriter.TraceError($"The operation contract {input.MethodBase.Name} does not define any fault contracts.");
                return false;
            }

            if (!contracts.Contains(faultType))
            {
                Facility.LogWriter.TraceError($"The operation contract {input.MethodBase.Name} does not define the fault contract {faultType.Name}.");
                return false;
            }

            return true;
        }
    }
}
