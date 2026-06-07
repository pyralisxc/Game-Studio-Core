using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Authoring profile for participant input ownership and preferred control schemes.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Profiles/Input Profile", fileName = "InputProfile")]
    public class InputProfile : ScriptableObject
    {
        [Tooltip("Primary input action asset used by this participant or pawn definition.")]
        public InputActionAsset actions;
        [Tooltip("Default action map name expected by shared and legacy bridges.")]
        public string primaryActionMap = "Player";

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

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
