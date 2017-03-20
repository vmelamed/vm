using vm.Aspects.Policies;

namespace vm.Aspects.Wcf.ServicePolicies
{
    /// <summary>
    /// Class ServiceCallData extends <see cref="T:CallData"/> with caller's address and the content of the custom data context (if present in the operation context).
    /// </summary>
    public class ServiceCallTraceData : CallTraceData
    {
        /// <summary>
        /// Gets or sets the caller address.
        /// </summary>
        public string CallerAddress { get; set; }
        /// <summary>
        /// Gets or sets the custom context.
        /// </summary>
        public object CustomContext { get; set; }
    }
}
