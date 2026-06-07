using NeonBlack.Gameplay.Core.Rules.Board;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Tabletop
{
    /// <summary>
    /// Scene object representing one logical tabletop board piece.
    /// </summary>
    [AddComponentMenu("NeonBlack/Gameplay/Tabletop/Tabletop Board Piece View")]
    public sealed class TabletopBoardPieceView : MonoBehaviour
    {
        private TabletopBoardGridPresenter _presenter;

        public string PieceId { get; private set; } = string.Empty;
        public BoardCoordinate Coordinate { get; private set; }
        public string LastIssue { get; private set; } = string.Empty;

        public void Initialize(TabletopBoardGridPresenter presenter, string pieceId, BoardCoordinate coordinate)
        {
            _presenter = presenter;
            PieceId = pieceId ?? string.Empty;
            SetCoordinate(coordinate);
            LastIssue = string.Empty;
        }

        public void SetCoordinate(BoardCoordinate coordinate)
        {
            Coordinate = coordinate;
            if (_presenter != null)
                transform.localPosition = _presenter.CoordinateToLocalPosition(coordinate) + Vector3.up * 0.1f;
        }

        public bool Select(out string issue)
        {
            if (_presenter == null)
            {
                issue = "Tabletop board piece is not connected to a presenter.";
                LastIssue = issue;
                return false;
            }

            bool accepted = _presenter.TrySelectCoordinate(Coordinate, out issue);
            LastIssue = issue;
            return accepted;
        }

        private void OnMouseDown()
        {
            Select(out _);
        }
    }
}
