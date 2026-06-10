using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Core.Rules.TurnPhase;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Unity-facing bridge between a project-owned board presenter and queued board actions.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Tabletop,
        Relevance = "Bridge between a board presenter and queued board actions (e.g. piece selection and movement).",
        NativeSetup = new[] { "Add to Tabletop board root", "Initialize with BoardRuntimeState and ActionQueue" },
        AssignmentFields = new[] { nameof(resolveQueuedMoveImmediately) },
        FirstProof = "Selecting a piece and a destination enqueues and resolves a move action."
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Tabletop/Tabletop Board Selection Bridge")]
    public sealed class TabletopBoardSelectionBridge : MonoBehaviour
    {
        [SerializeField] private bool resolveQueuedMoveImmediately;

        private BoardRuntimeState _boardState;
        private TurnRuntimeState _turnState;
        private IActionQueueService _actionQueue;
        private object _participant;
        private string _selectedPieceId;

        public bool ResolveQueuedMoveImmediately
        {
            get => resolveQueuedMoveImmediately;
            set => resolveQueuedMoveImmediately = value;
        }

        public bool IsInitialized => _boardState != null && _actionQueue != null;
        public bool HasSelection => !string.IsNullOrWhiteSpace(_selectedPieceId);
        public string SelectedPieceId => _selectedPieceId;
        public string LastIssue { get; private set; } = string.Empty;

        public void Initialize(
            BoardRuntimeState boardState,
            IActionQueueService actionQueue,
            object participant = null,
            TurnRuntimeState turnState = null)
        {
            _boardState = boardState;
            _actionQueue = actionQueue;
            _participant = participant;
            _turnState = turnState;
            ClearSelection();
        }

        public bool TrySelectPiece(string pieceId, out string issue)
        {
            if (!EnsureInitialized(out issue))
                return Fail(issue);

            if (string.IsNullOrWhiteSpace(pieceId))
                return Fail("Select a board piece before choosing a destination.", out issue);

            if (!_boardState.TryGetPiece(pieceId, out BoardPieceState piece))
                return Fail($"Piece `{pieceId}` does not exist.", out issue);

            if (piece.IsCaptured)
                return Fail($"Piece `{pieceId}` is captured.", out issue);

            if (_turnState != null && piece.OwnerSeat != _turnState.ActiveSeat)
                return Fail($"Piece `{pieceId}` is owned by seat {piece.OwnerSeat}, but active seat is {_turnState.ActiveSeat}.", out issue);

            _selectedPieceId = piece.PieceId;
            LastIssue = string.Empty;
            issue = string.Empty;
            return true;
        }

        public bool TrySelectPieceAt(BoardCoordinate coordinate, out string issue)
        {
            if (!EnsureInitialized(out issue))
                return Fail(issue);

            if (!_boardState.TryGetPieceAt(coordinate, out BoardPieceState piece))
                return Fail($"No board piece exists at `{coordinate}`.", out issue);

            return TrySelectPiece(piece.PieceId, out issue);
        }

        public bool TrySelectDestination(BoardCoordinate destination, out QueuedAction queuedAction, out string issue)
        {
            queuedAction = default;
            if (!EnsureInitialized(out issue))
                return Fail(issue);

            if (!HasSelection)
                return Fail("Select a board piece before choosing a destination.", out issue);

            BoardMoveActionPayload payload = new BoardMoveActionPayload(_selectedPieceId, destination);
            ActionExecutionContext context = new ActionExecutionContext(
                BoardMoveActionResolver.DefaultActionId,
                ownerObject: gameObject,
                participant: _participant,
                customPayload: payload);

            if (!_actionQueue.TryEnqueue(context, out queuedAction, out issue))
                return Fail(issue);

            ClearSelection();
            if (resolveQueuedMoveImmediately)
            {
                if (_actionQueue.PendingActions.Count == 0 || _actionQueue.PendingActions[0].QueueId != queuedAction.QueueId)
                {
                    LastIssue = "Queued move is waiting behind existing actions and was not resolved immediately.";
                    issue = string.Empty;
                    return true;
                }

                ActionResolutionResult result = _actionQueue.ResolveNext();
                if (!result.Succeeded)
                    return Fail(result.Message, out issue);
            }

            LastIssue = string.Empty;
            issue = string.Empty;
            return true;
        }

        public void ClearSelection()
        {
            _selectedPieceId = string.Empty;
            LastIssue = string.Empty;
        }

        private bool EnsureInitialized(out string issue)
        {
            if (_boardState == null)
            {
                issue = "Board runtime state is required before selecting tabletop spaces.";
                return false;
            }

            if (_actionQueue == null)
            {
                issue = "Action queue service is required before selecting tabletop spaces.";
                return false;
            }

            issue = string.Empty;
            return true;
        }

        private bool Fail(string issue)
        {
            LastIssue = issue ?? string.Empty;
            return false;
        }

        private bool Fail(string issue, out string reportedIssue)
        {
            reportedIssue = issue ?? string.Empty;
            return Fail(reportedIssue);
        }
    }
}
