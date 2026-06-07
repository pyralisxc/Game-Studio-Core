using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// Compatibility facade for the 2D pawn stack.
    /// Movement, presentation, and reaction ownership now live in dedicated 2D components.
    /// </summary>
    [AddComponentMenu("NeonBlack/Gameplay/Runtime 2D/Motor 2D")]
    [RequireComponent(typeof(Pawn2DMovementComponent))]
    [RequireComponent(typeof(Pawn2DPresentationComponent))]
    public sealed class Motor2D : MonoBehaviour, IPawnMotor, IPawnPresentationModule, IActorReactionResponder, IActorMovementModifierReceiver, IFacingDirectionProvider
    {
        private Pawn2DMovementComponent movement;
        private Pawn2DPresentationComponent presentation;

        public Vector2 MoveDirection
        {
            get => movement != null ? movement.MoveDirection : Vector2.zero;
            set
            {
                if (movement != null)
                    movement.MoveDirection = value;
            }
        }

        public Vector2 CurrentVelocity => movement != null ? movement.CurrentVelocity : Vector2.zero;
        public bool FacingRight => movement != null && movement.FacingRight;
        public bool IsDashing => movement != null && movement.IsDashing;
        public bool IsDead => movement != null && movement.IsDead;
        public float DashCooldownRemaining => movement != null ? movement.DashCooldownRemaining : 0f;
        public bool IsActionLocked => movement != null && movement.IsActionLocked;

        private void Awake()
        {
            movement = GetComponent<Pawn2DMovementComponent>();
            presentation = GetComponent<Pawn2DPresentationComponent>();
        }

        public bool TryDash(Vector2 direction)
        {
            bool started = movement != null && movement.TryDash(direction);
            if (started)
                presentation?.PlayDashFeedback();
            return started;
        }

        public void PlayDeathAnimation()
        {
            if (movement == null || movement.IsDead)
                return;

            movement.NotifyDeath();
            presentation?.PlayDeathFeedback();
        }

        public void ResetForRound(Vector3 position)
        {
            movement?.ResetForRound(position);
            presentation?.ResetForRound();
        }

        public void ResetMoveToIdle()
        {
            movement?.ResetMoveToIdle();
            presentation?.ResetMoveToIdle();
        }

        public void SetActionLock(bool locked) => movement?.SetActionLock(locked);
        public void ApplyReactionLock(float duration) => movement?.ApplyReactionLock(duration);
        public void ClearReactionLock() => movement?.ClearReactionLock();
        public void SetStatusMoveSpeedMultiplier(float multiplier) => movement?.SetStatusMoveSpeedMultiplier(multiplier);
        public void SetStatusActionLock(bool locked) => movement?.SetStatusActionLock(locked);

        public void ApplyMovementProfile(PawnProfileApplicationContext context, PawnMovementProfile movementProfile)
        {
            movement?.ApplyMovementProfile(context, movementProfile);
        }

        public void ApplyPresentationProfile(PawnProfileApplicationContext context, PawnPresentationProfile presentationProfile)
        {
            movement?.ApplyPresentationProfile(context, presentationProfile);
            presentation?.ApplyPresentationProfile(context, presentationProfile);
        }
    }
}
