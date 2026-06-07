using System;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct RpgOwnerKey : IEquatable<RpgOwnerKey>
    {
        public RpgOwnerKey(RpgOwnerKind kind, string stableId)
        {
            Kind = kind;
            StableId = string.IsNullOrWhiteSpace(stableId) ? string.Empty : stableId.Trim();
        }

        public RpgOwnerKind Kind { get; }
        public string StableId { get; }
        public bool IsValid => Kind != RpgOwnerKind.Unknown && !string.IsNullOrWhiteSpace(StableId);

        public bool Equals(RpgOwnerKey other)
        {
            return Kind == other.Kind && string.Equals(StableId, other.StableId, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is RpgOwnerKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Kind * 397) ^ StringComparer.Ordinal.GetHashCode(StableId ?? string.Empty);
            }
        }

        public override string ToString()
        {
            return IsValid ? $"{Kind}:{StableId}" : "Invalid RPG Owner";
        }

        public static bool operator ==(RpgOwnerKey left, RpgOwnerKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RpgOwnerKey left, RpgOwnerKey right)
        {
            return !left.Equals(right);
        }
    }
}
