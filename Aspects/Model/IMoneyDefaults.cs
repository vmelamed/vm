using System;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Interface IMoneyDefaults represents some global default properties and behaviors about <see cref="Money"/> type of objects.
    /// </summary>
    public interface IMoneyDefaults
    {
        /// <summary>
        /// Specifies the default system currency.
        /// </summary>
        string Currency { get; }

        /// <summary>
        /// Specifies the default number of digits after the decimal point stored in a <see cref="Money" /> type of object for the specified currency.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <returns>The default number of digits after the decimal point.</returns>
        int Decimals(string currency);

        /// <summary>
        /// Specifies the default rounding method for the specified currency.
        /// </summary>
        /// <param name="currency">The currency.</param>
        MidpointRounding Rounding(string currency);
    }
}
