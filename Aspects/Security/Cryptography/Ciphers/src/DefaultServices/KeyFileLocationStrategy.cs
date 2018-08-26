using System.Configuration;
using System.Reflection;

namespace vm.Aspects.Security.Cryptography.Ciphers.DefaultServices
{
    /// <summary>
    /// DefaultKeyLocationStrategy implements the following hierarchical rules for locating the file with the encrypted symmetric key:
    ///     <list type="number">
    ///         <item>if it is specified by the caller in the constructor's input parameter; otherwise</item>
    ///         <item>if it is specified in the <c>appSettings</c> section of the application's configuration file with key <c>symmetricKeyLocation</c>; otherwise</item>
    ///         <item>by the path and name of the entry assembly with added suffix &quot;.KEY&quot;, e.g. &quot;MyApp.exe.KEY&quot;.</item>.
    ///     </list>
    /// </summary>
    public sealed class KeyFileLocationStrategy : IKeyLocationStrategy
    {
        /// <summary>
        /// The default key location suffix - .key
        /// </summary>
        public const string DefaultKeyLocationSuffix = ".key";
        /// <summary>
        /// The application setting entry that might contain the name of the key container.
        /// </summary>
        public const string AppSettingsKeyContainerNameEntry = "symmetricKeyLocation";

        #region IKeyLocationStrategy Members

        /// <summary>
        /// Executes the key location strategy and returns the resolved store specific key location (here path and filename).
        /// </summary>
        /// <param name="keyLocation">
        /// The key location. Can be <see langword="null" />, empty or string consisting of whitespace characters only.
        /// </param>
        /// <returns>The store specific key location (here path and filename).</returns>
        /// <remarks>
        /// The method implements the strategy for determining the location of the file containing the encryption key.
        /// </remarks>
        public string GetKeyLocation(string keyLocation)
        {
            if (!keyLocation.IsNullOrWhiteSpace())
                return keyLocation;

            var location = ConfigurationManager.AppSettings[AppSettingsKeyContainerNameEntry];

            if (!location.IsNullOrWhiteSpace())
                return location;

            var fileName = Assembly.GetEntryAssembly().GetName().Name;

#if NETFRAMEWORK
            if (fileName.IsNullOrWhiteSpace())
            {
                fileName = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                var index = fileName?.LastIndexOf(".config", StringComparison.OrdinalIgnoreCase);

                if (index > -1)
                    fileName = fileName.Substring(0, index.Value);
            }

#endif

            if (fileName.IsNullOrWhiteSpace())
                fileName = "key";

            return fileName + DefaultKeyLocationSuffix;
        }

        #endregion
    }
}
