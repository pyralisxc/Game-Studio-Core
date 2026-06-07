using NeonBlack.Gameplay.Features.Combat;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(CombatSequenceDefinition))]
    public class CombatSequenceDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            CombatSequenceDefinition definition = (CombatSequenceDefinition)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Combat Sequence Definition",
                "A combat sequence orders combat actions into a reusable input lane such as primary combo, secondary combo, aerial chain, or special branch.",
                whenToUse: new[]
                {
                    "Use this for brawler/fighter combo chains.",
                    "Assign sequences to PawnCombatProfile or enemy combat systems that consume shared combat definitions."
                },
                createBefore: new[]
                {
                    "CombatActionDefinition assets for each step.",
                    "PawnCombatProfile or EnemyCombatProfile that will consume the sequence."
                },
                assignFirst: new[]
                {
                    "Set Input Type to the lane this sequence responds to.",
                    "Add actions in the exact order they should execute.",
                    "Choose reset/restart behavior for missed branches."
                },
                safeToCustomize: new[]
                {
                    "Empty sequences are useful placeholders but should not be assigned as active combat content.",
                    "Reset After Final Action keeps combo loops predictable.",
                    "Restart From First Action When Branch Fails makes early prototypes easier to read."
                },
                validation: new[]
                {
                    "Actions array has no missing entries.",
                    "Action input types match the sequence lane where possible.",
                    "Combo steps are ordered intentionally."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Combat_Definitions_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(definition), "Combat sequence definition is ready for combat profile assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(CombatSequenceDefinition definition)
        {
            List<string> issues = new List<string>();

            if (definition == null)
                return issues;

            if (string.IsNullOrWhiteSpace(definition.displayName))
                issues.Add("Display Name is required.");
            if (definition.actions == null || definition.actions.Length == 0)
                issues.Add("Actions is empty. This is fine as a placeholder, but assigned combat profiles need at least one action.");

            return issues;
        }
    }
}
