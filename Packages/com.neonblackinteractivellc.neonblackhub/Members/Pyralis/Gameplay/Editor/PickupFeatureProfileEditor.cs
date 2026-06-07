using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(PickupFeatureProfile))]
    public class PickupFeatureProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            PickupFeatureProfile profile = (PickupFeatureProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Pickup Feature Profile",
                "A pickup profile controls how an actor finds and collects pickup objects in 2D and 3D scenes.",
                whenToUse: new[]
                {
                    "Use this when a pawn or enemy can auto-collect or interact-collect pickups.",
                    "Keep pickup behavior profile-driven so arcade, platformer, and tabletop scenes can choose different collection rules."
                },
                createBefore: new[]
                {
                    "Pickup prefab with the expected collectible component/layer.",
                    "FeatureModuleDefinition for pickup behavior."
                },
                assignFirst: new[]
                {
                    "Enable Auto Collect and/or Interaction Collect.",
                    "Set 2D Collectible Layers and 3D Collectible Layers.",
                    "Tune Interaction Radius and Overlap Radius 3D."
                },
                safeToCustomize: new[]
                {
                    "Prefer Nearest Pickup is helpful when several pickups overlap.",
                    "Use interaction-only collection for deliberate tabletop/cursor selection."
                },
                validation: new[]
                {
                    "At least one collection path is enabled.",
                    "Layer masks include the pickup prefabs used by the scene.",
                    "Radii match the actor scale."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pickups_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Pickup feature profile is ready for pickup feature setup.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(PickupFeatureProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile != null && !profile.enableAutoCollect && !profile.enableInteractionCollect)
                issues.Add("Both auto collect and interaction collect are disabled. This is valid only for temporarily disabling pickups.");

            return issues;
        }
    }
}
