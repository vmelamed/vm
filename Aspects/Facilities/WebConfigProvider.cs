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
        public string this[string settingName] => WebConfigurationManager.AppSettings[settingName];
    }
}
