using NeonBlack.Gameplay.Core.Rules.TurnPhase;
using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rules
{
    /// <summary>
    /// Designer-authored seat order and phase list for turn-based rules.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Tabletop | AuthoringCapability.TurnBased, 
        Relevance = "Project-window creation path for tabletop and turn/menu action order.",
        AssignmentFields = new[] { nameof(participantSeats), nameof(phases) },
        FirstProof = "Verify the turn sequence in the Tabletop Board Grid Presenter.",
        NativeSetup = new[] { "Create Asset" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Rules/Turn Order Definition", fileName = "TurnOrderDefinition", order = -70)]
    public class TurnOrderDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            return GetValidationIssues();
        }

        public string turnOrderId = "turn.default";
        public string displayName = "Turn Order";
        public int[] participantSeats = { 0, 1 };
        public PhaseDefinition[] phases;

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(turnOrderId))
                turnOrderId = name;

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = string.IsNullOrWhiteSpace(turnOrderId) ? "Turn Order" : turnOrderId;
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(turnOrderId))
                issues.Add("Turn order id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            HashSet<int> seats = new HashSet<int>();
            if (participantSeats == null || participantSeats.Length == 0)
            {
                issues.Add("At least one participant seat is required.");
            }
            else
            {
                for (int i = 0; i < participantSeats.Length; i++)
                {
                    int seat = participantSeats[i];
                    if (seat < 0)
                        issues.Add($"Participant Seats[{i}] cannot be negative.");
                    else if (!seats.Add(seat))
                        issues.Add($"Participant seat `{seat}` is assigned more than once.");
                }
            }

            HashSet<string> phaseIds = new HashSet<string>();
            if (phases == null)
                return issues;

            for (int i = 0; i < phases.Length; i++)
            {
                PhaseDefinition phase = phases[i];
                if (phase == null)
                {
                    issues.Add($"Phases[{i}] is null.");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(phase.phaseId) && !phaseIds.Add(phase.phaseId))
                    issues.Add($"Phase `{phase.phaseId}` is assigned more than once.");

                List<string> phaseIssues = phase.GetValidationIssues();
                for (int issueIndex = 0; issueIndex < phaseIssues.Count; issueIndex++)
                    issues.Add($"Phase `{phase.phaseId}`: {phaseIssues[issueIndex]}");
            }

            return issues;
        }

        public TurnRuntimeState CreateRuntimeState()
        {
            int startingSeat = participantSeats != null && participantSeats.Length > 0 ? participantSeats[0] : -1;
            return new TurnRuntimeState(participantSeats, startingSeat);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
