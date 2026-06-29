using System.Collections.Generic;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Modules.Feedback;
using UnityEditor;

namespace NeonBlack.Gameplay.Modules.Feedback.Editor
{
    [CustomEditor(typeof(DamageNumber))]
    public sealed class DamageNumberEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            InspectorValidation.DrawValidationMessages(GetDamageNumberMessages(serializedObject), "DamageNumber is ready for pooled floating feedback.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<InspectorValidationIssue> GetDamageNumberMessages(SerializedObject serializedObject)
        {
            List<InspectorValidationIssue> messages = new List<InspectorValidationIssue>();
            RequirePositive(serializedObject, messages, "riseSpeed", "Rise Speed");
            RequireNonNegative(serializedObject, messages, "horizontalScatter", "Horizontal Scatter");
            RequirePositive(serializedObject, messages, "lifetime", "Lifetime");
            RequirePositive(serializedObject, messages, "fontSize", "Font Size");
            RequirePositive(serializedObject, messages, "criticalSizeMultiplier", "Critical Size Multiplier");
            return messages;
        }

        private static void RequirePositive(SerializedObject serializedObject, List<InspectorValidationIssue> messages, string propertyName, string displayName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null && property.floatValue <= 0f)
                messages.Add(InspectorValidationIssue.Required(displayName + " must be greater than zero."));
        }

        private static void RequireNonNegative(SerializedObject serializedObject, List<InspectorValidationIssue> messages, string propertyName, string displayName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null && property.floatValue < 0f)
                messages.Add(InspectorValidationIssue.Required(displayName + " cannot be negative."));
        }
    }

    [CustomEditor(typeof(DamageNumberSpawner))]
    public sealed class DamageNumberSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            InspectorValidation.DrawValidationMessages(GetSpawnerMessages(serializedObject), "DamageNumberSpawner is ready as an explicit damage-number sink.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<InspectorValidationIssue> GetSpawnerMessages(SerializedObject serializedObject)
        {
            List<InspectorValidationIssue> messages = new List<InspectorValidationIssue>();
            SerializedProperty initialPoolSize = serializedObject.FindProperty("initialPoolSize");
            SerializedProperty popupCamera = serializedObject.FindProperty("popupCamera");
            if (initialPoolSize != null && initialPoolSize.intValue < 1)
                messages.Add(InspectorValidationIssue.Required("Initial Pool Size should be at least 1."));

            if (popupCamera != null && popupCamera.objectReferenceValue == null)
                messages.Add(InspectorValidationIssue.Recommended("Popup Camera is empty. Numbers will not billboard until a camera is assigned at authoring time or runtime."));
            return messages;
        }
    }
}
