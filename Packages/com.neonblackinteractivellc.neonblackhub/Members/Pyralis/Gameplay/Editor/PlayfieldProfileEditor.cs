using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(PlayfieldProfile))]
    public class PlayfieldProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            PlayfieldProfile profile = (PlayfieldProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Playfield Profile",
                "A playfield profile defines movement-space rules separately from camera behavior. It tells modes and movement systems how wide, tall, or deep the playable space is.",
                whenToUse: new[]
                {
                    "Use this for arenas, side-scrollers, board bounds, encounter zones, generated chunks, and any setup with movement limits.",
                    "Skip detailed bounds until the scene needs clamping, screen wrap, depth movement, or arena locking."
                },
                createBefore: new[]
                {
                    "GameModeDefinition that will reference this profile.",
                    "Runtime pattern decision for 2D, 2.5D, 3D, board/card/tabletop, or camera-only control."
                },
                assignFirst: new[]
                {
                    "Choose Movement Mode.",
                    "Enable Clamp To Bounds only when movement should be constrained.",
                    "Set Min/Max Bounds and depth range to match the scene."
                },
                safeToCustomize: new[]
                {
                    "Allow Screen Wrap is useful for arcade loops but unusual for brawlers or board games.",
                    "Use Depth Axis for 2.5D brawler/fighter lanes.",
                    "Lock Arena Until Wave Clear is a mode rule for encounter rooms."
                },
                validation: new[]
                {
                    "GameModeDefinition references this profile.",
                    "Min bounds are less than max bounds.",
                    "Camera profile and playfield bounds agree on the intended space."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("CANONICAL_SETUP.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Playfield profile is ready for game-mode assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static System.Collections.Generic.List<string> GetValidationIssues(PlayfieldProfile profile)
        {
            System.Collections.Generic.List<string> issues = new System.Collections.Generic.List<string>();

            if (profile == null)
                return issues;

            if (profile.clampToBounds && profile.minBounds.x > profile.maxBounds.x)
                issues.Add("Min Bounds X should not exceed Max Bounds X.");
            if (profile.clampToBounds && profile.minBounds.y > profile.maxBounds.y)
                issues.Add("Min Bounds Y should not exceed Max Bounds Y.");
            if (profile.useDepthAxis && profile.minDepth > profile.maxDepth)
                issues.Add("Min Depth should not exceed Max Depth.");

            return issues;
        }
    }
}
