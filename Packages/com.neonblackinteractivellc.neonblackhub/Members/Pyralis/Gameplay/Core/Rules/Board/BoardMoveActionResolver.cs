using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Core.Rules.TurnPhase;

namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Resolves queued board move actions against logical board state.
    /// </summary>
    public sealed class BoardMoveActionResolver : IActionResolver
    {
        public const string DefaultActionId = "board.move";

        private readonly BoardRuntimeState _boardState;
        private readonly TurnRuntimeState _turnState;
        private readonly IBoardMovePolicy _movePolicy;
        private readonly string _actionId;

        public BoardMoveActionResolver(
            BoardRuntimeState boardState,
            TurnRuntimeState turnState = null,
            IBoardMovePolicy movePolicy = null,
            string actionId = DefaultActionId)
        {
            _boardState = boardState;
            _turnState = turnState;
            _movePolicy = movePolicy;
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

            if (_boardState == null)
                return ActionValidationResult.Failure("Board state is required.");

            if (!TryGetPayload(context, out BoardMoveActionPayload payload))
                return ActionValidationResult.Failure("Board move action requires a BoardMoveActionPayload.");

            if (string.IsNullOrWhiteSpace(payload.PieceId))
                return ActionValidationResult.Failure("Board move action requires a piece id.");

            if (!_boardState.TryGetPiece(payload.PieceId, out BoardPieceState piece))
                return ActionValidationResult.Failure($"Piece `{payload.PieceId}` does not exist.");

            if (piece.IsCaptured)
                return ActionValidationResult.Failure($"Piece `{payload.PieceId}` is captured.");

            if (_turnState != null && piece.OwnerSeat != _turnState.ActiveSeat)
                return ActionValidationResult.Failure($"Piece `{payload.PieceId}` is owned by seat {piece.OwnerSeat}, but active seat is {_turnState.ActiveSeat}.");

            if (!_boardState.HasSpace(payload.Destination))
                return ActionValidationResult.Failure($"Coordinate `{payload.Destination}` is outside the board.");

            _boardState.TryGetPieceAt(payload.Destination, out BoardPieceState destinationPiece);
            if (_movePolicy != null)
            {
                BoardMovePolicyContext policyContext = new BoardMovePolicyContext(_boardState, piece, payload.Destination, destinationPiece);
                if (!_movePolicy.IsMoveAllowed(policyContext, out string policyIssue))
                    return ActionValidationResult.Failure(policyIssue);
            }
            else if (destinationPiece != null)
            {
                return ActionValidationResult.Failure($"Coordinate `{payload.Destination}` is already occupied.");
            }

            return ActionValidationResult.Success();
        }

        public ActionResolutionResult ResolveAction(ActionExecutionContext context)
        {
            ActionValidationResult validation = ValidateAction(context);
            if (!validation.IsValid)
                return ActionResolutionResult.Rejected(validation.Message);

            TryGetPayload(context, out BoardMoveActionPayload payload);
            if (_boardState.TryGetPieceAt(payload.Destination, out BoardPieceState destinationPiece))
            {
                if (_movePolicy == null || !_movePolicy.AllowsCapture)
                    return ActionResolutionResult.Rejected($"Coordinate `{payload.Destination}` is already occupied.");

                if (!_boardState.TryCapturePiece(destinationPiece.PieceId, out string captureIssue))
                    return ActionResolutionResult.Failure(captureIssue);
            }

            return _boardState.TryMovePiece(payload.PieceId, payload.Destination, out string issue)
                ? ActionResolutionResult.Success($"Moved `{payload.PieceId}` to `{payload.Destination}`.")
                : ActionResolutionResult.Failure(issue);
        }

        private static bool TryGetPayload(ActionExecutionContext context, out BoardMoveActionPayload payload)
        {
            if (context != null && context.CustomPayload is BoardMoveActionPayload movePayload)
            {
                payload = movePayload;
                return true;
            }

            payload = default;
            return false;
        }
    }
}
