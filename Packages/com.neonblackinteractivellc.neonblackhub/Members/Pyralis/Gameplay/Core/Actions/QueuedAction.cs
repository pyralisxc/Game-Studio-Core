using System;

namespace NeonBlack.Gameplay.Core.Actions
{
    /// <summary>
    /// Immutable runtime entry for a pending action.
    /// </summary>
    public readonly struct QueuedAction
    {
        public QueuedAction(string queueId, long sequenceId, ActionExecutionContext context, DateTime queuedAtUtc)
        {
            QueueId = queueId ?? string.Empty;
            SequenceId = sequenceId;
            Context = context;
            QueuedAtUtc = queuedAtUtc;
        }

        public string QueueId { get; }
        public long SequenceId { get; }
        public ActionExecutionContext Context { get; }
        public DateTime QueuedAtUtc { get; }
        public string ActionId => Context != null ? Context.ActionId : string.Empty;
    }
}
