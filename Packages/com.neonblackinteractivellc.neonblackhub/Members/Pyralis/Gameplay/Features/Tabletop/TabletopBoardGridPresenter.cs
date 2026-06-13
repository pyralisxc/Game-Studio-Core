using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Core.Rules.Board;
using NeonBlack.Gameplay.Core.Rules.TurnPhase;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions.Rules;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Tabletop
{
    /// <summary>
    /// Scene-facing presenter that turns an authored board definition into selectable board objects.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Tabletop | AuthoringCapability.Grid, 
        Relevance = "Inspector Add Component path for a board presenter that can build selectable tabletop spaces.",
        AssignmentFields = new[] { nameof(boardDefinition), nameof(movePolicyDefinition), nameof(spacePrefab), nameof(piecePrefab) },
        FirstProofTargetId = "proof.board-card-action",
        FirstProof = "Click 'Rebuild Board' in the inspector and verify the grid is generated.",
        NativeSetup = new[] { "Add TabletopBoardGridPresenter to a scene object.", "Assign Board, Move Policy, and Turn Order definitions.", "Assign Space and Piece prefabs." },
        ExpertAdvice = "Bridges the abstract BoardDefinition to scene objects. It handles coordinate mapping (X,Y) to world positions. Ensure your cell size matches your visual assets.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/tabletop"
    )]
[AddComponentMenu("NeonBlack/Gameplay/Tabletop/Tabletop Board Grid Presenter")]
    public sealed class TabletopBoardGridPresenter : MonoBehaviour
    {
        [SerializeField] private BoardDefinition boardDefinition;
        [SerializeField] private BoardMovePolicyDefinition movePolicyDefinition;
        [SerializeField] private TurnOrderDefinition turnOrderDefinition;
        [SerializeField] private TabletopBoardSelectionBridge selectionBridge;
        [SerializeField] private GameObject spacePrefab;
        [SerializeField] private GameObject piecePrefab;
        [SerializeField] private Vector2 cellSize = Vector2.one;
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private bool resolveQueuedMoveImmediately = true;
        [SerializeField] private bool advanceTurnAfterResolvedMove = true;

        private readonly List<TabletopBoardSpaceView> _spaceViews = new List<TabletopBoardSpaceView>();
        private readonly List<TabletopBoardPieceView> _pieceViews = new List<TabletopBoardPieceView>();
        private BoardRuntimeState _boardState;
        private TurnRuntimeState _turnState;
        private ActionQueueService _actionQueue;

        public IReadOnlyList<TabletopBoardSpaceView> SpaceViews => _spaceViews;
        public IReadOnlyList<TabletopBoardPieceView> PieceViews => _pieceViews;
        public TabletopBoardSelectionBridge SelectionBridge => selectionBridge;
        public BoardRuntimeState BoardState => _boardState;
        public TurnRuntimeState TurnState => _turnState;
        public IActionQueueService ActionQueue => _actionQueue;
        public string LastIssue { get; private set; } = string.Empty;

        public bool ResolveQueuedMoveImmediately
        {
            get => resolveQueuedMoveImmediately;
            set
            {
                resolveQueuedMoveImmediately = value;
                if (selectionBridge != null)
                    selectionBridge.ResolveQueuedMoveImmediately = value;
            }
        }

        public void Configure(
            BoardDefinition board,
            BoardMovePolicyDefinition movePolicy = null,
            TabletopBoardSelectionBridge bridge = null,
            TurnOrderDefinition turnOrder = null)
        {
            boardDefinition = board;
            movePolicyDefinition = movePolicy;
            turnOrderDefinition = turnOrder;
            if (bridge != null)
                selectionBridge = bridge;
        }

        public bool RebuildBoard(out string issue)
        {
            ClearGeneratedViews();
            _boardState = null;
            _turnState = null;
            _actionQueue = null;

            if (boardDefinition == null)
                return Fail("BoardDefinition is required before building a tabletop board presenter.", out issue);

            _boardState = boardDefinition.CreateRuntimeState(out List<string> boardIssues);
            if (boardIssues != null && boardIssues.Count > 0)
                return FailBuild(string.Join(" ", boardIssues), out issue);

            IBoardMovePolicy movePolicy = null;
            if (movePolicyDefinition != null)
            {
                movePolicy = movePolicyDefinition.CreatePolicy(out List<string> policyIssues);
                if (policyIssues != null && policyIssues.Count > 0)
                    return FailBuild(string.Join(" ", policyIssues), out issue);
            }

            if (turnOrderDefinition != null)
            {
                List<string> turnIssues = turnOrderDefinition.GetValidationIssues();
                if (turnIssues != null && turnIssues.Count > 0)
                    return FailBuild(string.Join(" ", turnIssues), out issue);

                _turnState = turnOrderDefinition.CreateRuntimeState();
            }

            BoardMoveActionResolver resolver = new BoardMoveActionResolver(_boardState, _turnState, movePolicy);
            _actionQueue = new ActionQueueService(new IActionResolver[] { resolver });
            EnsureSelectionBridge();
            selectionBridge.ResolveQueuedMoveImmediately = resolveQueuedMoveImmediately;
            selectionBridge.Initialize(_boardState, _actionQueue, turnState: _turnState);

            for (int y = 0; y < _boardState.Height; y++)
            {
                for (int x = 0; x < _boardState.Width; x++)
                    CreateSpaceView(new BoardCoordinate(x, y));
            }

            foreach (BoardPieceState piece in _boardState.Pieces)
                CreatePieceView(piece);

            LastIssue = string.Empty;
            issue = string.Empty;
            return true;
        }

        public bool TrySelectCoordinate(BoardCoordinate coordinate, out string issue)
        {
            if (selectionBridge == null || !selectionBridge.IsInitialized)
                return Fail("TabletopBoardSelectionBridge must be initialized before selecting board spaces.", out issue);

            QueuedAction queuedAction = default;
            bool hadSelection = selectionBridge.HasSelection;
            bool accepted = hadSelection
                ? selectionBridge.TrySelectDestination(coordinate, out queuedAction, out issue)
                : selectionBridge.TrySelectPieceAt(coordinate, out issue);

            if (!accepted)
                return Fail(issue, out issue);

            RefreshPieceViews();
            if (hadSelection)
                AdvanceTurnAfterResolvedMove(queuedAction);

            LastIssue = string.Empty;
            issue = string.Empty;
            return true;
        }

        public Vector3 CoordinateToLocalPosition(BoardCoordinate coordinate)
        {
            return new Vector3(coordinate.X * cellSize.x, 0f, coordinate.Y * cellSize.y);
        }

        private void Start()
        {
            if (buildOnStart)
                RebuildBoard(out _);
        }

        private void EnsureSelectionBridge()
        {
            if (selectionBridge == null)
                selectionBridge = GetComponent<TabletopBoardSelectionBridge>();

            if (selectionBridge == null)
                selectionBridge = gameObject.AddComponent<TabletopBoardSelectionBridge>();
        }

        private void CreateSpaceView(BoardCoordinate coordinate)
        {
            GameObject instance = InstantiateView(spacePrefab, $"Space {coordinate.X},{coordinate.Y}");
            instance.transform.SetParent(transform, false);
            instance.transform.localPosition = CoordinateToLocalPosition(coordinate);
            TabletopBoardSpaceView view = instance.GetComponent<TabletopBoardSpaceView>();
            if (view == null)
                view = instance.AddComponent<TabletopBoardSpaceView>();

            view.Initialize(this, coordinate);
            _spaceViews.Add(view);
        }

        private void CreatePieceView(BoardPieceState piece)
        {
            GameObject instance = InstantiateView(GetPiecePrefab(piece), piece.PieceId);
            instance.transform.SetParent(transform, false);
            TabletopBoardPieceView view = instance.GetComponent<TabletopBoardPieceView>();
            if (view == null)
                view = instance.AddComponent<TabletopBoardPieceView>();

            view.Initialize(this, piece.PieceId, piece.Coordinate);
            _pieceViews.Add(view);
        }

        private GameObject InstantiateView(GameObject prefab, string fallbackName)
        {
            if (prefab != null)
                return Instantiate(prefab);

            GameObject instance = new GameObject(fallbackName);
            instance.AddComponent<BoxCollider>();
            return instance;
        }

        private GameObject GetPiecePrefab(BoardPieceState piece)
        {
            if (piece == null)
                return piecePrefab;

            if (boardDefinition != null && boardDefinition.startingPieces != null)
            {
                for (int i = 0; i < boardDefinition.startingPieces.Length; i++)
                {
                    BoardStartingPiece startingPiece = boardDefinition.startingPieces[i];
                    if (startingPiece.pieceInstanceId == piece.PieceId
                        && startingPiece.pieceDefinition != null
                        && startingPiece.pieceDefinition.visualPrefab != null)
                        return startingPiece.pieceDefinition.visualPrefab;
                }
            }

            return piecePrefab;
        }

        private void AdvanceTurnAfterResolvedMove(QueuedAction queuedAction)
        {
            if (!advanceTurnAfterResolvedMove || !resolveQueuedMoveImmediately || _turnState == null)
                return;

            if (_actionQueue == null)
                return;

            for (int i = 0; i < _actionQueue.PendingActions.Count; i++)
            {
                if (_actionQueue.PendingActions[i].QueueId == queuedAction.QueueId)
                    return;
            }

            if (!_turnState.TryAdvance(out string issue))
                LastIssue = issue;
        }

        private void RefreshPieceViews()
        {
            for (int i = _pieceViews.Count - 1; i >= 0; i--)
            {
                TabletopBoardPieceView view = _pieceViews[i];
                if (view == null)
                {
                    _pieceViews.RemoveAt(i);
                    continue;
                }

                if (_boardState != null && _boardState.TryGetPiece(view.PieceId, out BoardPieceState piece) && !piece.IsCaptured)
                    view.SetCoordinate(piece.Coordinate);
                else
                    view.gameObject.SetActive(false);
            }
        }

        private void ClearGeneratedViews()
        {
            DestroyViews(_pieceViews);
            DestroyViews(_spaceViews);
        }

        private static void DestroyViews<T>(List<T> views) where T : Component
        {
            for (int i = views.Count - 1; i >= 0; i--)
            {
                if (views[i] == null)
                    continue;

                GameObject viewObject = views[i].gameObject;
                if (Application.isPlaying)
                    Destroy(viewObject);
                else
                    DestroyImmediate(viewObject);
            }

            views.Clear();
        }

        private bool Fail(string message, out string issue)
        {
            issue = message ?? string.Empty;
            LastIssue = issue;
            return false;
        }

        private bool FailBuild(string message, out string issue)
        {
            ClearGeneratedViews();
            _boardState = null;
            _actionQueue = null;
            if (selectionBridge != null)
                selectionBridge.ClearSelection();

            return Fail(message, out issue);
        }
    }
}
