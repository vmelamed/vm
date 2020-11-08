using System;
using System.Threading;

#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class TaskDumpMetadata : IAsyncResult
    {
        [Dump(0)]
        public object? CreationOptions { get; set; }

        [Dump(1)]
        public object? Options { get; set; }

        [Dump(2, DumpNullValues = ShouldDump.Skip)]
        public object? AsyncState { get; set; }

        [Dump(3)]
        public object? Id { get; set; }

        [Dump(4)]
        public object? Status { get; set; }

        [Dump(5)]
        public object? IsCompleted { get; set; }

        [Dump(6)]
        public object? IsCanceled { get; set; }

        [Dump(7)]
        public object? IsFaulted { get; set; }

        [Dump(false)]
        public object? IsChildReplica { get; set; }

        [Dump(false)]
        public object? IsDelegateInvoked { get; set; }

        [Dump(8)]
        public object? IsExceptionObservedByParent { get; set; }

        [Dump(-1, DumpNullValues = ShouldDump.Skip)]
        public object? Exception { get; set; }

        [Dump(false)]
        public object? Result { get; set; }

        [Dump(false)]
        public object? Factory { get; set; }

        [Dump(false)]
        public object? CancellationToken { get; set; }

        [Dump(false)]
        public object? CapturedContext { get; set; }

        [Dump(false)]
        public object? CompletedEvent { get; set; }

        [Dump(false)]
        public object? ExceptionRecorded { get; set; }

        [Dump(false)]
        public object? ExecutingTaskScheduler { get; set; }

        [Dump(false)]
        public object? HandedOverChildReplica { get; set; }

        [Dump(false)]
        public object? SavedStateForNextReplica { get; set; }

        [Dump(false)]
        public object? SavedStateFromPreviousReplica { get; set; }

        [Dump(false)]
        public object? ShouldNotifyDebuggerOfWaitCompletion { get; set; }

        [Dump(false)]
        public object? DebuggerDisplayMethodDescription { get; set; }

        [Dump(false)]
        public object? DebuggerDisplayResultDescription { get; set; }

        [Dump(false)]
        public object? ResultOnSuccess { get; set; }

        [Dump(false)]
        public object? IsSelfReplicatingRoot { get; set; }

        [Dump(false)]
        public object? IsWaitNotificationEnabled { get; set; }

        [Dump(false)]
        public object? IsWaitNotificationEnabledOrNotRanToCompletion { get; set; }

        #region IAsyncResult Members

        [Dump(false)]
        object? IAsyncResult.AsyncState => null;

        [Dump(false)]
        WaitHandle IAsyncResult.AsyncWaitHandle => throw new NotImplementedException();

        [Dump(false)]
        bool IAsyncResult.CompletedSynchronously => false;

        [Dump(false)]
        bool IAsyncResult.IsCompleted => false;

        #endregion
    }
}
