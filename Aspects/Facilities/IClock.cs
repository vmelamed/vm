using System;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Interface IClock of a clock facility which provides the current UTC time.
    /// Intended to abstract out the <see cref="P:System.DateTime.UtcNow"/> behavior, 
    /// which would allow for introduction of predictable clocks used in unit tests.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Initializes the clock implementation.
        /// The implementation should tolerate multiple calls to this method even if the previous initializations were successful.
        /// </summary>
        /// <returns><see langword="true"/> if the initialization was successful, otherwise <see langword="false"/>.</returns>
        bool Initialize();

        /// <summary>
        /// Gets the DateTime UTC value at the current moment (e.g. DateTime.UtcNow).
        /// </summary>
        /// <value>The now.</value>
        DateTime UtcNow { get; }
    }
}
