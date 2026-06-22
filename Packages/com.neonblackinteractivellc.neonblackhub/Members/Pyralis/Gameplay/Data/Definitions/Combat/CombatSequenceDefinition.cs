using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    /// <summary>
    /// Authored ordered combo or action chain for one neutral combat lane.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        Relevance = "Defines a sequence of combat actions (combos) triggered by a specific input type.",
        NativeSetup = new[] { "Set Input Type.", "Add CombatActionDefinitions to the actions array." },
        AssignmentFields = new[] { nameof(inputType), nameof(actions) },
        FirstProof = "Verify the actor performs the sequence of animations and attacks in order.",
        ExpertAdvice = "Use sequences to build multi-hit brawler combos. Each action in the list must correspond to the correct combo step.",
        CapabilityPath = "Combat/Actions/Combat Sequence Definition",
        RuntimeFamilies = new[] { RuntimeCapabilityFamily.Combat }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Combat/Combat Sequence Definition", fileName = "CombatSequenceDefinition")]
    public class CombatSequenceDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (actions == null || actions.Length == 0)
                yield return PyralisRuntimeValidationIssue.Required("No actions assigned to this sequence.", nameof(actions), nameof(CombatSequenceDefinition), issueCode: "CombatSequence.Actions.Empty");

            if (actions == null)
                yield break;

            for (int i = 0; i < actions.Length; i++)
            {
                CombatActionDefinition action = actions[i];
                if (action == null)
                {
                    yield return PyralisRuntimeValidationIssue.Required(
                        $"Actions[{i}] is empty.",
                        $"{nameof(actions)}[{i}]",
                        nameof(CombatSequenceDefinition),
                        "Open the CombatSequenceDefinition and assign a CombatActionDefinition or remove the empty slot.",
                        "Every sequence action slot is assigned.",
                        "CombatSequence.Action.Missing");
                    continue;
                }

                foreach (PyralisRuntimeValidationIssue issue in action.GetRuntimeValidationIssues())
                {
                    if (issue != null && !string.IsNullOrWhiteSpace(issue.Message))
                    {
                        yield return new PyralisRuntimeValidationIssue(
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
