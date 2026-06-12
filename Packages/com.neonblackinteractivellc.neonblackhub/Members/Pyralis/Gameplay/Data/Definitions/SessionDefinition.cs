using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
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
    [AuthoringContract(
        Capability = AuthoringCapability.Session, 
        Priority = AuthoringPriority.Primary,
        SetupNodeId = "session.definition",
        Relevance = "Root configuration for a gameplay session. Defines the boundary of your game world and network authority.",
        AssignmentFields = new[] { nameof(sessionName), nameof(defaultGameMode), nameof(defaultParticipants), nameof(defaultInputProfile), nameof(networkMode), nameof(maxParticipants) },
        NativeSetup = new[] { "GameplaySessionBootstrap" },
        FirstProof = "Assign this to a GameplaySessionBootstrap in a new scene. It should be the first asset you create.",
        ExpertAdvice = "SessionDefinition is your session's 'Law'. For local-only prototypes, keep 'Local First' checked to bypass networking overhead. Assign a Default Input Profile here to save time on per-pawn setup.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/session"
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Definitions/Session Definition", fileName = "SessionDefinition", order = 0)]
    public class SessionDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            return GetValidationIssues();
        }

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
