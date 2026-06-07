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
///
/// Each module owns its domain completely â€” this class contains no gameplay logic.
///
/// Animator parameters required on any Animator in the hierarchy:
///   Booleans : IsMoving, IsSprinting, IsGrounded, IsCrouching, IsInAir, InCombat
///              IsHanging*, IsWallSliding*, IsSliding*, LookAround*
///   Triggers : Jump, DiveRoll, Slide, ClimbUp, MoveToIdle*, SideClimb, FwdClimb,
///              LedgeDrop*, KnockedBack, Interact
///   Floats   : ShimmySpeed*   (* = optional; detected at runtime via parameter scan)
///
/// Setup:
///   1. Add to a character root that also has CharacterController, HealthComponent,
///      KnockbackReceiver, and the four Pawn3D* module components.
///   2. Assign the InputSystem_Actions asset on Pawn3DInputModule.
///   3. Wire any PawnCombatBehaviour hit box zones and weapon data independently.
/// </summary>
[AddComponentMenu("NeonBlack/Gameplay/Runtime 3D/Motor 3D")]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(KnockbackReceiver))]
[RequireComponent(typeof(Pawn3DInputModule))]
[RequireComponent(typeof(Pawn3DMovementComponent))]
[RequireComponent(typeof(Pawn3DTraversalComponent))]
[RequireComponent(typeof(Pawn3DPresentationComponent))]
public class Motor3D : MonoBehaviour, ICharacterMotorState, IActorReactionResponder, IActorMovementModifierReceiver, IClimbTraversalActor
{
    // â”€â”€ Module references â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    private Pawn3DInputModule         _input;
    private Pawn3DMovementComponent   _movement;
    private Pawn3DTraversalComponent  _traversal;
    private Pawn3DPresentationComponent _presentation;
    private PawnCombatBehaviour       _combat;
    private HealthComponent           _health;
    private ActorFeatureHost          _featureHost;
    private IActorTraversalFeature    _traversalFeature;
    private IActorInteractionFeature  _interactionFeature;
    private IActorGuardFeature        _guardFeature;
    private float                     _reactionLockTimer;
    private bool                      _statusActionLocked;

    // â”€â”€ ICharacterMotorState â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    public bool IsGrounded  => _movement.State.IsGrounded;
    public bool IsAirborne  => !_movement.State.IsGrounded || _movement.State.VelocityY > 0f;
    public bool FacingRight => _movement.State.FacingRight;
    public bool IsActing
    {
        get => _movement.State.IsActing;
        set => _movement.SetActing(value);
    }

    public void ResetMoveToIdle() => _presentation.ResetMoveToIdle();

    // â”€â”€ Public accessors (for camera, UI, and other systems) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    public bool  IsBlocking           => _guardFeature?.IsGuarding ?? (_combat?.IsBlocking ?? false);
    public float BlockDamageReduction => _guardFeature?.BlockDamageReduction ?? (_combat?.BlockDamageReduction ?? 0f);
    public float BlockFrontalAngle    => _guardFeature?.BlockFrontalAngle ?? (_combat?.BlockFrontalAngle ?? 90f);
    public bool     IsCrouching     => _movement.State.IsCrouching;
    public bool     IsSprinting     => _movement.State.IsSprinting;
    public Vector3  CurrentVelocity => new Vector3(_movement.State.VelocityX, _movement.State.VelocityY, 0f);

    // â”€â”€ Unity lifecycle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    private void Awake()
    {
        _input        = GetComponent<Pawn3DInputModule>();
        _movement     = GetComponent<Pawn3DMovementComponent>();
        _traversal    = GetComponent<Pawn3DTraversalComponent>();
        _presentation = GetComponent<Pawn3DPresentationComponent>();
        _combat       = GetComponent<PawnCombatBehaviour>();
        _health       = GetComponent<HealthComponent>();
        _featureHost  = GetComponent<ActorFeatureHost>();

        if (_health != null)
            _health.OnDamaged.AddListener(_ => _movement.TriggerKnockBack());
    }

    // â”€â”€ Update â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    private void Update()
    {
        ResolveFeatureModules();

        if (_reactionLockTimer > 0f || _statusActionLocked)
        {
            if (_reactionLockTimer > 0f)
                _reactionLockTimer = Mathf.Max(0f, _reactionLockTimer - Time.deltaTime);
            _combat?.UpdateCombatTimers();
            _movement.ResetPhysicsFrame();
            _movement.ApplyMovement(Vector3.zero);
            _presentation.Apply(_traversalFeature != null ? _traversalFeature.ShimmyVelocityX : _traversal.ShimmyVelocityX);
            return;
        }

        // 1. Collect all input for this frame into a single snapshot.
        FrameInput fi = _input.CollectFrameInput();

        // 2. Advance combat timers (affects movement multipliers this frame).
        _combat?.UpdateCombatTimers();

        // 3. Resolve look-around mouse position and LookAround animator toggle.
        _presentation.UpdateLookAround(fi);

        // 4. Handle crouch and power-slide input.
        if (fi.CrouchPressed)
        {
            if (!_movement.TryStartPowerSlide())
                _movement.SetCrouch(true);
        }
        if (fi.CrouchReleased) _movement.SetCrouch(false);

        // 5. Dispatch combat input.
        if (fi.AttackPressed)      _combat?.HandleAttack();
        if (fi.KickPressed)        _combat?.HandleKick();
        if (fi.BlockPressed)
        {
            if (_guardFeature != null) _guardFeature.BeginGuard();
            else _combat?.HandleBlockStart();
        }
        if (fi.BlockReleased)
        {
            if (_guardFeature != null) _guardFeature.EndGuard();
            else _combat?.HandleBlockEnd();
        }
        if (fi.WeaponCycleDelta != 0) _combat?.CycleWeapon(fi.WeaponCycleDelta);

        // 6. Handle dodge roll.
        if (fi.RollPressed && _movement.TryStartDodge(fi.Move))
            _health?.ForceIFrames(_movement.DodgeDuration);

        // 7. Reset physics frame before ticking the model.
        _movement.ResetPhysicsFrame();

        // 8. Tick the movement model and get this frame's world-space velocity.
        Vector3 velocity = _movement.Tick(fi);

        // 9. While hanging, the traversal module drives movement directly â€” skip normal path.
        if ((_traversalFeature != null && _traversalFeature.HandleHangFrame(fi))
            || (_traversalFeature == null && _traversal.HandleHangFrame(fi)))
        {
            _presentation.Apply(_traversalFeature != null ? _traversalFeature.ShimmyVelocityX : _traversal.ShimmyVelocityX);
            return;
        }

        // 10. Probe for ledges and handle interact.
        if (_traversalFeature != null) _traversalFeature.ProbeTraversal();
        else _traversal.ProbeLedge();
        if (fi.InteractPressed)
        {
            if (_interactionFeature != null) _interactionFeature.TryHandleInteraction();
            else _traversal.HandleInteract();
        }

        // 11. Apply velocity to the CharacterController.
        _movement.ApplyMovement(velocity);

        // 12. Update animator, billboard, and visual feedback.
        _presentation.Apply(_traversalFeature != null ? _traversalFeature.ShimmyVelocityX : _traversal.ShimmyVelocityX);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) =>
        _movement.NotifyColliderHit(hit);

    // â”€â”€ Public API â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    /// <summary>Trigger the knocked-back hit-reaction. Wire to HealthComponent.OnDamaged or call from combat code.</summary>
    public void TriggerKnockedBack() => _movement.TriggerKnockBack();

    /// <summary>Play the ClimbUp animation. Call from an external ladder or climbable script.</summary>
    public void TriggerClimbUp() => _traversal.TriggerClimbUp();

    /// <summary>Swap the InputConfig (per-participant overrides). Delegates to the input module.</summary>
    public void SetInputConfig(InputConfig config, bool overrideExisting = true) =>
        _input.SetInputConfig(config, overrideExisting);

    /// <summary>Swap the raw InputActionAsset. Delegates to the input module.</summary>
    public void SetInputActions(InputActionAsset asset, bool overrideExisting = true) =>
        _input.SetInputActions(asset, overrideExisting);

    public void ApplyReactionLock(float duration)
    {
        _reactionLockTimer = Mathf.Max(_reactionLockTimer, duration);
        _presentation.ResetMoveToIdle();
    }

    public void ClearReactionLock()
    {
        _reactionLockTimer = 0f;
    }

    public void SetStatusMoveSpeedMultiplier(float multiplier)
    {
        _movement?.SetExternalSpeedMultiplier(multiplier);
    }

    public void SetStatusActionLock(bool locked)
    {
        _statusActionLocked = locked;
        if (locked)
            _presentation?.ResetMoveToIdle();
    }

    // â”€â”€ Traversal forwarding (for external ClimbZone triggers) â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    public void TryLedgeGrab(IClimbZone zone, float maxVelocityY = 0f)
    {
        if (_traversalFeature != null) _traversalFeature.TryLedgeGrab(zone, maxVelocityY);
        else _traversal.TryLedgeGrab(zone, maxVelocityY);
    }
    public void SetClimbZone(IClimbZone zone)
    {
        if (_traversalFeature != null) _traversalFeature.SetClimbZone(zone);
        else _traversal.SetClimbZone(zone);
    }
    public void ClearClimbZone()
    {
        if (_traversalFeature != null) _traversalFeature.ClearClimbZone();
        else _traversal.ClearClimbZone();
    }

    private void ResolveFeatureModules()
    {
        if (_featureHost == null)
            _featureHost = GetComponent<ActorFeatureHost>();

        if (_featureHost == null)
            return;

        _traversalFeature ??= _featureHost.TryGetInstalledFeature(out IActorTraversalFeature traversalFeature)
            ? traversalFeature
            : null;
        _interactionFeature ??= _featureHost.TryGetInstalledFeature(out IActorInteractionFeature interactionFeature)
            ? interactionFeature
            : null;
        _guardFeature ??= _featureHost.TryGetInstalledFeature(out IActorGuardFeature guardFeature)
            ? guardFeature
            : null;
    }
}
}
