using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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
        /// The start call trace - the category specific for tracing starting calls. Similar to Trace category but can be useful to audit service calls, etc. in production too. - "Start Call Trace"
        /// </summary>
        public const string StartCallTrace = "Start Call Trace";
        /// <summary>
        /// The end call trace - the category specific for tracing ending call. Similar to Trace category but can be useful to audit service calls, etc. in production too. - "End Call Trace"
        /// </summary>
        public const string EndCallTrace   = "End Call Trace";
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
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The LogWriter.</returns>
        public static LogWriter WriteMessage(
            this LogWriter logger,
            string category,
            TraceEventType severity,
            string format,
            params object[] args)
        {
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentException>(category != null  &&  category.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(category)+" cannot be null, empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(format != null  &&  format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be null, empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            if (!logger.IsLoggingEnabled())
                return logger;

            var entry = new LogEntry
            {
                Categories = new[] { category },
                Severity   = severity,
            };

            if (!logger.ShouldLog(entry))
                return logger;

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
        /// <param name="formatMessage">A delegate which performs the formatting of the message.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The LogWriter.</returns>
        public static LogWriter WriteMessage(
            this LogWriter logger,
            string category,
            TraceEventType severity,
            Func<string, object[], string> formatMessage,
            string format,
            params object[] args)
        {
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentException>(category != null  &&  category.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(category)+" cannot be null, empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(formatMessage != null, nameof(formatMessage));
            Contract.Requires<ArgumentException>(format != null  &&  format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be null, empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            if (!logger.IsLoggingEnabled())
                return logger;

            var entry = new LogEntry
            {
                Categories = new[] { category },
                Severity   = severity,
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
        /// <returns>LogWriter.</returns>
        public static LogWriter ExceptionMessage(
            this LogWriter logger,
            TraceEventType severity,
            Exception exception)
        {
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            return logger.WriteMessage(Exception, severity, (f, a) => exception.DumpString(), "dummy");
        }

        /// <summary>
        /// Constructs a log entry out of the dump of an exception and writes it to the specified log writer's event listeners from the Exception category with severity Information.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter ExceptionInfo(
            this LogWriter logger,
            Exception exception)
        {
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            return logger.ExceptionMessage(TraceEventType.Information, exception);
        }


        /// <summary>
        /// Constructs a log entry out of the dump of an exception and writes it to the specified log writer's event listeners from the Exception category with severity Warning.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter ExceptionWarning(
            this LogWriter logger,
            Exception exception)
        {
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            return logger.ExceptionMessage(TraceEventType.Warning, exception);
        }


        /// <summary>
        /// Constructs a log entry out of the dump of an exception and writes it to the specified log writer's event listeners from the Exception category with severity Error.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter ExceptionError(
            this LogWriter logger,
            Exception exception)
        {
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            return logger.ExceptionMessage(TraceEventType.Error, exception);
        }


        /// <summary>
        /// Constructs a log entry out of the dump of an exception and writes it to the specified log writer's event listeners from the Exception category with severity Critical.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>LogWriter.</returns>
        public static LogWriter ExceptionCritical(
            this LogWriter logger,
            Exception exception)
        {
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            return logger.ExceptionMessage(TraceEventType.Critical, exception);
        }
        #endregion

        #region Sending messages to the general log
        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the General category (usually associated with the windows event log).
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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            return logger.WriteMessage(General, severity, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the General category (usually associated with the windows event log) with severity Information.
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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            return logger.LogMessage(TraceEventType.Information, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the General category (usually associated with the windows event log) with severity Warning.
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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            return logger.LogMessage(TraceEventType.Warning, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the General category (usually associated with the windows event log) with severity Error.
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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            return logger.LogMessage(TraceEventType.Error, format, args);
        }

        /// <summary>
        /// Constructs a log entry and writes it to the specified log writer's event listeners 
        /// from the General category (usually associated with the windows event log) with severity Critical.
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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            return logger.WriteMessage(EventLog, severity, format, args);
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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            return logger.WriteMessage(Trace, severity, format, args);
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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            return logger.WriteMessage(Alert, severity, format, args);
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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            return logger.WriteMessage(Email, severity, format, args);
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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

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
            Contract.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Contract.Requires<ArgumentNullException>(format != null, nameof(format));
            Contract.Requires<ArgumentException>(format.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(format)+" cannot be empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(args != null, nameof(args));
            Contract.Ensures(Contract.Result<LogWriter>() != null);

            return logger.EmailMessage(TraceEventType.Critical, format, args);
        }
        #endregion
    }
}
