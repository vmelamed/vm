using System;
using System.Web.Configuration;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Class AppConfigProvider implements <see cref="IConfigurationProvider"/> based on the web application's configuration file (web.config)
    /// </summary>
    /// <seealso cref="IConfigurationProvider" />
    public class WebConfigProvider : IConfigurationProvider
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

                return WebConfigurationManager.AppSettings[settingName];
            }
        }
    }
}
