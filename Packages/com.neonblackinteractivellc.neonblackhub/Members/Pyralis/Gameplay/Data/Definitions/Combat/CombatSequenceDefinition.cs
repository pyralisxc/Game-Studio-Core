using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Definitions.Combat
{
    /// <summary>
    /// Authored ordered combo or action chain for one neutral combat lane.
    /// </summary>
    [AuthoringContract(
        Category = "Combat",
        CapabilityPath = "Combat/Actions/Combat Sequence Definition",
        Surface = AuthoringSurface.Goal,
        Summary = "Defines a sequence of combat actions (combos) triggered by a specific input type.",
        RequiredFields = new[] { nameof(inputType), nameof(actions) },
        SetupSteps = new[] { "Set Input Type.", "Add CombatActionDefinitions to the actions array." },
        SuccessChecks = new[] { "Verify the actor performs the sequence of animations and attacks in order." },
        Tags = new[] { "capability:Combat", "runtime:Combat" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Combat/Combat Sequence Definition", fileName = "CombatSequenceDefinition")]
    public class CombatSequenceDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (actions == null || actions.Length == 0)
                yield return RuntimeValidationIssue.Required("No actions assigned to this sequence.", nameof(actions), nameof(CombatSequenceDefinition), issueCode: "CombatSequence.Actions.Empty");

            if (actions == null)
                yield break;

            for (int i = 0; i < actions.Length; i++)
            {
                CombatActionDefinition action = actions[i];
                if (action == null)
                {
                    yield return RuntimeValidationIssue.Required(
                        $"Actions[{i}] is empty.",
                        $"{nameof(actions)}[{i}]",
                        nameof(CombatSequenceDefinition),
                        "Open the CombatSequenceDefinition and assign a CombatActionDefinition or remove the empty slot.",
                        "Every sequence action slot is assigned.",
                        "CombatSequence.Action.Missing");
                    continue;
                }

                foreach (RuntimeValidationIssue issue in action.GetRuntimeValidationIssues())
                {
                    if (issue != null && !string.IsNullOrWhiteSpace(issue.Message))
                    {
                        yield return new RuntimeValidationIssue(
                            $"Action `{action.displayName}`: {issue.Message}",
                            $"{nameof(actions)}[{i}]",
                            nameof(CombatSequenceDefinition),
                            "Open the referenced CombatActionDefinition and resolve the named issue.",
                            "Referenced CombatActionDefinition reports no validation issues.",
                            issue.Severity,
                            "CombatSequence.Action." + issue.IssueCode);
                    }
                }
            }
        }

        public string displayName = "Combat Sequence";
        public CombatInputType inputType = CombatInputType.Primary;
        public bool resetAfterFinalAction = true;
        public bool restartFromFirstActionWhenBranchFails = true;
        public CombatActionDefinition[] actions;

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = name;
            }

            if (actions == null)
            {
                actions = Array.Empty<CombatActionDefinition>();
                return;
            }

            List<CombatActionDefinition> sanitized = new List<CombatActionDefinition>(actions.Length);
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null)
                {
                    sanitized.Add(actions[i]);
                }
            }

            actions = sanitized.ToArray();
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
