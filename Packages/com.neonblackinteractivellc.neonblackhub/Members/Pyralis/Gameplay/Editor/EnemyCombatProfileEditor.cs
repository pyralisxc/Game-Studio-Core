using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(EnemyCombatProfile))]
    public class EnemyCombatProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            EnemyCombatProfile profile = (EnemyCombatProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Enemy Combat Profile",
                "An enemy combat profile controls attack lists, attack selection mode, range preference, cooldowns, and weighted decision tuning.",
                whenToUse: new[]
                {
                    "Use this for enemy archetypes that attack using authored EnemyAttack assets.",
                    "Use sequential mode for predictable patterns and random/priority selection for reactive AI."
                },
                createBefore: new[]
                {
                    "EnemyAttack assets for every selectable attack.",
                    "EnemyFeatureProfile or enemy prefab that will reference this profile."
                },
                assignFirst: new[]
                {
                    "Add attacks to Attack Sequence.",
                    "Choose Attack Mode and priority/range preferences.",
                    "Tune cooldown and selection weights."
                },
                safeToCustomize: new[]
                {
                    "Attack Range Override of zero lets attacks or AI perception decide.",
                    "Weight values are relative, so start simple.",
                    "Prefer Attacks Currently In Range is usually good for readable enemies."
                },
                validation: new[]
                {
                    "Attack Sequence has at least one attack for attacking enemies.",
                    "Attack assets match enemy prefab hit zones and animations.",
                    "Weights are non-negative."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Enemy_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Enemy combat profile is ready for enemy feature setup.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(EnemyCombatProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile == null)
                return issues;

            if (profile.attackSequence == null || profile.attackSequence.Length == 0)
                issues.Add("Attack Sequence is empty. Assign at least one EnemyAttack for attacking enemies.");

            return issues;
        }
    }
}
