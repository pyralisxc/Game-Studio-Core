using System;

namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Runtime state for a playable board space.
    /// </summary>
    [Serializable]
    public sealed class BoardSpaceState
    {
        public BoardSpaceState(BoardCoordinate coordinate, string spaceId = null, bool isActive = true)
        {
            Coordinate = coordinate;
            SpaceId = spaceId ?? coordinate.ToString();
            IsActive = isActive;
        }

        public BoardCoordinate Coordinate { get; }
        public string SpaceId { get; }
        public bool IsActive { get; }
    }
}
