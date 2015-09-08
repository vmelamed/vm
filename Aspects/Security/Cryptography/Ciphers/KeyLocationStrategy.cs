using System;
using System.Configuration;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The class KeyLocationStrategy implements the following hierarchical rules for locating the file with the encrypted symmetric key:
    ///     <list type="number">
    ///         <item>if it is specified by the caller in the constructor's input parameter; otherwise</item>
    ///         <item>if it is specified in the <c>appSettings</c> section of the application's configuration file with key <c>symmetricKeyLocation</c>; otherwise</item>
    ///         <item>by the path and name of the entry assembly with added suffix &quot;.KEY&quot;, e.g. &quot;MyApp.exe.KEY&quot;.</item>.
    ///     </list>
    /// </summary>
    sealed class KeyLocationStrategy : IKeyLocationStrategy
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
        /// Executes the key location strategy and initializes the property <see cref="IKeyManagement.KeyLocation" />.
        /// </summary>
        /// <param name="keyLocation">
        /// The key location. Can be <see langword="null" />, empty or string consisting of whitespace characters only.
        /// </param>
        /// <returns>System.String.</returns>
        /// <remarks>
        /// The method implements the strategy for determining the location of the file containing the encryption key.
        /// </remarks>
        public string GetKeyLocation(string keyLocation)
        {
            if (!string.IsNullOrWhiteSpace(keyLocation))
                return keyLocation;

            var location = ConfigurationManager.AppSettings[AppSettingsKeyContainerNameEntry];

            if (!string.IsNullOrWhiteSpace(location))
                return location;

            var fileName = AppDomain.CurrentDomain.SetupInformation.ApplicationName;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    var index = fileName.LastIndexOf(".exe.config", StringComparison.OrdinalIgnoreCase);

                    if (index == -1)
                        index = fileName.LastIndexOf(".dll.config", StringComparison.OrdinalIgnoreCase);
                    if (index == -1)
                        index = fileName.LastIndexOf(".config", StringComparison.OrdinalIgnoreCase);
                    if (index > 0)
                        fileName = fileName.Substring(0, index);
                }
            }

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "key";

            return fileName + DefaultKeyLocationSuffix;
        }

        #endregion
    }
}
