using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal.Editor
{
    [CustomEditor(typeof(TopDownHopProfile))]
    public sealed class TopDownHopProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Guided Authoring: Top Down Hop Profile",
                defaultOpen: false,
                new PyralisGuideSection(
                    "What This Is",
                    "TopDownHopProfile tunes a top-down or isometric hop. The pawn remains on the map plane while a visual child lifts on an arc.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")),
                new PyralisGuideSection(
                    "Use This For",
                    null,
                    new[]
                    {
                        "Zelda-like, isometric, tactics, or arcade top-down hop actions.",
                        "Animation-only or presentation-first jump actions where map X/Y movement should stay free.",
                        "Jump input that should not switch the pawn into side-view Rigidbody2D gravity."
                    }),
                new PyralisGuideSection(
                    "Customize Here",
                    null,
                    new[]
                    {
                        "Action Role chooses which InputProfile row triggers the hop. Jump is the usual beginner route.",
                        "Duration and Height tune the visible arc.",
                        "Cooldown prevents accidental repeated hops.",
                        "Trigger Jump Animation fires ActorAnimationSignal.Jump when the actor has an ActorAnimationDriver."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(GetIssues((TopDownHopProfile)target), "Top-down hop profile is ready for a FeatureModuleDefinition.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetIssues(TopDownHopProfile profile)
        {
            List<string> issues = new List<string>();
            if (profile == null)
                return issues;

            if (profile.duration <= 0f)
                issues.Add("Duration must be greater than zero.");
            if (profile.height <= 0f)
                issues.Add("Height should be greater than zero for a visible hop.");

            return issues;
        }
    }

    [CustomEditor(typeof(TopDownHopFeatureRuntime))]
    public sealed class TopDownHopFeatureRuntimeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Top Down Hop Feature Runtime",
                defaultOpen: false,
                new PyralisGuideSection(
                    "What This Is",
                    "TopDownHopFeatureRuntime handles an authored gameplay action such as Jump by lifting the actor visual while the map-plane body stays in place.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")),
                new PyralisGuideSection(
                    "Required Route",
                    null,
                    new[]
                    {
                        PyralisAuthoringContractGuideText.FeatureModuleSetup((TopDownHopFeatureRuntime)target),
                        "Assign a TopDownHopProfile to FeatureModuleDefinition > Profile Asset.",
                        "Add that FeatureModuleDefinition to PawnDefinition > Feature Modules.",
                        "Keep PawnMovementProfile in top-down/free 2D when Move should still drive X/Y."
                    }),
                new PyralisGuideSection(
                    "Customize Here",
                    null,
                    new[]
                    {
                        "Visual Transform can stay empty when the runtime should lift the first child SpriteRenderer or Animator.",
                        "Assign Visual Transform explicitly when the sprite art lives under a specific child.",
                        "Do not use this for side-view/platformer physics jump; use PawnMovementProfile > Allow 2D Jump for that route."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetMessages(serializedObject), "Top-down hop runtime is ready for feature-module installation.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty profile = serializedObject.FindProperty("hopProfile");
            SerializedProperty visual = serializedObject.FindProperty("visualTransform");

            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Hop Profile is empty. This is expected when FeatureModuleDefinition provides a TopDownHopProfile at runtime."));

            if (visual != null && visual.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Visual Transform is empty. Runtime will lift a child SpriteRenderer or Animator when possible. Assign this field when the pawn art lives under a specific child."));

            return messages;
        }
    }
}
