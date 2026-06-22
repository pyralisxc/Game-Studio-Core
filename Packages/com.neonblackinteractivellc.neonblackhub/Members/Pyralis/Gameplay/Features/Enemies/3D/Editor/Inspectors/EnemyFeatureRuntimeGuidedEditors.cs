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

            PyralisInspectorHandoff.DrawAuthoringButton("Enemy Ambient Feature Runtime", null);

            DrawDefaultInspector();
            PyralisInspectorValidation.DrawValidationMessages(GetProfileMessages(serializedObject, "ambientProfile", "EnemyAmbientFeatureProfile"), "EnemyAmbientFeatureRuntime is ready for enemy ambient feature wiring.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisInspectorValidationIssue> GetProfileMessages(SerializedObject serializedObject, string propertyName, string expectedProfileName)
        {
            List<PyralisInspectorValidationIssue> messages = new List<PyralisInspectorValidationIssue>();
            SerializedProperty profile = serializedObject.FindProperty(propertyName);
            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(PyralisInspectorValidationIssue.Optional(profile.displayName + " is empty. This is expected when FeatureModuleDefinition provides the " + expectedProfileName + " at runtime."));

            return messages;
        }
    }

    [CustomEditor(typeof(EnemyReactionFeatureRuntime))]
    public sealed class EnemyReactionFeatureRuntimeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorHandoff.DrawAuthoringButton("Enemy Reaction Feature Runtime", null);

            DrawDefaultInspector();
            PyralisInspectorValidation.DrawValidationMessages(GetProfileMessages(serializedObject), "EnemyReactionFeatureRuntime is ready for enemy reaction feature wiring.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisInspectorValidationIssue> GetProfileMessages(SerializedObject serializedObject)
        {
            List<PyralisInspectorValidationIssue> messages = new List<PyralisInspectorValidationIssue>();
            SerializedProperty profile = serializedObject.FindProperty("reactionProfile");
            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(PyralisInspectorValidationIssue.Optional("Reaction Profile is empty. This is expected when FeatureModuleDefinition provides the EnemyReactionProfile at runtime."));

            RequireOptionalInterface<IHitPauseSink>(serializedObject, messages, "hitPauseSink", "Hit Pause Sink", "IHitPauseSink");
            RequireOptionalInterface<ICameraShakeSink>(serializedObject, messages, "cameraShakeSink", "Camera Shake Sink", "ICameraShakeSink");
            return messages;
        }

        private static void RequireOptionalInterface<T>(SerializedObject serializedObject, List<PyralisInspectorValidationIssue> messages, string propertyName, string displayName, string interfaceName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == null)
                return;

            if (!(property.objectReferenceValue is T))
                messages.Add(PyralisInspectorValidationIssue.Required(displayName + " must reference a component that implements " + interfaceName + "."));
        }
    }
}
