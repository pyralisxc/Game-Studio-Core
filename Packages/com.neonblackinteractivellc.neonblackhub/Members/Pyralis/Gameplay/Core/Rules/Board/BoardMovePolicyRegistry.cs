using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Registry of standard tabletop move policies (Chess, Checkers, etc.).
    /// </summary>
    public static class BoardMovePolicyRegistry
    {
        public static IBoardMovePolicy King() => new BoardMovePolicy("policy.king", BoardMoveShape.Adjacent, 1, true);
        public static IBoardMovePolicy Queen() => new BoardMovePolicy("policy.queen", BoardMoveShape.OrthogonalOrDiagonal, int.MaxValue, true);
        public static IBoardMovePolicy Rook() => new BoardMovePolicy("policy.rook", BoardMoveShape.Orthogonal, int.MaxValue, true);
        public static IBoardMovePolicy Bishop() => new BoardMovePolicy("policy.bishop", BoardMoveShape.Diagonal, int.MaxValue, true);
        public static IBoardMovePolicy Knight() => new BoardMovePolicy("policy.knight", BoardMoveShape.Offset, 1, true, new[]
        {
            new BoardCoordinate(1, 2), new BoardCoordinate(1, -2),
            new BoardCoordinate(-1, 2), new BoardCoordinate(-1, -2),
            new BoardCoordinate(2, 1), new BoardCoordinate(2, -1),
            new BoardCoordinate(-2, 1), new BoardCoordinate(-2, -1)
        });
        
        public static IBoardMovePolicy PawnPush() => new BoardMovePolicy("policy.pawnPush", BoardMoveShape.Orthogonal, 1, false);
        
        public static IBoardMovePolicy Man() => new BoardMovePolicy("policy.checkersMan", BoardMoveShape.Diagonal, 1, false);
    }
}