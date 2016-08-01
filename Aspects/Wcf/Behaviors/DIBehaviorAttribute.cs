using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Class DIBehaviorAttribute. This class cannot be inherited. Should be applied to WCF service classes definitions that should not be created
    /// by WCF by using the <see cref="T:System.Activator"/> class but rather resolved from a DI container using the DI endpoint behavior.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    [DebuggerDisplay("{GetType().Name, nq} {TargetContract!=null ? TargetContract.Name : string.Empty, nq}, resolve name: {ResolveName}")]
    public sealed class DIBehaviorAttribute : Attribute, IServiceBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DIBehaviorAttribute"/> class.
        /// </summary>
        public DIBehaviorAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DIBehaviorAttribute"/> class.
        /// </summary>
        /// <param name="resolveName">
        /// The resolution name to use when resolving the instance.
        /// Plays the same role as applying a separate <see cref="ResolveNameAttribute"/>.
        /// </param>
        public DIBehaviorAttribute(string resolveName)
            : this(resolveName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DIBehaviorAttribute"/> class.
        /// </summary>
        /// <param name="targetContract">
        /// The target contract for which the behavior will be applied. If <see langword="null"/> (the default) the behavior will be applied to all interfaces of the 
        /// service.
        /// </param>
        public DIBehaviorAttribute(Type targetContract)
            : this(null, targetContract)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DIBehaviorAttribute"/> class.
        /// </summary>
        /// <param name="resolveName">The resolution name to use when resolving the instance.</param>
        /// <param name="targetContract">
        /// The target contract for which the behavior will be applied. If <see langword="null"/> (the default) the behavior will be applied to all interfaces of the 
        /// service.
        /// </param>
        public DIBehaviorAttribute(
            string resolveName,
            Type targetContract)
        {
            if (targetContract != null && !targetContract.IsInterface)
                throw new ArgumentException("The target contract must be an interface.", nameof(targetContract));

            ResolveName    = resolveName;
            TargetContract = targetContract;
        }

        /// <summary>
        /// Gets the DI container resolve name.
        /// </summary>
        public string ResolveName { get; }

        /// <summary>
        /// Gets the type of the contract to which the contract behavior is applicable.
        /// </summary>
        /// <value></value>
        /// <returns>The contract to which the contract behavior is applicable.</returns>
        public Type TargetContract { get; }

        #region IServiceBehavior
        void IServiceBehavior.AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
        {
            if (serviceDescription==null)
                throw new ArgumentNullException(nameof(serviceDescription));
            if (serviceHostBase==null)
                throw new ArgumentNullException(nameof(serviceHostBase));

            var interfaces = serviceDescription.ServiceType.GetInterfaces().Where(i => i.GetCustomAttribute<ServiceContractAttribute>() != null).ToList();

            if (serviceDescription.ServiceType.GetCustomAttribute<ServiceContractAttribute>() != null)
                interfaces.Add(serviceDescription.ServiceType);

            foreach (var cd in serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>().Where(d => d != null))
                foreach (var ep in cd.Endpoints)
                {
                    var contract = TargetContract;

                    if (contract == null)
                        contract = interfaces.First(
                                    i =>
                                    {
                                        var a = i.GetCustomAttribute<ServiceContractAttribute>();

                                        return (a.Name      == ep.ContractName       ||  a.Name      == null && ep.ContractName      == i.Name)  &&
                                               (a.Namespace == ep.ContractNamespace  ||  a.Namespace == null && ep.ContractNamespace == "http://tempuri.org/");
                                    });

                    if (contract != null)
                        ep.DispatchRuntime.InstanceProvider = new DIInstanceProvider(contract, ResolveName);
                }
        }

        void IServiceBehavior.Validate(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
        {
            // TODO: verify the behavior - see above.
        }
        #endregion
    }
}
