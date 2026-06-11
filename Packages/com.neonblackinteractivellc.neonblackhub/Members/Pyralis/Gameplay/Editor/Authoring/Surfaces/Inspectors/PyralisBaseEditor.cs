using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Composition;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    /// <summary>
    /// Base class for all Pyralis custom inspectors.
    /// Automatically integrates reflective AuthoringContract guidance and IRuntimeValidationProvider issues.
    /// </summary>
    public abstract class PyralisBaseEditor : UnityEditor.Editor
    {
        private PyralisAuthoringContract _contract;
        private bool _checkedContract;

        protected virtual void OnEnable()
        {
            if (target == null) return;
            _contract = PyralisAuthoringContractRegistry.FindByType(target.GetType());
            _checkedContract = true;
        }

        public override void OnInspectorGUI()
        {
            if (!_checkedContract) OnEnable();

            serializedObject.Update();

            if (_contract != null)
            {
                DrawPyralisOverlay();
            }

            DrawCustomInspector();

            if (_contract != null)
            {
                DrawValidationFooter();
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Override this to draw your custom inspector content.
        /// Defaults to DrawDefaultInspector().
        /// </summary>
        protected virtual void DrawCustomInspector()
        {
            DrawDefaultInspector();
        }

        private void DrawPyralisOverlay()
        {
            List<PyralisGuideSection> sections = new List<PyralisGuideSection>();
            
            if (!string.IsNullOrEmpty(_contract.Relevance))
                sections.Add(new PyralisGuideSection("Relevance", _contract.Relevance));
            
            if (!string.IsNullOrEmpty(_contract.ExpertAdvice))
                sections.Add(new PyralisGuideSection("Expert Advice", _contract.ExpertAdvice));

            if (_contract.NativeSetup != null && _contract.NativeSetup.Length > 0)
                sections.Add(new PyralisGuideSection("Setup Steps", null, _contract.NativeSetup));

            if (!string.IsNullOrEmpty(_contract.FirstProofTargetId))
                sections.Add(new PyralisGuideSection("First Proof", _contract.FirstProofTargetId));

            PyralisInspectorGuide.DrawFieldGuide(
                _contract.DisplayName + " (Pyralis Guide)",
                false, 
                sections.ToArray());
        }

        private void DrawValidationFooter()
        {
            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            // 1. Validate AssignmentFields
            if (_contract.AssignmentFields != null && _contract.AssignmentFields.Length > 0)
            {
                foreach (var field in _contract.AssignmentFields)
                {
                    SerializedProperty prop = serializedObject.FindProperty(field);
                    if (prop == null) continue;
                    
                    if (IsPropertyUnassigned(prop))
                        errors.Add($"{prop.displayName} is unassigned.");
                }
            }

            // 2. Support IRuntimeValidationProvider
            if (target is IRuntimeValidationProvider provider)
            {
                foreach (var issue in provider.GetRuntimeValidationIssues())
                {
                    warnings.Add(issue);
                }
            }

            // 3. Draw Messages
            foreach (var error in errors)
                EditorGUILayout.HelpBox(error, MessageType.Error);
            
            foreach (var warning in warnings)
                EditorGUILayout.HelpBox(warning, MessageType.Warning);

            if (errors.Count == 0 && warnings.Count == 0)
            {
                EditorGUILayout.HelpBox(_contract.DisplayName + " is ready for runtime.", MessageType.Info);
            }
        }

        private bool IsPropertyUnassigned(SerializedProperty prop)
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