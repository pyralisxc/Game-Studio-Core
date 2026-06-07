using System;

namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Reusable grid movement policy for regular tabletop, tactical, and token board moves.
    /// </summary>
    public sealed class BoardMovePolicy : IBoardMovePolicy
    {
        public BoardMovePolicy(
            string policyId,
            BoardMoveShape shape,
            int maxDistance,
            bool allowCapture,
            BoardCoordinate[] allowedOffsets = null)
        {
            PolicyId = string.IsNullOrWhiteSpace(policyId) ? "policy.boardMove" : policyId;
            Shape = shape;
            MaxDistance = maxDistance;
            AllowsCapture = allowCapture;
            _allowedOffsets = allowedOffsets == null
                ? Array.Empty<BoardCoordinate>()
                : (BoardCoordinate[])allowedOffsets.Clone();
        }

        private readonly BoardCoordinate[] _allowedOffsets;

        public string PolicyId { get; }
        public BoardMoveShape Shape { get; }
        public int MaxDistance { get; }
        public bool AllowsCapture { get; }

        public bool IsMoveAllowed(BoardMovePolicyContext context, out string issue)
        {
            if (context.BoardState == null)
            {
                issue = $"Policy `{PolicyId}` requires board state.";
                return false;
            }

            if (context.MovingPiece == null)
            {
                issue = $"Policy `{PolicyId}` requires a moving piece.";
                return false;
            }

            if (context.MovingPiece.IsCaptured)
            {
                issue = $"Policy `{PolicyId}` cannot move captured piece `{context.MovingPiece.PieceId}`.";
                return false;
            }

            if (!context.BoardState.HasSpace(context.Destination))
            {
                issue = $"Policy `{PolicyId}` rejects `{context.Destination}` because it is outside the board.";
                return false;
            }

            if (context.DestinationPiece != null)
            {
                if (context.DestinationPiece.OwnerSeat == context.MovingPiece.OwnerSeat)
                {
                    issue = $"Policy `{PolicyId}` rejects friendly occupied destination `{context.Destination}`.";
                    return false;
                }

                if (!AllowsCapture)
                {
                    issue = $"Policy `{PolicyId}` does not allow capture at `{context.Destination}`.";
                    return false;
                }
            }

            int deltaX = Math.Abs(context.Destination.X - context.MovingPiece.Coordinate.X);
            int deltaY = Math.Abs(context.Destination.Y - context.MovingPiece.Coordinate.Y);
            if (deltaX == 0 && deltaY == 0)
            {
                issue = $"Policy `{PolicyId}` rejects moves that stay on the same coordinate.";
                return false;
            }

            if (MaxDistance <= 0)
            {
                issue = $"Policy `{PolicyId}` requires Max Distance greater than zero.";
                return false;
            }

            if (!MatchesShape(deltaX, deltaY))
            {
                issue = $"Policy `{PolicyId}` rejects move shape from `{context.MovingPiece.Coordinate}` to `{context.Destination}`.";
                return false;
            }

            int distance = GetStepDistance(deltaX, deltaY);
            if (distance > MaxDistance)
            {
                issue = $"Policy `{PolicyId}` rejects distance {distance}; max distance is {MaxDistance}.";
                return false;
            }

            issue = string.Empty;
            return true;
        }

        private bool MatchesShape(int deltaX, int deltaY)
        {
            switch (Shape)
            {
                case BoardMoveShape.Any:
                    return true;
                case BoardMoveShape.Orthogonal:
                    return (deltaX == 0 && deltaY > 0) || (deltaY == 0 && deltaX > 0);
                case BoardMoveShape.Diagonal:
                    return deltaX == deltaY && deltaX > 0;
                case BoardMoveShape.OrthogonalOrDiagonal:
                    return (deltaX == 0 && deltaY > 0)
                        || (deltaY == 0 && deltaX > 0)
                        || (deltaX == deltaY && deltaX > 0);
                case BoardMoveShape.Adjacent:
                    return Math.Max(deltaX, deltaY) == 1;
                case BoardMoveShape.Offset:
                    return MatchesOffset(deltaX, deltaY);
                default:
                    return false;
            }
        }

        private bool MatchesOffset(int deltaX, int deltaY)
        {
            for (int i = 0; i < _allowedOffsets.Length; i++)
            {
                BoardCoordinate offset = _allowedOffsets[i];
                if (Math.Abs(offset.X) == deltaX && Math.Abs(offset.Y) == deltaY)
                    return true;
            }

            return false;
        }

        private int GetStepDistance(int deltaX, int deltaY)
        {
            if (Shape == BoardMoveShape.Offset)
                return 1;

            return Shape == BoardMoveShape.Orthogonal ? deltaX + deltaY : Math.Max(deltaX, deltaY);
        }
    }
}
