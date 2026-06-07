using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(EnemyAmbientFeatureProfile))]
    public class EnemyAmbientFeatureProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            EnemyAmbientFeatureProfile profile = (EnemyAmbientFeatureProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Enemy Ambient Feature Profile",
                "An enemy ambient profile tunes light non-combat behavior such as look-around timing and patrol-state restrictions.",
                whenToUse: new[]
                {
                    "Use this for enemies that should feel alive while idle or patrolling.",
                    "Keep it separate from combat so ambient behavior can be reused or disabled per archetype."
                },
                createBefore: new[]
                {
                    "Enemy prefab with an ambient/AI runtime that consumes this profile.",
                    "FeatureModuleDefinition that represents the ambient enemy feature."
                },
                assignFirst: new[]
                {
                    "Enable Ambient Look Around if this enemy should scan while idle.",
                    "Set Look Around Interval to the desired idle rhythm.",
                    "Decide whether ambient behavior requires a patrol state."
                },
                safeToCustomize: new[]
                {
                    "Suppress During Reaction Lock should usually stay on so hit reactions stay readable.",
                    "Require Patrol State can be off for stationary look-around enemies."
                },
                validation: new[]
                {
                    "Look Around Interval is not too small for the enemy's animation timing.",
                    "The runtime feature exists on the enemy prefab."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Enemy_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Enemy ambient profile is ready for an enemy feature module.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(EnemyAmbientFeatureProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile != null && profile.enableAmbientLookAround && profile.lookAroundInterval <= 0f)
                issues.Add("Look Around Interval must be greater than zero when ambient look-around is enabled.");

            return issues;
        }
    }
}
