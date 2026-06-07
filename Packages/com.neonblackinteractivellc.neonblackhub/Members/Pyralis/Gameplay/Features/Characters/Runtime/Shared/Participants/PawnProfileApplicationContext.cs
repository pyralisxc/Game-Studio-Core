using NeonBlack.Gameplay.Data.Definitions;
using UnityEngine;

namespace NeonBlack.Gameplay.Characters
{
    /// <summary>
    /// Lightweight context used when authored pawn profiles are applied to runtime modules.
    /// Keeps profile application independent from the PawnRoot MonoBehaviour itself.
    /// </summary>
    public readonly struct PawnProfileApplicationContext
    {
        public GameObject PawnObject { get; }
        public Transform PawnTransform => PawnObject != null ? PawnObject.transform : null;
        public PawnDefinition PawnDefinition { get; }
        public ParticipantHandle Participant { get; }

        public PawnProfileApplicationContext(GameObject pawnObject, PawnDefinition pawnDefinition, ParticipantHandle participant)
        {
            PawnObject = pawnObject;
            PawnDefinition = pawnDefinition;
            Participant = participant;
        }
    }
}
