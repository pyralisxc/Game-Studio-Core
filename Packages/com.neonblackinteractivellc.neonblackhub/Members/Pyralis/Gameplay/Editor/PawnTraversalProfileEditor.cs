using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(PawnTraversalProfile))]
    public class PawnTraversalProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            PawnTraversalProfile profile = (PawnTraversalProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Pawn Traversal Profile",
                "A pawn traversal profile stores optional movement abilities such as jump, climb, hang, dodge, and crouch.",
                whenToUse: new[]
                {
                    "Use this when a pawn needs platforming, dodge movement, climb zones, ledges, hang states, or crouch behavior.",
                    "Leave it simple for brawlers or fighters that only need jump/crouch."
                },
                createBefore: new[]
                {
                    "PawnDefinition that will reference this profile.",
                    "Pawn prefab with traversal components when climb, hang, dodge, or advanced traversal is enabled."
                },
                assignFirst: new[]
                {
                    "Enable only the traversal abilities this game actually uses.",
                    "Tune jump height, gravity, dodge distance/duration, and cooldowns.",
                    "Add scene traversal zones only after the basic pawn moves correctly."
                },
                safeToCustomize: new[]
                {
                    "Allow Jump and Allow Crouch are common defaults.",
                    "Climb and Hang should match actual climb/ledge scene components.",
                    "Dodge values can be tuned independently from base movement speed."
                },
                validation: new[]
                {
                    "PawnDefinition references this profile when traversal is needed.",
                    "Scene has climb/ledge zones when climb or hang is enabled.",
                    "Durations and distances are positive after OnValidate."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Pawn traversal profile is ready for PawnDefinition assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(PawnTraversalProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile != null && profile.allowDodge && profile.dodgeDuration <= 0f)
                issues.Add("Dodge Duration should be greater than zero when dodge is enabled.");

            return issues;
        }
    }
}
