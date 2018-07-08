using System;
using System.Diagnostics.CodeAnalysis;
using vm.Aspects.Diagnostics.Implementation;

namespace vm.Aspects.Diagnostics
{
    /// <summary>
    /// Contains a reference to the current <see cref="IServiceProvider"/> for internal use only.
    /// If it is not set externally by the client it will use internally the <see cref="InternalServiceProvider"/>.
    /// </summary>
    public static class ServiceProvider
    {
        /// <summary>
        /// Gets or sets the current service provider.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
        public static IServiceProvider Current { internal get; set; } = new InternalServiceProvider();
    }
}
