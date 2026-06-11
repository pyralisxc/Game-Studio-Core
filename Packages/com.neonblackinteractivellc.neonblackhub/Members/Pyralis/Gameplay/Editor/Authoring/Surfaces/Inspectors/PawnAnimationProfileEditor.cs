using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    [CustomEditor(typeof(PawnAnimationProfile))]
    public class PawnAnimationProfileEditor : PyralisBaseEditor
    {
        private SerializedProperty _animationDefinitionProperty;
        private SerializedProperty _baseControllerProperty;
        private SerializedProperty _spawnControllerOverrideProperty;
        private SerializedProperty _bindingsProperty;
        private bool _bindingsOpen = true;
        private bool _rawDebugOpen;

        protected override void OnEnable()
        {
            base.OnEnable();
            _animationDefinitionProperty = serializedObject.FindProperty("animationDefinition");
            _baseControllerProperty = serializedObject.FindProperty("baseController");
            _spawnControllerOverrideProperty = serializedObject.FindProperty("spawnControllerOverride");
            _bindingsProperty = serializedObject.FindProperty("bindings");
        }

        protected override void DrawCustomInspector()
        {
            PawnAnimationProfile profile = (PawnAnimationProfile)target;

            DrawCoreAssignments();
            DrawMappingWizard(profile);
            DrawAdvancedDebug();
        }

        private void DrawCoreAssignments()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Controller Assets", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_animationDefinitionProperty);
                EditorGUILayout.PropertyField(_baseControllerProperty);
                EditorGUILayout.PropertyField(_spawnControllerOverrideProperty);
            }
        }

        private void DrawMappingWizard(PawnAnimationProfile profile)
        {
            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Controller Mapping Wizard", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Use this workflow after assigning the Animator Controller your pawn visual actually equips. Suggested bindings are only a starting point; this profile remains the authority for how gameplay signals drive your chosen parameters.", MessageType.Info);

                DrawReadiness(profile);
                DrawToolButtons(profile);
                DrawIssueGroups(profile);
                DrawParameterInventory(profile);
                DrawBindingTable(profile);
            }
        }

        private static void DrawReadiness(PawnAnimationProfile profile)
        {
            PawnAnimationMappingSummary summary = NeonBlack.Gameplay.Editor.PawnAnimationProfileValidation.GetMappingSummary(profile);
            EditorGUILayout.LabelField("Readiness", summary.ReadinessLabel, EditorStyles.boldLabel);

            Rect progressRect = GUILayoutUtility.GetRect(18f, 18f);
            EditorGUI.ProgressBar(
                progressRect,
                summary.Coverage01,
                $"{summary.MappedSignalCount}/{summary.SupportedSignalCount} gameplay signals mapped");

            EditorGUILayout.LabelField(
                "Controller Parameters",
                summary.ControllerParameterCount.ToString());
            EditorGUILayout.LabelField(
                "Custom Blend Channels",
                summary.CustomChannelCount.ToString());
            EditorGUILayout.LabelField(
                "Validation Issues",
                summary.IssueCount.ToString());
        }

        private void DrawToolButtons(PawnAnimationProfile profile)
        {
            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(profile == null || profile.baseController == null);
                if (GUILayout.Button("Append Suggestions"))
                {
                    RunProfileMutation(profile, "Append Suggested Animation Bindings", () =>
                    {
                        NeonBlack.Gameplay.Editor.PawnAnimationProfileValidation.AppendSuggestedBindings(profile);
                    });
                }

                if (GUILayout.Button("Replace With Suggestions"))
                {
                    if (EditorUtility.DisplayDialog(
                        "Replace animation bindings?",
                        "This clears the current binding list and rebuilds it from the assigned Animator Controller parameters.",
                        "Replace",
                        "Cancel"))
                    {
                        RunProfileMutation(profile, "Replace Suggested Animation Bindings", () =>
                        {
                            NeonBlack.Gameplay.Editor.PawnAnimationProfileValidation.ReplaceWithSuggestedBindings(profile);
                        });
                    }
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(profile == null || _bindingsProperty.arraySize == 0);
                if (GUILayout.Button("Clear Bindings"))
                {
                    if (EditorUtility.DisplayDialog(
                        "Clear animation bindings?",
                        "This removes every binding from the profile.",
                        "Clear",
                        "Cancel"))
                    {
                        _bindingsProperty.ClearArray();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private static void DrawIssueGroups(PawnAnimationProfile profile)
        {
            Dictionary<string, List<string>> groups = NeonBlack.Gameplay.Editor.PawnAnimationProfileValidation.GetValidationIssueGroups(profile);
            if (groups.Count == 0)
            {
                EditorGUILayout.HelpBox("Pawn animation profile is ready for PawnDefinition assignment.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(4f);
            foreach (KeyValuePair<string, List<string>> group in groups)
            {
                if (group.Value == null || group.Value.Count == 0)
                    continue;

                string message = group.Key + Environment.NewLine + string.Join(Environment.NewLine, group.Value.Select(issue => "- " + issue));
                EditorGUILayout.HelpBox(message, MessageType.Warning);
            }
        }

        private static void DrawParameterInventory(PawnAnimationProfile profile)
        {
            IReadOnlyList<PawnAnimationParameterInfo> parameters = NeonBlack.Gameplay.Editor.PawnAnimationProfileValidation.GetInspectableParameters(profile);
            if (parameters.Count == 0)
                return;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Controller Parameters", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(string.Join(", ", parameters.Select(parameter => parameter.ToString()).ToArray()), EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawBindingTable(PawnAnimationProfile profile)
        {
            EditorGUILayout.Space(8f);
            _bindingsOpen = EditorGUILayout.Foldout(_bindingsOpen, $"Bindings ({_bindingsProperty.arraySize})", true, EditorStyles.foldoutHeader);
            if (!_bindingsOpen)
                return;

            IReadOnlyList<PawnAnimationParameterInfo> parameters = NeonBlack.Gameplay.Editor.PawnAnimationProfileValidation.GetInspectableParameters(profile);

            for (int i = 0; i < _bindingsProperty.arraySize; i++)
            {
                SerializedProperty bindingProperty = _bindingsProperty.GetArrayElementAtIndex(i);
                if (bindingProperty == null)
                    continue;

                DrawBindingRow(bindingProperty, parameters, i);
            }

            EditorGUILayout.Space(4f);
            if (GUILayout.Button("Add Binding"))
                AddEmptyBinding();
        }

        private void DrawBindingRow(
            SerializedProperty bindingProperty,
            IReadOnlyList<PawnAnimationParameterInfo> parameters,
            int index)
        {
            SerializedProperty signalProperty = bindingProperty.FindPropertyRelative("signal");
            SerializedProperty customKeyProperty = bindingProperty.FindPropertyRelative("customKey");
            SerializedProperty bindingTypeProperty = bindingProperty.FindPropertyRelative("bindingType");
            SerializedProperty parameterNameProperty = bindingProperty.FindPropertyRelative("parameterName");

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(GetBindingTitle(signalProperty, customKeyProperty, index), EditorStyles.boldLabel);

                    EditorGUI.BeginDisabledGroup(index == 0);
                    if (GUILayout.Button("Up", EditorStyles.miniButtonLeft, GUILayout.Width(42f)))
                        _bindingsProperty.MoveArrayElement(index, index - 1);
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup(index >= _bindingsProperty.arraySize - 1);
                    if (GUILayout.Button("Down", EditorStyles.miniButtonMid, GUILayout.Width(54f)))
                        _bindingsProperty.MoveArrayElement(index, index + 1);
                    EditorGUI.EndDisabledGroup();

                    if (GUILayout.Button("Remove", EditorStyles.miniButtonRight, GUILayout.Width(70f)))
                    {
                        _bindingsProperty.DeleteArrayElementAtIndex(index);
                        return;
                    }
                }

                EditorGUILayout.PropertyField(signalProperty);

                ActorAnimationSignal signal = (ActorAnimationSignal)signalProperty.enumValueIndex;
                if (signal == ActorAnimationSignal.Custom)
                    EditorGUILayout.PropertyField(customKeyProperty);

                EditorGUILayout.PropertyField(bindingTypeProperty);

                ActorAnimationBindingType bindingType = (ActorAnimationBindingType)bindingTypeProperty.enumValueIndex;
                DrawParameterSelector(parameterNameProperty, parameters, bindingType);
                DrawValueSource(bindingProperty, bindingType);
            }
        }

        private static string GetBindingTitle(
            SerializedProperty signalProperty,
            SerializedProperty customKeyProperty,
            int index)
        {
            ActorAnimationSignal signal = (ActorAnimationSignal)signalProperty.enumValueIndex;
            if (signal == ActorAnimationSignal.Custom && !string.IsNullOrWhiteSpace(customKeyProperty.stringValue))
                return $"Binding {index + 1}: Custom:{customKeyProperty.stringValue}";

            return $"Binding {index + 1}: {signal}";
        }

        private static void DrawParameterSelector(
            SerializedProperty parameterNameProperty,
            IReadOnlyList<PawnAnimationParameterInfo> parameters,
            ActorAnimationBindingType bindingType)
        {
            if (parameters == null || parameters.Count == 0)
            {
                EditorGUILayout.PropertyField(parameterNameProperty);
                return;
            }

            List<PawnAnimationParameterInfo> compatibleParameters = parameters
                .Where(parameter => NeonBlack.Gameplay.Editor.PawnAnimationProfileValidation.IsBindingTypeCompatible(bindingType, parameter.ParameterType))
                .ToList();


            if (compatibleParameters.Count == 0)
            {
                EditorGUILayout.HelpBox($"The assigned controller has no {bindingType}-compatible parameters. You can still type a parameter name manually.", MessageType.Warning);
                EditorGUILayout.PropertyField(parameterNameProperty);
                return;
            }

            string currentParameter = parameterNameProperty.stringValue;
            List<string> optionLabels = new List<string> { "(Select parameter)" };
            List<string> optionValues = new List<string> { string.Empty };
            optionLabels.AddRange(compatibleParameters.Select(parameter => parameter.ToString()));
            optionValues.AddRange(compatibleParameters.Select(parameter => parameter.Name));

            int selectedIndex = string.IsNullOrWhiteSpace(currentParameter)
                ? 0
                : optionValues.IndexOf(currentParameter);
            bool currentIsManual = selectedIndex < 0 && !string.IsNullOrWhiteSpace(currentParameter);
            if (currentIsManual)
            {
                optionLabels.Insert(1, $@"{currentParameter} (manual or mismatched)");
                optionValues.Insert(1, currentParameter);
                selectedIndex = 1;
            }

            optionLabels.Add("Manual entry...");
            optionValues.Add(null);

            if (selectedIndex < 0)
                selectedIndex = 0;

            EditorGUI.BeginChangeCheck();
            int nextIndex = EditorGUILayout.Popup("Animator Parameter", selectedIndex, optionLabels.ToArray());
            if (EditorGUI.EndChangeCheck()
                && nextIndex >= 0
                && nextIndex < optionValues.Count
                && optionValues[nextIndex] != null)
            {
                parameterNameProperty.stringValue = optionValues[nextIndex];
            }

            if (nextIndex == optionValues.Count - 1 || currentIsManual)
                EditorGUILayout.PropertyField(parameterNameProperty);
        }

        private static void DrawValueSource(SerializedProperty bindingProperty, ActorAnimationBindingType bindingType)
        {
            if (bindingType == ActorAnimationBindingType.Trigger)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                switch (bindingType)
                {
                    case ActorAnimationBindingType.Bool:
                        DrawSignalValueToggle(
                            bindingProperty.FindPropertyRelative("useSignalBool"),
                            bindingProperty.FindPropertyRelative("boolValue"),
                            "Use Signal Bool",
                            "Fixed Bool Value");
                        break;
                    case ActorAnimationBindingType.Float:
                        DrawSignalValueToggle(
                            bindingProperty.FindPropertyRelative("useSignalFloat"),
                            bindingProperty.FindPropertyRelative("floatValue"),
                            "Use Signal Float",
                            "Fixed Float Value");
                        break;
                    case ActorAnimationBindingType.Int:
                        DrawSignalValueToggle(
                            bindingProperty.FindPropertyRelative("useSignalInt"),
                            bindingProperty.FindPropertyRelative("intValue"),
                            "Use Signal Int",
                            "Fixed Int Value");
                        break;
                }
            }
        }

        private static void DrawSignalValueToggle(
            SerializedProperty useSignalProperty,
            SerializedProperty fallbackValueProperty,
            string useSignalLabel,
            string fallbackLabel)
        {
            EditorGUILayout.PropertyField(useSignalProperty, new GUIContent(useSignalLabel));
            if (!useSignalProperty.boolValue)
                EditorGUILayout.PropertyField(fallbackValueProperty, new GUIContent(fallbackLabel));
        }

        private void AddEmptyBinding()
        {
            int index = _bindingsProperty.arraySize;
            _bindingsProperty.InsertArrayElementAtIndex(index);

            SerializedProperty bindingProperty = _bindingsProperty.GetArrayElementAtIndex(index);
            bindingProperty.FindPropertyRelative("signal").enumValueIndex = (int)ActorAnimationSignal.Idle;
            bindingProperty.FindPropertyRelative("customKey").stringValue = string.Empty;
            bindingProperty.FindPropertyRelative("bindingType").enumValueIndex = (int)ActorAnimationBindingType.Bool;
            bindingProperty.FindPropertyRelative("parameterName").stringValue = string.Empty;
            bindingProperty.FindPropertyRelative("useSignalBool").boolValue = true;
            bindingProperty.FindPropertyRelative("boolValue").boolValue = true;
            bindingProperty.FindPropertyRelative("useSignalFloat").boolValue = true;
            bindingProperty.FindPropertyRelative("floatValue").floatValue = 1f;
            bindingProperty.FindPropertyRelative("useSignalInt").boolValue = true;
            bindingProperty.FindPropertyRelative("intValue").intValue = 1;
        }

        private void DrawAdvancedDebug()
        {
            EditorGUILayout.Space(4f);
            _rawDebugOpen = EditorGUILayout.Foldout(_rawDebugOpen, "Raw Serialized Debug", true);
            if (!_rawDebugOpen)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_bindingsProperty, true);
            }
        }

        private void RunProfileMutation(PawnAnimationProfile profile, string undoName, Action mutation)
        {
            if (profile == null || mutation == null)
                return;

            serializedObject.ApplyModifiedProperties();
            Undo.RecordObject(profile, undoName);
            mutation();
            EditorUtility.SetDirty(profile);
            serializedObject.Update();
        }
    }
}
