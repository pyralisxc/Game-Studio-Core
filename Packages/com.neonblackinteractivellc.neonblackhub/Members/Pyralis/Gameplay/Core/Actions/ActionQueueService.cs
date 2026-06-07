using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Actions
{
    /// <summary>
    /// In-memory FIFO action queue for rules-driven and menu-driven gameplay.
    /// </summary>
    public sealed class ActionQueueService : IActionQueueService
    {
        private readonly List<QueuedAction> _pendingActions;
        private readonly List<IActionResolver> _resolvers;
        private long _nextSequenceId;

        public ActionQueueService(IEnumerable<IActionResolver> resolvers = null)
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
