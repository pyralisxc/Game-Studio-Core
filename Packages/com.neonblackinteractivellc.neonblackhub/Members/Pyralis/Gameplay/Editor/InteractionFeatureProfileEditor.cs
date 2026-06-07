using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(InteractionFeatureProfile))]
    public class InteractionFeatureProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            InteractionFeatureProfile profile = (InteractionFeatureProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Interaction Feature Profile",
                "An interaction profile tunes generic actor interaction behavior: cooldown, fallback animation, and whether interaction is active.",
                whenToUse: new[]
                {
                    "Use this for actors that can press, open, talk, collect, inspect, or trigger nearby interactables.",
                    "Use it with an interaction FeatureModuleDefinition on pawns, enemies, or camera/cursor controlled setups."
                },
                createBefore: new[]
                {
                    "Interactable components or systems the actor will talk to.",
                    "FeatureModuleDefinition with an interaction runtime prefab."
                },
                assignFirst: new[]
                {
                    "Enable Interaction.",
                    "Set Interaction Cooldown to prevent repeated accidental triggers.",
                    "Decide whether unhandled interactions should still trigger an animation signal."
                },
                safeToCustomize: new[]
                {
                    "Cooldown can be zero for menu/cursor style interaction.",
                    "Fallback animation is useful for character actors and can be off for camera-only games."
                },
                validation: new[]
                {
                    "A runtime interaction receiver exists on the actor or scene.",
                    "InputProfile has an interact action when player-owned."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("AUTHORING_MODEL.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Interaction feature profile is ready for feature-module assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(InteractionFeatureProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile != null && profile.enableInteraction && profile.interactionCooldown < 0f)
                issues.Add("Interaction Cooldown cannot be negative.");

            return issues;
        }
    }
}
