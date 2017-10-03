using System;
using System.Configuration;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Class AppConfigProvider implements <see cref="IConfigurationProvider"/> based on the application's configuration file (derived from app.config)
    /// </summary>
    /// <seealso cref="IConfigurationProvider" />
    public class AppConfigProvider : IConfigurationProvider
    {
        /// <summary>
        /// Gets the (app)setting with the specified name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>System.String.</returns>
        public string this[string settingName]
        {
            get
            {
                if (settingName.IsNullOrWhiteSpace())
                    throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(settingName));

                return ConfigurationManager.AppSettings[settingName];
            }
        }
    }
}