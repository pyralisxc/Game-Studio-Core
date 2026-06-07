using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Input;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(InputProfile))]
    public class InputProfileEditor : UnityEditor.Editor
    {
        private const string DefaultProjectInputActionsPath = "Assets/InputSystem_Actions.inputactions";

        private GameplayInputActionRole _roleToAdd = GameplayInputActionRole.Move;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("actions"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("primaryActionMap"));
            DrawInputAssetShortcut(serializedObject.FindProperty("actions"));
            DrawActionBindings(serializedObject.FindProperty("actionBindings"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("preferredControlSchemes"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("touchFriendly"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("supportsGamepad"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("supportsKeyboardMouse"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("allowRuntimeRebinding"));

            InputProfile profile = (InputProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Input Profile",
                "An input profile describes which Input Actions and control expectations belong to a session, participant, or pawn.",
                whenToUse: new[]
                {
                    "Use this for player-controlled participants, pawns, camera/cursor control, local join, and runtime rebinding.",
                    "Use it as the translation table when your project uses custom action-map or action names.",
                    "AI-only/system participants can usually leave input profiles empty."
                },
                createBefore: new[]
                {
                    "Use the project stock Assets/InputSystem_Actions.inputactions for the beginner path, or a custom Unity Input Action Asset when this game needs different maps.",
                    "SessionDefinition, ParticipantDefinition, or PawnDefinition that will reference this profile."
                },
                assignFirst: new[]
                {
                    "Assign Actions. For a fresh project, use the object picker and choose InputSystem_Actions, or click Use Project InputSystem_Actions above.",
                    "Set Primary Action Map to Player when using the stock project asset.",
                    "Add only the Gameplay Actions this setup uses, then set each Unity Action Name to match your Input Action Asset.",
                    "Mark supported device families: gamepad, keyboard/mouse, touch."
                },
                safeToCustomize: new[]
                {
                    "Keep the stock InputSystem_Actions asset when its Player map already has the actions you need.",
                    "Use Add Built-In Action for Pyralis features such as Move, Jump, Attack, Interact, Sprint, or Roll.",
                    "Use Add Custom Action for creator-defined input that can feed custom gameplay or animation wiring.",
                    "Create a new Input Action Asset only when the project needs a different map layout or device scheme.",
                    "Preferred Control Schemes should match names inside the Input Action Asset.",
                    "Runtime rebinding can stay enabled for user-facing games.",
                    "Touch Friendly should be enabled only when the setup has touch UI or screen controls."
                },
                validation: new[]
                {
                    "Actions is assigned for player-owned input.",
                    "Primary Action Map matches an action map in the Input Action Asset.",
                    "Required action rows are present in the assigned Input Action Asset. Optional rows can be removed when the setup does not use them.",
                    "PlayerInputManager is only needed for local join flows."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("AUTHORING_MODEL.md")));

            bool hasActions = serializedObject.FindProperty("actions")?.objectReferenceValue != null;
            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile, hasActions), "Input profile is ready for assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawInputAssetShortcut(SerializedProperty actionsProperty)
        {
            InputActionAsset defaultActions = FindDefaultProjectInputActions();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Beginner Input Asset", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("For the first pawn proof, use the project stock InputSystem_Actions asset. Customize Gameplay Actions rows below before creating a separate Input Action Asset.", EditorStyles.wordWrappedMiniLabel);
                using (new EditorGUI.DisabledScope(defaultActions == null || actionsProperty == null))
                {
                    if (GUILayout.Button("Use Project InputSystem_Actions"))
                    {
                        actionsProperty.objectReferenceValue = defaultActions;
                    }
                }

                if (defaultActions == null)
                    EditorGUILayout.HelpBox($"Could not find {DefaultProjectInputActionsPath}. Use the Actions object picker to choose an existing Unity Input Action Asset.", MessageType.Info);
            }
        }

        private void DrawActionBindings(SerializedProperty bindings)
        {
            if (bindings == null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gameplay Actions", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("These rows map Pyralis gameplay roles to Unity Input Action names. The stock Player map already covers common actions; add or remove rows here to decide what this profile actually uses.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Role To Add", GUILayout.Width(80));
                _roleToAdd = (GameplayInputActionRole)EditorGUILayout.EnumPopup(_roleToAdd);
                using (new EditorGUI.DisabledScope(_roleToAdd == GameplayInputActionRole.Custom))
                {
                    if (GUILayout.Button("Add Built-In Action", GUILayout.Width(135)))
                        AddBinding(bindings, _roleToAdd);
                }
                if (GUILayout.Button("Add Custom Action"))
                    AddBinding(bindings, GameplayInputActionRole.Custom);
            }

            EditorGUILayout.HelpBox(GetRolePreview(_roleToAdd), MessageType.None);

            EditorGUILayout.PropertyField(bindings, includeChildren: true);
        }

        private static string GetRolePreview(GameplayInputActionRole role)
        {
            switch (role)
            {
                case GameplayInputActionRole.Move:
                    return "Move adds the required Vector2 row for first pawn movement proof.";
                case GameplayInputActionRole.Look:
                    return "Look adds a Vector2 row for aim, camera look, or cursor-facing style controls.";
                case GameplayInputActionRole.Jump:
                    return "Jump adds an optional button row for pawn locomotion or animation triggers.";
                case GameplayInputActionRole.Dash:
                    return "Dash adds an optional button row for quick movement abilities.";
                case GameplayInputActionRole.AttackPrimary:
                    return "Attack Primary adds an optional button row for primary combat, tool, or interaction actions.";
                case GameplayInputActionRole.AttackSecondary:
                    return "Attack Secondary adds an optional button row for alternate combat or tool actions.";
                case GameplayInputActionRole.Interact:
                    return "Interact adds an optional button row for world objects, pickups, conversations, or menus.";
                case GameplayInputActionRole.Block:
                    return "Block adds an optional button row for guard, defend, or shield actions.";
                case GameplayInputActionRole.Sprint:
                    return "Sprint adds an optional button row for faster movement.";
                case GameplayInputActionRole.Crouch:
                    return "Crouch adds an optional button row for stealth, posture, or traversal.";
                case GameplayInputActionRole.Previous:
                    return "Previous adds an optional button row for cycling selections or UI choices backward.";
                case GameplayInputActionRole.Next:
                    return "Next adds an optional button row for cycling selections or UI choices forward.";
                case GameplayInputActionRole.Roll:
                    return "Roll adds an optional button row for dodge-roll or evasive movement.";
                case GameplayInputActionRole.LookAround:
                    return "Look Around adds an optional button row for camera/cursor inspection modes.";
                case GameplayInputActionRole.Custom:
                    return "Use Add Custom Action for creator-defined input, animation-only triggers, or project-specific wiring.";
                default:
                    return "Adds a gameplay action row that maps a Pyralis role to a Unity Input Action.";
            }
        }

        private static void AddBinding(SerializedProperty bindings, GameplayInputActionRole role)
        {
            int index = bindings.arraySize;
            bindings.InsertArrayElementAtIndex(index);
            SerializedProperty element = bindings.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("role").enumValueIndex = (int)role;
            element.FindPropertyRelative("customKey").stringValue = role == GameplayInputActionRole.Custom ? "CustomAction" : string.Empty;
            element.FindPropertyRelative("actionMap").stringValue = string.Empty;
            element.FindPropertyRelative("actionName").stringValue = GameplayInputActionBinding.GetDefaultActionName(role);
            element.FindPropertyRelative("valueType").enumValueIndex = role == GameplayInputActionRole.Move || role == GameplayInputActionRole.Look
                ? (int)GameplayInputValueType.Vector2
                : (int)GameplayInputValueType.Button;
            element.FindPropertyRelative("requiredForProof").boolValue = role == GameplayInputActionRole.Move;
            element.FindPropertyRelative("animationSignalKey").stringValue = string.Empty;
        }

        private static List<string> GetValidationIssues(InputProfile profile, bool hasActions)
        {
            List<string> issues = new List<string>();

            if (profile == null)
                return issues;

            if (!hasActions)
                issues.Add("Actions should be assigned for player-owned input. Leave it empty only for AI/system-only usage.");

            if (string.IsNullOrWhiteSpace(profile.primaryActionMap))
                issues.Add("Primary Action Map should name the gameplay action map.");
            else if (profile.actions != null)
            {
                InputActionMap map = ParticipantInputProfileUtility.FindGameplayActionMap(profile.actions, profile);
                if (map == null)
                    issues.Add($"Primary Action Map '{profile.primaryActionMap}' was not found in Actions.");
                else
                {
                    AddBindingIssues(issues, profile);
                }
            }

            if (!profile.supportsGamepad && !profile.supportsKeyboardMouse && !profile.touchFriendly)
                issues.Add("At least one input surface should be supported for player-owned input.");

            return issues;
        }

        private static InputActionAsset FindDefaultProjectInputActions()
        {
            InputActionAsset defaultActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(DefaultProjectInputActionsPath);
            if (defaultActions != null)
                return defaultActions;

            string[] guids = AssetDatabase.FindAssets("InputSystem_Actions t:InputActionAsset", new[] { "Assets" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                defaultActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                if (defaultActions != null)
                    return defaultActions;
            }

            return null;
        }

        private static void AddBindingIssues(List<string> issues, InputProfile profile)
        {
            if (profile.actionBindings == null || profile.actionBindings.Length == 0)
            {
                issues.Add("Add at least one Gameplay Action row. Player-owned pawn proofs require Move.");
                return;
            }

            HashSet<string> keys = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            bool hasRequiredMove = false;

            for (int i = 0; i < profile.actionBindings.Length; i++)
            {
                GameplayInputActionBinding binding = profile.actionBindings[i];
                if (binding == null)
                {
                    issues.Add($"Gameplay Actions[{i}] is empty.");
                    continue;
                }

                string key = binding.EffectiveKey;
                if (!string.IsNullOrWhiteSpace(key) && !keys.Add(key))
                    issues.Add($"Gameplay Actions contains duplicate role/key '{key}'. Remove the duplicate row.");

                if (binding.role == GameplayInputActionRole.Move && binding.requiredForProof)
                    hasRequiredMove = true;

                if (string.IsNullOrWhiteSpace(binding.actionName))
                {
                    issues.Add($"Gameplay Actions[{i}] has no Unity Action Name.");
                    continue;
                }

                string mapName = binding.GetActionMap(profile.primaryActionMap);
                InputActionMap map = profile.actions.FindActionMap(mapName, throwIfNotFound: false);
                if (map == null)
                {
                    issues.Add($"Gameplay Actions[{i}] uses Action Map '{mapName}', but that map was not found in Actions.");
                    continue;
                }

                if (map.FindAction(binding.actionName, throwIfNotFound: false) == null)
                {
                    string severity = binding.requiredForProof ? "Required" : "Optional";
                    issues.Add($"{severity} action '{binding.actionName}' for {key} was not found in Action Map '{map.name}'. Update the row or add the action to the Input Action Asset.");
                }
            }

            if (!hasRequiredMove)
                issues.Add("Player-owned pawn proofs need a required Move action row. Add Built-In Action > Move or mark the existing Move row required.");
        }
    }
}
