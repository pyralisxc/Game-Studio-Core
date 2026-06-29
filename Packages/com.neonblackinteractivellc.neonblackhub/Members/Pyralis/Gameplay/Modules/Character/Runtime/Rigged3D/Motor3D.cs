using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Participants;
using UnityEngine;
using UnityEngine.InputSystem;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Character
{
/// <summary>
/// Coordinator for a 3D pawn. Sequences four sibling modules each frame and exposes
/// a combat-facing movement state contract to sibling gameplay modules.
/// </summary>
[AuthoringContract(
        Category = "Kinetic Motor3 D",
        CapabilityPath = "Movement/Traversal/Motor3D",
        Surface = AuthoringSurface.Goal,
        Summary = "Canonical 3D pawn motor; sequences input, movement, traversal, and presentation sibling modules.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/movement",
        RequiredFields = new[] { "Pawn3DInputModule", "Pawn3DMovementComponent", "Pawn3DTraversalComponent", "Pawn3DPresentationComponent" },
        SetupSteps = new[]
    {
        "Attach Motor3D to the 3D pawn root.",
        "Add all required 3D pawn modules (Input, Movement, Traversal, Presentation).",
        "Assign the InputSystem_Actions asset on Pawn3DInputModule.",
        "Ensure an Animator with the required parameters is present."
    },
        SuccessChecks = new[] { "Pawn responds to Move input and plays walk animations. Optional traversal features such as ledge-climb can extend the explicit traversal sibling when installed." },
        Tags = new[] { "capability:KineticMotor3D", "axiom:Dimensions3D", "axiom:Realtime", "lane:Pawn3D", "priority:Primary" }
    )]
[AddComponentMenu("NeonBlack/Gameplay/Runtime 3D/Motor 3D")]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Pawn3DInputModule))]
[RequireComponent(typeof(Pawn3DMovementComponent))]
[RequireComponent(typeof(Pawn3DPresentationComponent))]
public partial class Motor3D : GameplayTickBehaviour, IActorCombatMovementState, IActorReactionResponder, IActorMovementModifierReceiver, IClimbTraversalActor, IActorMotionStateReader
{
    private Motor3DRuntimeReferences _runtime;
    private float                     _reactionLockTimer;
    private bool                      _statusActionLocked;

    private Pawn3DInputModule Input => _runtime?.Input;
    private Pawn3DMovementComponent Movement => _runtime?.Movement;
    private IPawnTraversalModule Traversal => _runtime?.Traversal;
    private Pawn3DPresentationComponent Presentation => _runtime?.Presentation;
    private IActorCombatRequestReceiver CombatRequests => _runtime?.CombatRequests;
    private IActorHealthState Health => _runtime?.Health;
    private IActorDamageImmunityController DamageImmunity => _runtime?.DamageImmunity;
    private IActorTraversalFeature TraversalFeature => _runtime?.TraversalFeature;
    private IActorInteractionRequestReceiver InteractionRequests => _runtime?.InteractionRequests;
    private IActorGuardController GuardFeature => _runtime?.GuardFeature;
    protected override GameplayTickDomain TickDomain => GameplayTickDomain.Character;
    protected override bool UsesGameplayTick => true;

    //  Combat-facing movement state  //
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
    public bool  IsBlocking           => GuardFeature?.IsGuarding ?? false;
    public float BlockDamageReduction => GuardFeature?.BlockDamageReduction ?? 0f;
    public float BlockFrontalAngle    => GuardFeature?.BlockFrontalAngle ?? 90f;
    public bool     IsCrouching     => Movement.State.IsCrouching;
    public bool     IsSprinting     => Movement.State.IsSprinting;
    public Vector3  CurrentVelocity => new Vector3(Movement.State.VelocityX, Movement.State.VelocityY, Movement.State.VelocityZ);
    public Vector3 MotionVelocity => CurrentVelocity;

    //  Unity lifecycle  //
    private void Awake()
    {
        _runtime = Motor3DRuntimeReferences.Capture(gameObject);

        if (Health != null && Movement != null)
            Health.Damaged += HandleDamaged;
    }

    private void OnDestroy()
    {
        if (Health != null)
            Health.Damaged -= HandleDamaged;
    }

    private void HandleDamaged(float amount) => Movement?.TriggerKnockBack();

    private void OnControllerColliderHit(ControllerColliderHit hit) =>
        Movement.NotifyColliderHit(hit);

    //  Public API  //
    /// <summary>Trigger the knocked-back hit-reaction. Wire to IActorHealthState.Damaged or call from combat code.</summary>
    public void TriggerKnockedBack() => Movement.TriggerKnockBack();

    /// <summary>Play the ClimbUp animation. Call from an external ladder or climbable script.</summary>
    public void TriggerClimbUp() => Traversal.TriggerClimbUp();

    /// <summary>Swap the raw InputActionAsset. Delegates to the input module.</summary>
    public void SetInputActions(InputActionAsset asset, bool overrideExisting = true) =>
        Input.SetInputActions(asset, overrideExisting);

    private void ResolveDirectCapabilities()
    {
        _runtime?.ResolveDirectCapabilities();
    }
}
}
