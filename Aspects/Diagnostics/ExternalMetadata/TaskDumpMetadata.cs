using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class TaskDumpMetadata : System.IAsyncResult
    {
        [Dump(0)]
        public object CreationOptions;

        [Dump(1)]
        public object Options;

        [Dump(2, DumpNullValues=ShouldDump.Skip)]
        public object AsyncState;

        [Dump(3)]
        public object Id;

        [Dump(4)]
        public object Status;

        [Dump(5)]
        public object IsCompleted;

        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId="Cancelled", Justification="see TaskDump")]
        [Dump(6)]
        public object IsCancelled;

        [Dump(7)]
        public object IsFaulted;

        [Dump(false)]
        public object IsChildReplica;

        [Dump(false)]
        public object IsDelegateInvoked;

        [Dump(8)]
        public object IsExceptionObservedByParent;

        [Dump(-1, DumpNullValues=ShouldDump.Skip)]
        public object Exception;

        [Dump(-2, DumpNullValues=ShouldDump.Skip)]
        public object Result;

        [Dump(false)]
        public object CancellationToken;

        [Dump(false)]
        public object CapturedContext;

        [Dump(false)]
        public object CompletedEvent;

        [Dump(false)]
        public object ExceptionRecorded;

        [Dump(false)]
        public object ExecutingTaskScheduler;

        [Dump(false)]
        public object HandedOverChildReplica;

        [Dump(false)]
        public object SavedStateForNextReplica;

        [Dump(false)]
        public object SavedStateFromPreviousReplica;

        [Dump(false)]
        public object ShouldNotifyDebuggerOfWaitCompletion;

        [Dump(false)]
        public object DebuggerDisplayMethodDescription;

        [Dump(false)]
        public object DebuggerDisplayResultDescription;

        [Dump(false)]
        public object ResultOnSuccess;

        [Dump(false)]
        public object IsSelfReplicatingRoot;

        [Dump(false)]
        public object IsWaitNotificationEnabled;

        [Dump(false)]
        public object IsWaitNotificationEnabledOrNotRanToCompletion;

        #region IAsyncResult Members

        [Dump(false)]
        object IAsyncResult.AsyncState { get { throw new NotImplementedException(); } }

        [Dump(false)]
        WaitHandle IAsyncResult.AsyncWaitHandle { get { throw new NotImplementedException(); } }

        [Dump(false)]
        bool IAsyncResult.CompletedSynchronously { get { throw new NotImplementedException(); } }

        [Dump(false)]
        bool IAsyncResult.IsCompleted { get { throw new NotImplementedException(); } }

        #endregion
    }
}
