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
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.WCF;

namespace vm.Aspects.Wcf.ExceptionShieldingBehavior
{
    /// <summary>
    /// The behavior class that set up the <see cref="ExceptionShieldingErrorHandler"/> 
    /// for implementing the exception shielding process.
    /// </summary>
    public class ExceptionShieldingBehavior : IServiceBehavior, IContractBehavior
    {
        #region ExceptionShieldingBehavior Constructors

        private string exceptionPolicyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionShieldingBehavior"/> class.
        /// </summary>
        public ExceptionShieldingBehavior()
            : this(ExceptionShielding.DefaultExceptionPolicy)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionShieldingBehavior"/> class.
        /// </summary>
        /// <param name="exceptionPolicyName">Name of the exception policy.</param>
        public ExceptionShieldingBehavior(
            string exceptionPolicyName)
        {
            this.exceptionPolicyName = exceptionPolicyName;
        }

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
            // Not implemented.
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
            // Not implemented.
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
            foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
                AddErrorHandler(dispatcher);
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
            // Not implemented.
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
            // Not implemented.
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
            if (dispatchRuntime == null)
                throw new ArgumentNullException("dispatchRuntime");

            AddErrorHandler(dispatchRuntime.ChannelDispatcher);
        }

        /// <summary>
        /// Implement to confirm that the contract and endpoint can support the contract behavior.
        /// </summary>
        /// <param name="contractDescription">The contract to validate.</param>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint)
        {
            // Not implemented.
        }

        #endregion

        #region Private Members

        void AddErrorHandler(
            ChannelDispatcher channelDispatcher)
        {
            if (!channelDispatcher.IncludeExceptionDetailInFaults                              &&
                !channelDispatcher.ErrorHandlers.Any(h => h is ExceptionShieldingErrorHandler) &&
                !channelDispatcher.Endpoints.Any(mx => mx.ContractName == nameof(IMetadataExchange)))
                channelDispatcher.ErrorHandlers.Add(new ExceptionShieldingErrorHandler(exceptionPolicyName));
        }

        #endregion
    }
}
