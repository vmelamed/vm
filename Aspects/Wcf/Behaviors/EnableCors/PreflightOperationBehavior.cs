using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace vm.Aspects.Wcf.Behaviors
{
    class PreflightOperationBehavior : IOperationBehavior
    {
        readonly ISet<string> _allowedMethods = new SortedSet<string>();
        readonly string[] _allowedOrigins;
        readonly int _maxAge;

        public PreflightOperationBehavior(
            string[] allowedOrigins = null,
            int maxAge = 600)
        {
            _allowedOrigins = allowedOrigins;
            _maxAge         = maxAge;
        }

        public void AddAllowedMethod(
            string httpMethod)
        {
            _allowedMethods.Add(httpMethod);
        }

        internal string AllowedMethods => string.Join(", ", _allowedMethods);

        // IOperationBehavior:
        public void ApplyDispatchBehavior(
            OperationDescription operationDescription,
            DispatchOperation dispatchOperation)
        {
            dispatchOperation.Invoker = new PreflightOperationInvoker(operationDescription.Messages[1].Action, _allowedMethods, _allowedOrigins, _maxAge);
        }

        public void AddBindingParameters(
            OperationDescription operationDescription,
            BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(
            OperationDescription operationDescription,
            ClientOperation clientOperation)
        {
        }

        public void Validate(OperationDescription operationDescription)
        {
        }
    }
}
