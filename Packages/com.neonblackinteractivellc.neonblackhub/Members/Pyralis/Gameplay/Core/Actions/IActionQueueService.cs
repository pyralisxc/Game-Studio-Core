using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Core.Actions
{
    /// <summary>
    /// Service boundary for queued action selection and rules resolution.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.TurnBased,
        Relevance = "Interface for queuing and resolving discrete gameplay actions.",
        ExpertAdvice = "Use IActionQueueService to manage the order of operations in turn-based games or command-heavy realtime systems. Resolvers must be registered to handle specific ActionIds.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/core"
    )]
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
