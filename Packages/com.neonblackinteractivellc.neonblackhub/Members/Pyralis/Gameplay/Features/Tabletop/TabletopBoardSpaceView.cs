using NeonBlack.Gameplay.Core.Rules.Board;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Tabletop
{
    /// <summary>
    /// Selectable scene object for one logical tabletop board coordinate.
    /// </summary>
    [AddComponentMenu("NeonBlack/Gameplay/Tabletop/Tabletop Board Space View")]
    public sealed class TabletopBoardSpaceView : MonoBehaviour
    {
        private TabletopBoardGridPresenter _presenter;

        public BoardCoordinate Coordinate { get; private set; }
        public string LastIssue { get; private set; } = string.Empty;

        public void Initialize(TabletopBoardGridPresenter presenter, BoardCoordinate coordinate)
        {
            _presenter = presenter;
            Coordinate = coordinate;
            LastIssue = string.Empty;
        }

        public bool Select(out string issue)
        {
            if (_presenter == null)
            {
                issue = "Tabletop board space is not connected to a presenter.";
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
