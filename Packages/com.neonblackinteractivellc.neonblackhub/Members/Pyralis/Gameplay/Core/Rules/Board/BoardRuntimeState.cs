using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Authoritative logical board state for tabletop-style games.
    /// </summary>
    public sealed class BoardRuntimeState
    {
        private readonly Dictionary<BoardCoordinate, BoardSpaceState> _spacesByCoordinate;
        private readonly Dictionary<string, BoardPieceState> _piecesById;
        private readonly Dictionary<BoardCoordinate, string> _occupancyByCoordinate;

        private BoardRuntimeState(int width, int height)
        {
            Width = width;
            Height = height;
            _spacesByCoordinate = new Dictionary<BoardCoordinate, BoardSpaceState>();
            _piecesById = new Dictionary<string, BoardPieceState>();
            _occupancyByCoordinate = new Dictionary<BoardCoordinate, string>();
        }

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyCollection<BoardSpaceState> Spaces => _spacesByCoordinate.Values;
        public IReadOnlyCollection<BoardPieceState> Pieces => _piecesById.Values;

        public static BoardRuntimeState CreateRectangular(int width, int height)
        {
            BoardRuntimeState state = new BoardRuntimeState(width, height);
            if (width <= 0 || height <= 0)
                return state;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    BoardCoordinate coordinate = new BoardCoordinate(x, y);
                    state._spacesByCoordinate.Add(coordinate, new BoardSpaceState(coordinate));
                }
            }

            return state;
        }

        public bool HasSpace(BoardCoordinate coordinate)
        {
            return _spacesByCoordinate.ContainsKey(coordinate);
        }

        public bool TryGetSpace(BoardCoordinate coordinate, out BoardSpaceState space)
        {
            return _spacesByCoordinate.TryGetValue(coordinate, out space);
        }

        public bool TryGetPiece(string pieceId, out BoardPieceState piece)
        {
            if (string.IsNullOrWhiteSpace(pieceId))
            {
                piece = null;
                return false;
            }

            return _piecesById.TryGetValue(pieceId, out piece);
        }

        public bool TryGetPieceAt(BoardCoordinate coordinate, out BoardPieceState piece)
        {
            piece = null;
            if (!_occupancyByCoordinate.TryGetValue(coordinate, out string pieceId))
                return false;

            return _piecesById.TryGetValue(pieceId, out piece);
        }

        public bool TryAddPiece(BoardPieceState piece, out string issue)
        {
            if (piece == null)
            {
                issue = "Piece is null.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(piece.PieceId))
            {
                issue = "Piece id is empty.";
                return false;
            }

            if (piece.IsCaptured)
            {
                issue = $"Piece `{piece.PieceId}` is already captured.";
                return false;
            }

            if (_piecesById.ContainsKey(piece.PieceId))
            {
                issue = $"Piece `{piece.PieceId}` already exists.";
                return false;
            }

            if (!HasSpace(piece.Coordinate))
            {
                issue = $"Coordinate `{piece.Coordinate}` is outside the board.";
                return false;
            }

            if (_occupancyByCoordinate.ContainsKey(piece.Coordinate))
            {
                issue = $"Coordinate `{piece.Coordinate}` is already occupied.";
                return false;
            }

            _piecesById.Add(piece.PieceId, piece);
            _occupancyByCoordinate.Add(piece.Coordinate, piece.PieceId);
            issue = string.Empty;
            return true;
        }

        public bool TryMovePiece(string pieceId, BoardCoordinate destination, out string issue)
        {
            if (!TryGetPiece(pieceId, out BoardPieceState piece))
            {
                issue = $"Piece `{pieceId}` does not exist.";
                return false;
            }

            if (piece.IsCaptured)
            {
                issue = $"Piece `{pieceId}` is captured.";
                return false;
            }

            if (!HasSpace(destination))
            {
                issue = $"Coordinate `{destination}` is outside the board.";
                return false;
            }

            if (_occupancyByCoordinate.ContainsKey(destination))
            {
                issue = $"Coordinate `{destination}` is already occupied.";
                return false;
            }

            _occupancyByCoordinate.Remove(piece.Coordinate);
            piece.MoveTo(destination);
            _occupancyByCoordinate[destination] = piece.PieceId;
            issue = string.Empty;
            return true;
        }

        public bool TryCapturePiece(string pieceId, out string issue)
        {
            if (!TryGetPiece(pieceId, out BoardPieceState piece))
            {
                issue = $"Piece `{pieceId}` does not exist.";
                return false;
            }

            if (piece.IsCaptured)
            {
                issue = $"Piece `{pieceId}` is already captured.";
                return false;
            }

            _occupancyByCoordinate.Remove(piece.Coordinate);
            piece.Capture();
            issue = string.Empty;
            return true;
        }
    }
}
