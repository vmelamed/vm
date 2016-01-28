//===============================================================================
// The code below is modified Enterprise Library code.
//===============================================================================
// Microsoft patterns & practices Enterprise Library
// Exception Handling Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.WCF;

namespace vm.Aspects.Wcf.ExceptionShieldingBehavior
{
    /// <summary>
    /// Indicates that an implementation service class will use exception shielding. 
    /// </summary>
    /// <remarks>
    /// Add this attribute to your service implementation class or your service contract interface 
    /// and configure your host configuration file to use the Enterprise Library Exception Handling 
    /// Application Block adding the <see cref="FaultContractExceptionHandler"/> class to the 
    /// exceptionHandlers collection and set your FaultContract type that maps to a particular exception.
    /// </remarks>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Interface,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class ExceptionShieldingAttribute : Attribute, IServiceBehavior, IContractBehavior, IErrorHandler
    {
        #region ExceptionShieldingAttribute Members 

        IServiceBehavior _serviceBehavior;
        IContractBehavior _contractBehavior;
        IErrorHandler _errorHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionShieldingAttribute"/> class.
        /// </summary>
        public ExceptionShieldingAttribute()
            : this(ExceptionShielding.DefaultExceptionPolicy)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionShieldingAttribute"/> class.
        /// </summary>
        /// <param name="exceptionPolicyName">Name of the exception policy.</param>
        public ExceptionShieldingAttribute(
            string exceptionPolicyName)
        {
            Contract.Requires<ArgumentNullException>(exceptionPolicyName!=null, nameof(exceptionPolicyName));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(exceptionPolicyName), "The argument "+nameof(exceptionPolicyName)+" cannot be empty or consist of whitespace characters only.");

            ExceptionPolicyName = exceptionPolicyName;

            //The ServiceHost applies behaviors in the following order:
            // Contract
            // Operation
            // Endpoint
            // Service

            var behavior = new ExceptionShieldingBehavior(exceptionPolicyName);

            _contractBehavior = behavior;
            _serviceBehavior  = behavior;
            _errorHandler     = new ExceptionShieldingErrorHandler(exceptionPolicyName);
        }

        /// <summary>
        /// Gets or sets the name of the exception policy.
        /// </summary>
        /// <value>The name of the exception policy.</value>
        public string ExceptionPolicyName { get; }

        #endregion

        #region IServiceBehavior Members

        /// <summary>
        /// Validates the specified description.
        /// </summary>
        /// <param name="serviceDescription">The description.</param>
        /// <param name="serviceHostBase">The service host base.</param>
        public void Validate(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
        {
            _serviceBehavior.Validate(serviceDescription, serviceHostBase);
        }

        /// <summary>
        /// Adds the binding parameters.
        /// </summary>
        /// <param name="serviceDescription">The description.</param>
        /// <param name="serviceHostBase">The service host base.</param>
        /// <param name="endpoints">The endpoints.</param>
        /// <param name="bindingParameters">The parameters.</param>
        public void AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
            _serviceBehavior.AddBindingParameters(serviceDescription, serviceHostBase, endpoints, bindingParameters);
        }

        /// <summary>
        /// Applies the dispatch behavior.
        /// </summary>
        /// <param name="serviceDescription">The description.</param>
        /// <param name="serviceHostBase">The service host base.</param>
        public void ApplyDispatchBehavior(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
        {
            _serviceBehavior.ApplyDispatchBehavior(serviceDescription, serviceHostBase);
        }

        #endregion

        #region IContractBehavior Members

        /// <summary>
        /// Configures any binding elements to support the contract behavior.
        /// </summary>
        /// <param name="contractDescription">The contract description to modify.</param>
        /// <param name="endpoint">The endpoint to modify.</param>
        /// <param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
            _contractBehavior.AddBindingParameters(contractDescription, endpoint, bindingParameters);
        }

        /// <summary>
        /// Implements a modification or extension of the client across a contract.
        /// </summary>
        /// <param name="contractDescription">The contract description for which the extension is intended.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="clientRuntime">The client runtime.</param>
        public void ApplyClientBehavior(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint,
            ClientRuntime clientRuntime)
        {
            _contractBehavior.ApplyClientBehavior(contractDescription, endpoint, clientRuntime);
        }

        /// <summary>
        /// Implements a modification or extension of the client across a contract.
        /// </summary>
        /// <param name="contractDescription">The contract description to be modified.</param>
        /// <param name="endpoint">The endpoint that exposes the contract.</param>
        /// <param name="dispatchRuntime">The dispatch runtime that controls service execution.</param>
        public void ApplyDispatchBehavior(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint,
            DispatchRuntime dispatchRuntime)
        {
            _contractBehavior.ApplyDispatchBehavior(contractDescription, endpoint, dispatchRuntime);
        }

        /// <summary>
        /// Implement to confirm that the contract and endpoint can support the contract behavior.
        /// </summary>
        /// <param name="contractDescription">The contract to validate.</param>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
            _contractBehavior.Validate(contractDescription, endpoint);
        }

        #endregion

        #region IErrorHandler Members

        /// <summary>
        /// Enables error-related processing and returns a value that indicates whether subsequent HandleError implementations are called.
        /// </summary>
        /// <param name="error">The exception thrown during processing.</param>
        /// <returns>
        /// true if subsequent <see cref="T:System.ServiceModel.Dispatcher.IErrorHandler"></see> implementations must not be called; otherwise, false. The default is false.
        /// </returns>
        public bool HandleError(
            Exception error)
        {
            return _errorHandler.HandleError(error);
        }

        /// <summary>
        /// Enables the creation of a custom <see cref="T:System.ServiceModel.FaultException`1"></see> that is returned from an exception in the course of a service method.
        /// </summary>
        /// <param name="error">The <see cref="T:System.Exception"></see> object thrown in the course of the service operation.</param>
        /// <param name="version">The SOAP version of the message.</param>
        /// <param name="fault">The <see cref="T:System.ServiceModel.Channels.Message"></see> object that is returned to the client, or service, in the duplex case.</param>
        public void ProvideFault(
            Exception error,
            MessageVersion version,
            ref Message fault)
        {
            _errorHandler.ProvideFault(error, version, ref fault);
        }

        #endregion

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrWhiteSpace(ExceptionPolicyName));
            Contract.Invariant(_errorHandler != null);
            Contract.Invariant(_contractBehavior != null);
            Contract.Invariant(_serviceBehavior != null);
        }

    }
}
