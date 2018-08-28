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
        public const string DefaultKeyLocation = ".key";

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
            => !keyLocation.IsNullOrWhiteSpace() ? keyLocation : DefaultKeyLocation;

        #endregion
    }
}
