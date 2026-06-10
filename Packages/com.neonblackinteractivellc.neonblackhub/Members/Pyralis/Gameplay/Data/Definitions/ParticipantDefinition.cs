using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    /// <summary>
    /// Authored seat/participant defaults used by sessions and local join flows.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Session, 
        Relevance = "Defines a player or NPC seat within a session, including their default pawn and input configuration.",
        AssignmentFields = new[] { nameof(displayName), nameof(defaultPawn), nameof(inputProfile), nameof(teamIndex) },
        FirstProof = "Add this Participant Definition to the 'Default Participants' array in a Session Definition."
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
