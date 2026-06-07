using NeonBlack.Gameplay.Core.Rules.Board;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Definitions.Rules;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public class CoreRulesDefinitionTests : PyralisEditorTestSupport
    {
        [Test]
        public void BoardDefinition_GetValidationIssues_FlagsInvalidStartingPieces()
        {
            BoardDefinition board = ScriptableObject.CreateInstance<BoardDefinition>();
            board.boardId = "board.invalid";
            board.displayName = "Invalid Board";
            board.width = 2;
            board.height = 2;
            board.startingPieces = new[]
            {
                new BoardStartingPiece("piece.a", null, 0, new BoardCoordinate(0, 0)),
                new BoardStartingPiece("piece.a", null, 1, new BoardCoordinate(3, 0))
            };

            System.Collections.Generic.List<string> issues = board.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("piece definition")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("assigned more than once")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("outside the board")), Is.True);

            Object.DestroyImmediate(board);
        }

        [Test]
        public void BoardDefinition_CreateRuntimeState_PlacesStartingPieces()
        {
            BoardPieceDefinition piece = ScriptableObject.CreateInstance<BoardPieceDefinition>();
            piece.pieceId = "piece.pawn";
            piece.displayName = "Pawn";

            BoardDefinition board = ScriptableObject.CreateInstance<BoardDefinition>();
            board.boardId = "board.test";
            board.displayName = "Test Board";
            board.width = 2;
            board.height = 1;
            board.startingPieces = new[]
            {
                new BoardStartingPiece("piece.a", piece, 0, new BoardCoordinate(0, 0)),
                new BoardStartingPiece("piece.b", piece, 1, new BoardCoordinate(1, 0))
            };

            BoardRuntimeState state = board.CreateRuntimeState(out System.Collections.Generic.List<string> issues);

            Assert.That(issues, Is.Empty);
            Assert.That(state.TryGetPieceAt(new BoardCoordinate(0, 0), out BoardPieceState first), Is.True);
            Assert.That(first.PieceDefinitionId, Is.EqualTo("piece.pawn"));
            Assert.That(state.TryGetPieceAt(new BoardCoordinate(1, 0), out BoardPieceState second), Is.True);
            Assert.That(second.OwnerSeat, Is.EqualTo(1));

            Object.DestroyImmediate(board);
            Object.DestroyImmediate(piece);
        }

        [Test]
        public void BoardMovePolicyDefinition_CreatePolicy_RejectsDiagonalForOrthogonalPolicy()
        {
            BoardMovePolicyDefinition policyDefinition = ScriptableObject.CreateInstance<BoardMovePolicyDefinition>();
            policyDefinition.policyId = "policy.orthogonal";
            policyDefinition.displayName = "Orthogonal Step";
            policyDefinition.shape = BoardMoveShape.Orthogonal;
            policyDefinition.maxDistance = 1;
            policyDefinition.allowCapture = false;

            IBoardMovePolicy policy = policyDefinition.CreatePolicy(out System.Collections.Generic.List<string> issues);
            BoardMovePolicyContext context = new BoardMovePolicyContext(
                BoardRuntimeState.CreateRectangular(3, 3),
                new BoardPieceState("piece.a", "pawn", 0, new BoardCoordinate(1, 1)),
                new BoardCoordinate(2, 2),
                destinationPiece: null);

            Assert.That(issues, Is.Empty);
            Assert.That(policy.IsMoveAllowed(context, out string issue), Is.False);
            Assert.That(issue, Does.Contain("policy.orthogonal"));

            Object.DestroyImmediate(policyDefinition);
        }

        [Test]
        public void BoardMovePolicyDefinition_GetValidationIssues_FlagsInvalidDistance()
        {
            BoardMovePolicyDefinition policyDefinition = ScriptableObject.CreateInstance<BoardMovePolicyDefinition>();
            policyDefinition.policyId = "policy.invalid";
            policyDefinition.displayName = "Invalid";
            policyDefinition.shape = BoardMoveShape.Orthogonal;
            policyDefinition.maxDistance = 0;

            System.Collections.Generic.List<string> issues = policyDefinition.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("Max distance")), Is.True);

            Object.DestroyImmediate(policyDefinition);
        }

        [Test]
        public void BoardMovePolicyDefinition_CreatePolicy_UsesAuthoredOffsets()
        {
            BoardMovePolicyDefinition policyDefinition = ScriptableObject.CreateInstance<BoardMovePolicyDefinition>();
            policyDefinition.policyId = "policy.knight";
            policyDefinition.displayName = "Knight";
            policyDefinition.shape = BoardMoveShape.Offset;
            policyDefinition.maxDistance = 1;
            policyDefinition.allowCapture = true;
            policyDefinition.allowedOffsets = new[]
            {
                new BoardMoveOffset(1, 2),
                new BoardMoveOffset(2, 1)
            };
            IBoardMovePolicy policy = policyDefinition.CreatePolicy(out System.Collections.Generic.List<string> issues);
            BoardRuntimeState boardState = BoardRuntimeState.CreateRectangular(4, 4);
            BoardPieceState piece = new BoardPieceState("piece.knight", "knight", 0, new BoardCoordinate(1, 1));
            Assert.That(boardState.TryAddPiece(piece, out _), Is.True);

            bool allowed = policy.IsMoveAllowed(
                new BoardMovePolicyContext(boardState, piece, new BoardCoordinate(3, 2)),
                out string issue);

            Assert.That(issues, Is.Empty);
            Assert.That(allowed, Is.True, issue);

            Object.DestroyImmediate(policyDefinition);
        }

        [Test]
        public void BoardMovePolicyDefinition_GetValidationIssues_FlagsMissingOffsets()
        {
            BoardMovePolicyDefinition policyDefinition = ScriptableObject.CreateInstance<BoardMovePolicyDefinition>();
            policyDefinition.policyId = "policy.offset";
            policyDefinition.displayName = "Offset";
            policyDefinition.shape = BoardMoveShape.Offset;
            policyDefinition.maxDistance = 1;
            policyDefinition.allowedOffsets = System.Array.Empty<BoardMoveOffset>();

            System.Collections.Generic.List<string> issues = policyDefinition.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("Allowed offsets")), Is.True);

            Object.DestroyImmediate(policyDefinition);
        }

        [Test]
        public void BoardTerminalConditionDefinition_CreateCondition_EvaluatesSideEliminated()
        {
            BoardTerminalConditionDefinition conditionDefinition = ScriptableObject.CreateInstance<BoardTerminalConditionDefinition>();
            conditionDefinition.conditionId = "condition.side-eliminated";
            conditionDefinition.displayName = "Side Eliminated";
            conditionDefinition.kind = BoardTerminalConditionKind.SideEliminated;
            conditionDefinition.observedSeat = 1;
            conditionDefinition.winningSeat = 0;

            IBoardTerminalCondition condition = conditionDefinition.CreateCondition(out System.Collections.Generic.List<string> issues);
            BoardRuntimeState boardState = BoardRuntimeState.CreateRectangular(2, 1);
            Assert.That(boardState.TryAddPiece(new BoardPieceState("piece.a", "pawn", 0, new BoardCoordinate(0, 0)), out _), Is.True);

            BoardTerminalEvaluationResult result = condition.Evaluate(boardState);

            Assert.That(issues, Is.Empty);
            Assert.That(result.IsTerminal, Is.True);
            Assert.That(result.WinningSeat, Is.EqualTo(0));

            Object.DestroyImmediate(conditionDefinition);
        }

        [Test]
        public void BoardTerminalConditionDefinition_GetValidationIssues_FlagsInvalidSideEliminatedSeats()
        {
            BoardTerminalConditionDefinition conditionDefinition = ScriptableObject.CreateInstance<BoardTerminalConditionDefinition>();
            conditionDefinition.conditionId = "condition.invalid";
            conditionDefinition.displayName = "Invalid";
            conditionDefinition.kind = BoardTerminalConditionKind.SideEliminated;
            conditionDefinition.observedSeat = -1;
            conditionDefinition.winningSeat = -1;

            System.Collections.Generic.List<string> issues = conditionDefinition.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("Observed seat")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("Winning seat")), Is.True);

            Object.DestroyImmediate(conditionDefinition);
        }

        [Test]
        public void TurnOrderDefinition_GetValidationIssues_FlagsMissingSeatsAndDuplicatePhases()
        {
            PhaseDefinition phase = ScriptableObject.CreateInstance<PhaseDefinition>();
            phase.phaseId = "phase.move";
            phase.displayName = "Move";

            TurnOrderDefinition definition = ScriptableObject.CreateInstance<TurnOrderDefinition>();
            definition.turnOrderId = "turn.invalid";
            definition.displayName = "Invalid Turn Order";
            definition.participantSeats = new[] { 0, 0 };
            definition.phases = new[] { phase, phase };

            System.Collections.Generic.List<string> issues = definition.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("Participant seat `0` is assigned more than once")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("Phase `phase.move` is assigned more than once")), Is.True);

            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(phase);
        }

        [Test]
        public void TurnOrderDefinition_CreateRuntimeState_UsesFirstSeat()
        {
            TurnOrderDefinition definition = ScriptableObject.CreateInstance<TurnOrderDefinition>();
            definition.turnOrderId = "turn.two-player";
            definition.displayName = "Two Player";
            definition.participantSeats = new[] { 0, 1 };

            NeonBlack.Gameplay.Core.Rules.TurnPhase.TurnRuntimeState state = definition.CreateRuntimeState();

            Assert.That(state.ActiveSeat, Is.EqualTo(0));
            Assert.That(state.SeatOrder.ToArray(), Is.EqualTo(new[] { 0, 1 }));

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void GameModeDefinition_GetValidationIssues_IncludesCoreRuleDefinitionIssues()
        {
            BoardDefinition board = ScriptableObject.CreateInstance<BoardDefinition>();
            board.boardId = "";
            board.displayName = "";
            board.width = 0;
            board.height = 0;

            TurnOrderDefinition turnOrder = ScriptableObject.CreateInstance<TurnOrderDefinition>();
            turnOrder.turnOrderId = "";
            turnOrder.displayName = "";
            turnOrder.participantSeats = System.Array.Empty<int>();

            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.boardDefinition = board;
            mode.turnOrderDefinition = turnOrder;
            BoardTerminalConditionDefinition terminalCondition = ScriptableObject.CreateInstance<BoardTerminalConditionDefinition>();
            terminalCondition.conditionId = "";
            terminalCondition.displayName = "";
            terminalCondition.kind = BoardTerminalConditionKind.SideEliminated;
            terminalCondition.observedSeat = -1;
            terminalCondition.winningSeat = -1;
            mode.boardTerminalConditions = new[] { terminalCondition };

            System.Collections.Generic.List<string> issues = mode.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("Board definition")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("Turn order definition")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("Board terminal condition")), Is.True);

            Object.DestroyImmediate(terminalCondition);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(turnOrder);
            Object.DestroyImmediate(board);
        }
    }
}
