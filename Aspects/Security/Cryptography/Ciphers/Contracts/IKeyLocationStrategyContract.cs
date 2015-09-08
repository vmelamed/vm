using System;
using System.Diagnostics.Contracts;

namespace vm.Aspects.Security.Cryptography.Ciphers.Contracts
{
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
