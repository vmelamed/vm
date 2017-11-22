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
    public sealed class AllowUnauthenticatedAttribute : Attribute
    {
        /// <summary>
        /// The default role to be assigned to the unauthenticated principal.
        /// </summary>
        public const string DefaultName = "Anonymous";
        /// <summary>
        /// The default role to be assigned to the unauthenticated principal.
        /// </summary>
        public const string DefaultRole = "Guest";

        /// <summary>
        /// Gets the name of the unauthenticated principal.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the role of the unauthenticated principal.
        /// </summary>
        public string Role { get; }

        /// <summary>
        /// Gets a value indicating whether unauthenticated calls are allowed.
        /// </summary>
        public bool UnauthenticatedCallsAllowed { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AllowUnauthenticatedAttribute" /> class.
        /// </summary>
        /// <param name="name">The name of the unauthenticated principal.</param>
        /// <param name="role">The role of the unauthenticated principal.</param>
        /// <param name="unauthenticatedCallsAllowed">if set to <c>true</c> unauthenticated calls are allowed.</param>
        /// <exception cref="System.ArgumentException">The argument cannot be null, empty string or consist of whitespace characters only. - name</exception>
        public AllowUnauthenticatedAttribute(
            string name = DefaultName,
            string role = DefaultRole,
            bool unauthenticatedCallsAllowed = true)
        {
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(name));

            Name = name;
            Role = role;
            UnauthenticatedCallsAllowed = unauthenticatedCallsAllowed;
        }
    }
}
