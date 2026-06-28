using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Core.Actions
{
    /// <summary>
    /// Service boundary for queued action selection and rules resolution.
    /// </summary>
    [AuthoringContract(
        Category = "Turn Based",
        Surface = AuthoringSurface.RequiredSetup,
        Summary = "Interface for queuing and resolving discrete gameplay actions.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/core",
        Tags = new[] { "capability:TurnBased" },
        Selectable = false
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
