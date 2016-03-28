using System;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Marks the associated <see langword="class"/> or <see langword="struct"/> as immutable.
    /// NOTE that the attribute is not inherited.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Struct,
        AllowMultiple = false,
        Inherited = true)]
    public sealed class ImmutableAttribute : Attribute
    {
    }
}
