using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Traversal;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Interaction;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Config;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Features.Characters
{
/// <summary>
/// Coordinator for a 3D pawn. Sequences four sibling modules each frame and exposes
/// the <see cref="ICharacterMotorState"/> contract to systems like <see cref="PawnCombatBehaviour"/>.
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.KineticMotor3D,
    Priority = AuthoringPriority.Primary,
    Lane = "Pawn3D",
    Relevance = "Canonical 3D pawn motor; sequences input, movement, traversal, and presentation sibling modules.",
    Axioms = AuthoringWorldAxiom.Dimensions3D | AuthoringWorldAxiom.Realtime,
    NativeSetup = new[]
    {
        "Attach Motor3D to the 3D pawn root.",
        "Add all required 3D pawn modules (Input, Movement, Traversal, Presentation).",
        "Assign the InputSystem_Actions asset on Pawn3DInputModule.",
        "Ensure an Animator with the required parameters is present."
    },
    AssignmentFields = new[] { "Pawn3DInputModule", "Pawn3DMovementComponent", "Pawn3DTraversalComponent", "Pawn3DPresentationComponent" },
    FirstProof = "Pawn responds to Move input and plays walk animations. Optional traversal features such as ledge-climb can extend the explicit traversal sibling when installed.",
    ExpertAdvice = "Motor3D is the explicit 3D pawn coordinator. It sequences sibling modules in a deterministic order; ActorFeatureHost may add optional capabilities, but it should not construct the pawn's core movement identity. Ensure CharacterController 'Skin Width' is at least 10% of the radius to prevent jitter on slopes.",
    DocumentationURL = "https://docs.neonblack.com/pyralis/movement"
)]
[AddComponentMenu("NeonBlack/Gameplay/Runtime 3D/Motor 3D")]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(KnockbackReceiver))]
[RequireComponent(typeof(Pawn3DInputModule))]
[RequireComponent(typeof(Pawn3DMovementComponent))]
[RequireComponent(typeof(Pawn3DTraversalComponent))]
[RequireComponent(typeof(Pawn3DPresentationComponent))]
public partial class Motor3D : MonoBehaviour, ICharacterMotorState, IActorReactionResponder, IActorMovementModifierReceiver, IClimbTraversalActor
{
    private Motor3DRuntimeReferences _runtime;
    private float                     _reactionLockTimer;
    private bool                      _statusActionLocked;

    private Pawn3DInputModule Input => _runtime?.Input;
    private Pawn3DMovementComponent Movement => _runtime?.Movement;
    private Pawn3DTraversalComponent Traversal => _runtime?.Traversal;
    private Pawn3DPresentationComponent Presentation => _runtime?.Presentation;
    private PawnCombatBehaviour Combat => _runtime?.Combat;
    private HealthComponent Health => _runtime?.Health;
    private IActorTraversalFeature TraversalFeature => _runtime?.TraversalFeature;
    private IActorInteractionFeature InteractionFeature => _runtime?.InteractionFeature;
    private IActorGuardFeature GuardFeature => _runtime?.GuardFeature;

    //  ICharacterMotorState  //
    public bool IsGrounded  => Movement.State.IsGrounded;
    public bool IsAirborne  => !Movement.State.IsGrounded || Movement.State.VelocityY > 0f;
    public bool FacingRight => Movement.State.FacingRight;
    public bool IsActing
    {
        get => Movement.State.IsActing;
        set => Movement.SetActing(value);
    }

    public void ResetMoveToIdle() => Presentation.ResetMoveToIdle();

    //  Public accessors (for camera, UI, and other systems)  //
    public bool  IsBlocking           => GuardFeature?.IsGuarding ?? (Combat?.IsBlocking ?? false);
    public float BlockDamageReduction => GuardFeature?.BlockDamageReduction ?? (Combat?.BlockDamageReduction ?? 0f);
    public float BlockFrontalAngle    => GuardFeature?.BlockFrontalAngle ?? (Combat?.BlockFrontalAngle ?? 90f);
    public bool     IsCrouching     => Movement.State.IsCrouching;
    public bool     IsSprinting     => Movement.State.IsSprinting;
    public Vector3  CurrentVelocity => new Vector3(Movement.State.VelocityX, Movement.State.VelocityY, Movement.State.VelocityZ);

    //  Unity lifecycle  //
    private void Awake()
    {
        _runtime = Motor3DRuntimeReferences.Capture(gameObject);

        if (Health != null && Movement != null)
            Health.OnDamaged.AddListener(_ => Movement.TriggerKnockBack());
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) =>
        Movement.NotifyColliderHit(hit);

    //  Public API  //
    /// <summary>Trigger the knocked-back hit-reaction. Wire to HealthComponent.OnDamaged or call from combat code.</summary>
    public void TriggerKnockedBack() => Movement.TriggerKnockBack();

    /// <summary>Play the ClimbUp animation. Call from an external ladder or climbable script.</summary>
    public void TriggerClimbUp() => Traversal.TriggerClimbUp();

    /// <summary>Swap the InputConfig (per-participant overrides). Delegates to the input module.</summary>
    public void SetInputConfig(InputConfig config, bool overrideExisting = true) =>
        Input.SetInputConfig(config, overrideExisting);

    /// <summary>Swap the raw InputActionAsset. Delegates to the input module.</summary>
    public void SetInputActions(InputActionAsset asset, bool overrideExisting = true) =>
        Input.SetInputActions(asset, overrideExisting);

    private void ResolveFeatureModules()
    {
        _runtime?.ResolveFeatureModules();
    }
}
}
