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
    public sealed partial class Pawn2DPresentationComponent : MonoBehaviour, IPawnPresentationModule, IRuntimeValidationProvider
    {
        private const float MovementInputThresholdSqr = 0.01f;
        private const float PresentationVelocityThreshold = 0.1f;
        private const float DeformationVelocityThreshold = 0.2f;
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

        private bool IsMovingForPresentation()
        {
            return movement != null
                && (movement.MoveDirection.sqrMagnitude > MovementInputThresholdSqr
                    || movement.CurrentVelocity.sqrMagnitude > MovementInputThresholdSqr);
        }
    }
}
