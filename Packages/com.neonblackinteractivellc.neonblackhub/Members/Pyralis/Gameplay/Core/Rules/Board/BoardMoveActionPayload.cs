using System;

namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Payload for an action that moves one logical board piece.
    /// </summary>
    [Serializable]
    public readonly struct BoardMoveActionPayload
    {
        public BoardMoveActionPayload(string pieceId, BoardCoordinate destination)
        {
            PieceId = pieceId ?? string.Empty;
            Destination = destination;
        }

        public string PieceId { get; }
        public BoardCoordinate Destination { get; }
    }
}
