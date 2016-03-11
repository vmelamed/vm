using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace vm.Aspects.Wcf.Behaviors
{
    class PreflightOperationBehavior : IOperationBehavior
    {
        readonly ISet<string> _allowedMethods = new SortedSet<string>();

        public void AddAllowedMethod(
            string httpMethod)
        {
            _allowedMethods.Add(httpMethod);
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

        public void ApplyDispatchBehavior(
            OperationDescription operationDescription,
            DispatchOperation dispatchOperation)
        {
            dispatchOperation.Invoker = new PreflightOperationInvoker(operationDescription.Messages[1].Action, _allowedMethods);
        }

        public void Validate(OperationDescription operationDescription)
        {
        }
    }
}
