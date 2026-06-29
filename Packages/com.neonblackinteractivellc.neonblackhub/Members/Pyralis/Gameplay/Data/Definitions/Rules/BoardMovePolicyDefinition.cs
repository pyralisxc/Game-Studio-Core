using NeonBlack.Gameplay.Data.Tabletop;
using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Definitions.Rules
{
    [System.Serializable]
    public struct BoardMoveOffset
    {
        public int x;
        public int y;

        public BoardMoveOffset(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public BoardCoordinate ToCoordinate()
        {
            return new BoardCoordinate(x, y);
        }
    }

    /// <summary>
    /// Designer-authored movement policy for board and tabletop pieces.
    /// </summary>
    [AuthoringContract(
        Category = "Tabletop, Grid",
        CapabilityPath = "Tabletop/Board/Board Move Policy Definition",
        Surface = AuthoringSurface.Goal,
        Summary = "Project-window creation path for tabletop legal-move policy.",
        RequiredFields = new[] { nameof(policyId), nameof(shape), nameof(maxDistance) },
        SuccessChecks = new[] { "Verify that pieces can only move according to the shape and distance defined in this policy." },
        Tags = new[] { "capability:Tabletop", "capability:Grid", "runtime:BoardCardTabletop" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Rules/Board Move Policy", fileName = "BoardMovePolicy", order = -80)]
    public class BoardMovePolicyDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            return RuntimeValidationIssueUtility.FromLocalValidationMessages(GetValidationIssues(), this);
        }

        public string policyId = "policy.boardMove";
        public string displayName = "Board Move Policy";
        public BoardMoveShape shape = BoardMoveShape.OrthogonalOrDiagonal;
        public int maxDistance = 1;
        public bool allowCapture;
        public BoardMoveOffset[] allowedOffsets;

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(policyId))
                policyId = name;

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = string.IsNullOrWhiteSpace(policyId) ? "Board Move Policy" : policyId;
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(policyId))
                issues.Add("Policy id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (maxDistance <= 0)
                issues.Add("Max distance must be greater than zero.");

            if (shape == BoardMoveShape.Offset && (allowedOffsets == null || allowedOffsets.Length == 0))
                issues.Add("Allowed offsets are required for offset move policies.");

            return issues;
        }

        public IBoardMovePolicy CreatePolicy(out List<string> issues)
        {
            issues = GetValidationIssues();
            BoardCoordinate[] offsets = CreateAllowedOffsets();
            return new BoardMovePolicy(policyId, shape, maxDistance, allowCapture, offsets);
        }

        private BoardCoordinate[] CreateAllowedOffsets()
        {
            if (allowedOffsets == null || allowedOffsets.Length == 0)
                return System.Array.Empty<BoardCoordinate>();

            BoardCoordinate[] offsets = new BoardCoordinate[allowedOffsets.Length];
            for (int i = 0; i < allowedOffsets.Length; i++)
                offsets[i] = allowedOffsets[i].ToCoordinate();

            return offsets;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
