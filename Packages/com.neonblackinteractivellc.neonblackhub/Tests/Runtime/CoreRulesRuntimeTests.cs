using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Core.Rules.Board;
using NeonBlack.Gameplay.Core.Rules.TurnPhase;
using NeonBlack.Gameplay.Data.Definitions.Rules;
using NeonBlack.Gameplay.Features.Tabletop;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public class CoreRulesRuntimeTests
    {
        [Test]
        public void BoardRuntimeState_MovePiece_UpdatesOccupancy()
        {
            BoardRuntimeState state = BoardRuntimeState.CreateRectangular(2, 2);
            BoardPieceState piece = new BoardPieceState("piece.p1", "pawn", ownerSeat: 0, new BoardCoordinate(0, 0));
            Assert.That(state.TryAddPiece(piece, out string addIssue), Is.True, addIssue);

            Assert.That(state.TryMovePiece("piece.p1", new BoardCoordinate(1, 0), out string moveIssue), Is.True, moveIssue);

            Assert.That(state.TryGetPieceAt(new BoardCoordinate(0, 0), out _), Is.False);
            Assert.That(state.TryGetPieceAt(new BoardCoordinate(1, 0), out BoardPieceState moved), Is.True);
            Assert.That(moved.PieceId, Is.EqualTo("piece.p1"));
        }

        [Test]
        public void BoardRuntimeState_CapturePiece_RemovesCapturedPieceFromOccupancy()
        {
            BoardRuntimeState state = BoardRuntimeState.CreateRectangular(2, 1);
            Assert.That(state.TryAddPiece(new BoardPieceState("piece.a", "pawn", 0, new BoardCoordinate(0, 0)), out _), Is.True);
            Assert.That(state.TryAddPiece(new BoardPieceState("piece.b", "pawn", 1, new BoardCoordinate(1, 0)), out _), Is.True);

            Assert.That(state.TryCapturePiece("piece.b", out string issue), Is.True, issue);

            Assert.That(state.TryGetPiece("piece.b", out BoardPieceState captured), Is.True);
            Assert.That(captured.IsCaptured, Is.True);
            Assert.That(state.TryGetPieceAt(new BoardCoordinate(1, 0), out _), Is.False);
        }

        [Test]
        public void TurnRuntimeState_AdvanceTurn_UpdatesSeatAndRound()
        {
            TurnRuntimeState state = new TurnRuntimeState(new[] { 0, 1 }, startingSeat: 0);

            Assert.That(state.ActiveSeat, Is.EqualTo(0));
            Assert.That(state.RoundIndex, Is.EqualTo(1));

            Assert.That(state.TryAdvance(out string firstIssue), Is.True, firstIssue);
            Assert.That(state.ActiveSeat, Is.EqualTo(1));
            Assert.That(state.RoundIndex, Is.EqualTo(1));

            Assert.That(state.TryAdvance(out string secondIssue), Is.True, secondIssue);
            Assert.That(state.ActiveSeat, Is.EqualTo(0));
            Assert.That(state.RoundIndex, Is.EqualTo(2));
        }

        [Test]
        public void ActionQueueService_EnqueueAndResolve_ProcessesActionsInOrder()
        {
            RecordingActionResolver resolver = new RecordingActionResolver();
            ActionQueueService queue = new ActionQueueService(new IActionResolver[] { resolver });

            Assert.That(queue.TryEnqueue(new ActionExecutionContext("action.first"), out QueuedAction first, out string firstIssue), Is.True, firstIssue);
            Assert.That(queue.TryEnqueue(new ActionExecutionContext("action.second"), out QueuedAction second, out string secondIssue), Is.True, secondIssue);

            ActionResolutionResult firstResult = queue.ResolveNext();
            ActionResolutionResult secondResult = queue.ResolveNext();

            Assert.That(firstResult.Succeeded, Is.True);
            Assert.That(secondResult.Succeeded, Is.True);
            Assert.That(resolver.ResolvedActionIds, Is.EqualTo(new[] { "action.first", "action.second" }));
            Assert.That(first.SequenceId, Is.LessThan(second.SequenceId));
            Assert.That(queue.PendingCount, Is.EqualTo(0));
        }

        [Test]
        public void ActionQueueService_Enqueue_RejectedWhenValidationFails()
        {
            RecordingActionResolver resolver = new RecordingActionResolver(
                "action.blocked",
                ActionValidationResult.Failure("blocked"));
            ActionQueueService queue = new ActionQueueService(new IActionResolver[] { resolver });

            bool accepted = queue.TryEnqueue(new ActionExecutionContext("action.blocked"), out _, out string issue);

            Assert.That(accepted, Is.False);
            Assert.That(issue, Does.Contain("blocked"));
            Assert.That(queue.PendingCount, Is.EqualTo(0));
        }

        [Test]
        public void ActionQueueService_Cancel_RemovesPendingAction()
        {
            RecordingActionResolver resolver = new RecordingActionResolver();
            ActionQueueService queue = new ActionQueueService(new IActionResolver[] { resolver });
            Assert.That(queue.TryEnqueue(new ActionExecutionContext("action.cancel"), out QueuedAction queued, out _), Is.True);

            Assert.That(queue.TryCancel(queued.QueueId, out string issue), Is.True, issue);

            Assert.That(queue.PendingCount, Is.EqualTo(0));
            Assert.That(queue.ResolveNext().Status, Is.EqualTo(ActionResolutionStatus.Pending));
        }

        [Test]
        public void BoardMoveActionResolver_ResolveAction_MovesPieceThroughQueue()
        {
            BoardRuntimeState state = BoardRuntimeState.CreateRectangular(2, 1);
            Assert.That(state.TryAddPiece(new BoardPieceState("piece.a", "pawn", 0, new BoardCoordinate(0, 0)), out _), Is.True);

            BoardMoveActionResolver resolver = new BoardMoveActionResolver(state);
            ActionQueueService queue = new ActionQueueService(new IActionResolver[] { resolver });
            BoardMoveActionPayload payload = new BoardMoveActionPayload("piece.a", new BoardCoordinate(1, 0));

            Assert.That(queue.TryEnqueue(new ActionExecutionContext(BoardMoveActionResolver.DefaultActionId, customPayload: payload), out _, out string issue), Is.True, issue);

            ActionResolutionResult result = queue.ResolveNext();

            Assert.That(result.Succeeded, Is.True, result.Message);
            Assert.That(state.TryGetPieceAt(new BoardCoordinate(0, 0), out _), Is.False);
            Assert.That(state.TryGetPieceAt(new BoardCoordinate(1, 0), out BoardPieceState moved), Is.True);
            Assert.That(moved.PieceId, Is.EqualTo("piece.a"));
        }

        [Test]
        public void BoardMoveActionResolver_ValidateAction_RejectsInactiveSeat()
        {
            BoardRuntimeState boardState = BoardRuntimeState.CreateRectangular(2, 1);
            Assert.That(boardState.TryAddPiece(new BoardPieceState("piece.a", "pawn", 1, new BoardCoordinate(0, 0)), out _), Is.True);
            TurnRuntimeState turnState = new TurnRuntimeState(new[] { 0, 1 }, startingSeat: 0);
            BoardMoveActionResolver resolver = new BoardMoveActionResolver(boardState, turnState);
            BoardMoveActionPayload payload = new BoardMoveActionPayload("piece.a", new BoardCoordinate(1, 0));

            ActionValidationResult validation = resolver.ValidateAction(new ActionExecutionContext(BoardMoveActionResolver.DefaultActionId, customPayload: payload));

            Assert.That(validation.IsValid, Is.False);
            Assert.That(validation.Message, Does.Contain("active seat"));
        }

        [Test]
        public void BoardMoveActionResolver_ValidateAction_RejectsMoveOutsidePolicy()
        {
            BoardRuntimeState boardState = BoardRuntimeState.CreateRectangular(3, 3);
            Assert.That(boardState.TryAddPiece(new BoardPieceState("piece.a", "pawn", 0, new BoardCoordinate(1, 1)), out _), Is.True);
            BoardMovePolicy policy = new BoardMovePolicy(
                "policy.orthogonal",
                BoardMoveShape.Orthogonal,
                maxDistance: 1,
                allowCapture: false);
            BoardMoveActionResolver resolver = new BoardMoveActionResolver(boardState, movePolicy: policy);
            BoardMoveActionPayload payload = new BoardMoveActionPayload("piece.a", new BoardCoordinate(2, 2));

            ActionValidationResult validation = resolver.ValidateAction(new ActionExecutionContext(BoardMoveActionResolver.DefaultActionId, customPayload: payload));

            Assert.That(validation.IsValid, Is.False);
            Assert.That(validation.Message, Does.Contain("policy.orthogonal"));
        }

        [Test]
        public void BoardMoveActionResolver_ResolveAction_AllowsMoveInsidePolicy()
        {
            BoardRuntimeState boardState = BoardRuntimeState.CreateRectangular(3, 3);
            Assert.That(boardState.TryAddPiece(new BoardPieceState("piece.a", "pawn", 0, new BoardCoordinate(1, 1)), out _), Is.True);
            BoardMovePolicy policy = new BoardMovePolicy(
                "policy.orthogonal",
                BoardMoveShape.Orthogonal,
                maxDistance: 1,
                allowCapture: false);
            BoardMoveActionResolver resolver = new BoardMoveActionResolver(boardState, movePolicy: policy);
            BoardMoveActionPayload payload = new BoardMoveActionPayload("piece.a", new BoardCoordinate(2, 1));

            ActionResolutionResult result = resolver.ResolveAction(new ActionExecutionContext(BoardMoveActionResolver.DefaultActionId, customPayload: payload));

            Assert.That(result.Succeeded, Is.True, result.Message);
            Assert.That(boardState.TryGetPieceAt(new BoardCoordinate(2, 1), out BoardPieceState moved), Is.True);
            Assert.That(moved.PieceId, Is.EqualTo("piece.a"));
        }

        [Test]
        public void BoardMoveActionResolver_ResolveAction_CapturesOpponentWhenPolicyAllowsCapture()
        {
            BoardRuntimeState boardState = BoardRuntimeState.CreateRectangular(4, 1);
            Assert.That(boardState.TryAddPiece(new BoardPieceState("piece.a", "pawn", 0, new BoardCoordinate(0, 0)), out _), Is.True);
            Assert.That(boardState.TryAddPiece(new BoardPieceState("piece.b", "pawn", 1, new BoardCoordinate(1, 0)), out _), Is.True);
            BoardMovePolicy policy = new BoardMovePolicy(
                "policy.capture-step",
                BoardMoveShape.Orthogonal,
                maxDistance: 1,
                allowCapture: true);
            BoardMoveActionResolver resolver = new BoardMoveActionResolver(boardState, movePolicy: policy);
            BoardMoveActionPayload payload = new BoardMoveActionPayload("piece.a", new BoardCoordinate(1, 0));

            ActionResolutionResult result = resolver.ResolveAction(new ActionExecutionContext(BoardMoveActionResolver.DefaultActionId, customPayload: payload));

            Assert.That(result.Succeeded, Is.True, result.Message);
            Assert.That(boardState.TryGetPiece("piece.b", out BoardPieceState captured), Is.True);
            Assert.That(captured.IsCaptured, Is.True);
            Assert.That(boardState.TryGetPieceAt(new BoardCoordinate(1, 0), out BoardPieceState moved), Is.True);
            Assert.That(moved.PieceId, Is.EqualTo("piece.a"));
        }

        [Test]
        public void TabletopBoardSelectionBridge_SelectPieceThenDestination_EnqueuesBoardMove()
        {
            BoardRuntimeState boardState = BoardRuntimeState.CreateRectangular(2, 1);
            Assert.That(boardState.TryAddPiece(new BoardPieceState("piece.a", "pawn", 0, new BoardCoordinate(0, 0)), out _), Is.True);
            BoardMoveActionResolver resolver = new BoardMoveActionResolver(boardState);
            ActionQueueService queue = new ActionQueueService(new IActionResolver[] { resolver });
            UnityEngine.GameObject bridgeObject = new UnityEngine.GameObject("Tabletop Selection Bridge");
            TabletopBoardSelectionBridge bridge = bridgeObject.AddComponent<TabletopBoardSelectionBridge>();
            bridge.Initialize(boardState, queue);

            Assert.That(bridge.TrySelectPieceAt(new BoardCoordinate(0, 0), out string selectIssue), Is.True, selectIssue);
            Assert.That(bridge.TrySelectDestination(new BoardCoordinate(1, 0), out QueuedAction queuedAction, out string moveIssue), Is.True, moveIssue);

            Assert.That(queuedAction.ActionId, Is.EqualTo(BoardMoveActionResolver.DefaultActionId));
            Assert.That(queue.PendingCount, Is.EqualTo(1));
            Assert.That(bridge.HasSelection, Is.False);
            Assert.That(queue.ResolveNext().Succeeded, Is.True);
            Assert.That(boardState.TryGetPieceAt(new BoardCoordinate(1, 0), out BoardPieceState moved), Is.True);
            Assert.That(moved.PieceId, Is.EqualTo("piece.a"));

            UnityEngine.Object.DestroyImmediate(bridgeObject);
        }

        [Test]
        public void TabletopBoardSelectionBridge_SelectDestinationWithoutPiece_ReportsActionableIssue()
        {
            BoardRuntimeState boardState = BoardRuntimeState.CreateRectangular(2, 1);
            ActionQueueService queue = new ActionQueueService();
            UnityEngine.GameObject bridgeObject = new UnityEngine.GameObject("Tabletop Selection Bridge");
            TabletopBoardSelectionBridge bridge = bridgeObject.AddComponent<TabletopBoardSelectionBridge>();
            bridge.Initialize(boardState, queue);

            bool accepted = bridge.TrySelectDestination(new BoardCoordinate(1, 0), out _, out string issue);

            Assert.That(accepted, Is.False);
            Assert.That(issue, Does.Contain("Select a board piece"));

            UnityEngine.Object.DestroyImmediate(bridgeObject);
        }

        [Test]
        public void TabletopBoardSelectionBridge_ResolveImmediately_DoesNotResolveOlderQueuedAction()
        {
            BoardRuntimeState boardState = BoardRuntimeState.CreateRectangular(4, 1);
            Assert.That(boardState.TryAddPiece(new BoardPieceState("piece.a", "pawn", 0, new BoardCoordinate(0, 0)), out _), Is.True);
            Assert.That(boardState.TryAddPiece(new BoardPieceState("piece.b", "pawn", 0, new BoardCoordinate(1, 0)), out _), Is.True);
            BoardMoveActionResolver resolver = new BoardMoveActionResolver(boardState);
            ActionQueueService queue = new ActionQueueService(new IActionResolver[] { resolver });
            Assert.That(queue.TryEnqueue(
                new ActionExecutionContext(
                    BoardMoveActionResolver.DefaultActionId,
                    customPayload: new BoardMoveActionPayload("piece.a", new BoardCoordinate(2, 0))),
                out _,
                out string firstIssue), Is.True, firstIssue);
            UnityEngine.GameObject bridgeObject = new UnityEngine.GameObject("Tabletop Selection Bridge");
            TabletopBoardSelectionBridge bridge = bridgeObject.AddComponent<TabletopBoardSelectionBridge>();
            bridge.ResolveQueuedMoveImmediately = true;
            bridge.Initialize(boardState, queue);

            Assert.That(bridge.TrySelectPiece("piece.b", out string selectIssue), Is.True, selectIssue);
            Assert.That(bridge.TrySelectDestination(new BoardCoordinate(3, 0), out _, out string moveIssue), Is.True, moveIssue);

            Assert.That(queue.PendingCount, Is.EqualTo(2));
            Assert.That(bridge.LastIssue, Does.Contain("waiting behind existing actions"));
            Assert.That(boardState.TryGetPieceAt(new BoardCoordinate(0, 0), out BoardPieceState original), Is.True);
            Assert.That(original.PieceId, Is.EqualTo("piece.a"));

            UnityEngine.Object.DestroyImmediate(bridgeObject);
        }

        [Test]
        public void TabletopBoardGridPresenter_RebuildBoard_CreatesSelectableSceneViews()
        {
            BoardPieceDefinition pieceDefinition = ScriptableObject.CreateInstance<BoardPieceDefinition>();
            pieceDefinition.pieceId = "piece.pawn";
            BoardDefinition boardDefinition = ScriptableObject.CreateInstance<BoardDefinition>();
            boardDefinition.width = 2;
            boardDefinition.height = 1;
            boardDefinition.startingPieces = new[]
            {
                new BoardStartingPiece("piece.a", pieceDefinition, 0, new BoardCoordinate(0, 0))
            };
            GameObject presenterObject = new GameObject("Tabletop Board Presenter");
            TabletopBoardGridPresenter presenter = presenterObject.AddComponent<TabletopBoardGridPresenter>();
            presenter.ResolveQueuedMoveImmediately = true;
            presenter.Configure(boardDefinition);

            Assert.That(presenter.RebuildBoard(out string buildIssue), Is.True, buildIssue);

            Assert.That(presenter.SpaceViews.Count, Is.EqualTo(2));
            Assert.That(presenter.PieceViews.Count, Is.EqualTo(1));
            Assert.That(presenter.SelectionBridge, Is.Not.Null);
            Assert.That(presenter.TrySelectCoordinate(new BoardCoordinate(0, 0), out string selectIssue), Is.True, selectIssue);
            Assert.That(presenter.SelectionBridge.HasSelection, Is.True);

            Assert.That(presenter.TrySelectCoordinate(new BoardCoordinate(1, 0), out string moveIssue), Is.True, moveIssue);

            Assert.That(presenter.BoardState.TryGetPieceAt(new BoardCoordinate(1, 0), out BoardPieceState moved), Is.True);
            Assert.That(moved.PieceId, Is.EqualTo("piece.a"));
            Assert.That(presenter.PieceViews[0].Coordinate, Is.EqualTo(new BoardCoordinate(1, 0)));

            Object.DestroyImmediate(presenterObject);
            Object.DestroyImmediate(boardDefinition);
            Object.DestroyImmediate(pieceDefinition);
        }

        [Test]
        public void TabletopBoardGridPresenter_RebuildBoard_UsesTurnOrderAndAdvancesAfterResolvedMove()
        {
            BoardPieceDefinition whitePiece = ScriptableObject.CreateInstance<BoardPieceDefinition>();
            whitePiece.pieceId = "piece.white";
            BoardPieceDefinition blackPiece = ScriptableObject.CreateInstance<BoardPieceDefinition>();
            blackPiece.pieceId = "piece.black";
            BoardDefinition boardDefinition = ScriptableObject.CreateInstance<BoardDefinition>();
            boardDefinition.width = 3;
            boardDefinition.height = 1;
            boardDefinition.startingPieces = new[]
            {
                new BoardStartingPiece("piece.white.a", whitePiece, 0, new BoardCoordinate(0, 0)),
                new BoardStartingPiece("piece.black.a", blackPiece, 1, new BoardCoordinate(2, 0))
            };
            TurnOrderDefinition turnOrderDefinition = ScriptableObject.CreateInstance<TurnOrderDefinition>();
            turnOrderDefinition.participantSeats = new[] { 0, 1 };
            GameObject presenterObject = new GameObject("Tabletop Board Presenter");
            TabletopBoardGridPresenter presenter = presenterObject.AddComponent<TabletopBoardGridPresenter>();
            presenter.ResolveQueuedMoveImmediately = true;
            presenter.Configure(boardDefinition, turnOrder: turnOrderDefinition);

            Assert.That(presenter.RebuildBoard(out string buildIssue), Is.True, buildIssue);
            Assert.That(presenter.TurnState.ActiveSeat, Is.EqualTo(0));

            Assert.That(presenter.TrySelectCoordinate(new BoardCoordinate(2, 0), out string blackSelectIssue), Is.False);
            Assert.That(blackSelectIssue, Does.Contain("active seat"));
            Assert.That(presenter.TurnState.ActiveSeat, Is.EqualTo(0));

            Assert.That(presenter.TrySelectCoordinate(new BoardCoordinate(0, 0), out string whiteSelectIssue), Is.True, whiteSelectIssue);
            Assert.That(presenter.TrySelectCoordinate(new BoardCoordinate(1, 0), out string whiteMoveIssue), Is.True, whiteMoveIssue);

            Assert.That(presenter.TurnState.ActiveSeat, Is.EqualTo(1));
            Assert.That(presenter.BoardState.TryGetPieceAt(new BoardCoordinate(1, 0), out BoardPieceState moved), Is.True);
            Assert.That(moved.PieceId, Is.EqualTo("piece.white.a"));

            Object.DestroyImmediate(presenterObject);
            Object.DestroyImmediate(turnOrderDefinition);
            Object.DestroyImmediate(boardDefinition);
            Object.DestroyImmediate(blackPiece);
            Object.DestroyImmediate(whitePiece);
        }

        [Test]
        public void TabletopBoardGridPresenter_RebuildBoard_UsesBoardPieceVisualPrefab()
        {
            GameObject visualPrefab = new GameObject("Token Visual");
            BoardPieceDefinition pieceDefinition = ScriptableObject.CreateInstance<BoardPieceDefinition>();
            pieceDefinition.pieceId = "piece.visual";
            pieceDefinition.visualPrefab = visualPrefab;
            BoardDefinition boardDefinition = ScriptableObject.CreateInstance<BoardDefinition>();
            boardDefinition.width = 1;
            boardDefinition.height = 1;
            boardDefinition.startingPieces = new[]
            {
                new BoardStartingPiece("piece.visual.a", pieceDefinition, 0, new BoardCoordinate(0, 0))
            };
            GameObject presenterObject = new GameObject("Tabletop Board Presenter");
            TabletopBoardGridPresenter presenter = presenterObject.AddComponent<TabletopBoardGridPresenter>();
            presenter.Configure(boardDefinition);

            Assert.That(presenter.RebuildBoard(out string buildIssue), Is.True, buildIssue);

            Assert.That(presenter.PieceViews.Count, Is.EqualTo(1));
            Assert.That(presenter.PieceViews[0].gameObject.name, Does.StartWith("Token Visual"));

            Object.DestroyImmediate(presenterObject);
            Object.DestroyImmediate(boardDefinition);
            Object.DestroyImmediate(pieceDefinition);
            Object.DestroyImmediate(visualPrefab);
        }

        [Test]
        public void TabletopBoardPieceView_Select_ForwardsCurrentCoordinateToPresenter()
        {
            BoardPieceDefinition pieceDefinition = ScriptableObject.CreateInstance<BoardPieceDefinition>();
            pieceDefinition.pieceId = "piece.pawn";
            BoardDefinition boardDefinition = ScriptableObject.CreateInstance<BoardDefinition>();
            boardDefinition.width = 2;
            boardDefinition.height = 1;
            boardDefinition.startingPieces = new[]
            {
                new BoardStartingPiece("piece.a", pieceDefinition, 0, new BoardCoordinate(0, 0))
            };
            GameObject presenterObject = new GameObject("Tabletop Board Presenter");
            TabletopBoardGridPresenter presenter = presenterObject.AddComponent<TabletopBoardGridPresenter>();
            presenter.Configure(boardDefinition);
            Assert.That(presenter.RebuildBoard(out string buildIssue), Is.True, buildIssue);

            Assert.That(presenter.PieceViews[0].Select(out string selectIssue), Is.True, selectIssue);

            Assert.That(presenter.SelectionBridge.SelectedPieceId, Is.EqualTo("piece.a"));

            Object.DestroyImmediate(presenterObject);
            Object.DestroyImmediate(boardDefinition);
            Object.DestroyImmediate(pieceDefinition);
        }

        [Test]
        public void TabletopBoardGridPresenter_RebuildBoardWithoutDefinition_ReportsActionableIssue()
        {
            GameObject presenterObject = new GameObject("Tabletop Board Presenter");
            TabletopBoardGridPresenter presenter = presenterObject.AddComponent<TabletopBoardGridPresenter>();

            bool built = presenter.RebuildBoard(out string issue);

            Assert.That(built, Is.False);
            Assert.That(issue, Does.Contain("BoardDefinition"));

            Object.DestroyImmediate(presenterObject);
        }

        [Test]
        public void TabletopBoardGridPresenter_RebuildBoardWithInvalidDefinition_DoesNotLeavePartialRuntimeState()
        {
            BoardPieceDefinition pieceDefinition = ScriptableObject.CreateInstance<BoardPieceDefinition>();
            pieceDefinition.pieceId = "piece.pawn";
            BoardDefinition boardDefinition = ScriptableObject.CreateInstance<BoardDefinition>();
            boardDefinition.width = 2;
            boardDefinition.height = 1;
            boardDefinition.startingPieces = new[]
            {
                new BoardStartingPiece("piece.a", pieceDefinition, 0, new BoardCoordinate(0, 0)),
                new BoardStartingPiece("piece.b", pieceDefinition, 1, new BoardCoordinate(0, 0))
            };
            GameObject presenterObject = new GameObject("Tabletop Board Presenter");
            TabletopBoardGridPresenter presenter = presenterObject.AddComponent<TabletopBoardGridPresenter>();
            presenter.Configure(boardDefinition);

            bool built = presenter.RebuildBoard(out string issue);

            Assert.That(built, Is.False);
            Assert.That(issue, Does.Contain("occupied"));
            Assert.That(presenter.BoardState, Is.Null);
            Assert.That(presenter.SpaceViews.Count, Is.EqualTo(0));
            Assert.That(presenter.PieceViews.Count, Is.EqualTo(0));

            Object.DestroyImmediate(presenterObject);
            Object.DestroyImmediate(boardDefinition);
            Object.DestroyImmediate(pieceDefinition);
        }

        [Test]
        public void BoardMovePolicy_IsMoveAllowed_AllowsKnightStyleOffset()
        {
            BoardRuntimeState boardState = BoardRuntimeState.CreateRectangular(4, 4);
            BoardPieceState piece = new BoardPieceState("piece.knight", "knight", 0, new BoardCoordinate(1, 1));
            Assert.That(boardState.TryAddPiece(piece, out _), Is.True);
            BoardMovePolicy policy = new BoardMovePolicy(
                "policy.knight",
                BoardMoveShape.Offset,
                maxDistance: 1,
                allowCapture: true,
                allowedOffsets: new[] { new BoardCoordinate(1, 2), new BoardCoordinate(2, 1) });

            bool allowed = policy.IsMoveAllowed(
                new BoardMovePolicyContext(boardState, piece, new BoardCoordinate(2, 3)),
                out string issue);

            Assert.That(allowed, Is.True, issue);
        }

        [Test]
        public void BoardMovePolicy_IsMoveAllowed_RejectsMoveOutsideOffsets()
        {
            BoardRuntimeState boardState = BoardRuntimeState.CreateRectangular(4, 4);
            BoardPieceState piece = new BoardPieceState("piece.knight", "knight", 0, new BoardCoordinate(1, 1));
            Assert.That(boardState.TryAddPiece(piece, out _), Is.True);
            BoardMovePolicy policy = new BoardMovePolicy(
                "policy.knight",
                BoardMoveShape.Offset,
                maxDistance: 1,
                allowCapture: true,
                allowedOffsets: new[] { new BoardCoordinate(1, 2), new BoardCoordinate(2, 1) });

            bool allowed = policy.IsMoveAllowed(
                new BoardMovePolicyContext(boardState, piece, new BoardCoordinate(2, 2)),
                out string issue);

            Assert.That(allowed, Is.False);
            Assert.That(issue, Does.Contain("policy.knight"));
        }

        [Test]
        public void BoardTerminalCondition_Evaluate_SideEliminatedReturnsWinner()
        {
            BoardRuntimeState boardState = BoardRuntimeState.CreateRectangular(2, 1);
            Assert.That(boardState.TryAddPiece(new BoardPieceState("piece.a", "pawn", 0, new BoardCoordinate(0, 0)), out _), Is.True);
            Assert.That(boardState.TryAddPiece(new BoardPieceState("piece.b", "pawn", 1, new BoardCoordinate(1, 0)), out _), Is.True);
            Assert.That(boardState.TryCapturePiece("piece.b", out _), Is.True);
            BoardTerminalCondition condition = new BoardTerminalCondition(
                "condition.side-eliminated",
                BoardTerminalConditionKind.SideEliminated,
                observedSeat: 1,
                winningSeat: 0);

            BoardTerminalEvaluationResult result = condition.Evaluate(boardState);

            Assert.That(result.IsTerminal, Is.True);
            Assert.That(result.WinningSeat, Is.EqualTo(0));
            Assert.That(result.Message, Does.Contain("condition.side-eliminated"));
        }

        [Test]
        public void BoardTerminalCondition_Evaluate_ObjectiveOccupiedReturnsOccupyingSeat()
        {
            BoardRuntimeState boardState = BoardRuntimeState.CreateRectangular(3, 1);
            Assert.That(boardState.TryAddPiece(new BoardPieceState("piece.a", "pawn", 1, new BoardCoordinate(2, 0)), out _), Is.True);
            BoardTerminalCondition condition = new BoardTerminalCondition(
                "condition.objective",
                BoardTerminalConditionKind.ObjectiveOccupied,
                observedSeat: -1,
                winningSeat: -1,
                objectiveCoordinate: new BoardCoordinate(2, 0));

            BoardTerminalEvaluationResult result = condition.Evaluate(boardState);

            Assert.That(result.IsTerminal, Is.True);
            Assert.That(result.WinningSeat, Is.EqualTo(1));
        }

        [Test]
        public void TurnAdvanceActionResolver_ResolveAction_AdvancesTurnThroughQueue()
        {
            TurnRuntimeState turnState = new TurnRuntimeState(new[] { 0, 1 }, startingSeat: 0);
            TurnAdvanceActionResolver resolver = new TurnAdvanceActionResolver(turnState);
            ActionQueueService queue = new ActionQueueService(new IActionResolver[] { resolver });

            Assert.That(queue.TryEnqueue(new ActionExecutionContext(TurnAdvanceActionResolver.DefaultActionId), out _, out string issue), Is.True, issue);

            ActionResolutionResult result = queue.ResolveNext();

            Assert.That(result.Succeeded, Is.True, result.Message);
            Assert.That(turnState.ActiveSeat, Is.EqualTo(1));
        }

        private sealed class RecordingActionResolver : IActionResolver
        {
            private readonly string _invalidActionId;
            private readonly ActionValidationResult _invalidResult;

            public RecordingActionResolver(
                string invalidActionId = null,
                ActionValidationResult invalidResult = default)
            {
                _invalidActionId = invalidActionId;
                _invalidResult = invalidResult;
                ResolvedActionIds = new List<string>();
            }

            public List<string> ResolvedActionIds { get; }

            public bool CanResolve(ActionExecutionContext context)
            {
                return context != null;
            }

            public ActionValidationResult ValidateAction(ActionExecutionContext context)
            {
                if (context != null && context.ActionId == _invalidActionId)
                    return _invalidResult;

                return ActionValidationResult.Success();
            }

            public ActionResolutionResult ResolveAction(ActionExecutionContext context)
            {
                ResolvedActionIds.Add(context.ActionId);
                return ActionResolutionResult.Success(context.ActionId);
            }
        }
    }
}
