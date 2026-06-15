using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    [AuthoringContract(
        Capability = AuthoringCapability.Animation | AuthoringCapability.VFX, 
        Relevance = "2D pawn presentation facade; maps movement state into sprite facing/tint, animation signals, squash/stretch, tilt, and dash/death feedback.",
        Axioms = AuthoringWorldAxiom.Dimensions2D,
        NativeSetup = new[] { "Add on the same root as Motor2D.", "Assign SpriteRenderer." },
        AssignmentFields = new[] { nameof(spriteRenderer), nameof(movingTint), nameof(tiltEnabled), nameof(stretchAmount), nameof(squashSnapSpeed), nameof(tiltSpeed) },
        FirstProof = "Move the pawn and verify the sprite tilts and tints according to velocity.",
        ExpertAdvice = "Keep this as the single 2D presentation facade for beginner prefabs. Sprite facing/tint, animation parameters, deformation, and feedback audio are separate internal lanes and can later become dedicated presenter scripts when a route needs deeper specialization."
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Characters/2D/Pawn 2D Presentation Component")]
    [RequireComponent(typeof(Pawn2DMovementComponent))]
    [RequireComponent(typeof(ActorAnimationDriver))]
    public sealed class Pawn2DPresentationComponent : MonoBehaviour, IPawnPresentationModule, IRuntimeValidationProvider
    {
        private const float MovementInputThresholdSqr = 0.01f;
        private const float PresentationVelocityThreshold = 0.1f;
        private const float DeformationVelocityThreshold = 0.2f;

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (spriteRenderer == null && GetComponentInChildren<SpriteRenderer>(true) == null)
                yield return "Sprite Renderer is empty and no child SpriteRenderer was found.";
            if (stretchAmount < 1f)
                yield return "Stretch Amount should be at least 1.";
        }
        [Header("Sprite")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private bool spriteDefaultFacesRight = true;
        [SerializeField] private Color movingTint = Color.white;
        [SerializeField] private Color idleTint = Color.white;

        [Header("Squash & Stretch")]
        [SerializeField] private bool squashStretchEnabled = true;
        [SerializeField, Range(1f, 1.5f)] private float stretchAmount = 1.15f;
        [SerializeField] private float squashSnapSpeed = 10f;

        [Header("Tilt")]
        [SerializeField] private bool tiltEnabled = true;
        [SerializeField, Range(0f, 90f)] private float maxTiltAngle = 12f;
        [SerializeField, Range(1f, 720f)] private float tiltSpeed = 200f;

        [Header("Animator")]
        [SerializeField] private Animator animator;
        [SerializeField, Min(0f)] private float idleDelay = 0.15f;

        [Header("Death")]
        [SerializeField] private AudioClip deathClip;
        [SerializeField] private AudioClip dashClip;

        private Pawn2DMovementComponent movement;
        private ActorAnimationDriver animationDriver;
        private AudioSource audioSource;
        private Vector3 baseScale;
        private float currentTiltAngle;
        private float movingHoldTimer;

        private void Awake()
        {
            movement = GetComponent<Pawn2DMovementComponent>();
            animationDriver = GetComponent<ActorAnimationDriver>();
            baseScale = transform.localScale;
            animator ??= GetComponent<Animator>();
            spriteRenderer ??= GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>(true);
            EnsureAudioSource();
        }

        private void EnsureAudioSource()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            audioSource.playOnAwake = false;
        }

        private void Update()
        {
            if (movement == null)
                return;

            TickAnimationSignalLane();
            TickSpriteFacingAndTintLane();
            TickDeformationLane();
        }

        public void ApplyPresentationProfile(PawnProfileApplicationContext context, PawnPresentationProfile presentationProfile)
        {
            if (presentationProfile != null)
            {
                spriteDefaultFacesRight = presentationProfile.spriteDefaultFacesRight;
                Color participantTint = context.Participant?.Definition != null
                    ? context.Participant.Definition.tint
                    : Color.white;
                idleTint = MultiplyTint(presentationProfile.primaryTint, participantTint);
                movingTint = idleTint;
            }

            animationDriver?.ApplyProfiles(
                presentationProfile,
                context.PawnDefinition != null ? context.PawnDefinition.animationProfile : null);

            if (presentationProfile != null && spriteRenderer != null)
                spriteRenderer.color = idleTint;
        }

        private static Color MultiplyTint(Color baseTint, Color participantTint)
        {
            return new Color(
                baseTint.r * participantTint.r,
                baseTint.g * participantTint.g,
                baseTint.b * participantTint.b,
                baseTint.a * participantTint.a);
        }

        public void ResetForRound()
        {
            ResetTransientVisualState();

            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }

            if (spriteRenderer != null)
                spriteRenderer.color = idleTint;

            animationDriver?.SetBoolSignal(ActorAnimationSignal.Move, false);
            animationDriver?.SetBoolSignal(ActorAnimationSignal.Idle, true);
            animationDriver?.SetBoolSignal(ActorAnimationSignal.Dash, false);
        }

        public void PlayDashFeedback()
        {
            if (dashClip != null)
                audioSource.PlayOneShot(dashClip);
            animationDriver?.TriggerSignal(ActorAnimationSignal.Dash);
        }

        public void PlayDeathFeedback()
        {
            ResetTransientVisualState();
            animationDriver?.TriggerSignal(ActorAnimationSignal.Death);
            if (deathClip != null)
                audioSource.PlayOneShot(deathClip);
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            try { Handheld.Vibrate(); } catch { }
#endif
        }

        public void ResetMoveToIdle()
        {
            movingHoldTimer = 0f;
            animationDriver?.SetBoolSignal(ActorAnimationSignal.Move, false);
            animationDriver?.SetBoolSignal(ActorAnimationSignal.Idle, true);
        }

        private void ResetTransientVisualState()
        {
            movingHoldTimer = 0f;
            currentTiltAngle = 0f;
            transform.rotation = Quaternion.identity;
            transform.localScale = baseScale;
        }

        private void TickAnimationSignalLane()
        {
            if (animationDriver == null || movement == null)
                return;

            bool moving = IsMovingForPresentation();
            animationDriver.SetBoolSignal(ActorAnimationSignal.Move, moving);
            animationDriver.SetBoolSignal(ActorAnimationSignal.Idle, !moving);
            animationDriver.SetBoolSignal(ActorAnimationSignal.Dash, movement.IsDashing);
            ApplyBlendTreeChannels();
        }

        private void ApplyBlendTreeChannels()
        {
            Vector2 velocity = movement.CurrentVelocity;
            float speed = velocity.magnitude;
            float normalizedSpeed = Mathf.Clamp01(speed / Mathf.Max(0.01f, movement.MoveSpeed));

            animationDriver.SetFloatCustom("Speed", speed);
            animationDriver.SetFloatCustom("NormalizedSpeed", normalizedSpeed);
            animationDriver.SetFloatCustom("MoveX", movement.MoveDirection.x);
            animationDriver.SetFloatCustom("MoveY", movement.MoveDirection.y);
            animationDriver.SetFloatCustom("VelocityX", velocity.x);
            animationDriver.SetFloatCustom("VelocityY", velocity.y);
        }

        private void TickSpriteFacingAndTintLane()
        {
            if (spriteRenderer == null || movement == null)
                return;
            if (movement.IsDead)
                return;

            if (IsMovingForPresentation())
                movingHoldTimer = idleDelay;
            else
                movingHoldTimer -= Time.deltaTime;
            bool moving = movingHoldTimer > 0f;

            if (animator == null)
                spriteRenderer.color = moving ? movingTint : idleTint;

            bool facingRight = movement.FacingRight;
            animationDriver?.SetFacing(facingRight);

            if (movement.MoveDirection.x > 0.05f)
            {
                spriteRenderer.flipX = !spriteDefaultFacesRight;
            }
            else if (movement.MoveDirection.x < -0.05f)
            {
                spriteRenderer.flipX = spriteDefaultFacesRight;
            }
        }

        private void TickDeformationLane()
        {
            if (movement.IsDead)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, baseScale, squashSnapSpeed * Time.deltaTime);
                return;
            }

            if (squashStretchEnabled)
                TickSquashStretchLane();

            if (tiltEnabled)
                TickTiltLane();
        }

        private void TickTiltLane()
        {
            Vector2 velocity = movement.CurrentVelocity;
            float targetAngle = 0f;
            float speed = velocity.magnitude;
            if (speed > PresentationVelocityThreshold)
            {
                float velAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                float lean = -Mathf.Sin(velAngle * Mathf.Deg2Rad);
                float speedT = Mathf.Clamp01(speed / Mathf.Max(0.01f, movement.MoveSpeed));
                targetAngle = lean * maxTiltAngle * speedT;

                if (spriteRenderer != null && spriteRenderer.flipX)
                    targetAngle = -targetAngle;
            }

            currentTiltAngle = Mathf.MoveTowards(currentTiltAngle, targetAngle, tiltSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, currentTiltAngle);
        }

        private void TickSquashStretchLane()
        {
            Vector3 targetScale = baseScale;
            Vector2 velocity = movement.CurrentVelocity;
            float speed = velocity.magnitude;
            if (speed > DeformationVelocityThreshold)
            {
                float t = Mathf.Clamp01(speed / Mathf.Max(0.01f, movement.MoveSpeed));
                float stretch = Mathf.Lerp(1f, stretchAmount, t);
                bool horizontal = Mathf.Abs(velocity.x) >= Mathf.Abs(velocity.y);
                targetScale = horizontal
                    ? new Vector3(baseScale.x * stretch, baseScale.y / stretch, baseScale.z)
                    : new Vector3(baseScale.x / stretch, baseScale.y * stretch, baseScale.z);
            }

            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, squashSnapSpeed * Time.deltaTime);
        }

        private bool IsMovingForPresentation()
        {
            return movement != null
                && (movement.MoveDirection.sqrMagnitude > MovementInputThresholdSqr
                    || movement.CurrentVelocity.sqrMagnitude > MovementInputThresholdSqr);
        }
    }
}
