using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using vm.Aspects.Wcf.Behaviors;

namespace vm.Aspects.Wcf.EnableCorsService
{
    /// <remarks>
    /// Based on: https://blogs.msdn.microsoft.com/carlosfigueira/2012/05/14/implementing-cors-support-in-wcf/
    /// </remarks>
    public class EnableCorsServiceHost : ServiceHost
    {
        Type _contractType;

        public EnableCorsServiceHost(
            Type serviceType,
            Uri[] baseAddresses) : base(serviceType, baseAddresses)
        {
            _contractType = GetContractType(serviceType);
        }

        protected override void OnOpening()
        {
            var endpoint = AddServiceEndpoint(_contractType, new WebHttpBinding(), "");

            var corsEnabledOperations = endpoint
                                            .Contract
                                            .Operations
                                            .Where(o => o.Behaviors.Find<EnableCorsAttribute>() != null)
                                            .ToList();

            AddPreflightOperations(endpoint, corsEnabledOperations);

            endpoint.Behaviors.Add(new WebHttpBehavior());
            endpoint.Behaviors.Add(new EnableCorsEndpointBehavior());

            base.OnOpening();
        }

        static Type GetContractType(Type serviceType)
        {
            if (HasServiceContract(serviceType))
                return serviceType;

            var possibleContractTypes = serviceType
                                            .GetInterfaces()
                                            .Where(i => HasServiceContract(i))
                                            .ToArray();

            switch (possibleContractTypes.Length)
            {
            case 1:
                return possibleContractTypes[0];

            case 0:
                throw new InvalidOperationException(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Service type {0} does not implement any interface decorated with the {1}.",
                                    serviceType.FullName,
                                    nameof(ServiceContractAttribute)));

            default:
                throw new InvalidOperationException(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Service type {0} implements multiple interfaces decorated with the {1}, which is not supported by this factory.",
                                    serviceType.FullName,
                                    nameof(ServiceContractAttribute)));
            }
        }

        static bool HasServiceContract(Type type) => Attribute.IsDefined(type, typeof(ServiceContractAttribute), false);

        void AddPreflightOperations(
            ServiceEndpoint endpoint,
            List<OperationDescription> corsOperations)
        {
            var uriTemplates = new Dictionary<string, PreflightOperationBehavior>(StringComparer.OrdinalIgnoreCase);

            foreach (var operation in corsOperations)
            {
                if (operation.Behaviors.Find<WebGetAttribute>() != null  ||
                    operation.IsOneWay)                                         // no need to add preflight operation for GET requests, and no support for 1-way messages                    
                    continue;

                string originalUriTemplate;
                var originalWebInvokeAttribute = operation.Behaviors.Find<WebInvokeAttribute>();

                if (originalWebInvokeAttribute != null && originalWebInvokeAttribute.UriTemplate != null)
                    originalUriTemplate = NormalizeTemplate(originalWebInvokeAttribute.UriTemplate);
                else
                    originalUriTemplate = operation.Name;

                var originalMethod = (originalWebInvokeAttribute != null && originalWebInvokeAttribute.Method != null)
                                            ? originalWebInvokeAttribute.Method
                                            : "POST";

                if (uriTemplates.ContainsKey(originalUriTemplate))
                    // there is already an OPTIONS operation for this URI, we can reuse it
                    uriTemplates[originalUriTemplate].AddAllowedMethod(originalMethod);
                else
                {
                    ContractDescription contract            = operation.DeclaringContract;
                    OperationDescription preflightOperation = new OperationDescription(operation.Name + Constants.PreflightSuffix, contract);
                    MessageDescription inputMessage         = new MessageDescription(operation.Messages[0].Action + Constants.PreflightSuffix, MessageDirection.Input);

                    inputMessage.Body.Parts.Add(new MessagePartDescription("input", contract.Namespace) { Index = 0, Type = typeof(Message) });
                    preflightOperation.Messages.Add(inputMessage);

                    MessageDescription outputMessage = new MessageDescription(operation.Messages[1].Action + Constants.PreflightSuffix, MessageDirection.Output);

                    outputMessage.Body.ReturnValue = new MessagePartDescription(preflightOperation.Name + "Return", contract.Namespace) { Type = typeof(Message) };
                    preflightOperation.Messages.Add(outputMessage);

                    var webInvokeAttribute = new WebInvokeAttribute();

                    webInvokeAttribute.UriTemplate = originalUriTemplate;
                    webInvokeAttribute.Method      = "OPTIONS";

                    preflightOperation.Behaviors.Add(webInvokeAttribute);
                    preflightOperation.Behaviors.Add(new DataContractSerializerOperationBehavior(preflightOperation));

                    PreflightOperationBehavior preflightOperationBehavior = new PreflightOperationBehavior(preflightOperation);

                    preflightOperationBehavior.AddAllowedMethod(originalMethod);
                    preflightOperation.Behaviors.Add(preflightOperationBehavior);
                    uriTemplates.Add(originalUriTemplate, preflightOperationBehavior);

                    contract.Operations.Add(preflightOperation);
                }
            }
        }

        string NormalizeTemplate(
            string template)
        {
            int queryIndex = template.IndexOf('?');

            if (queryIndex >= 0)
                template = template.Substring(0, queryIndex);

            int paramIndex;

            while ((paramIndex = template.IndexOf('{')) >= 0)
            {
                // Replacing all named parameters with wild-cards
                int endParamIndex = template.IndexOf('}', paramIndex);

                if (endParamIndex >= 0)
                    template = template.Substring(0, paramIndex) + '*' + template.Substring(endParamIndex + 1);
            }

            return template;
        }
    }
}