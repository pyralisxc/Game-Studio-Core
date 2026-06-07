using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(PawnPresentationProfile))]
    public class PawnPresentationProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            PawnPresentationProfile profile = (PawnPresentationProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Pawn Presentation Profile",
                "A pawn presentation profile tells Pyralis how a pawn looks and faces the camera: 2D sprite, 2.5D billboard, or rigged 3D.",
                whenToUse: new[]
                {
                    "Use this for every pawn-backed actor with visuals.",
                    "Pair it with PawnAnimationProfile when the pawn uses Animator-driven animation."
                },
                createBefore: new[]
                {
                    "PawnDefinition that will reference this profile.",
                    "Pawn prefab with sprite, billboard, or rigged model hierarchy.",
                    "Camera setup if Use Shared Camera is enabled."
                },
                assignFirst: new[]
                {
                    "Choose Presentation Mode.",
                    "Set billboard and facing options for Sprite2D or Billboard2_5D.",
                    "Set rig type for Rigged3D.",
                    "Configure shadow mode only after the main pawn visual works."
                },
                safeToCustomize: new[]
                {
                    "HUD Prefab can stay empty until world-space pawn HUD is needed.",
                    "Tint is safe for team/player identity.",
                    "Shadow prefab/sprite settings are optional visual polish."
                },
                validation: new[]
                {
                    "PawnDefinition references this profile.",
                    "Pawn prefab visual hierarchy matches the selected presentation mode.",
                    "Feature modules support this presentation mode."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Pawn presentation profile is ready for PawnDefinition assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(PawnPresentationProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile == null)
                return issues;

            if (profile.presentationMode == ActorPresentationMode.Rigged3D && !profile.castModelShadows && !profile.receiveModelShadows)
                issues.Add("Rigged3D pawns usually cast or receive model shadows unless the art style intentionally avoids them.");

            return issues;
        }
    }
}
