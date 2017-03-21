using Microsoft.Practices.Unity.InterceptionExtension;
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
            bool callData)
        {
            try
            {
                return await base.ContinueWith<TResult>(input, methodReturn, callData);
            }
            catch (Exception x)
            {
                throw TransformException(input, x);
            }
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
            bool callData)
        {
            if (methodReturn.Exception == null)
                return methodReturn;

            return input.CreateExceptionMethodReturn(
                            TransformException(input, methodReturn.Exception));
        }

        static ReaderWriterLockSlim _sync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        static IDictionary<MethodBase, IEnumerable<Type>> _faultContracts = new Dictionary<MethodBase, IEnumerable<Type>>();

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "vm.Aspects.Wcf.FaultContracts.Fault.set_Message(System.String)", Justification = "For programmers' eyes only.")]
        static FaultException TransformException(
            IMethodInvocation input,
            Exception exception)
        {
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));
            Contract.Ensures(Contract.Result<FaultException>() != null);

            var factory = Fault.TryGetExceptionToFaultFactory(exception.GetType());

            if (factory == null)
                return new WebFaultException<Fault>(
                                new Fault()
                                {
                                    Message = $"The service threw {exception.GetType().Name} which could not be converted to one of the supported fault contracts of the called method.\n{exception.DumpString()}"
                                },
                                HttpStatusCode.InternalServerError);

            FaultException newException;
            var fault = factory(exception);

            fault.HandlingInstanceId = Facility.GuidGenerator.NewGuid();

            if (IsFaultSupported(input, fault.GetType()))
                newException = (FaultException)typeof(WebFaultException<>)
                                 .MakeGenericType(fault.GetType())
                                 .GetConstructor(new Type[] { fault.GetType(), typeof(HttpStatusCode) })
                                 .Invoke(new object[] { fault, fault.HttpStatusCode });
            else
                newException = new WebFaultException(HttpStatusCode.InternalServerError)
                                    .PopulateData(
                                        new SortedDictionary<string, string>
                                        {
                                            ["Fault"] = fault.DumpString()
                                        });

            Facility.LogWriter.LogError($"ServiceExceptionHandlingCallHandler caught:");
            Facility.LogWriter.ExceptionError(exception, new Dictionary<string, object> { ["HandlingInstanceId:"] = fault.HandlingInstanceId });
            Facility.LogWriter.LogError($"ServiceExceptionHandlingCallHandler throws:");
            Facility.LogWriter.ExceptionError(exception, new Dictionary<string, object> { ["HandlingInstanceId:"] = fault.HandlingInstanceId });

            return newException;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        static bool IsFaultSupported(
            IMethodInvocation input,
            Type faultType)
        {
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));

            IEnumerable<Type> contracts;

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
