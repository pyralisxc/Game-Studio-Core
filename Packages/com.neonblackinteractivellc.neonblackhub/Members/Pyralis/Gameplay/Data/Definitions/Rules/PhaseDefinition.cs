using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rules
{
    /// <summary>
    /// Designer-authored phase within a turn.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Tabletop | AuthoringCapability.TurnBased, 
        Relevance = "Project-window creation path for turn phase rules.",
        AssignmentFields = new[] { nameof(phaseId), nameof(displayName), nameof(allowsActionSelection) },
        FirstProof = "Verify that the phase allows or restricts actions as defined.",
        NativeSetup = new[] { "Create Asset" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Rules/Phase Definition", fileName = "PhaseDefinition", order = -60)]
    public class PhaseDefinition : ScriptableObject
    {
        public string phaseId = "phase.default";
        public string displayName = "Phase";
        public bool allowsActionSelection = true;
        public bool endsTurnWhenComplete;

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(phaseId))
                phaseId = name;

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = string.IsNullOrWhiteSpace(phaseId) ? "Phase" : phaseId;
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(phaseId))
                issues.Add("Phase id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
