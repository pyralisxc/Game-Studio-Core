using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Composition;
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
                PyralisResolvedInspectorGuide.DrawHeader(_contract);
            }

            DrawDefaultInspector();

            if (_contract != null)
            {
                PyralisResolvedInspectorGuide.DrawValidationFooter(_contract, target, serializedObject);
            }
        }
    }

    internal static class PyralisResolvedInspectorGuide
    {
        public static void DrawHeader(ResolvedAuthoringContract contract)
        {
            List<PyralisGuideSection> sections = new List<PyralisGuideSection>();
            
            if (!string.IsNullOrEmpty(contract.Relevance))
                sections.Add(new PyralisGuideSection("Relevance", contract.Relevance));
            
            if (!string.IsNullOrEmpty(contract.ExpertAdvice))
                sections.Add(new PyralisGuideSection("Expert Advice", contract.ExpertAdvice));

            if (contract.NativeSetup != null && contract.NativeSetup.Length > 0)
                sections.Add(new PyralisGuideSection("Setup Steps", null, contract.NativeSetup));

            if (!string.IsNullOrEmpty(contract.FirstProofTargetId))
                sections.Add(new PyralisGuideSection("First Proof", contract.FirstProofTargetId));

            PyralisInspectorGuide.DrawFieldGuide(
                contract.DisplayName + " (Pyralis Guide)",
                false, 
                sections.ToArray());
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
                    warnings.Add(issue);
                }
            }

            foreach (var error in errors)
                EditorGUILayout.HelpBox(error, MessageType.Error);
            
            foreach (var warning in warnings)
                EditorGUILayout.HelpBox(warning, MessageType.Warning);

            if (errors.Count == 0 && warnings.Count == 0)
            {
                EditorGUILayout.HelpBox(contract.DisplayName + " is ready for runtime.", MessageType.Info);
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

    internal static class PyralisInspectorHandoff
    {
        public static void DrawAuthoringButton()
        {
            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Open Pyralis Authoring"))
                NeonBlack.Gameplay.Editor.PyralisAuthoringWindow.Open();
        }
    }
}
