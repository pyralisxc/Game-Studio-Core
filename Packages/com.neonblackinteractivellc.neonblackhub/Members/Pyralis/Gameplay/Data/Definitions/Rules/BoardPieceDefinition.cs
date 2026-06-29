using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Definitions.Rules
{
    /// <summary>
    /// Designer-authored logical board piece identity.
    /// </summary>
    [AuthoringContract(
        Category = "Tabletop, Grid",
        CapabilityPath = "Tabletop/Board/Board Piece Definition",
        Surface = AuthoringSurface.Goal,
        Summary = "Project-window creation path for tabletop board pieces.",
        RequiredFields = new[] { nameof(pieceId), nameof(displayName), nameof(visualPrefab) },
        SuccessChecks = new[] { "Verify the piece is instantiated correctly on the board with the assigned visual prefab." },
        Tags = new[] { "capability:Tabletop", "capability:Grid", "runtime:BoardCardTabletop" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Rules/Board Piece Definition", fileName = "BoardPieceDefinition", order = -90)]
    public class BoardPieceDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            return RuntimeValidationIssueUtility.FromLocalValidationMessages(GetValidationIssues(), this);
        }

        public string pieceId = "piece.default";
        public string displayName = "Piece";
        public string pieceFamily = "General";
        public GameObject visualPrefab;
        public string[] tags;

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(pieceId))
                pieceId = name;

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = string.IsNullOrWhiteSpace(pieceId) ? "Piece" : pieceId;

            if (string.IsNullOrWhiteSpace(pieceFamily))
                pieceFamily = "General";
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(pieceId))
                issues.Add("Piece id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (string.IsNullOrWhiteSpace(pieceFamily))
                issues.Add("Piece family is required.");

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
