using System;

namespace vm.Aspects.Wcf.Behaviors.AuthorizationManager
{
    /// <summary>
    /// Class AllowOpenIdUnauthenticatedAttribute. This class cannot be inherited.
    /// Specifies that the method of the interface or class or the single method to which the attribute is applied can be called unauthenticated.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Method,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class AllowOpenIdUnauthenticatedAttribute : Attribute
    {
    }
}
