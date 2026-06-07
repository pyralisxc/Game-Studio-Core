using NeonBlack.Gameplay.Features.Combat;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(ProjectileImpactDefinition))]
    public class ProjectileImpactDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            ProjectileImpactDefinition definition = (ProjectileImpactDefinition)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Projectile Impact Definition",
                "A projectile impact definition controls hit/miss VFX, audio, hit pause, camera shake, and miss effect behavior.",
                whenToUse: new[]
                {
                    "Use this when projectiles should share impact feedback across weapons or actions.",
                    "Use one impact definition per feedback family, not necessarily one per projectile."
                },
                createBefore: new[]
                {
                    "Hit and miss effect prefabs if visual feedback is needed.",
                    "Audio clips for hit and miss responses.",
                    "ProjectileDefinition assets that will reference this impact."
                },
                assignFirst: new[]
                {
                    "Set Impact Id and Display Name.",
                    "Assign hit/miss effect prefabs and sounds.",
                    "Enable hit pause or camera shake only when the impact should feel heavy."
                },
                safeToCustomize: new[]
                {
                    "Hit Pause can stay off for rapid-fire weapons.",
                    "Camera Shake should be subtle for repeated projectile hits.",
                    "Spawn Miss Effect At Max Distance is useful for hitscan tracers and magic rays."
                },
                validation: new[]
                {
                    "Enabled hit pause has positive duration.",
                    "Enabled camera shake has positive intensity and duration.",
                    "Effect Lifetime matches the VFX prefab duration."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Combat_Definitions_Setup.md")));

            List<string> issues = definition != null ? definition.GetValidationIssues() : new List<string>();
            PyralisInspectorGuide.DrawValidationIssues(issues, "Projectile impact definition is ready for projectile assignment.");

            serializedObject.ApplyModifiedProperties();
        }
    }
}
