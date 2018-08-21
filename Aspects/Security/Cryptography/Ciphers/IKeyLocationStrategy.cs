namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The objects implementing this interface provide a strategy for determining the encryption key location name
    /// (e.g. paths and filenames) from a logical name (e.g. key name stored by a configuration provider).
    /// </summary>
    public interface IKeyLocationStrategy
    {
        /// <summary>
        /// Executes the key location strategy and returns the key's location name
        /// as it would be understood by an associated <see cref="IKeyStorage"/> instance.
        /// </summary>
        /// <param name="keyLocation">
        /// The logical key location.
        /// </param>
        /// <returns>
        /// The key's location name, as it would be understood by an associated <see cref="IKeyStorage"/> instance.
        /// </returns>
        /// <remarks>
        /// Possible strategies might be:
        ///     <list type="bullet">
        ///         <item>
        ///         simply the <paramref name="keyLocation"/>, if it is not <see langword="null"/>, empty or string consisting of whitespace characters only; or
        ///         </item><item>
        ///         the value specified in the <c>appSettings</c> section of the application's configuration file with key <c>symmetricKeyLocation</c>, if it exists; or
        ///         </item><item>
        ///         the path and name of the entry assembly with appended suffix &quot;.KEY&quot;, e.g. &quot;MyApp.exe.KEY&quot; or
        ///         </item><item>
        ///         the key access name in an HSM; etc.
        ///         </item>
        ///     </list>
        /// </remarks>
        string GetKeyLocation(string keyLocation);
    }
}
