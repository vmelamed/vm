using System;
using System.Diagnostics.Contracts;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Provides a default implementation of <see cref="IMoneyDefaults"/>.
    /// </summary>
    public class MoneyDefaults : IMoneyDefaults
    {
        /// <summary>
        /// Specifies the default system currency - the old good green $.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public string Currency => "USD";

        /// <summary>
        /// Specifies the default number of digits after the decimal point stored in a <see cref="Money" /> 
        /// type of object for the specified currency - here 2 (to the penny).
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <returns>The default number of digits after the decimal point.</returns>
        public int Decimals(string currency) => 2;

        /// <summary>
        /// Specifies the default rounding method for the specified currency - here the banking rounding, i.e. to the even number of pennies.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <returns>A <see cref="MidpointRounding" /> value.</returns>
        public MidpointRounding Rounding(string currency)
        {
            Contract.Ensures(Contract.Result<MidpointRounding>() == MidpointRounding.ToEven);

            return MidpointRounding.ToEven;
        }
    }
}
