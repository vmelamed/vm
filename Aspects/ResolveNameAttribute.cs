using System;

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
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(name));

            Name = name;
        }

        /// <summary>
        /// Gets the resolve name.
        /// </summary>
        public string Name { get; }
    }
}
