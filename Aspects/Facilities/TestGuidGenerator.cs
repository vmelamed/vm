using System;
using Microsoft.Practices.Unity;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Implements IGuidGenerator such that returns a predictable and testable sequence of returned values,
    /// e.g. {00000000-0000-0000-0000-000000000000}, {00000001-0000-0000-0000-000000000000}, {00000002-0000-0000-0000-000000000000}, etc.
    /// </summary>
    public class TestGuidGenerator : IGuidGenerator
    {
        readonly object _sync = new object();
        readonly int _start;
        readonly int _increment;
        int _variable;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestGuidGenerator"/> class with starting value of 
        /// {00000000-0000-0000-0000-000000000000} and incrementing the integer in the first 4 bytes by 1:
        /// {00000001-0000-0000-0000-000000000000},
        /// {00000002-0000-0000-0000-000000000000}, etc.
        /// </summary>
        [InjectionConstructor]
        public TestGuidGenerator()
            : this(0, 1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestGuidGenerator"/> class with start and increment values
        /// for the integer in the 4 bytes of the generated GUID.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="increment">The increment.</param>
        public TestGuidGenerator(
            int start,
            int increment)
        {
            _variable  = 
            _start     = start;
            _increment = increment;
        }

        /// <summary>
        /// Gets the starting GUID.
        /// </summary>
        public Guid StartGuid
        {
            get { return new Guid(_start, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }); }
        }

        /// <summary>
        /// Gets the GUID that will be returned by the next call to <see cref="M:NewGuid"/>.
        /// </summary>
        public Guid NextGuid
        {
            get { return new Guid(_variable, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }); }
        }

        /// <summary>
        /// Gets the last unique identifier returned by <see cref="M:NewGuid"/>.
        /// </summary>
        public Guid LastGuid
        {
            get { return new Guid(_variable-_increment, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }); }
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public Guid Reset()
        {
            _variable = _start;
            return NextGuid;
        }

        #region IGuidGenerator Members
        /// <summary>
        /// Creates a new GUID value.
        /// </summary>
        /// <returns>New GUID.</returns>
        public Guid NewGuid()
        {
            lock (_sync)
            {
                var v = _variable;

                _variable += _increment;
                return new Guid(v, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
            }
        }
        #endregion
    }
}
