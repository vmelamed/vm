using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Interface IWcfContextUtilities abstracts some of the behaviors and attributes derived from the <see cref="OperationContext"/> and the <see cref="WebOperationContext"/>
    /// </summary>
    public interface IWcfContextUtilities
    {
        /// <summary>
        /// Gets a value indicating whether the current code runs in an operation context.
        /// </summary>
        bool HasOperationContext { get; }

        /// <summary>
        /// Gets a value indicating whether the current code runs in a web operation context.
        /// </summary>
        bool HasWebOperationContext { get; }

        /// <summary>
        /// Gets the operation action.
        /// </summary>
        string OperationAction { get; }

        /// <summary>
        /// Gets the method corresponding to the operation action.
        /// </summary>
        MethodInfo OperationMethod { get; }

        /// <summary>
        /// Gets the attributes on the method implementing the current operation declared on both the interface and the service class.
        /// </summary>
        Attribute[] OperationMethodAllAttributes { get; }
        /// <summary>
        /// Gets the <see cref="WebHttpBehavior"/>.
        /// </summary>
        WebHttpBehavior WebHttpBehavior { get; }

        /// <summary>
        /// Gets the fault action either from the fault's type or if not specified, from the action in the current <see cref="OperationContext"/>.
        /// </summary>
        /// <param name="faultContractType">Type of the fault contract.</param>
        /// <returns>The fault action.</returns>
        string GetFaultedAction(Type faultContractType);
    }
}
