using System.Collections.Generic;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Modules.Feedback;
using UnityEditor;

namespace NeonBlack.Gameplay.Modules.Feedback.Editor
{
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
