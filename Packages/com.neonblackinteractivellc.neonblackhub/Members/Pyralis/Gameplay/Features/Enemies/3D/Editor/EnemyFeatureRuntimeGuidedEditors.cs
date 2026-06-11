using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Enemies;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies.Editor
{
    [CustomEditor(typeof(EnemyAmbientFeatureRuntime))]
    public sealed class EnemyAmbientFeatureRuntimeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawEnemyFeatureGuide(
                "Inspector Field Guide: Enemy Ambient Feature Runtime",
                "EnemyAmbientFeatureRuntime plays ambient look-around animation signals while an enemy is patrolling and not reaction-locked.",
                PyralisAuthoringContractGuideText.FeatureModuleSetup((EnemyAmbientFeatureRuntime)target),
                "Place this on a feature runtime prefab installed by ActorFeatureHost on an EnemyAI actor.");

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetProfileMessages(serializedObject, "ambientProfile", "EnemyAmbientFeatureProfile"), "EnemyAmbientFeatureRuntime is ready for enemy ambient feature wiring.");
            serializedObject.ApplyModifiedProperties();
        }

        internal static void DrawEnemyFeatureGuide(string title, string whatThisIs, string moduleSetup, string actorSetup)
        {
            PyralisInspectorGuide.DrawFieldGuide(
                title,
                new PyralisGuideSection(
                    "What This Is",
                    whatThisIs,
                    manualPath: PyralisInspectorGuide.AuthoringDocPath("RUNTIME_PATTERN_COOKBOOK.md")),
                new PyralisGuideSection(
                    "Feature Module Fields",
                    null,
                    new[]
                    {
                        moduleSetup,
                        "Set Runtime Prefab to a prefab containing this runtime component.",
                        "Keep Supported Presentation Modes aligned with the enemy prefab this feature targets.",
                        "Use EnemyFeatureProfile or another actor setup route to install the feature at runtime."
                    }),
                new PyralisGuideSection(
                    "Actor Fields",
                    null,
                    new[] { actorSetup }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not place this runtime component directly on a scene enemy unless a custom bootstrap initializes it.",
                        "Do not leave the profile type mismatched with the runtime's expected profile.",
                        "Do not expect animation signals without ActorAnimationDriver on the actor stack."
                    }));
        }

        private static List<PyralisGuideIssue> GetProfileMessages(SerializedObject serializedObject, string propertyName, string expectedProfileName)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty profile = serializedObject.FindProperty(propertyName);
            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional(profile.displayName + " is empty. This is expected when FeatureModuleDefinition provides the " + expectedProfileName + " at runtime."));

            return messages;
        }
    }

    [CustomEditor(typeof(EnemyReactionFeatureRuntime))]
    public sealed class EnemyReactionFeatureRuntimeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EnemyAmbientFeatureRuntimeEditor.DrawEnemyFeatureGuide(
                "Inspector Field Guide: Enemy Reaction Feature Runtime",
                "EnemyReactionFeatureRuntime converts enemy damage and death events into hurt, stagger, hit pause, camera shake, knockback clearing, and actor feedback signals.",
                PyralisAuthoringContractGuideText.FeatureModuleSetup((EnemyReactionFeatureRuntime)target),
                "The enemy actor should have HealthComponent, ActorAnimationDriver, KnockbackReceiver when reactions or knockback are used, and explicit impact feedback sinks when hit pause or camera shake is enabled.");

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetProfileMessages(serializedObject), "EnemyReactionFeatureRuntime is ready for enemy reaction feature wiring.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetProfileMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty profile = serializedObject.FindProperty("reactionProfile");
            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Reaction Profile is empty. This is expected when FeatureModuleDefinition provides the EnemyReactionProfile at runtime."));

            RequireOptionalInterface<IHitPauseSink>(serializedObject, messages, "hitPauseSink", "Hit Pause Sink", "IHitPauseSink");
            RequireOptionalInterface<ICameraShakeSink>(serializedObject, messages, "cameraShakeSink", "Camera Shake Sink", "ICameraShakeSink");
            return messages;
        }

        private static void RequireOptionalInterface<T>(SerializedObject serializedObject, List<PyralisGuideIssue> messages, string propertyName, string displayName, string interfaceName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == null)
                return;

            if (!(property.objectReferenceValue is T))
                messages.Add(PyralisGuideIssue.Required(displayName + " must reference a component that implements " + interfaceName + "."));
        }
    }
}
