using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Definitions
{
    /// <summary>
    /// Authored seat/participant defaults used by sessions and local join flows.
    /// </summary>
    [AuthoringContract(
        StableId = "participant.default",
        Category = "Participants",
        CapabilityPath = "Core Setup/Participants/Participant Definition",
        Surface = AuthoringSurface.Goal,
        Summary = "Defines a player, AI, seat, hand, faction, or command owner within a session, including the preferred input profile and optional default pawn.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/session",
        RequiredFields = new[] { nameof(displayName) },
        PrerequisiteStableIds = new[] { "mode.definition" },
        RouteStage = "Participant Asset",
        RouteOrder = 50,
        SetupDomain = "Participants",
        ProofTarget = "ParticipantDefinition is assigned to the active SessionDefinition.",
        NativeActionKind = AuthoringActionKind.CreateAsset,
        SetupSteps = new[] { "Assign an InputProfile when this participant receives player input.", "Assign a PawnDefinition only for pawn-backed routes." },
        SuccessChecks = new[] { "Add this Participant Definition to the 'Default Participants' array in a Session Definition." },
        RoleTags = new[] { "IntentRouteEssential", "ParticipantRouteSupport", "Participant", "InputOwner", "PawnOwner" },
        Tags = new[] { "capability:Participants", "runtime:PlatformCore", "runtime:CharacterPawnGameplay" }
    )]
[CreateAssetMenu(menuName = "NeonBlack/Definitions/Participant Definition", fileName = "ParticipantDefinition", order = 20)]
    public class ParticipantDefinition : ScriptableObject
    {
        public string displayName = "Participant";
        public bool autoJoin = true;
        public int teamIndex = 0;
        public int preferredSeatIndex = -1;
        public Color tint = Color.white;
        public PawnDefinition defaultPawn;
        public InputProfile inputProfile;

        public void Sanitize()
        {
            teamIndex = Mathf.Max(0, teamIndex);
            preferredSeatIndex = Mathf.Max(-1, preferredSeatIndex);
            if (string.IsNullOrWhiteSpace(displayName))
                displayName = "Participant";
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
