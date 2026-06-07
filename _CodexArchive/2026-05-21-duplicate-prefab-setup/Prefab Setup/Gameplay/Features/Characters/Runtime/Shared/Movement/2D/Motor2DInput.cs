using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// Per-frame input for <see cref="Motor2DModel.Tick"/>.
    /// The MonoBehaviour layer populates this from the InputSystem and passes it each FixedUpdate.
    /// </summary>
    public struct Motor2DInput
    {
        /// <summary>Normalised movement direction set by <c>PlayerInputHandler</c> each frame.</summary>
        public Vector2 MoveDirection;
    }
}
