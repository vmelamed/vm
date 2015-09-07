using System;
using Microsoft.Practices.Unity;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Implements IClock by generating predictable and testable values returned from the property <see cref="UtcNow"/>.
    /// E.g. 2013/3/11 00:00:00.000, 2013/3/11 00:00:01.000, 2013/3/11 00:00:02.000, etc.
    /// </summary>
    public class TestClock : IClock
    {
        readonly object _sync = new object();
        readonly TimeSpan _timeIncrement;
        DateTime _startTime;
        DateTime _currentTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClock"/> class by setting the initial time stamp at 1/1/2015 and time increment 1 sec.
        /// </summary>
        [InjectionConstructor]
        public TestClock()
            : this(new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc), new TimeSpan(0, 0, 1))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClock"/> class with starting time and time increment.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="timeIncrement">The time increment.</param>
        public TestClock(
            DateTime startTime,
            TimeSpan timeIncrement)
        {
            _startTime     = startTime;
            _currentTime   = _startTime;
            _timeIncrement = timeIncrement;
        }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>The start time.</value>
        public DateTime StartTime
        {
            get
            {
                lock (_sync)
                    return _startTime;
            }
            set
            {
                lock (_sync)
                    _startTime = _currentTime = value;
            }
        }

        /// <summary>
        /// Gets the next time stamp.
        /// </summary>
        /// <value>The next time.</value>
        public DateTime NextTime
        {
            get
            {
                lock (_sync)
                    return _currentTime;
            }
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        /// <returns>DateTime.</returns>
        public DateTime Reset()
        {
            lock (_sync)
                return _currentTime = _startTime;
        }

        #region IClock Members
        /// <summary>
        /// Initializes the implementation.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/>.
        /// </returns>
        public bool Initialize()
        {
            return true;
        }

        /// <summary>
        /// Gets the start DateTime plus as many intervals as the calls made to this property.
        /// Gets a predictable value useful for unit testing along with <see cref="P:NextTime"/>.
        /// </summary>
        public DateTime UtcNow
        {
            get
            {
                lock (_sync)
                {
                    var now = _currentTime;

                    _currentTime += _timeIncrement;
                    return now;
                }
            }
        }
        #endregion
    }
}
