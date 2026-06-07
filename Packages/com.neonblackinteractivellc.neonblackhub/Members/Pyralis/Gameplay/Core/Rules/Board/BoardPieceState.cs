using System;

namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Runtime state for one logical board piece.
    /// </summary>
    [Serializable]
    public sealed class BoardPieceState
    {
        public BoardPieceState(string pieceId, string pieceDefinitionId, int ownerSeat, BoardCoordinate coordinate)
        {
            PieceId = pieceId;
            PieceDefinitionId = pieceDefinitionId;
            OwnerSeat = ownerSeat;
            Coordinate = coordinate;
        }

        public string PieceId { get; }
        public string PieceDefinitionId { get; }
        public int OwnerSeat { get; }
        public BoardCoordinate Coordinate { get; private set; }
        public bool IsCaptured { get; private set; }

        internal void MoveTo(BoardCoordinate coordinate)
        {
            Coordinate = coordinate;
            IsCaptured = false;
        }

        internal void Capture()
        {
            IsCaptured = true;
        }
    }
}
