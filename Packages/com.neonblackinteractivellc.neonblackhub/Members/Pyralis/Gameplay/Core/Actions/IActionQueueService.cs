using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Actions
{
    /// <summary>
    /// Service boundary for queued action selection and rules resolution.
    /// </summary>
    public interface IActionQueueService
    {
        int PendingCount { get; }
        IReadOnlyList<QueuedAction> PendingActions { get; }
        void RegisterResolver(IActionResolver resolver);
        bool TryEnqueue(ActionExecutionContext context, out QueuedAction queuedAction, out string issue);
        bool TryCancel(string queueId, out string issue);
        ActionResolutionResult ResolveNext();
    }
}
