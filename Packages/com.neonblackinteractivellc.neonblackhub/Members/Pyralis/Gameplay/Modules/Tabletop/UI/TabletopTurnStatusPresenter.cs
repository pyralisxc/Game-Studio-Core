using System.Collections.Generic;
using TMPro;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Tabletop
{
    /// <summary>
    /// Lightweight UI binding for local tabletop turn proofs.
    /// </summary>
    [AuthoringContract(
        Category = "Tabletop",
        CapabilityPath = "Tabletop/Board/Tabletop Turn Status Presenter",
        Surface = AuthoringSurface.Goal,
        Summary = "LIGHTWEIGHT UI binding that shows which tabletop seat acts next.",
        RequiredFields = new[] { nameof(boardPresenter), nameof(label) },
        SetupSteps = new[] { "Add to Tabletop HUD", "Assign BoardPresenter and TMP Label", "Rename seat labels only when this board uses custom seat names." },
        SuccessChecks = new[] { "The HUD label correctly displays the name of the active participant's seat." },
        Tags = new[] { "capability:Tabletop" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Tabletop/Tabletop Turn Status Presenter")]
    public sealed class TabletopTurnStatusPresenter : MonoBehaviour, IRuntimeValidationProvider
    {
        [SerializeField] private TabletopBoardGridPresenter boardPresenter;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private string seatZeroName = "White";
        [SerializeField] private string seatOneName = "Black";
        [SerializeField] private string fallbackFormat = "Seat {0} to move";

        public string CurrentText { get; private set; } = string.Empty;

        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (boardPresenter == null)
                yield return RuntimeValidationIssue.Required("Board Presenter is required to read tabletop turn state.");

            if (label == null)
                yield return RuntimeValidationIssue.Required("TMP Label is required to display tabletop turn status.");

            if (string.IsNullOrWhiteSpace(fallbackFormat))
                yield return RuntimeValidationIssue.Recommended("Fallback Format is empty, so unexpected seat IDs may render blank status text.");
        }

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
