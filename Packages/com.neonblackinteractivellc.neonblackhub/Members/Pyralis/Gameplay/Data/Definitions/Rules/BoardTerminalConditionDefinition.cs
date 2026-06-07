using NeonBlack.Gameplay.Core.Rules.Board;
using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rules
{
    /// <summary>
    /// Designer-authored terminal condition for board and tabletop games.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Rules/Board Terminal Condition", fileName = "BoardTerminalCondition", order = -50)]
    public class BoardTerminalConditionDefinition : ScriptableObject
    {
        public string conditionId = "condition.boardTerminal";
        public string displayName = "Board Terminal Condition";
        public BoardTerminalConditionKind kind = BoardTerminalConditionKind.SideEliminated;
        public int observedSeat = 1;
        public int winningSeat = 0;
        public BoardCoordinate objectiveCoordinate;

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(conditionId))
                conditionId = name;

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = string.IsNullOrWhiteSpace(conditionId) ? "Board Terminal Condition" : conditionId;
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(conditionId))
                issues.Add("Condition id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (kind == BoardTerminalConditionKind.SideEliminated)
            {
                if (observedSeat < 0)
                    issues.Add("Observed seat must be zero or greater for side-eliminated conditions.");

                if (winningSeat < 0)
                    issues.Add("Winning seat must be zero or greater for side-eliminated conditions.");
            }

            return issues;
        }

        public IBoardTerminalCondition CreateCondition(out List<string> issues)
        {
            issues = GetValidationIssues();
            return new BoardTerminalCondition(conditionId, kind, observedSeat, winningSeat, objectiveCoordinate);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
