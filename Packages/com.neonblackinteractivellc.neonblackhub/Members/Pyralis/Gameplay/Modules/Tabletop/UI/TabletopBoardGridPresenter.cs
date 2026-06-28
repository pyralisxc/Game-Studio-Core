using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Data.Tabletop;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions.Rules;
using NeonBlack.Gameplay.Modules.Tabletop.Runtime;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Tabletop
{
    /// <summary>
    /// Scene-facing presenter that turns an authored board definition into selectable board objects.
    /// </summary>
    [AuthoringContract(
        StableId = "proof.board-card-action",
        Category = "Tabletop, Grid",
        CapabilityPath = "Tabletop/Board/Tabletop Board Grid Presenter",
        Surface = AuthoringSurface.Goal,
        Summary = "Inspector Add Component path for a board presenter that can build selectable tabletop spaces.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/tabletop",
        RequiredFields = new[] { nameof(boardDefinition), nameof(movePolicyDefinition), nameof(turnOrderDefinition), nameof(selectionBridge), nameof(spacePrefab), nameof(piecePrefab) },
        SetupSteps = new[] { "Add TabletopBoardGridPresenter and TabletopBoardSelectionBridge to the same scene object.", "Assign Board, Move Policy, Turn Order, and Selection Bridge references.", "Assign Space and Piece prefabs." },
        SuccessChecks = new[] { "Click 'Rebuild Board' in the inspector and verify the grid is generated." },
        Tags = new[] { "capability:Tabletop", "capability:Grid" }
    )]
    [RequireComponent(typeof(TabletopBoardSelectionBridge))]
    [AddComponentMenu("NeonBlack/Gameplay/Tabletop/Tabletop Board Grid Presenter")]
    public sealed class TabletopBoardGridPresenter : MonoBehaviour, IRuntimeValidationProvider
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

        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (boardDefinition == null)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "BoardDefinition is required before building a tabletop board presenter.",
                    nameof(boardDefinition),
                    nameof(TabletopBoardGridPresenter),
                    "Assign TabletopBoardGridPresenter.boardDefinition to the BoardDefinition for this board route.",
                    "TabletopBoardGridPresenter can create a board runtime state.",
                    "TabletopBoardGridPresenter.BoardDefinition.Missing");
            }

            if (selectionBridge == null && GetComponent<TabletopBoardSelectionBridge>() == null)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "TabletopBoardSelectionBridge is required before board spaces can be selected.",
                    nameof(selectionBridge),
                    nameof(TabletopBoardGridPresenter),
                    "Add TabletopBoardSelectionBridge as a sibling component or assign it to TabletopBoardGridPresenter.selectionBridge.",
                    "TabletopBoardGridPresenter can initialize the board selection bridge.",
                    "TabletopBoardGridPresenter.SelectionBridge.Missing");
            }

            if (cellSize.x <= 0f || cellSize.y <= 0f)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Cell Size must be greater than zero on both axes.",
                    nameof(cellSize),
                    nameof(TabletopBoardGridPresenter),
                    "Set TabletopBoardGridPresenter.cellSize X and Y above zero.",
                    "Board spaces can be positioned at visible grid intervals.",
                    "TabletopBoardGridPresenter.CellSize.Minimum");
            }

            if (spacePrefab == null)
            {
                yield return PyralisRuntimeValidationIssue.Recommended(
                    "Space Prefab is empty. Runtime can create plain fallback spaces, but authored board art should use a prefab with TabletopBoardSpaceView.",
                    nameof(spacePrefab),
                    nameof(TabletopBoardGridPresenter),
                    "Assign a board-space prefab to TabletopBoardGridPresenter.spacePrefab when the proof needs visible authored board spaces.",
                    "Board spaces use authored project visuals.",
                    "TabletopBoardGridPresenter.SpacePrefab.Missing");
            }

            if (piecePrefab == null)
            {
                yield return PyralisRuntimeValidationIssue.Recommended(
                    "Piece Prefab is empty. Runtime can use piece-specific BoardPieceDefinition visual prefabs or create fallback pieces.",
                    nameof(piecePrefab),
                    nameof(TabletopBoardGridPresenter),
                    "Assign a fallback piece prefab or assign visual prefabs on BoardPieceDefinition assets.",
                    "Board pieces use authored project visuals.",
                    "TabletopBoardGridPresenter.PiecePrefab.Missing");
            }
        }

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
            if (!TryResolveSelectionBridge(out issue))
                return false;

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

        private bool TryResolveSelectionBridge(out string issue)
        {
            if (selectionBridge == null)
                selectionBridge = GetComponent<TabletopBoardSelectionBridge>();

            if (selectionBridge == null)
                return Fail("Assign TabletopBoardSelectionBridge or add it as a sibling component before rebuilding the board.", out issue);

            issue = string.Empty;
            return true;
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
