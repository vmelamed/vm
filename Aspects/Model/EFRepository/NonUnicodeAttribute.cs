using System;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;

namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Class UnicodeAttribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class NonUnicodeAttribute : Attribute
    {
    }

    /// <summary>
    /// Class UnicodeAttributeConvention enables the attribute <see cref="NonUnicodeAttribute"/>
    /// </summary>
    /// <seealso cref="System.Data.Entity.ModelConfiguration.Conventions.Convention" />
    public sealed class NonUnicodeAttributeConvention : Convention
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NonUnicodeAttributeConvention"/> class.
        /// </summary>
        public NonUnicodeAttributeConvention()
        {
            Properties<string>()
                .Where(p => p.GetCustomAttributes<NonUnicodeAttribute>().Any())
                .Configure(p => p.IsUnicode(false))
                ;
        }
    }
}
