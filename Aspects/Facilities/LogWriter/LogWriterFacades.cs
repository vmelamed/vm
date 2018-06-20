using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Class LogWriterFacades: a set of EL LogWriter logging facades.
    /// </summary>
    public static class LogWriterFacades
    {
        // Log categories:

        /// <summary>
        /// The general log category - can be defined to be anything... - "General"
        /// </summary>
        public const string General        = "General";
        /// <summary>
        /// The alert log category - can be defined to go to all usual places as well as to some specific e-mail addresses, e.g. operations team - "Alert"
        /// </summary>
        public const string Alert          = "Alert";
        /// <summary>
        /// The exception - the category for logging program exceptions to file, MSMQ, DB, etc. for later analysis - "Exception"
        /// </summary>
        public const string Exception      = "Exception";
        /// <summary>
        /// The trace - the category for tracing events/messages - probably a file+ VS output pane, should be disabled in production - "Trace"
        /// </summary>
        public const string Trace          = "Trace";
        /// <summary>
        /// The start call trace - the category specific for pre-call tracing. Similar to Trace category but can be useful to audit service calls, etc. in production too. - "Call Start"
        /// </summary>
        public const string StartCallTrace = "Call Start";
        /// <summary>
        /// The end call trace - the category specific for post-call tracing. Similar to Trace category but can be useful to audit service calls, etc. in production too. - "Call End"
        /// </summary>
        public const string EndCallTrace   = "Call End";
        /// <summary>
        /// The event log - the category that specifically sends messages to the Windows event log, e.g. "Service xyz started." - "Event Log"
        /// </summary>
        public const string EventLog       = "Event Log";
        /// <summary>
        /// The email log - the category sending messages to some specific e-mail addresses.
        /// </summary>
        public const string Email          = "Email";

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer.
        /// </summary>
        /// <param name="logger">The log writer.</param>
        /// <param name="category">The category.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="extendedProperties">The extended properties.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The LogWriter.</returns>
        public static LogWriter WriteMessage(
            this LogWriter logger,
            string category,
            TraceEventType severity,
            IDictionary<string, object> extendedProperties = null,
            string format = null,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (category.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(category));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (!logger.IsLoggingEnabled())
                return logger;

            var entry = new LogEntry
            {
                Categories         = new[] { category },
                Severity           = severity,
                ExtendedProperties = extendedProperties,
            };

            if (!logger.ShouldLog(entry))
                return logger;

            if (!format.IsNullOrEmpty())
                switch (args.Length)
                {
                case 0:
                    entry.Message = format;
                    break;

                case 1:
                    entry.Message = string.Format(CultureInfo.InvariantCulture, format, args[0]);
                    break;

                case 2:
                    entry.Message = string.Format(CultureInfo.InvariantCulture, format, args[0], args[1]);
                    break;

                case 3:
                    entry.Message = string.Format(CultureInfo.InvariantCulture, format, args[0], args[1], args[2]);
                    break;

                default:
                    entry.Message = string.Format(CultureInfo.InvariantCulture, format, args);
                    break;
                }

            logger.Write(entry);

            return logger;
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer.
        /// </summary>
        /// <param name="logger">The log writer.</param>
        /// <param name="category">The category.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="formatMessage">A delegate which performs the formatting of the message. The delegate must be able to handle null parameters.</param>
        /// <param name="format">The format.</param>
        /// <param name="extendedProperties">The additional properties.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The LogWriter.</returns>
        public static LogWriter WriteMessage(
            this LogWriter logger,
            string category,
            TraceEventType severity,
            Func<string, object[], string> formatMessage,
            IDictionary<string, object> extendedProperties = null,
            string format = null,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (category.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(category));
            if (formatMessage == null)
                throw new ArgumentNullException(nameof(formatMessage));

            if (!logger.IsLoggingEnabled())
                return logger;

            var entry = new LogEntry
            {
                Categories         = new[] { category },
                Severity           = severity,
                ExtendedProperties = extendedProperties,
            };

            if (logger.ShouldLog(entry))
            {
                entry.Message = formatMessage(format, args);
                logger.Write(entry);
            }

            return logger;
        }

        #region Logging exceptions
        /// <summary>
        /// Constructs a log entry out of the dump of an exception and writes it to the specified log writer's event listeners from the Exception category.
        /// </summary>
        /// <param name="logger">The log writer.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="exception">The exception to be logged.</param>
        /// <param name="extendedProperties">The additional properties.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter ExceptionMessage(
            this LogWriter logger,
            TraceEventType severity,
            Exception exception,
            IDictionary<string, object> extendedProperties = null)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return logger.WriteMessage(Exception, severity, (_, __) => exception.DumpString(), extendedProperties);
        }

        /// <summary>
        /// Constructs a log entry out of the dump of an exception and writes it to the specified log writer's event listeners from the Exception category with severity Information.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="extendedProperties">The additional properties.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter ExceptionInfo(
            this LogWriter logger,
            Exception exception,
            IDictionary<string, object> extendedProperties = null)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return logger.ExceptionMessage(TraceEventType.Information, exception, extendedProperties);
        }


        /// <summary>
        /// Constructs a log entry out of the dump of an exception and writes it to the specified log writer's event listeners from the Exception category with severity Warning.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="extendedProperties">The additional properties.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter ExceptionWarning(
            this LogWriter logger,
            Exception exception,
            IDictionary<string, object> extendedProperties = null)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return logger.ExceptionMessage(TraceEventType.Warning, exception, extendedProperties);
        }


        /// <summary>
        /// Constructs a log entry out of the dump of an exception and writes it to the specified log writer's event listeners from the Exception category with severity Error.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="extendedProperties">The additional properties.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter ExceptionError(
            this LogWriter logger,
            Exception exception,
            IDictionary<string, object> extendedProperties = null)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return logger.ExceptionMessage(TraceEventType.Error, exception, extendedProperties);
        }


        /// <summary>
        /// Constructs a log entry out of the dump of an exception and writes it to the specified log writer's event listeners from the Exception category with severity Critical.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="extendedProperties">The additional properties.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter ExceptionCritical(
            this LogWriter logger,
            Exception exception,
            IDictionary<string, object> extendedProperties = null)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return logger.ExceptionMessage(TraceEventType.Critical, exception, extendedProperties);
        }
        #endregion

        #region Sending messages to the general log
        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the General category.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter LogMessage(
            this LogWriter logger,
            TraceEventType severity,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.WriteMessage(General, severity, null, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the General category with severity Information.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter LogInfo(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.LogMessage(TraceEventType.Information, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the General category with severity Warning.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter LogWarning(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.LogMessage(TraceEventType.Warning, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the General category with severity Error.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter LogError(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.LogMessage(TraceEventType.Error, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the General category with severity Critical.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter LogCritical(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.LogMessage(TraceEventType.Critical, format, args);
        }
        #endregion

        #region Sending messages to the Windows event log
        // Note: if the messages that you log looks like this in the event viewer:
        //      The description for Event ID 50 from source vm.Aspects cannot be found. Either the component that raises this event 
        //      is not installed on your local computer or the installation is corrupted. You can install or repair the component on the local computer.
        //      If the event originated on another computer, the display information had to be saved with the event.
        //      The following information was included with the event:
        //      [the event message you logged]
        //
        // Run the file EventLogMessages.reg (replace "vm.Aspects" with the name of your event source, if needed)

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the EventLog category (usually associated with the windows event log).
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter EventLogMessage(
            this LogWriter logger,
            TraceEventType severity,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.WriteMessage(EventLog, severity, null, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the EventLog category (usually associated with the windows event log) with severity Information.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter EventLogInfo(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.EventLogMessage(TraceEventType.Information, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the EventLog category (usually associated with the windows event log) with severity Warning.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter EventLogWarning(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.EventLogMessage(TraceEventType.Warning, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the EventLog category (usually associated with the windows event log) with severity Error.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter EventLogError(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.EventLogMessage(TraceEventType.Error, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the EventLog category (usually associated with the windows event log) with severity Critical.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter EventLogCritical(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.EventLogMessage(TraceEventType.Critical, format, args);
        }
        #endregion

        #region Sending messages to the trace log
        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the Trace category.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter TraceMessage(
            this LogWriter logger,
            TraceEventType severity,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.WriteMessage(Trace, severity, null, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the Trace category with severity Information.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter TraceInfo(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.TraceMessage(TraceEventType.Information, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the Trace category with severity Warning.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter TraceWarning(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.TraceMessage(TraceEventType.Warning, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the Trace category with severity Error.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter TraceError(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.TraceMessage(TraceEventType.Error, format, args);
        }
        #endregion

        #region Sending messages to the alert log
        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the Alert category.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        static LogWriter AlertMessage(
            this LogWriter logger,
            TraceEventType severity,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.WriteMessage(Alert, severity, null, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the Alert category with severity Information.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter AlertInfo(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.AlertMessage(TraceEventType.Information, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the Alert category with severity Warning.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static LogWriter AlertWarning(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.AlertMessage(TraceEventType.Warning, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the Alert category with severity Error.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static LogWriter AlertError(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.AlertMessage(TraceEventType.Error, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the Alert category with severity Critical.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static LogWriter AlertCritical(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.AlertMessage(TraceEventType.Critical, format, args);
        }
        #endregion

        #region Sending messages via E-mail
        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// (usually sends an e-mail to some recipients) from the Email category.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        static LogWriter EmailMessage(
            this LogWriter logger,
            TraceEventType severity,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.WriteMessage(Email, severity, null, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// (usually sends an e-mail to some recipients) from the Email category with severity Information.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter EmailInfo(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.EmailMessage(TraceEventType.Information, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// (usually sends an e-mail to some recipients) from the Email category with severity Warning.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter EmailWarning(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.EmailMessage(TraceEventType.Warning, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// (usually sends an e-mail to some recipients) from the Email category with severity Error.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter EmailError(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.EmailMessage(TraceEventType.Error, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// (usually sends an e-mail to some recipients) from the Email category with severity Critical.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter EmailCritical(
            this LogWriter logger,
            string format,
            params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (format.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return logger.EmailMessage(TraceEventType.Critical, format, args);
        }
        #endregion

        /// <summary>
        /// Gets the text of the test log so far (presuming that there is only one).
        /// </summary>
        public static string GetTestLogText(
            this LogWriter logger,
            bool resetTestLog = false)
        {
            string logText = null;

            logger.Configure(
                logConfig =>
                {
                    var testLog = logConfig
                                    .AllTraceListeners
                                    .OfType<TestTraceListener>()
                                    .FirstOrDefault();

                    if (testLog == null)
                        return;

                    logText = testLog.LogText;

                    if (resetTestLog)
                        testLog.Reset();
                });

            return logText;
        }

        /// <summary>
        /// Resets the text of the test log (presuming that there is only one).
        /// </summary>
        public static void ResetTestLog(
            this LogWriter logger)
            => logger.Configure(
                        logConfig => (logConfig
                                        .AllTraceListeners
                                        .OfType<TestTraceListener>()
                                        .FirstOrDefault())?.Reset());
    }
}
