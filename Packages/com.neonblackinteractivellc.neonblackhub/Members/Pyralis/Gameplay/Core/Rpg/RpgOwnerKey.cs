using System;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Rpg
{
    [Serializable]
    public struct RpgOwnerKey : IEquatable<RpgOwnerKey>
{
        [SerializeField] private RpgOwnerKind kind;
        [SerializeField] private string stableId;

        public RpgOwnerKey(RpgOwnerKind kind, string stableId)
        {
            this.kind = kind;
            this.stableId = string.IsNullOrWhiteSpace(stableId) ? string.Empty : stableId.Trim();
        }

        public RpgOwnerKind Kind => kind;
        public string StableId => stableId;
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
