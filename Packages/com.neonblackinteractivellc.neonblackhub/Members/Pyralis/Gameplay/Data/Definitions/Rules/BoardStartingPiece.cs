using NeonBlack.Gameplay.Core.Rules.Board;
using System;

namespace NeonBlack.Gameplay.Data.Definitions.Rules
{
    /// <summary>
    /// Authoring entry for an initial piece on a board.
    /// </summary>
    [Serializable]
    public struct BoardStartingPiece
    {
        public string pieceInstanceId;
        public BoardPieceDefinition pieceDefinition;
        public int ownerSeat;
        public BoardCoordinate coordinate;

        public BoardStartingPiece(string pieceInstanceId, BoardPieceDefinition pieceDefinition, int ownerSeat, BoardCoordinate coordinate)
        {
            this.pieceInstanceId = pieceInstanceId;
            this.pieceDefinition = pieceDefinition;
            this.ownerSeat = ownerSeat;
            this.coordinate = coordinate;
        }
    }
}
