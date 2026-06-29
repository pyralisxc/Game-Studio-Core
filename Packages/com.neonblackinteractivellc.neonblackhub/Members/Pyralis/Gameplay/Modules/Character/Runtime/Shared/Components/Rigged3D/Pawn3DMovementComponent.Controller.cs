using NeonBlack.Gameplay.Modules.Character;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    public sealed partial class Pawn3DMovementComponent
    {
        /// <summary>
        /// Apply model velocity + knockback via CharacterController and record
        /// this frame's physics results for the next <see cref="Tick"/> call.
        /// </summary>
        public void ApplyMovement(Vector3 modelVelocity, float deltaTime)
        {
            if (!_runtime.Controller.enabled) return;

            ResetPhysicsFrame();

            Vector3 knockbackVelocity = Vector3.zero;
            if (_runtime.Knockback != null)
            {
                _runtime.Knockback.Tick(deltaTime);
                knockbackVelocity = _runtime.Knockback.Velocity;
            }

            CollisionFlags flags = _runtime.Controller.Move((modelVelocity + knockbackVelocity) * deltaTime);

            bool byCollision = (flags & CollisionFlags.Below) != 0;
            bool byProbe = false;
            if (!byCollision && modelVelocity.y <= 0f)
            {
                float radius = Mathf.Clamp(groundCheckRadius, 0.02f, _runtime.Controller.radius * 0.95f);
                Vector3 origin = GetGroundProbeOrigin();
                byProbe = Physics.SphereCast(origin, radius, Vector3.down, out _,
                    Mathf.Max(groundProbeExtraDistance, 0.02f), groundLayer, QueryTriggerInteraction.Ignore);
            }

            _physicsFrame.GroundedByCollision = byCollision;
            _physicsFrame.GroundedByProbe = byProbe;
        }

        /// <summary>
        /// Feed a CharacterController surface contact into this frame's physics accumulator.
        /// Call from <see cref="Motor3D.OnControllerColliderHit"/>.
        /// </summary>
        public void NotifyColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y > 0.1f)
            {
                _physicsFrame.GroundNormal = hit.normal;
            }
            else if (hit.normal.y >= -0.1f)
            {
                Vector3 moveDir = new Vector3(_model.State.VelocityX, 0f, _model.State.VelocityZ);
                if (moveDir.sqrMagnitude > 0.01f && Vector3.Dot(moveDir.normalized, -hit.normal) > 0.3f)
                    _physicsFrame.HasWallContact = true;
            }
        }

        private Vector3 GetGroundProbeOrigin()
        {
            Bounds b = _runtime.Controller.bounds;
            return new Vector3(b.center.x, b.min.y + 0.02f, b.center.z);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            CharacterController controller = _runtime?.Controller;
            if (!Application.isPlaying || controller == null) return;

            Bounds b = controller.bounds;
            Vector3 origin = new Vector3(b.center.x, b.min.y + 0.02f, b.center.z);
            float radius = Mathf.Clamp(groundCheckRadius, 0.02f, controller.radius * 0.95f);
            Gizmos.color = _model.State.IsGrounded ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
            Gizmos.DrawSphere(origin, radius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + Vector3.down * Mathf.Max(groundProbeExtraDistance, 0.02f));
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(new Vector3(b.center.x, b.min.y, b.center.z), 0.03f);
        }
#endif
    }
}
