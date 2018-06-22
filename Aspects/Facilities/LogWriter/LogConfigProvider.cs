using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using CommonServiceLocator;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

using Unity;
using Unity.Registration;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Class LogConfigProvider. Exposes the registrar for the log configurations to be registered as defaults.
    /// </summary>
    public static class LogConfigProvider
    {
        /// <summary>
        /// Gets the log configuration default registrar.
        /// </summary>
        public static ContainerRegistrar Registrar { get; } = new LogConfigurationsRegistrar();

        /// <summary>
        /// The resolve name for the test log configuration in the DI container - TestLogger.
        /// </summary>
        public const string TestLogConfigurationResolveName = "TestLogger";
        /// <summary>
        /// The default name for the enterprise library configuration file name - EntLib.config.
        /// </summary>
        public const string LogConfigurationFileName = "EntLib.config";

        static string _logConfigurationFileName;
        static IConfigurationSource _logConfiguration;

        /// <summary>
        /// Class LogConfigurationsRegistrar. Registers in code some default development configurations.
        /// The configuration of the log is best if it comes from a configuration file, as it will change from 
        /// environment to environment.
        /// Hint: use XML configurations for the various environments.
        /// </summary>
        private class LogConfigurationsRegistrar : ContainerRegistrar
        {
            public override void Reset(
                IUnityContainer container = null)
            {
                Logger.Reset();
                base.Reset(container);
            }

            /// <summary>
            /// Does the actual work of the registration.
            /// </summary>
            /// <param name="container">The container where to register the defaults.</param>
            /// <param name="registrations">The registrations dictionary used for faster lookup of the existing registrations.</param>
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, IContainerRegistration> registrations)
            {
                if (container == null)
                    throw new ArgumentNullException(nameof(container));
                if (registrations == null)
                    throw new ArgumentNullException(nameof(registrations));

                container
                    .RegisterInstanceIfNot<LoggingConfiguration>(registrations, ConfigureDebugLog());
            }

            /// <summary>
            /// The inheriting types should override this method if they need to register different configuration for unit testing purposes.
            /// The default implementation calls <see cref="ContainerRegistrar.DoRegister" />.
            /// </summary>
            /// <param name="container">The container where to register the defaults.</param>
            /// <param name="registrations">The registrations dictionary used for faster lookup of the existing registrations.</param>
            protected override void DoTestRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, IContainerRegistration> registrations)
            {
                if (container == null)
                    throw new ArgumentNullException(nameof(container));
                if (registrations == null)
                    throw new ArgumentNullException(nameof(registrations));

                container
                    .RegisterInstanceIfNot<LoggingConfiguration>(registrations, ConfigureTestLog());
            }
        }

        /// <summary>
        /// Creates a debug suitable log configuration, where all sources are associated with the VS output window.
        /// </summary>
        /// <returns>LoggingConfiguration.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EL will do it.")]
        static LoggingConfiguration ConfigureDebugLog()
        {
            // configure a log that outputs everything in the debugger output window:

            var logConfig = new LoggingConfiguration();
            var traceListener = new AsynchronousTraceListenerWrapper(new DefaultTraceListener());

            logConfig.AddLogSource(LogWriterFacades.General, SourceLevels.All, true, traceListener);
            logConfig.AddLogSource(LogWriterFacades.Exception, SourceLevels.All, true, traceListener);
            logConfig.AddLogSource(LogWriterFacades.Alert, SourceLevels.All, true, traceListener);
            logConfig.AddLogSource(LogWriterFacades.Email, SourceLevels.All, true, traceListener);
            logConfig.AddLogSource(LogWriterFacades.EventLog, SourceLevels.All, true, traceListener);
            logConfig.AddLogSource(LogWriterFacades.Trace, SourceLevels.All, true, traceListener);
            logConfig.AddLogSource(LogWriterFacades.StartCallTrace, SourceLevels.All, true, traceListener);
            logConfig.AddLogSource(LogWriterFacades.EndCallTrace, SourceLevels.All, true, traceListener);

            logConfig.Filters
                     .Add(new LogEnabledFilter("All", true));

            return logConfig;
        }


        /// <summary>
        /// Creates a test suitable log configuration, where all sources are associated with the in-memory test trace listener.
        /// </summary>
        /// <returns>LoggingConfiguration.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "EL will do it.")]
        static LoggingConfiguration ConfigureTestLog()
        {
            // configure a log that outputs everything in the in-memory test listener (List<string>) log:
            var logConfig = new LoggingConfiguration();
            var traceListener = new TestTraceListener();

            logConfig.AddLogSource(LogWriterFacades.General, SourceLevels.All, true, traceListener);
            logConfig.AddLogSource(LogWriterFacades.Exception, SourceLevels.All, true, traceListener);
            logConfig.AddLogSource(LogWriterFacades.Alert, SourceLevels.All, true, traceListener);
            logConfig.AddLogSource(LogWriterFacades.Email, SourceLevels.All, true, traceListener);
            logConfig.AddLogSource(LogWriterFacades.EventLog, SourceLevels.All, true, traceListener);
            logConfig.AddLogSource(LogWriterFacades.Trace, SourceLevels.All, true, traceListener);
            logConfig.AddLogSource(LogWriterFacades.StartCallTrace, SourceLevels.All, true, traceListener);
            logConfig.AddLogSource(LogWriterFacades.EndCallTrace, SourceLevels.All, true, traceListener);

            logConfig.Filters
                     .Add(new LogEnabledFilter("All", true));

            return logConfig;
        }

        /// <summary>
        /// Creates a log writer from registered logging configuration in the DI container.
        /// </summary>
        /// <param name="logConfigurationResolveName">
        /// The resolve name of the log configuration. Can be <see langword="null"/> or empty.
        /// </param>
        /// <returns>LogWriter.</returns>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// If no registered logging configuration can be found in the DI container.
        /// </exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "N/A")]
        public static LogWriter CreateLogWriterFromContainer(
            string logConfigurationResolveName)
        {
            _logConfigurationFileName = logConfigurationResolveName;

            using (var configuration = ConfigurationSourceFactory.Create())
                try
                {
                    return new LogWriter(
                                    ServiceLocator.Current.GetInstance<LoggingConfiguration>(_logConfigurationFileName));
                }
                catch (Exception x)
                {
                    // wrap and throw
                    throw new ConfigurationErrorsException(
                                $"Cannot find a logging configuration with resolve name \"{logConfigurationResolveName}\".", x);
                }
        }

        /// <summary>
        /// Creates a log writer from a configuration file or from the current application's configuration file (app.config or web.config).
        /// </summary>
        /// <param name="configFileName">
        /// The name of the configuration file. 
        /// If it is <see langword="null"/> or empty or it doesn't exist the current application's configuration file will be used.
        /// </param>
        /// <returns>LogWriter.</returns>
        /// <exception cref="System.Configuration.ConfigurationErrorsException"></exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "N/A")]
        public static LogWriter CreateLogWriterFromConfigFile(
            string configFileName)
        {
            if (!string.IsNullOrWhiteSpace(configFileName))
                if (Path.IsPathRooted(configFileName))
                    _logConfigurationFileName = configFileName;
                else
                {
                    var data = AppDomain
                                   .CurrentDomain
                                   .GetData("APP_CONFIG_FILE");

                    Debug.Assert(data != null);

                    var path = Path.GetDirectoryName(data.ToString());

                    configFileName = Path.Combine(path, configFileName);
                    _logConfigurationFileName = !string.IsNullOrWhiteSpace(configFileName) && File.Exists(configFileName)
                                                            ? configFileName
                                                            : null;
                }

            try
            {
                // if the file name is null or empty or does not exists - go for the app.config
                var logConfigSource = _logConfigurationFileName == null
                                                    ? (IConfigurationSource)new SystemConfigurationSource()
                                                    : (IConfigurationSource)new FileConfigurationSource(_logConfigurationFileName);
                var logger = new LogWriterFactory(logConfigSource).Create();

                if (_logConfiguration != null)
                {
                    _logConfiguration.SourceChanged -= (o, e) => Facility.FacilitiesRegistrar.RefreshLogger(
                                                                    CreateLogWriterFromConfigFile(_logConfigurationFileName));
                    _logConfiguration.Dispose();
                }

                _logConfiguration = logConfigSource;
                _logConfiguration.SourceChanged += (o, e) => Facility.FacilitiesRegistrar.RefreshLogger(
                                                                CreateLogWriterFromConfigFile(_logConfigurationFileName));
                return logger;
            }
            catch (Exception x)
            {
                if (_logConfiguration != null)
                    _logConfiguration.Dispose();

                // wrap and throw
                throw new ConfigurationErrorsException(
                            string.Format(
                                    "There was an error loading the configuration from {0}: {1}",
                                    configFileName.IsNullOrWhiteSpace() ? "the configuration file" : configFileName,
                                    x.Message),
                            x);
            }
        }

        /// <summary>
        /// Creates a log writer either from a file or from registered log configuration in the DI container.
        /// </summary>
        /// <param name="configFileName">
        /// The name of the configuration file. 
        /// If it is <see langword="null"/> or empty the current application's configuration file will be used.
        /// </param>
        /// <param name="resolveName">
        /// The resolve name of the log configuration. Can be <see langword="null"/> or empty.
        /// </param>
        /// <param name="containerOverConfigFile">
        /// By default the file configuration has precedence over the container resolved configuration.
        /// If you want to change the precedence, set this parameter to <see langword="true"/>.
        /// </param>
        /// <returns>Microsoft.Practices.EnterpriseLibrary.Logging.LogWriter instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This is the return value.")]
        public static LogWriter CreateLogWriter(
            string configFileName,
            string resolveName,
            bool containerOverConfigFile = false)
        {
            LogWriter logWriter;

            try
            {
                logWriter = containerOverConfigFile
                                ? CreateLogWriterFromContainer(resolveName)
                                : CreateLogWriterFromConfigFile(configFileName);
            }
            catch (ConfigurationErrorsException)
            {
                logWriter = containerOverConfigFile
                                ? CreateLogWriterFromConfigFile(configFileName)
                                : CreateLogWriterFromContainer(resolveName);
            }

            Logger.Reset();
            Logger.SetLogWriter(logWriter);

            return logWriter;
        }
    }
}
