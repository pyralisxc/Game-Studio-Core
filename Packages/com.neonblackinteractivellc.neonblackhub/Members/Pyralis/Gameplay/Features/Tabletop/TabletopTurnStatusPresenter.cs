using TMPro;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Tabletop
{
    /// <summary>
    /// Lightweight UI binding for local tabletop turn proofs.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Tabletop,
        Relevance = "LIGHTWEIGHT UI binding that shows which tabletop seat acts next.",
        NativeSetup = new[] { "Add to Tabletop HUD", "Assign BoardPresenter and TMP Label" },
        AssignmentFields = new[] { nameof(boardPresenter), nameof(label), nameof(seatZeroName), nameof(seatOneName) },
        FirstProof = "The HUD label correctly displays the name of the active participant's seat."
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Tabletop/Tabletop Turn Status Presenter")]
    public sealed class TabletopTurnStatusPresenter : MonoBehaviour
    {
        [SerializeField] private TabletopBoardGridPresenter boardPresenter;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private string seatZeroName = "White";
        [SerializeField] private string seatOneName = "Black";
        [SerializeField] private string fallbackFormat = "Seat {0} to move";

        public string CurrentText { get; private set; } = string.Empty;

        public void Configure(TabletopBoardGridPresenter presenter, TextMeshProUGUI targetLabel)
        {
            boardPresenter = presenter;
            label = targetLabel;
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            string text = ResolveText();
            CurrentText = text;
            if (label != null && label.text != text)
                label.text = text;
        }

        private string ResolveText()
        {
            if (boardPresenter == null || boardPresenter.TurnState == null)
                return "Board turn order not ready";

            int activeSeat = boardPresenter.TurnState.ActiveSeat;
            string seatName = ResolveSeatName(activeSeat);
            return string.IsNullOrWhiteSpace(seatName)
                ? string.Format(fallbackFormat, activeSeat)
                : seatName + " to move";
        }

        private string ResolveSeatName(int seat)
        {
            if (seat == 0)
                return seatZeroName;

            if (seat == 1)
                return seatOneName;

            return string.Empty;
        }
    }
}
