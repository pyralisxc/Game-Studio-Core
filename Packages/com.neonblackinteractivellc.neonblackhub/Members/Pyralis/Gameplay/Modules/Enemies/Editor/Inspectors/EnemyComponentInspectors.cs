using System.Collections.Generic;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Enemies;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Enemies.Editor
{
    [CustomEditor(typeof(EnemyAmbientComponent))]
    public sealed class EnemyAmbientComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            InspectorValidation.DrawValidationMessages(GetProfileMessages(serializedObject, "ambientProfile", "EnemyAmbientProfile"), "EnemyAmbientComponent is ready for direct enemy ambient setup.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<InspectorValidationIssue> GetProfileMessages(SerializedObject serializedObject, string propertyName, string expectedProfileName)
        {
            List<InspectorValidationIssue> messages = new List<InspectorValidationIssue>();
            SerializedProperty profile = serializedObject.FindProperty(propertyName);
            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(InspectorValidationIssue.Required("Assign " + expectedProfileName + " to " + profile.displayName + " so this direct enemy component can run."));

            return messages;
        }
    }

    [CustomEditor(typeof(EnemyReactionComponent))]
    public sealed class EnemyReactionComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            InspectorValidation.DrawValidationMessages(GetProfileMessages(serializedObject), "EnemyReactionComponent is ready for direct enemy reaction setup.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<InspectorValidationIssue> GetProfileMessages(SerializedObject serializedObject)
        {
            List<InspectorValidationIssue> messages = new List<InspectorValidationIssue>();
            SerializedProperty profile = serializedObject.FindProperty("reactionProfile");
            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(InspectorValidationIssue.Required("Assign an EnemyReactionProfile so this direct enemy component can run reactions."));

            RequireOptionalInterface<IHitPauseSink>(serializedObject, messages, "hitPauseSink", "Hit Pause Sink", "IHitPauseSink");
            RequireOptionalInterface<ICameraShakeSink>(serializedObject, messages, "cameraShakeSink", "Camera Shake Sink", "ICameraShakeSink");
            return messages;
        }

        private static void RequireOptionalInterface<T>(SerializedObject serializedObject, List<InspectorValidationIssue> messages, string propertyName, string displayName, string interfaceName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == null)
                return;

            if (!(property.objectReferenceValue is T))
                messages.Add(InspectorValidationIssue.Required(displayName + " must reference a component that implements " + interfaceName + "."));
        }
    }
}
