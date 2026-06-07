using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(EnemyReactionProfile))]
    public class EnemyReactionProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            EnemyReactionProfile profile = (EnemyReactionProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Enemy Reaction Profile",
                "An enemy reaction profile controls hit lock, stagger thresholds, hit pause, camera shake, and knockback cleanup for enemy actors.",
                whenToUse: new[]
                {
                    "Use this when enemy hits should create readable reaction timing.",
                    "Share one profile across enemy families, then branch profiles for bosses or heavy units."
                },
                createBefore: new[]
                {
                    "EnemyFeatureProfile that will reference this reaction profile.",
                    "Combat or damage system that publishes hit/stagger/death events."
                },
                assignFirst: new[]
                {
                    "Set Hurt Lock Duration for light hit readability.",
                    "Set Stagger Damage Threshold and Stagger Lock Duration for heavier reactions.",
                    "Tune Hit Pause and Camera Shake lightly."
                },
                safeToCustomize: new[]
                {
                    "Zero durations are allowed for snappy arcade enemies.",
                    "Clear Knockback On Death is usually helpful for controlled death presentation."
                },
                validation: new[]
                {
                    "Reaction locks are not longer than the enemy's expected attack cadence.",
                    "Camera shake is subtle enough for repeated hits."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Enemy_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Enemy reaction profile is ready for enemy feature setup.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(EnemyReactionProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile != null && profile.enableReactions && profile.hurtLockDuration <= 0f && profile.staggerLockDuration <= 0f)
                issues.Add("Reactions are enabled, but both hurt and stagger lock durations are zero.");

            return issues;
        }
    }
}
