using System;
using System.Diagnostics.Contracts;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The objects implementing the interface <c>IKeyLocationStrategy</c> implement a strategy for determining the encryption key location name, 
    /// e.g. paths and filenames.
    /// </summary>
    [ContractClass(typeof(IKeyLocationStrategyContract))]
    public interface IKeyLocationStrategy
    {
        /// <summary>
        /// Executes the key location strategy and returns the key's location name.
        /// </summary>
        /// <param name="keyLocation">
        /// The seeding key location. Can be <see langword="null"/>, empty or string consisting of whitespace characters only.
        /// </param>
        /// <returns>
        /// The key's location name.
        /// </returns>
        /// <remarks>
        /// The method implements the strategy for determining the location of the file containing the encryption key.
        /// Possible strategy might be to return:
        ///     <list type="number">
        ///         <item>
        ///         the <paramref name="keyLocation"/>, if it is not <see langword="null"/>, empty or string consisting of whitespace characters only;
        ///         otherwise
        ///         </item><item>
        ///         the value specified in the <c>appSettings</c> section of the application's configuration file with key <c>symmetricKeyLocation</c> 
        ///         if it exists; otherwise
        ///         </item><item>
        ///         the path and name of the entry assembly with appended suffix &quot;.KEY&quot;, e.g. &quot;MyApp.exe.KEY&quot;.
        ///         </item>
        ///     </list>
        /// </remarks>
        string GetKeyLocation(string keyLocation);
    }

    [ContractClassFor(typeof(IKeyLocationStrategy))]
    abstract class IKeyLocationStrategyContract : IKeyLocationStrategy
    {
        #region IKeyLocationStrategy Members
        public string GetKeyLocation(string keyLocation)
        {
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()), "The key location cannot be null, empty or consist of whitespace characters only.");

            throw new NotImplementedException();
        }
        #endregion
    }
}
