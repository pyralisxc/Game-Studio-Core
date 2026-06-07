using System;

namespace NeonBlack.Gameplay.Characters
{
    /// <summary>
    /// Stable runtime identifier for a participant seat.
    /// </summary>
    [Serializable]
    public struct ParticipantId : IEquatable<ParticipantId>
    {
        public int Value;
        public ParticipantId(int value)
        {
            Value = value;
        }

        public bool Equals(ParticipantId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ParticipantId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => $"P{Value}";

        public static implicit operator int(ParticipantId id) => id.Value;
    }
}
