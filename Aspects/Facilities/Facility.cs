using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.ServiceLocation;
using vm.Aspects.Threading;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Contains a few commonly used facilities. The class is supposed to be a singleton built by the DI container.
    /// </summary>
    public static partial class Facility
    {
        static Lazy<IClock> _clock;
        static Lazy<IGuidGenerator> _guidGenerator;
        static Lazy<ValidatorFactory> _validatorFactory;
        static Lazy<ExceptionManager> _exceptionManager;
        static Lazy<LogWriter> _logWriter;
        static ReaderWriterLockSlim _syncLogger = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        static void InitializeFacilities()
        {
            _clock            = new Lazy<IClock>(() => ServiceLocator.Current.GetInstance<IClock>(), true);
            _guidGenerator    = new Lazy<IGuidGenerator>(() => ServiceLocator.Current.GetInstance<IGuidGenerator>(), true);
            _validatorFactory = new Lazy<ValidatorFactory>(() => ServiceLocator.Current.GetInstance<ValidatorFactory>(), true);
            _exceptionManager = new Lazy<ExceptionManager>(() => ServiceLocator.Current.GetInstance<Lazy<ExceptionManager>>().Value, true);
            _logWriter        = new Lazy<LogWriter>(() => ServiceLocator.Current.GetInstance<Lazy<LogWriter>>().Value, true);
        }

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification="InitializeFacilities is called from Reset too.")]
        static Facility()
        {
            InitializeFacilities();
        }

        /// <summary>
        /// Gets the clock.
        /// </summary>
        public static IClock Clock
        {
            get { return _clock.Value; }
        }

        /// <summary>
        /// Gets the GUID generator.
        /// </summary>
        public static IGuidGenerator GuidGenerator
        {
            get { return _guidGenerator.Value; }
        }

        /// <summary>
        /// Gets the EL log writer.
        /// </summary>
        public static LogWriter LogWriter
        {
            get
            {
                using (_syncLogger.ReaderLock())
                    return _logWriter.Value;
            }
        }

        /// <summary>
        /// Gets the EL exception manager.
        /// </summary>
        public static ExceptionManager ExceptionManager
        {
            get { return _exceptionManager.Value; }
        }

        /// <summary>
        /// Gets the EL validator factory.
        /// </summary>
        public static ValidatorFactory ValidatorFactory
        {
            get { return _validatorFactory.Value; }
        }
    }
}
