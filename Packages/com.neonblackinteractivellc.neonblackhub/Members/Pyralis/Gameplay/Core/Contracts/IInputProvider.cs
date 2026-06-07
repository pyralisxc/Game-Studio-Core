using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Input abstraction for gameplay modules that should be independent of a specific input source.
    /// </summary>
    public interface IInputProvider
    {
        Vector2 Move { get; }
        Vector2 Look { get; }
        bool IsSprinting { get; }
    }
}
