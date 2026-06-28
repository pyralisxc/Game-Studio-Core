using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Tabletop.Runtime
{
    /// <summary>
    /// Tabletop-owned in-memory FIFO action queue for rules-driven board actions.
    /// </summary>
    [AuthoringContract(
        Category = "Tabletop",
        CapabilityPath = "Tabletop/Actions/Tabletop Action Queue Service",
        Surface = AuthoringSurface.Service,
        Summary = "Processes tabletop action execution requests and resolves them via registered tabletop resolvers.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/actions",
        RequiredFields = new[] { nameof(_pendingActions), nameof(_resolvers) },
        RequiredInterfaces = new[] { typeof(IActionQueueService) },
        SetupSteps = new[] { "Create TabletopActionQueueService when the tabletop board presenter builds runtime state.", "Register one or more tabletop IActionResolver implementations." },
        SuccessChecks = new[] { "PendingCount increments when an action is successfully enqueued." },
        Tags = new[] { "capability:Tabletop", "axiom:TurnBased" },
        Selectable = false
    )]
    public sealed class TabletopActionQueueService : IActionQueueService
    {
        private readonly List<QueuedAction> _pendingActions;
        private readonly List<IActionResolver> _resolvers;
        private long _nextSequenceId;

        public TabletopActionQueueService(IEnumerable<IActionResolver> resolvers = null)
        {
            _pendingActions = new List<QueuedAction>();
            _resolvers = new List<IActionResolver>();
            if (resolvers == null)
                return;

            foreach (IActionResolver resolver in resolvers)
            {
                if (resolver != null)
                    _resolvers.Add(resolver);
            }
        }

        public int PendingCount => _pendingActions.Count;
        public IReadOnlyList<QueuedAction> PendingActions => _pendingActions;

        public void RegisterResolver(IActionResolver resolver)
        {
            if (resolver != null && !_resolvers.Contains(resolver))
                _resolvers.Add(resolver);
        }

        public bool TryEnqueue(ActionExecutionContext context, out QueuedAction queuedAction, out string issue)
        {
            queuedAction = default;
            if (context == null)
            {
                issue = "Action context is null.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(context.ActionId))
            {
                issue = "Action id is required.";
                return false;
            }

            IActionResolver resolver = FindResolver(context);
            if (resolver == null)
            {
                issue = $"No action resolver can handle `{context.ActionId}`.";
                return false;
            }

            ActionValidationResult validation = resolver.ValidateAction(context);
            if (!validation.IsValid)
            {
                issue = string.IsNullOrWhiteSpace(validation.Message)
                    ? $"Action `{context.ActionId}` was rejected by validation."
                    : validation.Message;
                return false;
            }

            long sequenceId = _nextSequenceId++;
            queuedAction = new QueuedAction(
                "action.queue." + sequenceId,
                sequenceId,
                context,
                DateTime.UtcNow);
            _pendingActions.Add(queuedAction);
            issue = string.Empty;
            return true;
        }

        public bool TryCancel(string queueId, out string issue)
        {
            if (string.IsNullOrWhiteSpace(queueId))
            {
                issue = "Queue id is required.";
                return false;
            }

            for (int i = 0; i < _pendingActions.Count; i++)
            {
                if (_pendingActions[i].QueueId != queueId)
                    continue;

                _pendingActions.RemoveAt(i);
                issue = string.Empty;
                return true;
            }

            issue = $"Queued action `{queueId}` was not found.";
            return false;
        }

        public ActionResolutionResult ResolveNext()
        {
            if (_pendingActions.Count == 0)
                return ActionResolutionResult.Pending("No queued actions are pending.");

            QueuedAction action = _pendingActions[0];
            _pendingActions.RemoveAt(0);

            IActionResolver resolver = FindResolver(action.Context);
            if (resolver == null)
                return ActionResolutionResult.Rejected($"No action resolver can handle `{action.ActionId}`.");

            return resolver.ResolveAction(action.Context);
        }

        private IActionResolver FindResolver(ActionExecutionContext context)
        {
            for (int i = 0; i < _resolvers.Count; i++)
            {
                IActionResolver resolver = _resolvers[i];
                if (resolver != null && resolver.CanResolve(context))
                    return resolver;
            }

            return null;
        }
    }
}
