using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(PawnAnimationProfile))]
    public class PawnAnimationProfileEditor : UnityEditor.Editor
    {
        private SerializedProperty _animationDefinitionProperty;
        private SerializedProperty _baseControllerProperty;
        private SerializedProperty _spawnControllerOverrideProperty;
        private SerializedProperty _bindingsProperty;
        private bool _bindingsOpen = true;
        private bool _rawDebugOpen;

        private void OnEnable()
        {
            _animationDefinitionProperty = serializedObject.FindProperty("animationDefinition");
            _baseControllerProperty = serializedObject.FindProperty("baseController");
            _spawnControllerOverrideProperty = serializedObject.FindProperty("spawnControllerOverride");
            _bindingsProperty = serializedObject.FindProperty("bindings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            PawnAnimationProfile profile = (PawnAnimationProfile)target;

            DrawCoreAssignments();

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Pawn Animation Profile",
                "A pawn animation profile maps Pyralis gameplay signals to whichever Unity Animator Controller this pawn equips. It keeps movement/combat/traversal code from hardcoding Animator parameter names.",
                whenToUse: new[]
                {
                    "Use this when a pawn has sprite, 2.5D, or rigged 3D animation driven by an Animator.",
                    "Use it when importing a third-party, Aseprite, rigged, or project-authored controller whose parameters do not match Pyralis defaults.",
                    "Use it with ActorAnimationDriver on the pawn prefab."
                },
                createBefore: new[]
                {
                    "ActorAnimationDefinition listing the supported animation signals.",
                    "Animator Controller equipped by the pawn visual, found in the folderbase or package where your art/controller assets live.",
                    "PawnPresentationProfile declaring Sprite2D, Billboard2_5D, or Rigged3D presentation."
                },
                assignFirst: new[]
                {
                    "Assign Animation Definition.",
                    "Assign Base Controller to the same controller used by the visual Animator.",
                    "Add bindings from gameplay signals to Animator parameters/triggers/floats/ints/bools."
                },
                safeToCustomize: new[]
                {
                    "Spawn Controller Override can stay empty unless the pawn needs special spawn animation state.",
                    "Bindings are project-specific. Map Pyralis signals to your controller parameters instead of renaming the controller just for Pyralis.",
                    "You can duplicate this profile per character when Animator parameters differ."
                },
                validation: new[]
                {
                    "ActorAnimationDriver is on the pawn prefab.",
                    "Every binding signal is supported by the assigned ActorAnimationDefinition.",
                    "Animator parameter names match the actual Animator Controller."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")));

            DrawMappingWizard(profile);
            DrawAdvancedDebug();

            serializedObject.ApplyModifiedProperties();
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
                EditorGUILayout.HelpBox("Use this workflow after assigning the Animator Controller your pawn visual actually equips. Find that controller in your folderbase or package, drag it into Base Controller, then let suggestions inspect its existing parameters. Suggested bindings are only a starting point; this profile remains the authority for how gameplay signals drive your chosen parameters.", MessageType.Info);

                DrawReadiness(profile);
                DrawToolButtons(profile);
                DrawIssueGroups(profile);
                DrawParameterInventory(profile);
                DrawBindingTable(profile);
            }
        }

        private static void DrawReadiness(PawnAnimationProfile profile)
        {
            PawnAnimationMappingSummary summary = PawnAnimationProfileValidation.GetMappingSummary(profile);
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
                        PawnAnimationProfileValidation.AppendSuggestedBindings(profile);
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
                            PawnAnimationProfileValidation.ReplaceWithSuggestedBindings(profile);
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
            Dictionary<string, List<string>> groups = PawnAnimationProfileValidation.GetValidationIssueGroups(profile);
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
            IReadOnlyList<PawnAnimationParameterInfo> parameters = PawnAnimationProfileValidation.GetInspectableParameters(profile);
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

            IReadOnlyList<PawnAnimationParameterInfo> parameters = PawnAnimationProfileValidation.GetInspectableParameters(profile);

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
                .Where(parameter => PawnAnimationProfileValidation.IsBindingTypeCompatible(bindingType, parameter.ParameterType))
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
                optionLabels.Insert(1, $"{currentParameter} (manual or mismatched)");
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

    public readonly struct PawnAnimationParameterInfo
    {
        public readonly string Name;
        public readonly AnimatorControllerParameterType ParameterType;

        public PawnAnimationParameterInfo(string name, AnimatorControllerParameterType parameterType)
        {
            Name = name;
            ParameterType = parameterType;
        }

        public override string ToString()
        {
            return $"{Name} ({ParameterType})";
        }
    }

    public readonly struct PawnAnimationMappingSummary
    {
        public readonly bool HasDefinition;
        public readonly bool HasController;
        public readonly int ControllerParameterCount;
        public readonly int BindingCount;
        public readonly int MappedSignalCount;
        public readonly int SupportedSignalCount;
        public readonly int CustomChannelCount;
        public readonly int IssueCount;

        public PawnAnimationMappingSummary(
            bool hasDefinition,
            bool hasController,
            int controllerParameterCount,
            int bindingCount,
            int mappedSignalCount,
            int supportedSignalCount,
            int customChannelCount,
            int issueCount)
        {
            HasDefinition = hasDefinition;
            HasController = hasController;
            ControllerParameterCount = controllerParameterCount;
            BindingCount = bindingCount;
            MappedSignalCount = mappedSignalCount;
            SupportedSignalCount = supportedSignalCount;
            CustomChannelCount = customChannelCount;
            IssueCount = issueCount;
        }

        public float Coverage01
        {
            get
            {
                if (SupportedSignalCount <= 0)
                    return 1f;

                return Mathf.Clamp01((float)MappedSignalCount / SupportedSignalCount);
            }
        }

        public string ReadinessLabel
        {
            get
            {
                if (!HasController)
                    return "Needs base Animator Controller";

                if (BindingCount == 0)
                    return "Ready for suggested bindings";

                if (IssueCount > 0)
                    return "Needs binding review";

                if (!HasDefinition)
                    return "Usable, but definition should be assigned";

                return "Ready";
            }
        }
    }

    public static class PawnAnimationProfileValidation
    {
        private static readonly IReadOnlyDictionary<ActorAnimationSignal, string[]> ParameterAliases = new Dictionary<ActorAnimationSignal, string[]>
        {
            { ActorAnimationSignal.Idle, new[] { "Idle", "IsIdle" } },
            { ActorAnimationSignal.Move, new[] { "Move", "Moving", "IsMoving" } },
            { ActorAnimationSignal.Sprint, new[] { "Sprint", "Sprinting", "IsSprinting" } },
            { ActorAnimationSignal.Crouch, new[] { "Crouch", "Crouching", "IsCrouching" } },
            { ActorAnimationSignal.Jump, new[] { "Jump" } },
            { ActorAnimationSignal.Fall, new[] { "Fall", "Falling", "IsFalling", "IsInAir" } },
            { ActorAnimationSignal.Land, new[] { "Land", "Landed" } },
            { ActorAnimationSignal.Dash, new[] { "Dash", "Dodge", "DodgeFwd", "DiveRoll" } },
            { ActorAnimationSignal.Slide, new[] { "Slide", "IsSliding" } },
            { ActorAnimationSignal.AttackPrimary, new[] { "AttackPrimary", "Attack", "RightPunch", "LeftPunch" } },
            { ActorAnimationSignal.AttackSecondary, new[] { "AttackSecondary", "Attack2", "RightKick", "LeftKick" } },
            { ActorAnimationSignal.AttackAerial, new[] { "AttackAerial", "AerialAttack", "Knee" } },
            { ActorAnimationSignal.BlockStart, new[] { "BlockStart", "Block" } },
            { ActorAnimationSignal.BlockLoop, new[] { "BlockLoop", "Block", "IsBlocking" } },
            { ActorAnimationSignal.BlockEnd, new[] { "BlockEnd" } },
            { ActorAnimationSignal.Hurt, new[] { "Hurt", "Hit", "KnockedBack" } },
            { ActorAnimationSignal.Stagger, new[] { "Stagger", "Stunned" } },
            { ActorAnimationSignal.Death, new[] { "Death", "Die", "Dead", "IsDead" } },
            { ActorAnimationSignal.ClimbStart, new[] { "ClimbStart", "ClimbUp" } },
            { ActorAnimationSignal.ClimbLoop, new[] { "ClimbLoop", "Climb" } },
            { ActorAnimationSignal.ClimbEnd, new[] { "ClimbEnd" } },
            { ActorAnimationSignal.Hang, new[] { "Hang", "Hanging", "IsHanging" } },
            { ActorAnimationSignal.Shimmy, new[] { "Shimmy", "ShimmySpeed" } },
            { ActorAnimationSignal.Interact, new[] { "Interact", "Use" } },
            { ActorAnimationSignal.LookAround, new[] { "LookAround", "Look" } },
            { ActorAnimationSignal.Spawn, new[] { "Spawn", "Respawn" } },
            { ActorAnimationSignal.Despawn, new[] { "Despawn" } },
            { ActorAnimationSignal.SideClimb, new[] { "SideClimb" } },
            { ActorAnimationSignal.ForwardClimb, new[] { "ForwardClimb", "FwdClimb" } },
            { ActorAnimationSignal.LedgeDrop, new[] { "LedgeDrop" } }
        };

        private static readonly IReadOnlyDictionary<string, string[]> BlendTreeFloatAliases = new Dictionary<string, string[]>
        {
            { "Speed", new[] { "Speed", "MoveSpeed", "MovementSpeed", "GroundSpeed", "LocomotionSpeed" } },
            { "NormalizedSpeed", new[] { "NormalizedSpeed", "NormalizedMoveSpeed", "MoveSpeed01", "Speed01", "SpeedNormalized" } },
            { "MoveX", new[] { "MoveX", "Horizontal", "InputX", "DirectionX" } },
            { "MoveY", new[] { "MoveY", "Vertical", "InputY", "DirectionY" } },
            { "MoveZ", new[] { "MoveZ", "Forward", "InputZ", "DirectionZ" } },
            { "VelocityX", new[] { "VelocityX" } },
            { "VelocityY", new[] { "VelocityY", "VerticalVelocity", "YVelocity" } },
            { "VelocityZ", new[] { "VelocityZ", "ForwardVelocity", "ZVelocity" } }
        };

        public static List<string> GetValidationIssues(PawnAnimationProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile == null)
                return issues;

            if (profile.animationDefinition == null)
                issues.Add("Assign an Actor Animation Definition so supported signals are explicit.");

            if (profile.baseController == null)
                issues.Add("Assign a base Animator Controller. The animation stack is Unity-Animator-driven.");

            Dictionary<string, AnimatorControllerParameterType> controllerParameters = profile.baseController != null
                ? GetControllerParameters(profile.baseController)
                : null;
            bool canInspectController = profile.baseController == null || controllerParameters != null;

            if (profile.baseController != null && !canInspectController)
                issues.Add($"Controller '{profile.baseController.name}' cannot be inspected for parameters. Use a standard Animator Controller or Animator Override Controller for editor validation.");

            AddMissingSpriteFrameIssues(profile.baseController, issues);

            if (profile.bindings == null || profile.bindings.Length == 0)
            {
                issues.Add("Add bindings so supported animation signals can drive Animator parameters.");
            }
            else
            {
                HashSet<string> seenBindingKeys = new HashSet<string>();

                foreach (ActorAnimationBinding binding in profile.bindings)
                {
                    if (binding == null)
                        continue;

                    string bindingLabel = GetBindingLabel(binding);
                    string bindingKey = $"{binding.signal}|{binding.customKey}|{binding.bindingType}|{binding.parameterName}";
                    if (!seenBindingKeys.Add(bindingKey))
                        issues.Add($"Binding '{bindingLabel}' is duplicated.");

                    if (binding.signal != ActorAnimationSignal.Custom
                        && profile.animationDefinition != null
                        && !profile.animationDefinition.SupportsSignal(binding.signal))
                        issues.Add($"Binding '{binding.parameterName}' uses {binding.signal}, but that signal is not listed on the assigned definition.");

                    if (binding.signal == ActorAnimationSignal.Custom && string.IsNullOrWhiteSpace(binding.customKey))
                        issues.Add($"Binding '{bindingLabel}' uses Custom but has no custom key.");

                    if (string.IsNullOrWhiteSpace(binding.parameterName))
                    {
                        issues.Add($"Binding '{bindingLabel}' has no Animator parameter name.");
                        continue;
                    }

                    if (controllerParameters == null)
                        continue;

                    if (!controllerParameters.TryGetValue(binding.parameterName.Trim(), out AnimatorControllerParameterType parameterType))
                    {
                        issues.Add($"Binding '{bindingLabel}' targets missing Animator parameter '{binding.parameterName}'.");
                        continue;
                    }

                    if (!IsBindingTypeCompatible(binding.bindingType, parameterType))
                        issues.Add($"Binding '{bindingLabel}' is {binding.bindingType}, but Animator parameter '{binding.parameterName}' is {parameterType}.");
                }
            }

            return issues;
        }

        public static Dictionary<string, List<string>> GetValidationIssueGroups(PawnAnimationProfile profile)
        {
            Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();
            List<string> issues = GetValidationIssues(profile);

            foreach (string issue in issues)
            {
                if (string.IsNullOrWhiteSpace(issue))
                    continue;

                AddIssue(groups, GetIssueGroup(issue), issue);
            }

            return groups;
        }

        public static IReadOnlyList<string> GetInspectableParameterNames(PawnAnimationProfile profile)
        {
            return GetInspectableParameters(profile)
                .Select(parameter => parameter.ToString())
                .ToArray();
        }

        public static IReadOnlyList<PawnAnimationParameterInfo> GetInspectableParameters(PawnAnimationProfile profile)
        {
            Dictionary<string, AnimatorControllerParameterType> parameters = profile != null
                ? GetControllerParameters(profile.baseController)
                : null;

            if (parameters == null)
                return Array.Empty<PawnAnimationParameterInfo>();

            return parameters
                .OrderBy(pair => pair.Key)
                .Select(pair => new PawnAnimationParameterInfo(pair.Key, pair.Value))
                .ToArray();
        }

        public static IReadOnlyList<PawnAnimationParameterInfo> GetCompatibleParameters(
            PawnAnimationProfile profile,
            ActorAnimationBindingType bindingType)
        {
            return GetInspectableParameters(profile)
                .Where(parameter => IsBindingTypeCompatible(bindingType, parameter.ParameterType))
                .ToArray();
        }

        public static PawnAnimationMappingSummary GetMappingSummary(PawnAnimationProfile profile)
        {
            if (profile == null)
                return new PawnAnimationMappingSummary(false, false, 0, 0, 0, 0, 0, 0);

            IReadOnlyList<PawnAnimationParameterInfo> parameters = GetInspectableParameters(profile);
            List<string> issues = GetValidationIssues(profile);
            HashSet<ActorAnimationSignal> supportedSignals = GetSupportedSignals(profile);
            HashSet<ActorAnimationSignal> mappedSignals = new HashSet<ActorAnimationSignal>(
                (profile.bindings ?? Array.Empty<ActorAnimationBinding>())
                    .Where(binding => binding != null && binding.signal != ActorAnimationSignal.Custom && !string.IsNullOrWhiteSpace(binding.parameterName))
                    .Select(binding => binding.signal));

            int mappedSupportedCount = mappedSignals.Count(signal => supportedSignals.Contains(signal));
            int customChannelCount = (profile.bindings ?? Array.Empty<ActorAnimationBinding>())
                .Count(binding => binding != null && binding.signal == ActorAnimationSignal.Custom && !string.IsNullOrWhiteSpace(binding.customKey));

            return new PawnAnimationMappingSummary(
                profile.animationDefinition != null,
                profile.baseController != null,
                parameters.Count,
                profile.bindings != null ? profile.bindings.Length : 0,
                mappedSupportedCount,
                supportedSignals.Count,
                customChannelCount,
                issues.Count);
        }

        public static void AppendSuggestedBindings(PawnAnimationProfile profile)
        {
            if (profile == null)
                return;

            Dictionary<string, AnimatorControllerParameterType> parameters = GetControllerParameters(profile.baseController);
            if (parameters == null || parameters.Count == 0)
                return;

            List<ActorAnimationBinding> bindings = profile.bindings != null
                ? profile.bindings.Where(binding => binding != null).ToList()
                : new List<ActorAnimationBinding>();

            HashSet<ActorAnimationSignal> existingSignals = new HashSet<ActorAnimationSignal>(
                bindings
                    .Where(binding => binding.signal != ActorAnimationSignal.Custom)
                    .Select(binding => binding.signal));
            HashSet<ActorAnimationSignal> supportedSignals = GetSupportedSignals(profile);

            foreach (KeyValuePair<ActorAnimationSignal, string[]> signalAliases in ParameterAliases)
            {
                if (existingSignals.Contains(signalAliases.Key))
                    continue;

                if (!supportedSignals.Contains(signalAliases.Key))
                    continue;

                if (!TryFindParameter(parameters, signalAliases.Value, out string parameterName, out AnimatorControllerParameterType parameterType))
                    continue;

                bindings.Add(new ActorAnimationBinding
                {
                    signal = signalAliases.Key,
                    parameterName = parameterName,
                    bindingType = ToBindingType(parameterType)
                });
            }

            HashSet<string> existingCustomFloatKeys = new HashSet<string>(
                bindings
                    .Where(binding => binding.signal == ActorAnimationSignal.Custom && binding.bindingType == ActorAnimationBindingType.Float)
                    .Select(binding => binding.customKey),
                StringComparer.Ordinal);

            foreach (KeyValuePair<string, string[]> floatAliases in BlendTreeFloatAliases)
            {
                if (existingCustomFloatKeys.Contains(floatAliases.Key))
                    continue;

                if (!TryFindParameter(parameters, floatAliases.Value, out string parameterName, out AnimatorControllerParameterType parameterType))
                    continue;

                if (parameterType != AnimatorControllerParameterType.Float)
                    continue;

                bindings.Add(new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Custom,
                    customKey = floatAliases.Key,
                    parameterName = parameterName,
                    bindingType = ActorAnimationBindingType.Float
                });
            }

            profile.bindings = bindings.ToArray();
        }

        public static void ReplaceWithSuggestedBindings(PawnAnimationProfile profile)
        {
            if (profile == null)
                return;

            profile.bindings = Array.Empty<ActorAnimationBinding>();
            AppendSuggestedBindings(profile);
        }

        public static ActorAnimationBindingType ToBindingType(AnimatorControllerParameterType parameterType)
        {
            return parameterType switch
            {
                AnimatorControllerParameterType.Bool => ActorAnimationBindingType.Bool,
                AnimatorControllerParameterType.Float => ActorAnimationBindingType.Float,
                AnimatorControllerParameterType.Int => ActorAnimationBindingType.Int,
                AnimatorControllerParameterType.Trigger => ActorAnimationBindingType.Trigger,
                _ => ActorAnimationBindingType.Bool
            };
        }

        public static bool IsBindingTypeCompatible(ActorAnimationBindingType bindingType, AnimatorControllerParameterType parameterType)
        {
            return bindingType switch
            {
                ActorAnimationBindingType.Bool => parameterType == AnimatorControllerParameterType.Bool,
                ActorAnimationBindingType.Float => parameterType == AnimatorControllerParameterType.Float,
                ActorAnimationBindingType.Int => parameterType == AnimatorControllerParameterType.Int,
                ActorAnimationBindingType.Trigger => parameterType == AnimatorControllerParameterType.Trigger,
                _ => false
            };
        }

        private static Dictionary<string, AnimatorControllerParameterType> GetControllerParameters(RuntimeAnimatorController controller)
        {
            AnimatorController animatorController = GetAnimatorController(controller);
            if (animatorController == null)
                return controller == null ? new Dictionary<string, AnimatorControllerParameterType>() : null;

            Dictionary<string, AnimatorControllerParameterType> parameters = new Dictionary<string, AnimatorControllerParameterType>();
            foreach (AnimatorControllerParameter parameter in animatorController.parameters)
            {
                if (!parameters.ContainsKey(parameter.name))
                    parameters.Add(parameter.name, parameter.type);
            }

            return parameters;
        }

        private static AnimatorController GetAnimatorController(RuntimeAnimatorController controller)
        {
            if (controller is AnimatorController animatorController)
                return animatorController;

            if (controller is AnimatorOverrideController overrideController)
                return overrideController.runtimeAnimatorController as AnimatorController;

            return null;
        }

        private static void AddMissingSpriteFrameIssues(RuntimeAnimatorController controller, List<string> issues)
        {
            if (controller == null)
                return;

            AnimationClip[] clips = controller.animationClips;
            if (clips == null || clips.Length == 0)
                return;

            int missingFrameCount = 0;
            HashSet<string> affectedClips = new HashSet<string>();

            foreach (AnimationClip clip in clips)
            {
                if (clip == null)
                    continue;

                EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                foreach (EditorCurveBinding binding in bindings)
                {
                    if (binding.type != typeof(SpriteRenderer) || binding.propertyName != "m_Sprite")
                        continue;

                    ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                    foreach (ObjectReferenceKeyframe keyframe in keyframes)
                    {
                        if (keyframe.value != null)
                            continue;

                        missingFrameCount++;
                        affectedClips.Add(clip.name);
                    }
                }
            }

            if (missingFrameCount == 0)
                return;

            string clipList = string.Join(", ", affectedClips.OrderBy(name => name).Take(6));
            if (affectedClips.Count > 6)
                clipList += $", and {affectedClips.Count - 6} more";

            issues.Add($"Controller '{controller.name}' has {missingFrameCount} missing SpriteRenderer sprite frame reference(s) in clip(s): {clipList}. Reimport or restore the sprite sheet/art package used by this controller before using it on a Sprite2D pawn.");
        }

        private static bool TryFindParameter(
            IReadOnlyDictionary<string, AnimatorControllerParameterType> parameters,
            IReadOnlyList<string> aliases,
            out string parameterName,
            out AnimatorControllerParameterType parameterType)
        {
            foreach (string alias in aliases)
            {
                if (parameters.TryGetValue(alias, out parameterType))
                {
                    parameterName = alias;
                    return true;
                }
            }

            foreach (KeyValuePair<string, AnimatorControllerParameterType> parameter in parameters)
            {
                string normalizedParameter = NormalizeName(parameter.Key);
                if (aliases.Any(alias => NormalizeName(alias) == normalizedParameter))
                {
                    parameterName = parameter.Key;
                    parameterType = parameter.Value;
                    return true;
                }
            }

            parameterName = string.Empty;
            parameterType = AnimatorControllerParameterType.Bool;
            return false;
        }

        private static string NormalizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            char[] characters = value
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray();

            return new string(characters);
        }

        private static HashSet<ActorAnimationSignal> GetSupportedSignals(PawnAnimationProfile profile)
        {
            if (profile != null
                && profile.animationDefinition != null
                && profile.animationDefinition.supportedSignals != null
                && profile.animationDefinition.supportedSignals.Length > 0)
            {
                return new HashSet<ActorAnimationSignal>(
                    profile.animationDefinition.supportedSignals.Where(signal => signal != ActorAnimationSignal.Custom));
            }

            return new HashSet<ActorAnimationSignal>(
                Enum.GetValues(typeof(ActorAnimationSignal))
                    .Cast<ActorAnimationSignal>()
                    .Where(signal => signal != ActorAnimationSignal.Custom));
        }

        private static void AddIssue(Dictionary<string, List<string>> groups, string group, string issue)
        {
            if (!groups.TryGetValue(group, out List<string> groupIssues))
            {
                groupIssues = new List<string>();
                groups.Add(group, groupIssues);
            }

            groupIssues.Add(issue);
        }

        private static string GetIssueGroup(string issue)
        {
            if (issue.Contains("Assign ") || issue.Contains("Add bindings") || issue.Contains("cannot be inspected"))
                return "Setup";

            if (issue.Contains("missing Animator parameter") || issue.Contains("but Animator parameter"))
                return "Animator Parameter Mismatch";

            if (issue.Contains("missing SpriteRenderer sprite frame"))
                return "Sprite Frame References";

            if (issue.Contains("duplicated"))
                return "Duplicate Bindings";

            if (issue.Contains("not listed on the assigned definition"))
                return "Unsupported Signals";

            if (issue.Contains("Custom") || issue.Contains("custom key"))
                return "Custom Channels";

            return "Other";
        }

        private static string GetBindingLabel(ActorAnimationBinding binding)
        {
            if (binding == null)
                return "Null binding";

            if (binding.signal == ActorAnimationSignal.Custom && !string.IsNullOrWhiteSpace(binding.customKey))
                return $"{binding.signal}:{binding.customKey}";

            return binding.signal.ToString();
        }
    }
}
