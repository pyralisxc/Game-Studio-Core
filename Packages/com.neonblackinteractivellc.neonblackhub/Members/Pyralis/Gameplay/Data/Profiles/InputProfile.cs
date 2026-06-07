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
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Input Profile", fileName = "InputProfile", order = -90)]
    public class InputProfile : ScriptableObject
    {
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

            System.Collections.Generic.HashSet<string> seenSchemes = new System.Collections.Generic.HashSet<string>();
            System.Collections.Generic.List<string> sanitizedSchemes = new System.Collections.Generic.List<string>();
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
