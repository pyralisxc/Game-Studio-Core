using System.Collections.Generic;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Data.Participants;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Traversal.Editor
{
    [CustomEditor(typeof(Pawn3DTraversalComponent))]
    public sealed class Pawn3DTraversalComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            InspectorValidation.DrawValidationMessages(GetMessages(serializedObject, (Pawn3DTraversalComponent)target), "Pawn3DTraversalComponent is ready for direct 3D pawn traversal setup.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<InspectorValidationIssue> GetMessages(SerializedObject serializedObject, Pawn3DTraversalComponent runtime)
        {
            List<InspectorValidationIssue> messages = new List<InspectorValidationIssue>();
            SerializedProperty profile = serializedObject.FindProperty("traversalProfile");
            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(InspectorValidationIssue.Required("Assign a PawnTraversalProfile so this direct pawn component can apply traversal tuning."));

            GameObject root = runtime != null ? runtime.gameObject : null;
            if (root != null && root.GetComponent<Pawn3DTraversalComponent>() == null)
                messages.Add(InspectorValidationIssue.Required("Pawn3DTraversalComponent is required on the same GameObject."));

            if (root != null && !HasComponent(root, "NeonBlack.Gameplay.Modules.Character.Motor3D"))
                messages.Add(InspectorValidationIssue.Required("Motor3D is missing from this actor root."));

            if (root != null && root.GetComponent<IPawnTraversalMovementController>() == null)
                messages.Add(InspectorValidationIssue.Required("Pawn3DMovementComponent is missing from this actor root."));

            return messages;
        }

        private static bool HasComponent(GameObject root, string typeFullName)
        {
            if (root == null || string.IsNullOrWhiteSpace(typeFullName))
                return false;

            Component[] components = root.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component != null && component.GetType().FullName == typeFullName)
                    return true;
            }

            return false;
        }
    }
}
