using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    /// <summary>
    /// Authored seat/participant defaults used by sessions and local join flows.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Participants, 
        SetupNodeId = "participant.default",
        Relevance = "Defines a player, AI, seat, hand, faction, or command owner within a session, including the preferred input profile and optional default pawn.",
        AssignmentFields = new[] { nameof(displayName), nameof(defaultPawn), nameof(inputProfile), nameof(teamIndex) },
        FirstProof = "Add this Participant Definition to the 'Default Participants' array in a Session Definition.",
        ExpertAdvice = "ParticipantDefinitions represent seats or control owners. Put the InputProfile here when this participant is who controls the route. Assign a PawnDefinition only for pawn-backed actors; no-pawn routes can control boards, hands, cursors, cameras, factions, menus, or action surfaces.",
        NativeSetup = new[] { "Assign an InputProfile when this participant receives player input.", "Assign a PawnDefinition only for pawn-backed routes." },
        DocumentationURL = "https://docs.neonblack.com/pyralis/session"
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
