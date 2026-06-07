using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    /// <summary>
    /// Authored seat/participant defaults used by sessions and local join flows.
    /// </summary>
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
