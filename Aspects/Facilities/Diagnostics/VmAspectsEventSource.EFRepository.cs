using System;
using System.Diagnostics.Tracing;

namespace vm.Aspects.Facilities.Diagnostics
{
    /// <summary>
    /// Class EtwEventSource. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="EventSource" />
    public sealed partial class VmAspectsEventSource
    {
        #region Unit of work events
        /// <summary>
        /// Event marking the start of a unit of work.
        /// </summary>
        [Event(UnitOfWorkStartId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Uow, Message = "Unit of work started.")]
        public void UnitOfWorkStart()
        {
            if (IsEnabled())
                WriteEvent(UnitOfWorkStartId);
        }

        /// <summary>
        /// Event marking the successful end of a unit of work.
        /// </summary>
        [Event(UnitOfWorkStopId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Uow, Message = "Unit of work stopped.")]
        public void UnitOfWorkStop()
        {
            if (IsEnabled())
                WriteEvent(UnitOfWorkStopId);
        }

        /// <summary>
        /// Event marking the unsuccessful a unit of work. Includes the exception.
        /// </summary>
        [NonEvent]
        public void UnitOfWorkFailed(
            Exception ex)
        {
            if (IsEnabled(EventLevel.Error, Keywords.vmAspects | Keywords.Uow))
                UnitOfWorkFailed(ex.DumpString());
        }

        [Event(UnitOfWorkFailedId, Level = EventLevel.Error, Keywords = Keywords.vmAspects | Keywords.Uow, Message = "Unit of work failed.")]
        void UnitOfWorkFailed(
            string exceptionDump)
        {
            if (IsEnabled())
                WriteEvent(UnitOfWorkFailedId, exceptionDump);
        }
        #endregion

        #region EFRepositoryBase events
        /// <summary>
        /// Event marking the start of a repository initialization.
        /// </summary>
        [NonEvent]
        public void EFRepositoryInitializationStart(
            object repository)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects | Keywords.EFRepository))
                EFRepositoryInitializationStart(repository.GetType().FullName);
        }

        /// <summary>
        /// Event marking the end of a repository initialization.
        /// </summary>
        [NonEvent]
        public void EFRepositoryInitializationStop(
            object repository)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects | Keywords.EFRepository))
                EFRepositoryInitializationStop(repository.GetType().FullName);
        }

        /// <summary>
        /// Event marking the successful end of a repository initialization.
        /// </summary>
        [NonEvent]
        public void EFRepositoryInitializationFailed(
            object repository,
            Exception ex)
        {
            if (IsEnabled(EventLevel.Error, Keywords.vmAspects | Keywords.EFRepository))
                EFRepositoryInitializationFailed(repository.GetType().FullName, ex.DumpString());
        }

        /// <summary>
        /// Event marking saving the repository changes operation.
        /// </summary>
        [NonEvent]
        public void EFRepositoryCommitChanges(
            object repository)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects | Keywords.EFRepository))
                EFRepositoryCommitChanges(repository.GetType().FullName);
        }

        /// <summary>
        /// Marks the generation of the repository mapping views cache.
        /// </summary>
        /// <param name="repository">The repository.</param>
        [NonEvent]
        public void EFRepositoryMappingViewsCache(
            object repository)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects | Keywords.EFRepository))
                EFRepositoryMappingViewsCache(repository.GetType().FullName);
        }

        /// <summary>
        /// Event marking the start of a repository initialization.
        /// </summary>
        [Event(EFRepositoryInitializationStartId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.EFRepository, Message = "{0} initialization started.")]
        void EFRepositoryInitializationStart(
            string RepositoryType)
        {
            if (IsEnabled())
                WriteEvent(EFRepositoryInitializationStartId, RepositoryType);
        }

        /// <summary>
        /// Event marking the successful end of a repository initialization.
        /// </summary>
        [Event(EFRepositoryInitializationStopId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.EFRepository, Message = "{0} initialization stopped.")]
        void EFRepositoryInitializationStop(
            string RepositoryType)
        {
            if (IsEnabled())
                WriteEvent(EFRepositoryInitializationStopId, RepositoryType);
        }

        /// <summary>
        /// Event marking the failure of a repository initialization.
        /// </summary>
        [Event(EFRepositoryInitializationFailedId, Level = EventLevel.Critical, Keywords = Keywords.vmAspects | Keywords.EFRepository, Message = "{0} initialization failed.")]
        void EFRepositoryInitializationFailed(
            string RepositoryType,
            string ExceptionDump)
        {
            if (IsEnabled())
                WriteEvent(EFRepositoryInitializationFailedId, RepositoryType, ExceptionDump);
        }

        /// <summary>
        /// Event marking saving the changes operation.
        /// </summary>
        [Event(EFRepositoryCommitChangesId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.EFRepository, Message = "{0} committing changes.")]
        void EFRepositoryCommitChanges(
            string RepositoryType)
        {
            if (IsEnabled())
                WriteEvent(EFRepositoryCommitChangesId, RepositoryType);
        }

        /// <summary>
        /// Marks the generation of the repository mapping views cache.
        /// </summary>
        /// <param name="RepositoryType">The repository type name.</param>
        [Event(EFRepositoryMappingViewsCacheId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.EFRepository, Message = "Generating mapping views for repository {0}")]
        void EFRepositoryMappingViewsCache(
            string RepositoryType)
        {
            if (IsEnabled())
                WriteEvent(EFRepositoryMappingViewsCacheId, RepositoryType);
        }
        #endregion
    }
}
