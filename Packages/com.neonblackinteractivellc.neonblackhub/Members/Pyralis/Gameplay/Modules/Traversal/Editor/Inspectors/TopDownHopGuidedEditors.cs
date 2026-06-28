using System.Collections.Generic;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Traversal.Editor
{
    [CustomEditor(typeof(TopDownHopProfile))]
    public sealed class TopDownHopProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            PyralisInspectorValidation.DrawValidationIssues(GetIssues((TopDownHopProfile)target), "Top-down hop profile is ready for direct TopDownHopComponent assignment.");
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

    [CustomEditor(typeof(TopDownHopComponent))]
    public sealed class TopDownHopComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            PyralisInspectorValidation.DrawValidationMessages(GetMessages(serializedObject), "Top-down hop runtime is ready for direct pawn component setup.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisInspectorValidationIssue> GetMessages(SerializedObject serializedObject)
        {
            List<PyralisInspectorValidationIssue> messages = new List<PyralisInspectorValidationIssue>();
            SerializedProperty profile = serializedObject.FindProperty("hopProfile");
            SerializedProperty visual = serializedObject.FindProperty("visualTransform");

            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(PyralisInspectorValidationIssue.Required("Assign a TopDownHopProfile so this direct pawn component can run the hop action."));

            if (visual != null && visual.objectReferenceValue == null)
                messages.Add(PyralisInspectorValidationIssue.Optional("Visual Transform is empty. Runtime will lift a child SpriteRenderer or Animator when possible. Assign this field when the pawn art lives under a specific child."));

            return messages;
        }
    }
}
