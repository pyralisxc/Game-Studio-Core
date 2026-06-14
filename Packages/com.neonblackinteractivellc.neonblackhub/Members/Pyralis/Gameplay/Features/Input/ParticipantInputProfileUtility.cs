using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine.InputSystem;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Input
{
    /// <summary>
    /// Shared helpers for resolving and applying authored input profiles.
    /// </summary>
    public static class ParticipantInputProfileUtility
    {
        public static InputProfile ResolveEffectiveInputProfile(
            ParticipantDefinition participantDefinition,
            PawnDefinition pawnDefinition,
            InputProfile sessionDefaultInputProfile)
        {
            if (participantDefinition != null && participantDefinition.inputProfile != null)
                return participantDefinition.inputProfile;
            if (pawnDefinition != null && pawnDefinition.defaultInputProfile != null)
                return pawnDefinition.defaultInputProfile;

            return sessionDefaultInputProfile;
        }

        public static void ApplyToPlayerInput(PlayerInput playerInput, InputProfile profile)
        {
            if (playerInput == null || profile == null)
                return;

            if (profile.actions != null && playerInput.actions != profile.actions)
                playerInput.actions = profile.actions;

            if (playerInput.actions == null)
                return;

            if (!string.IsNullOrWhiteSpace(profile.primaryActionMap) &&
                playerInput.actions.FindActionMap(profile.primaryActionMap, throwIfNotFound: false) != null)
            {
                playerInput.SwitchCurrentActionMap(profile.primaryActionMap);
            }

            if (profile.preferredControlSchemes != null)
            {
                for (int i = 0; i < profile.preferredControlSchemes.Length; i++)
                {
                    string scheme = profile.preferredControlSchemes[i];
                    if (string.IsNullOrWhiteSpace(scheme))
                        continue;

                    if (playerInput.actions.FindControlScheme(scheme).HasValue)
                    {
                        playerInput.defaultControlScheme = scheme;
                        break;
                    }
                }
            }

            playerInput.neverAutoSwitchControlSchemes = profile.preferredControlSchemes != null &&
                profile.preferredControlSchemes.Length > 0;
        }

        public static InputActionMap FindGameplayActionMap(InputActionAsset actions, InputProfile profile)
        {
            if (actions == null)
                return null;

            string actionMapName = profile != null && !string.IsNullOrWhiteSpace(profile.primaryActionMap)
                ? profile.primaryActionMap
                : "Player";

            return actions.FindActionMap(actionMapName, throwIfNotFound: false);
        }

        public static InputAction FindAction(InputActionMap actionMap, string actionName)
        {
            if (actionMap == null || string.IsNullOrWhiteSpace(actionName))
                return null;

            return actionMap.FindAction(actionName, throwIfNotFound: false);
        }

        public static InputAction FindAction(InputActionAsset actions, InputProfile profile, GameplayInputActionRole role)
        {
            if (actions == null)
                return null;

            GameplayInputActionBinding binding = profile != null ? profile.FindBinding(role) : null;
            string actionName = binding != null
                ? binding.actionName
                : GameplayInputActionBinding.GetDefaultActionName(role);
            string mapName = profile != null && binding != null
                ? profile.GetActionMap(binding)
                : profile != null && !string.IsNullOrWhiteSpace(profile.primaryActionMap)
                    ? profile.primaryActionMap
                    : "Player";

            InputActionMap map = actions.FindActionMap(mapName, throwIfNotFound: false);
            return FindAction(map, actionName);
        }

        public static InputAction FindAction(InputActionMap actionMap, InputProfile profile, GameplayInputActionRole role)
        {
            GameplayInputActionBinding binding = profile != null ? profile.FindBinding(role) : null;
            string actionName = binding != null
                ? binding.actionName
                : GameplayInputActionBinding.GetDefaultActionName(role);

            return FindAction(actionMap, actionName);
        }

        public static string GetActionName(InputProfile profile, GameplayInputActionRole role)
        {
            return profile != null
                ? profile.GetActionName(role)
                : GameplayInputActionBinding.GetDefaultActionName(role);
        }

        public static bool HasRequiredBinding(InputProfile profile, GameplayInputActionRole role)
        {
            GameplayInputActionBinding binding = profile != null ? profile.FindBinding(role) : null;
            return binding != null && binding.requiredForGameplay;
        }

        public static void LogMissingAction(Object context, string componentName, InputProfile profile, string actionRole, string actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                return;

            string mapName = profile != null && !string.IsNullOrWhiteSpace(profile.primaryActionMap)
                ? profile.primaryActionMap
                : "Player";
            Debug.LogWarning($"[{componentName}] InputProfile maps {actionRole} to '{mapName}/{actionName}', but that action was not found. Update the InputProfile action names or the Input Action Asset.", context);
        }
    }
}
