using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace vm.Aspects.Wcf.EnableCorsService
{
    /// <remarks>
    /// Based on: https://blogs.msdn.microsoft.com/carlosfigueira/2012/05/14/implementing-cors-support-in-wcf/
    /// </remarks>
    class PreflightOperationBehavior : IOperationBehavior
    {
        OperationDescription _preflightOperation;
        List<string> _allowedMethods;

        public PreflightOperationBehavior(
            OperationDescription preflightOperation)
        {
            _preflightOperation = preflightOperation;
            _allowedMethods = new List<string>();
        }

        public void AddAllowedMethod(string httpMethod)
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

        public void Validate(
            OperationDescription operationDescription)
        {
        }
    }
}
