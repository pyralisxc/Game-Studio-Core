using System;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Stable grid address used by board and tabletop rule services.
    /// </summary>
    [Serializable]
    public struct BoardCoordinate : IEquatable<BoardCoordinate>
    {
        [SerializeField] private int x;
        [SerializeField] private int y;

        public BoardCoordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int X => x;
        public int Y => y;

        public bool Equals(BoardCoordinate other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is BoardCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return $"{X},{Y}";
        }

        public static bool operator ==(BoardCoordinate left, BoardCoordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoardCoordinate left, BoardCoordinate right)
        {
            return !left.Equals(right);
        }
    }
}
