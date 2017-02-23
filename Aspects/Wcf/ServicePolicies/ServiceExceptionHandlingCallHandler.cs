using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using Microsoft.Practices.Unity.InterceptionExtension;
using vm.Aspects.Facilities;
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
    public sealed class ServiceExceptionHandlingCallHandler : ICallHandler
    {
        int ICallHandler.Order { get; set; }

        IMethodReturn ICallHandler.Invoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext)
        {
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (getNext == null)
                throw new ArgumentNullException(nameof(getNext));

            var methodReturn = getNext().Invoke(input, getNext);

            if (methodReturn.Exception == null)
                return methodReturn;

            var exception = methodReturn.Exception;
            var faultType = Fault.GetFaultTypeForException(exception.GetType());
            Fault fault = null;

            if (!IsFaultSupported(input, faultType))

                return input.CreateExceptionMethodReturn(
                                new WebFaultException<Fault>(
                                        new Fault() { Message = $"The service threw {exception.GetType().Name} which could not be converted to one of the supported fault contracts of the called method.\n{methodReturn.Exception.DumpString()}" },
                                        HttpStatusCode.InternalServerError));

            fault = Fault.FaultFactory(exception);

            return input.CreateExceptionMethodReturn(
                            (Exception)typeof(WebFaultException<>)
                                            .MakeGenericType(faultType)
                                            .GetConstructor(new Type[] { faultType, typeof(HttpStatusCode) })
                                            .Invoke(new object[] { fault, fault.HttpStatusCode }));
        }

        static ReaderWriterLockSlim _sync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        static IDictionary<MethodBase, IEnumerable<Type>> _faultContracts = new Dictionary<MethodBase, IEnumerable<Type>>();

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
