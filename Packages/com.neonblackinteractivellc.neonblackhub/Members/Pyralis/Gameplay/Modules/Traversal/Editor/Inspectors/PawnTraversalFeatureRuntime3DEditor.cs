using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Traversal.Editor
{
    [CustomEditor(typeof(PawnTraversalFeatureRuntime3D))]
    public sealed class PawnTraversalFeatureRuntime3DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorHandoff.DrawAuthoringButton("Pawn Traversal Feature Runtime 3D", null);

            DrawDefaultInspector();
            PyralisInspectorValidation.DrawValidationMessages(GetMessages(serializedObject, (PawnTraversalFeatureRuntime3D)target), "PawnTraversalFeatureRuntime3D is ready for 3D actor traversal feature wiring.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisInspectorValidationIssue> GetMessages(SerializedObject serializedObject, PawnTraversalFeatureRuntime3D runtime)
        {
            List<PyralisInspectorValidationIssue> messages = new List<PyralisInspectorValidationIssue>();
            SerializedProperty profile = serializedObject.FindProperty("traversalProfile");
            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(PyralisInspectorValidationIssue.Optional("Traversal Profile is empty. This is expected when FeatureModuleDefinition provides the PawnTraversalProfile at runtime."));

            GameObject root = runtime != null ? runtime.gameObject : null;
            if (root != null && root.GetComponent<Pawn3DTraversalComponent>() == null)
                messages.Add(PyralisInspectorValidationIssue.Required("Pawn3DTraversalComponent is required on the same GameObject."));

            if (root != null && root.GetComponent<Motor3D>() == null)
                messages.Add(PyralisInspectorValidationIssue.Required("Motor3D is missing from this actor root."));

            if (root != null && root.GetComponent<Pawn3DMovementComponent>() == null)
                messages.Add(PyralisInspectorValidationIssue.Required("Pawn3DMovementComponent is missing from this actor root."));

            return messages;
        }
    }
}
