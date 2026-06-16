using UnityEngine;

namespace NeonBlack.Gameplay.Characters
{
    public sealed partial class Pawn3DMovementComponent
    {
        /// <summary>Enter or exit crouch. Respects ceiling clearance when standing up.</summary>
        public void SetCrouch(bool crouch)
        {
            if (!allowCrouch)
            {
                _model.SetCrouching(false);
                return;
            }

            if (crouch)
            {
                _model.SetCrouching(true);
                _runtime.Controller.height = crouchHeight;
                _runtime.Controller.center = crouchCenter;
                return;
            }

            if (!CanStandUp()) return;
            _model.SetCrouching(false);
            _runtime.Controller.height = normalHeight;
            _runtime.Controller.center = normalCenter;
        }

        private bool CanStandUp()
        {
            Vector3 center = transform.TransformPoint(normalCenter);
            float radius = Mathf.Max(_runtime.Controller.radius - _capsuleSkin, 0.01f);
            float half = Mathf.Max(normalHeight * 0.5f - radius, 0f);
            return !Physics.CheckCapsule(
                center + Vector3.up * half,
                center - Vector3.up * half,
                radius,
                groundLayer,
                QueryTriggerInteraction.Ignore);
        }
    }
}
