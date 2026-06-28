using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

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
        StableId = "session.definition",
        Category = "Session",
        CapabilityPath = "Core Setup/Session/Session Definition",
        Surface = AuthoringSurface.Goal,
        Summary = "Root configuration for a gameplay session. Defines the boundary of your game world and network authority.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/session",
        RequiredFields = new[] { nameof(sessionName), nameof(defaultGameMode), nameof(defaultParticipants), nameof(networkMode), nameof(maxParticipants) },
        PrerequisiteStableIds = new[] { "bootstrap.root" },
        RouteStage = "Session Asset",
        RouteOrder = 20,
        SetupDomain = "Session",
        ProofTarget = "SessionDefinition is assigned to GameplaySessionBootstrap.",
        NativeActionKind = AuthoringActionKind.CreateAsset,
        SetupSteps = new[] { "GameplaySessionBootstrap" },
        SuccessChecks = new[] { "Assign this to a GameplaySessionBootstrap in a new scene. It should be the first asset you create." },
        RoleTags = new[] { "IntentRouteEssential", "CoreRouteAnchor" },
        Tags = new[] { "capability:Session", "runtime:PlatformCore", "priority:Primary" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Definitions/Session Definition", fileName = "SessionDefinition", order = 0)]
    public class SessionDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (string.IsNullOrWhiteSpace(sessionName))
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Session name is required.",
                    nameof(sessionName),
                    nameof(SessionDefinition),
                    "Set SessionDefinition.sessionName to a readable session name.",
                    "SessionDefinition has a non-empty session name.",
                    "SessionDefinition.SessionName.Required");
            }

            if (maxParticipants < 1)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Max participants must be at least 1.",
                    nameof(maxParticipants),
                    nameof(SessionDefinition),
                    "Set SessionDefinition.maxParticipants to 1 or higher.",
                    "SessionDefinition.maxParticipants is at least 1.",
                    "SessionDefinition.MaxParticipants.Minimum");
            }

            if (defaultGameMode == null)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Default game mode is not assigned.",
                    nameof(defaultGameMode),
                    nameof(SessionDefinition),
                    "Create or assign a GameModeDefinition on SessionDefinition.defaultGameMode.",
                    "SessionDefinition.defaultGameMode references the game rules for this scene.",
                    "SessionDefinition.DefaultGameMode.Missing");
            }

            if (networkMode != GameplayNetworkMode.LocalOnly && localFirst)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Networked sessions should set Local First to false so setup tooling treats NGO as the authority path.",
                    nameof(localFirst),
                    nameof(SessionDefinition),
                    "Disable SessionDefinition.localFirst for networked session routes.",
                    "Networked SessionDefinition uses NGO authority setup.",
                    "SessionDefinition.LocalFirst.NetworkedMismatch");
            }

            if (defaultParticipants == null || defaultParticipants.Length == 0)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "At least one default participant should be assigned.",
                    nameof(defaultParticipants),
                    nameof(SessionDefinition),
                    "Create or assign at least one ParticipantDefinition in SessionDefinition.defaultParticipants.",
                    "SessionDefinition.defaultParticipants has at least one participant.",
                    "SessionDefinition.DefaultParticipants.Missing");
                yield break;
            }

            int effectiveMaxParticipants = GetEffectiveMaxParticipants();
            if (defaultParticipants.Length > effectiveMaxParticipants)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    $"Session has {defaultParticipants.Length} default participants but only supports {effectiveMaxParticipants} participants.",
                    nameof(defaultParticipants),
                    nameof(SessionDefinition),
                    "Inspect SessionDefinition.defaultParticipants and max participant fields.",
                    "Default participant count fits inside the effective max participant count.",
                    "SessionDefinition.DefaultParticipants.ExceedsMax");
            }

            HashSet<int> preferredSeats = new HashSet<int>();
            for (int i = 0; i < defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = defaultParticipants[i];
                if (participant == null)
                {
                    yield return PyralisRuntimeValidationIssue.Required(
                        $"Default participant slot {i} is empty.",
                        nameof(defaultParticipants),
                        nameof(SessionDefinition),
                        $"Assign a ParticipantDefinition to SessionDefinition.defaultParticipants[{i}] or remove the empty slot.",
                        "Every SessionDefinition.defaultParticipants slot references a ParticipantDefinition.",
                        "SessionDefinition.DefaultParticipants.EmptySlot." + i);
                    continue;
                }

                if (participant.preferredSeatIndex < 0)
                    continue;

                if (participant.preferredSeatIndex >= effectiveMaxParticipants)
                {
                    yield return PyralisRuntimeValidationIssue.Required(
                        $"Participant `{participant.displayName}` prefers seat {participant.preferredSeatIndex}, outside max participant count {effectiveMaxParticipants}.",
                        nameof(defaultParticipants),
                        nameof(SessionDefinition),
                        "Inspect the ParticipantDefinition preferred seat index or increase the session participant limit.",
                        "Every preferred seat index is inside the effective participant range.",
                        "SessionDefinition.ParticipantSeat.OutOfRange." + i);
                    continue;
                }

                if (!preferredSeats.Add(participant.preferredSeatIndex))
                {
                    yield return PyralisRuntimeValidationIssue.Recommended(
                        $"Preferred seat {participant.preferredSeatIndex} is assigned more than once; runtime can reassign duplicates, but prefabs/scenes should author seats clearly.",
                        nameof(defaultParticipants),
                        nameof(SessionDefinition),
                        "Inspect ParticipantDefinition preferred seat indexes and keep authored seats distinct.",
                        "Preferred seat indexes are unique when authored.",
                        "SessionDefinition.ParticipantSeat.Duplicate." + participant.preferredSeatIndex);
                }
            }
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
                int effectiveMaxParticipants = GetEffectiveMaxParticipants();
                if (defaultParticipants.Length > effectiveMaxParticipants)
                    issues.Add($"Session has {defaultParticipants.Length} default participants but only supports {effectiveMaxParticipants} participants.");

                HashSet<int> preferredSeats = new HashSet<int>();
                for (int i = 0; i < defaultParticipants.Length; i++)
                {
                    ParticipantDefinition participant = defaultParticipants[i];
                    if (participant == null)
                    {
                        issues.Add($"Default participant slot {i} is empty.");
                        continue;
                    }

                    if (participant.preferredSeatIndex < 0)
                        continue;

                    if (participant.preferredSeatIndex >= effectiveMaxParticipants)
                    {
                        issues.Add($"Participant `{participant.displayName}` prefers seat {participant.preferredSeatIndex}, outside max participant count {effectiveMaxParticipants}.");
                        continue;
                    }

                    if (!preferredSeats.Add(participant.preferredSeatIndex))
                        issues.Add($"Preferred seat {participant.preferredSeatIndex} is assigned more than once; runtime can reassign duplicates, but prefabs/scenes should author seats clearly.");
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
