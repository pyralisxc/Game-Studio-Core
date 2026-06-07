using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine.InputSystem;

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
    }
}
