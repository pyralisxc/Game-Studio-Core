using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    public enum GameplayNetworkMode
    {
        LocalOnly,
        NetcodeHost,
        NetcodeClient,
        NetcodeServer
    }

    /// <summary>
    /// Top-level session definition for local-first, N-participant-ready gameplay startup.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Definitions/Session Definition", fileName = "SessionDefinition", order = 0)]
    public class SessionDefinition : ScriptableObject
    {
        public string sessionName = "NeonBlack Gameplay Session";
        public GameplayNetworkMode networkMode = GameplayNetworkMode.LocalOnly;
        public bool localFirst = true;
        public bool autoStartHost = true;
        public bool allowLateJoin = true;
        public bool sharedCameraByDefault = true;
        public bool allowSplitScreen = false;
        public int maxParticipants = 4;
        public GameModeDefinition defaultGameMode;
        public InputProfile defaultInputProfile;
        public SettingsProfile settingsProfile;
        public ParticipantDefinition[] defaultParticipants;

        public int GetEffectiveMaxParticipants()
        {
            int modeOverride = defaultGameMode != null ? defaultGameMode.maxParticipantsOverride : 0;
            return modeOverride > 0 ? modeOverride : Mathf.Max(1, maxParticipants);
        }

        public List<string> GetValidationIssues()
        {
            var issues = new List<string>();

            if (string.IsNullOrWhiteSpace(sessionName))
                issues.Add("Session name is required.");

            if (maxParticipants < 1)
                issues.Add("Max participants must be at least 1.");

            if (defaultGameMode == null)
                issues.Add("Default game mode is not assigned.");

            if (networkMode != GameplayNetworkMode.LocalOnly && localFirst)
                issues.Add("Networked sessions should set Local First to false so setup tooling treats NGO as the authority path.");

            if (defaultParticipants == null || defaultParticipants.Length == 0)
            {
                issues.Add("At least one default participant should be assigned.");
            }
            else
            {
                for (int i = 0; i < defaultParticipants.Length; i++)
                {
                    if (defaultParticipants[i] == null)
                        issues.Add($"Default participant slot {i} is empty.");
                }
            }

            return issues;
        }

        public void Sanitize()
        {
            maxParticipants = Mathf.Max(1, maxParticipants);
            if (string.IsNullOrWhiteSpace(sessionName))
                sessionName = "NeonBlack Gameplay Session";

            if (networkMode != GameplayNetworkMode.LocalOnly)
                localFirst = false;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
