using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Class TestTraceListener. Stores the messages in <see cref="T:IList{string}"/>.
    /// </summary>
    [ConfigurationElementType(typeof(CustomTraceListenerData))]
    public class TestTraceListener : TraceListener
    {
        /// <summary>
        /// Synchronization object.
        /// </summary>
        static object _sync = new object();
        /// <summary>
        /// The list of messages.
        /// </summary>
        static IList<string> _messages = new List<string> { string.Empty };

        /// <summary>
        /// Gets all messages written to the moment.
        /// </summary>
        public static IReadOnlyCollection<string> Messages
        {
            get
            {
                lock (_sync)
                    return new ReadOnlyCollection<string>(_messages.ToList());
            }
        }

        /// <summary>
        /// Resets the messages.
        /// </summary>
        public static void Reset()
        {
            lock (_sync)
                _messages = new List<string> { string.Empty };
        }

        /// <summary>
        /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
        /// Here appends the argument <paramref name="message"/> to the last message in the list.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void Write(
            string message)
        {
            lock (_sync)
            {
                var msg = _messages.Last() + message;

                _messages.RemoveAt(_messages.Count() - 1);
                _messages.Add(msg);
            }
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// Here appends the argument <paramref name="message"/> to the last message in the list and adds a new empty message to the list.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void WriteLine(
            string message)
        {
            lock (_sync)
            {
                Write(message);
                _messages.Add(string.Empty);
            }
        }
    }
}
