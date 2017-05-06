using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Interface IConfigurationProvider represents a configuration source (from file, DB, etc.) which provides a set of key-value pairs - the settings.
    /// </summary>
    [ContractClass(typeof(IConfigurationProviderContract))]
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Gets the value of a setting with the specified setting name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>The value of the setting. May be null</returns>
        string this[string settingName] { get; }
    }

    #region IConfigurationProvider contract binding
    [ContractClassFor(typeof(IConfigurationProvider))]
    abstract class IConfigurationProviderContract : IConfigurationProvider
    {
        public string this[string settingName]
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                Contract.Ensures(Contract.Result<string>().Any(c => !char.IsWhiteSpace(c)));

                throw new NotImplementedException();
            }
        }
    }
    #endregion
}
