using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// Physics results accumulated by the <see cref="Pawn3DMovementComponent"/> MonoBehaviour during one
    /// Update frame â€” from CharacterController.Move, ground probes, and OnControllerColliderHit.
    /// Passed to <see cref="BrawlerMovementModel.Tick"/> on the following frame so the model
    /// can update grounding state without touching Unity physics APIs directly.
    /// </summary>
    public struct MovementPhysicsFrame
    {
        /// <summary>CollisionFlags.Below was set after CharacterController.Move this frame.</summary>
        public bool    GroundedByCollision;
        /// <summary>SphereCast fallback probe confirmed ground contact this frame.</summary>
        public bool    GroundedByProbe;
        /// <summary>Surface normal from the most recent ground contact (OnControllerColliderHit).</summary>
        public Vector3 GroundNormal;
        /// <summary>True if a mostly-vertical surface was hit while pressing toward it.</summary>
        public bool    HasWallContact;

        /// <summary>Safe starting state: upright ground normal, no contact.</summary>
        public static MovementPhysicsFrame Default => new MovementPhysicsFrame
        {
            GroundNormal = Vector3.up
        };
    }
}
