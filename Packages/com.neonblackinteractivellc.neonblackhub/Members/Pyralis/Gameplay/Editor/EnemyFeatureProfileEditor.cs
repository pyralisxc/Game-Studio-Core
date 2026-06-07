using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(EnemyFeatureProfile))]
    public class EnemyFeatureProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            EnemyFeatureProfile profile = (EnemyFeatureProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Enemy Feature Profile",
                "An enemy feature profile gathers enemy combat, reactions, and actor feature modules into one reusable enemy setup asset.",
                whenToUse: new[]
                {
                    "Use this for enemies that need authored combat behavior, reaction tuning, pickups, status, or feedback modules.",
                    "Use Feature Modules to attach optional systems without hard-coding every enemy prefab."
                },
                createBefore: new[]
                {
                    "EnemyCombatProfile when the enemy attacks.",
                    "EnemyReactionProfile when the enemy responds to hits, stagger, or death.",
                    "FeatureModuleDefinition assets for optional actor features."
                },
                assignFirst: new[]
                {
                    "Assign Combat Profile for attack selection.",
                    "Assign Reaction Profile for hit/stagger feel.",
                    "Add Feature Modules used by this enemy archetype."
                },
                safeToCustomize: new[]
                {
                    "Combat and reaction profiles can be shared across many enemy types.",
                    "Feature module order is less important than unique module ids, but duplicates should be fixed."
                },
                validation: new[]
                {
                    "Feature module ids are unique.",
                    "Modules support the enemy presentation mode.",
                    "Runtime prefabs and profile assets match each module id."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Enemy_Setup.md")));

            List<string> issues = profile != null ? profile.GetValidationIssues() : new List<string>();
            PyralisInspectorGuide.DrawValidationIssues(issues, "Enemy feature profile is ready for enemy prefab assignment.");

            serializedObject.ApplyModifiedProperties();
        }
    }
}
