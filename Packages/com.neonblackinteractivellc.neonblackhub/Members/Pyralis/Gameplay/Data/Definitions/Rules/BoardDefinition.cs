using NeonBlack.Gameplay.Core.Rules.Board;
using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rules
{
    /// <summary>
    /// Designer-authored board layout for tabletop and grid-based rules.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Tabletop | AuthoringCapability.Grid, 
        Relevance = "Project-window creation path for tabletop board layouts and starting pieces.",
        AssignmentFields = new[] { nameof(width), nameof(height), nameof(startingPieces) },
        FirstProofTargetId = "proof.board-card-action",
        FirstProof = "Verify the board dimensions and starting pieces are correct in the Board Presenter.",
        NativeSetup = new[] { "Create Asset" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Rules/Board Definition", fileName = "BoardDefinition", order = -100)]
    public class BoardDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            return GetValidationIssues();
        }

        public string boardId = "board.default";
        public string displayName = "Board";
        public int width = 8;
        public int height = 8;
        public BoardStartingPiece[] startingPieces;

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(boardId))
                boardId = name;

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = string.IsNullOrWhiteSpace(boardId) ? "Board" : boardId;

            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(boardId))
                issues.Add("Board id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (width <= 0)
                issues.Add("Width must be greater than zero.");

            if (height <= 0)
                issues.Add("Height must be greater than zero.");

            HashSet<string> pieceIds = new HashSet<string>();
            HashSet<BoardCoordinate> occupiedCoordinates = new HashSet<BoardCoordinate>();
            if (startingPieces == null)
                return issues;

            for (int i = 0; i < startingPieces.Length; i++)
            {
                BoardStartingPiece piece = startingPieces[i];
                if (string.IsNullOrWhiteSpace(piece.pieceInstanceId))
                    issues.Add($"Starting Pieces[{i}] piece instance id is required.");
                else if (!pieceIds.Add(piece.pieceInstanceId))
                    issues.Add($"Starting piece `{piece.pieceInstanceId}` is assigned more than once.");

                if (piece.pieceDefinition == null)
                    issues.Add($"Starting piece `{piece.pieceInstanceId}` requires a piece definition.");
                else
                {
                    List<string> pieceIssues = piece.pieceDefinition.GetValidationIssues();
                    for (int issueIndex = 0; issueIndex < pieceIssues.Count; issueIndex++)
                        issues.Add($"Starting piece `{piece.pieceInstanceId}` definition: {pieceIssues[issueIndex]}");
                }

                if (!IsInsideBoard(piece.coordinate))
                    issues.Add($"Starting piece `{piece.pieceInstanceId}` coordinate `{piece.coordinate}` is outside the board.");
                else if (!occupiedCoordinates.Add(piece.coordinate))
                    issues.Add($"Starting piece coordinate `{piece.coordinate}` is occupied more than once.");
            }

            return issues;
        }

        public BoardRuntimeState CreateRuntimeState(out List<string> issues)
        {
            issues = GetValidationIssues();
            BoardRuntimeState state = BoardRuntimeState.CreateRectangular(width, height);
            if (issues.Count > 0 || startingPieces == null)
                return state;

            for (int i = 0; i < startingPieces.Length; i++)
            {
                BoardStartingPiece startingPiece = startingPieces[i];
                BoardPieceState piece = new BoardPieceState(
                    startingPiece.pieceInstanceId,
                    startingPiece.pieceDefinition.pieceId,
                    startingPiece.ownerSeat,
                    startingPiece.coordinate);

                if (!state.TryAddPiece(piece, out string issue))
                    issues.Add(issue);
            }

            return state;
        }

        private bool IsInsideBoard(BoardCoordinate coordinate)
        {
            return coordinate.X >= 0
                && coordinate.Y >= 0
                && coordinate.X < width
                && coordinate.Y < height;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
