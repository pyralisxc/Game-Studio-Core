using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(HazardImpactProfile))]
    public class HazardImpactProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            HazardImpactProfile profile = (HazardImpactProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Hazard Impact Profile",
                "A hazard impact profile defines what a hazard does on contact: target filtering, damage ticks, knockback, collectible destruction, and status effects.",
                whenToUse: new[]
                {
                    "Use this when several HazardData assets should share damage/knockback/status behavior.",
                    "Assign it to HazardData for direct hit payloads."
                },
                createBefore: new[]
                {
                    "StatusEffectDefinition assets if contact applies poison, burn, stun, slow, shield, or other effects.",
                    "HazardData asset that will reference this impact profile."
                },
                assignFirst: new[]
                {
                    "Set Effect Id and Targeting.",
                    "Set Damage Per Tick and Tick Interval.",
                    "Set Knockback Force and optional Status Effects."
                },
                safeToCustomize: new[]
                {
                    "Damage Per Tick can be zero for pure knockback or status hazards.",
                    "Status Effects can stay empty for simple damage hazards.",
                    "Destroy Collectibles On Contact is useful for obstacle hazards."
                },
                validation: new[]
                {
                    "Effect Id is stable.",
                    "Tick Interval is positive.",
                    "Status Effects has no missing entries."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Hazard_Difficulty_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Hazard impact profile is ready for HazardData assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(HazardImpactProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile == null)
                return issues;
            if (string.IsNullOrWhiteSpace(profile.effectId))
                issues.Add("Effect Id is required.");
            if (profile.tickInterval <= 0f)
                issues.Add("Tick Interval must be greater than zero.");

            if (profile.statusEffects != null)
            {
                for (int i = 0; i < profile.statusEffects.Length; i++)
                {
                    if (profile.statusEffects[i] == null)
                        issues.Add($"Status Effects[{i}] is empty.");
                }
            }

            return issues;
        }
    }
}
