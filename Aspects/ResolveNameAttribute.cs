using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vm.Aspects
{
    /// <summary>
    /// The attribute is intended to be used for named registrations of types in the DI container at runtime.
    /// </summary>
    [Serializable]
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Delegate |
        AttributeTargets.Interface |
        AttributeTargets.Struct,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class ResolveNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveNameAttribute"/> class.
        /// </summary>
        /// <param name="name">The resolve name.</param>
        /// <exception cref="System.ArgumentException">The name cannot be null, empty or whitespace only string.;name</exception>
        public ResolveNameAttribute(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null, nameof(name));
            Contract.Requires<ArgumentException>(name.Length > 0, "The argument "+nameof(name)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(name.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(name)+" cannot be empty or consist of whitespace characters only.");

            Name = name;
        }

        /// <summary>
        /// Gets the resolve name.
        /// </summary>
        public string Name { get; }
    }
}
