using NeonBlack.Gameplay.Core.Actions;

namespace NeonBlack.Gameplay.Core.Rules.TurnPhase
{
    /// <summary>
    /// Resolves queued end-turn actions against turn runtime state.
    /// </summary>
    public sealed class TurnAdvanceActionResolver : IActionResolver
    {
        public const string DefaultActionId = "turn.advance";

        private readonly TurnRuntimeState _turnState;
        private readonly string _actionId;

        public TurnAdvanceActionResolver(TurnRuntimeState turnState, string actionId = DefaultActionId)
        {
            _turnState = turnState;
            _actionId = string.IsNullOrWhiteSpace(actionId) ? DefaultActionId : actionId;
        }

        public bool CanResolve(ActionExecutionContext context)
        {
            return context != null && context.ActionId == _actionId;
        }

        public ActionValidationResult ValidateAction(ActionExecutionContext context)
        {
            if (!CanResolve(context))
                return ActionValidationResult.Failure($"Action must be `{_actionId}`.");

            if (_turnState == null)
                return ActionValidationResult.Failure("Turn state is required.");

            if (_turnState.SeatOrder == null || _turnState.SeatOrder.Count == 0)
                return ActionValidationResult.Failure("Turn order has no seats.");

            return ActionValidationResult.Success();
        }

        public ActionResolutionResult ResolveAction(ActionExecutionContext context)
        {
            ActionValidationResult validation = ValidateAction(context);
            if (!validation.IsValid)
                return ActionResolutionResult.Rejected(validation.Message);

            return _turnState.TryAdvance(out string issue)
                ? ActionResolutionResult.Success($"Advanced turn to seat {_turnState.ActiveSeat}.")
                : ActionResolutionResult.Failure(issue);
        }
    }
}
