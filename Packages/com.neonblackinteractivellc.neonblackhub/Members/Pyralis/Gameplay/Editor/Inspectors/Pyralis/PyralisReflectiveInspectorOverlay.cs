using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    /// <summary>
    /// Global inspector overlay that reflectively injects Pyralis authoring guidance
    /// into any MonoBehavior or ScriptableObject that has an [AuthoringContract].
    /// </summary>
    [CustomEditor(typeof(Object), true)]
    [CanEditMultipleObjects]
    public sealed class PyralisReflectiveInspectorOverlay : UnityEditor.Editor
    {
        private ResolvedAuthoringContract _contract;
        private bool _checkedContract;

        private void OnEnable()
        {
            if (target == null) return;
            
            _contract = ResolvedAuthoringContractRegistry.FindByType(target.GetType());
            _checkedContract = true;
        }

        public override void OnInspectorGUI()
        {
            if (!_checkedContract) OnEnable();

            if (_contract != null)
            {
                PyralisResolvedInspectorValidation.DrawHeader(_contract);
            }

            DrawDefaultInspector();

            if (_contract != null)
            {
                PyralisResolvedInspectorValidation.DrawValidationFooter(_contract, target, serializedObject);
            }
        }
    }

    internal static class PyralisResolvedInspectorValidation
    {
        public static void DrawHeader(ResolvedAuthoringContract contract)
        {
            string context = string.IsNullOrWhiteSpace(contract.DisplayName)
                ? "Pyralis Authoring"
                : contract.DisplayName;
            PyralisInspectorHandoff.DrawAuthoringButton(
                context,
                "Local contract surface. Use Pyralis Authoring for route guidance and proof readiness.");
        }

        public static void DrawValidationFooter(ResolvedAuthoringContract contract, Object target, SerializedObject serializedObject)
        {
            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            if (contract.AssignmentFields != null && contract.AssignmentFields.Length > 0)
            {
                foreach (var field in contract.AssignmentFields)
                {
                    SerializedProperty prop = serializedObject.FindProperty(field);
                    if (prop == null) continue;
                    
                    if (IsPropertyUnassigned(prop))
                        errors.Add($"{prop.displayName} is unassigned.");
                }
            }

            if (target is IRuntimeValidationProvider provider)
            {
                foreach (var issue in provider.GetRuntimeValidationIssues())
                {
                    if (issue != null && !string.IsNullOrWhiteSpace(issue.Message))
                        warnings.Add(issue.Message);
                }
            }

            foreach (var error in errors)
                EditorGUILayout.HelpBox(error, MessageType.Error);
            
            foreach (var warning in warnings)
                EditorGUILayout.HelpBox(warning, MessageType.Warning);

            if (errors.Count == 0 && warnings.Count == 0)
            {
                EditorGUILayout.HelpBox("No field-local contract issues found.", MessageType.Info);
            }
        }

        private static bool IsPropertyUnassigned(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue == null;
                case SerializedPropertyType.String:
                    return string.IsNullOrEmpty(prop.stringValue);
                case SerializedPropertyType.ArraySize:
                    return prop.arraySize == 0;
                default:
                    return false;
            }
        }
    }
}
