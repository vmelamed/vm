using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text.RegularExpressions;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Wcf.Behaviors
{
#pragma warning disable 3015
    /// <summary>
    /// Enable CORS for endpoints with <see cref="WebHttpBinding"/>.
    /// </summary>
    /// <seealso cref="Attribute" />
    /// <seealso cref="IOperationBehavior" />
    /// <remarks>
    /// Based on: https://blogs.msdn.microsoft.com/carlosfigueira/2012/05/14/implementing-cors-support-in-wcf/
    /// </remarks>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class EnableCorsAttribute : Attribute, IContractBehavior
    {
        readonly string[] _allowedOriginsResolveNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableCorsAttribute"/> class.
        /// </summary>
        public EnableCorsAttribute()
        {
            _allowedOriginsResolveNames = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableCorsAttribute"/> class.
        /// </summary>
        /// <param name="allowedOriginsResolveNames">The allowed origins resolve names.</param>
        public EnableCorsAttribute(
            params string[] allowedOriginsResolveNames)
        {
            Contract.Requires<ArgumentNullException>(allowedOriginsResolveNames != null, nameof(allowedOriginsResolveNames));

            _allowedOriginsResolveNames = allowedOriginsResolveNames;
        }

        /// <summary>
        /// Gets the resolved allowed origins.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] AllowedOriginsResolveNames => _allowedOriginsResolveNames;

        /// <summary/>
        #region IContractBehavior
        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }

        /// <summary/>
        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
        }

        /// <summary/>
        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        /// <summary/>
        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }
        #endregion

        internal void AddPreflightOperationSelectors(
            ServiceEndpoint endpoint)
        {
            Contract.Requires<ArgumentNullException>(endpoint != null, nameof(endpoint));

            var uriTemplates = new SortedDictionary<string, PreflightOperationBehavior>();
            string[] allowedOrigins = null;

            if (_allowedOriginsResolveNames?.Any() == true)
                allowedOrigins = _allowedOriginsResolveNames.Select(n => ServiceLocator.Current.GetInstance<string>(n)).ToArray();

            foreach (var operation in endpoint.Contract.Operations.ToList())
                AddPreflightOperationSelector(operation, allowedOrigins, uriTemplates);

            endpoint.Behaviors.Add(new EnableCorsEndpointBehavior(allowedOrigins));
        }

        static void AddPreflightOperationSelector(
            OperationDescription operation,
            string[] allowedOrigins = null,
            IDictionary<string, PreflightOperationBehavior> uriTemplates = null)
        {
            Contract.Requires<ArgumentNullException>(operation != null, nameof(operation));

            if (operation.IsOneWay)
                // no support for 1-way messages
                return;

            string originalUriTemplate;
            string originalMethod;

            var webInvoke = operation.Behaviors.Find<WebInvokeAttribute>();

            if (webInvoke != null)
            {
                if (webInvoke.Method == "OPTIONS")
                    return;

                originalMethod = webInvoke.Method ?? "POST";
                originalUriTemplate = webInvoke.UriTemplate;
            }
            else
            {
                var webGet = operation.Behaviors.Find<WebGetAttribute>();

                if (webGet == null)
                    return;

                originalMethod = "GET";
                originalUriTemplate = webGet.UriTemplate;
            }

            originalUriTemplate = originalUriTemplate != null ? NormalizeTemplate(originalUriTemplate) : operation.Name;

            if (originalUriTemplate.IsNullOrWhiteSpace())
                originalUriTemplate = "/";

            PreflightOperationBehavior preflightOperationBehavior = null;

            if (uriTemplates?.TryGetValue(originalUriTemplate, out preflightOperationBehavior) == true)
            {
                // there is already an OPTIONS operation for this URI, we can reuse it, just add the method
                preflightOperationBehavior?.AddAllowedMethod(originalMethod);
                return;
            }

            var contract = operation.DeclaringContract;
            var inputMessage = new MessageDescription(operation.Messages[0].Action + Constants.PreflightSuffix, MessageDirection.Input);

            inputMessage
                .Body
                .Parts
                .Add(
                    new MessagePartDescription("input", contract.Namespace)
                    {
                        Index = 0,
                        Type = typeof(Message),
                    });

            var outputMessage = new MessageDescription(operation.Messages[1].Action + Constants.PreflightSuffix, MessageDirection.Output);

            outputMessage
                .Body
                .ReturnValue = new MessagePartDescription(operation.Name + Constants.PreflightSuffix + "Return", contract.Namespace)
                {
                    Type = typeof(Message),
                };

            preflightOperationBehavior = new PreflightOperationBehavior(allowedOrigins);

            preflightOperationBehavior.AddAllowedMethod(originalMethod);

            var preflightOperation = new OperationDescription(operation.Name + Constants.PreflightSuffix, contract);

            preflightOperation.Messages.Add(inputMessage);
            preflightOperation.Messages.Add(outputMessage);

            preflightOperation.Behaviors.Add(new WebInvokeAttribute
            {
                Method      = "OPTIONS",
                UriTemplate = originalUriTemplate,
            });
            preflightOperation.Behaviors.Add(new DataContractSerializerOperationBehavior(preflightOperation));
            preflightOperation.Behaviors.Add(preflightOperationBehavior);

            contract.Operations.Add(preflightOperation);

            uriTemplates?.Add(originalUriTemplate, preflightOperationBehavior);
        }

        static Regex _rexNamedParams = new Regex(@"{[^}]+}(?:/{[^}]+})*", RegexOptions.Compiled);

        static string NormalizeTemplate(string uriTemplate)
        {
            Contract.Requires<ArgumentNullException>(uriTemplate != null, nameof(uriTemplate));

            int queryIndex = uriTemplate.IndexOf('?');

            if (queryIndex >= 0)
                // no query string used for this
                uriTemplate = uriTemplate.Substring(0, queryIndex);

            return _rexNamedParams.Replace(uriTemplate, "*");
        }
    }
#pragma warning restore 3015
}
