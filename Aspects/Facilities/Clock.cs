using System;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Class Clock - wraps <see cref="P:System.DateTime.UtcNow"/>.
    /// </summary>
    public class Clock : IClock
    {
        #region IClock Members
        /// <summary>
        /// Initializes the clock implementation.
        /// The implementation should tolerate multiple calls to this method even if the previous initializations were successful.
        /// </summary>
        /// <returns><see langword="true" /> if the initialization was successful, otherwise <see langword="false" />.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Initialize() => true;

        /// <summary>
        /// Gets the DateTime UTC value at the current moment (e.g. DateTime.UtcNow).
        /// </summary>
        /// <value>The now.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        public DateTime UtcNow => DateTime.UtcNow;

        #endregion
    }
}
