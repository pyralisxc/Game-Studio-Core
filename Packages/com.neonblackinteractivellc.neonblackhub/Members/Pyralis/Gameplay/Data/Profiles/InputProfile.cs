using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Data.Profiles
{
    public enum GameplayInputActionRole
    {
        Move,
        Look,
        Jump,
        Dash,
        AttackPrimary,
        AttackSecondary,
        Interact,
        Block,
        Sprint,
        Crouch,
        Previous,
        Next,
        Roll,
        LookAround,
        Custom
    }

    public enum GameplayInputValueType
    {
        Button,
        Vector2,
        Axis,
        PassThrough
    }

    [System.Serializable]
    public class GameplayInputActionBinding
    {
        [Tooltip("Pyralis gameplay role that reads this input. Use Custom for project-specific actions.")]
        public GameplayInputActionRole role = GameplayInputActionRole.Move;
        [Tooltip("Only used when Role is Custom. This should be stable enough for gameplay and animation wiring.")]
        public string customKey = string.Empty;
        [Tooltip("Optional action map override. Leave empty to use Primary Action Map.")]
        public string actionMap = string.Empty;
        [Tooltip("Action name inside the Unity Input Action Asset.")]
        public string actionName = "Move";
        [Tooltip("Expected value shape for authoring validation and creator guidance.")]
        public GameplayInputValueType valueType = GameplayInputValueType.Vector2;
        [Tooltip("Whether this action must exist for the selected setup to be considered ready.")]
        public bool requiredForProof = true;
        [Tooltip("Optional animation signal key. Built-in roles can map through PawnAnimationProfile; custom keys can drive ActorAnimationSignal.Custom.")]
        public string animationSignalKey = string.Empty;

        public string EffectiveKey => role == GameplayInputActionRole.Custom
            ? SanitizeKey(customKey)
            : role.ToString();

        public static GameplayInputActionBinding BuiltIn(
            GameplayInputActionRole role,
            string actionName,
            GameplayInputValueType valueType,
            bool requiredForProof)
        {
            return new GameplayInputActionBinding
            {
                role = role,
                actionName = actionName,
                valueType = valueType,
                requiredForProof = requiredForProof
            };
        }

        public void Sanitize(string defaultActionMap)
        {
            customKey = SanitizeKey(customKey);
            actionMap = string.IsNullOrWhiteSpace(actionMap) ? string.Empty : actionMap.Trim();
            actionName = string.IsNullOrWhiteSpace(actionName) ? GetDefaultActionName(role) : actionName.Trim();
            animationSignalKey = string.IsNullOrWhiteSpace(animationSignalKey) ? string.Empty : animationSignalKey.Trim();

            if (role == GameplayInputActionRole.Custom && string.IsNullOrWhiteSpace(customKey))
                customKey = actionName;

            if (role != GameplayInputActionRole.Custom)
                customKey = string.Empty;
        }

        public string GetActionMap(string fallback)
        {
            return !string.IsNullOrWhiteSpace(actionMap)
                ? actionMap
                : fallback;
        }

        public static string GetDefaultActionName(GameplayInputActionRole role)
        {
            return role switch
            {
                GameplayInputActionRole.AttackPrimary => "Attack",
                GameplayInputActionRole.AttackSecondary => "Kick",
                GameplayInputActionRole.LookAround => "LookAround",
                GameplayInputActionRole.Custom => "CustomAction",
                _ => role.ToString()
            };
        }

        private static string SanitizeKey(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    /// <summary>
    /// Authoring profile for participant input ownership and preferred control schemes.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Input,
        Priority = AuthoringPriority.AuxiliaryDefault,
        Lane = "Input",
        Relevance = "Maps high-level gameplay actions (Move, Jump, Interact) to Unity Input System actions.",
        Axioms = AuthoringWorldAxiom.None,
        ProfileType = typeof(InputProfile),
        AssignmentFields = new[] { nameof(actions), nameof(actionBindings), nameof(primaryActionMap) },
        FirstProofTargetId = "proof.1p-pawn-movement",
        FirstProof = "Verify that input actions mapped in this profile correctly drive character movement and actions.",
        ExpertAdvice = "InputProfile decouples gameplay logic from physical keys. Use the action role to map common verbs (Jump, Dash) across different control schemes.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/input",
        NativeSetup = new[] 
        { 
            "Create an InputProfile asset.",
            "Assign a Unity Input Action Asset.",
            "Define action names for Move, Jump, Interact, etc."
        }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Input Profile", fileName = "InputProfile", order = -90)]
    public class InputProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            List<string> issues = new List<string>();

            if (actions == null)
                issues.Add("Actions should be assigned for player-owned input. Leave it empty only for AI/system-only usage.");

            if (string.IsNullOrWhiteSpace(primaryActionMap))
                issues.Add("Primary Action Map should name the gameplay action map.");
            else if (actions != null)
            {
                InputActionMap map = actions.FindActionMap(primaryActionMap, throwIfNotFound: false);
                if (map == null)
                    issues.Add($"Primary Action Map '{primaryActionMap}' was not found in Actions.");
                else
                {
                    AddBindingIssues(issues);
                }
            }

            if (!supportsGamepad && !supportsKeyboardMouse && !touchFriendly)
                issues.Add("At least one input surface should be supported for player-owned input.");

            return issues;
        }

        private void AddBindingIssues(List<string> issues)
        {
            if (actionBindings == null || actionBindings.Length == 0)
            {
                issues.Add("Add at least one Gameplay Action row. Player-owned pawn proofs require Move.");
                return;
            }

            HashSet<string> keys = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            bool hasRequiredMove = false;

            for (int i = 0; i < actionBindings.Length; i++)
            {
                GameplayInputActionBinding binding = actionBindings[i];
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

                string mapName = binding.GetActionMap(primaryActionMap);
                InputActionMap map = actions.FindActionMap(mapName, throwIfNotFound: false);
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

        [Tooltip("Primary input action asset used by this participant or pawn definition.")]
        public InputActionAsset actions;
        [Tooltip("Default action map name expected by shared and compatibility input bridges.")]
        public string primaryActionMap = "Player";

        [Header("Gameplay Actions")]
        [Tooltip("Add the gameplay actions this setup actually uses. Missing optional actions do not block proofs.")]
        public GameplayInputActionBinding[] actionBindings =
        {
            GameplayInputActionBinding.BuiltIn(GameplayInputActionRole.Move, "Move", GameplayInputValueType.Vector2, true),
            GameplayInputActionBinding.BuiltIn(GameplayInputActionRole.Jump, "Jump", GameplayInputValueType.Button, false),
            GameplayInputActionBinding.BuiltIn(GameplayInputActionRole.AttackPrimary, "Attack", GameplayInputValueType.Button, false),
            GameplayInputActionBinding.BuiltIn(GameplayInputActionRole.Interact, "Interact", GameplayInputValueType.Button, false)
        };

        [Tooltip("Preferred control schemes for PlayerInput joining and rebinding UX.")]
        public string[] preferredControlSchemes;

        public bool touchFriendly = false;
        public bool supportsGamepad = true;
        public bool supportsKeyboardMouse = true;
        public bool allowRuntimeRebinding = true;

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(primaryActionMap))
                primaryActionMap = "Player";

            actionBindings ??= System.Array.Empty<GameplayInputActionBinding>();
            for (int i = 0; i < actionBindings.Length; i++)
                actionBindings[i]?.Sanitize(primaryActionMap);

            if (preferredControlSchemes == null)
                return;

            HashSet<string> seenSchemes = new HashSet<string>();
            List<string> sanitizedSchemes = new List<string>();
            for (int i = 0; i < preferredControlSchemes.Length; i++)
            {
                string scheme = preferredControlSchemes[i];
                if (string.IsNullOrWhiteSpace(scheme) || !seenSchemes.Add(scheme))
                    continue;

                sanitizedSchemes.Add(scheme);
            }

            preferredControlSchemes = sanitizedSchemes.ToArray();
        }

        public GameplayInputActionBinding FindBinding(GameplayInputActionRole role)
        {
            if (actionBindings == null)
                return null;

            for (int i = 0; i < actionBindings.Length; i++)
            {
                GameplayInputActionBinding binding = actionBindings[i];
                if (binding != null && binding.role == role)
                    return binding;
            }

            return null;
        }

        public GameplayInputActionBinding FindCustomBinding(string customKey)
        {
            if (string.IsNullOrWhiteSpace(customKey) || actionBindings == null)
                return null;

            string key = customKey.Trim();
            for (int i = 0; i < actionBindings.Length; i++)
            {
                GameplayInputActionBinding binding = actionBindings[i];
                if (binding == null || binding.role != GameplayInputActionRole.Custom)
                    continue;

                if (string.Equals(binding.EffectiveKey, key, System.StringComparison.OrdinalIgnoreCase))
                    return binding;
            }

            return null;
        }

        public string GetActionName(GameplayInputActionRole role)
        {
            GameplayInputActionBinding binding = FindBinding(role);
            return binding != null
                ? binding.actionName
                : GameplayInputActionBinding.GetDefaultActionName(role);
        }

        public string GetActionMap(GameplayInputActionBinding binding)
        {
            return binding != null
                ? binding.GetActionMap(primaryActionMap)
                : primaryActionMap;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
