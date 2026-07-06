using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Data.Participants;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Combat
{
    [AuthoringContract(
        Category = "Melee Flow",
        CapabilityPath = "Combat/Actions/Pawn Combat Behaviour",
        Surface = AuthoringSurface.Goal,
        Summary = "Primary pawn combat controller; handles sequences, combos, and delegates to modules.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/combat",
        RequiredFields = new[] { nameof(primarySequence), nameof(secondarySequence), nameof(aerialSequence) },
        RequiredInterfaces = new[] { typeof(IPawnCombatModule) },
        SuccessChecks = new[] { "Perform an attack combo in Play Mode and verify that 'HitBox.Fire()' is called via animation events and damage is applied." },
        Tags = new[] { "capability:MeleeFlow", "axiom:Realtime", "lane:Combat", "priority:Primary" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Modules/Combat/Pawn/Pawn Combat Behaviour")]
    [RequireComponent(typeof(PawnHitBoxModule))]
    public partial class PawnCombatBehaviour : GameplayTickBehaviour, IPawnCombatModule, IActorCombatMovementInfluence, IActorCombatRequestReceiver, IActorGuardController, ICombatActionStateReader, IDamageModifier, IActorCombatModifierReceiver, IRuntimeValidationProvider
    {
        private PawnCombatRuntimeReferences _runtime;
        private PawnComboProcessor _comboProcessor;
        private readonly PawnDamageHandler _damageHandler = new PawnDamageHandler();
        private readonly CombatActionStateMachine _actionStateMachine = new CombatActionStateMachine();

        private float _attackTimer;
        private float _kickTimer;
        private int _aerialAttackCount;
        private float _aerialTimer;
        private float _combatTimer;
        private int _activeWeaponIndex;
        private bool _isBlocking;
        private IActorAnimationController _animationDriver;
        private HealthComponent _health;

        private IActorCombatMovementState Motor => _runtime?.Motor;
        private IActorFeedbackPublisher FeedbackPublisher => _runtime?.FeedbackPublisher;
        private PawnHitBoxModule HitBoxModule => _runtime?.HitBoxModule;

        public bool IsBlocking => _isBlocking;
        public bool IsGuarding => IsBlocking;
        public float BlockDamageReduction => blockDamageReduction;
        public float BlockFrontalAngle => blockFrontalAngle;
        public float AttackTimer => _attackTimer;
        public float KickTimer => _kickTimer;
        public float AttackMoveMultiplier => attackMoveMultiplier;
        public float AerialAttackMoveMultiplier => aerialAttackMoveMultiplier;
        public float CombatTimer => _combatTimer;
        public CombatActionState ActionState => _actionStateMachine.CurrentState;
        protected override GameplayTickDomain TickDomain => GameplayTickDomain.Combat;
        protected override bool UsesGameplayTick => true;

        public void Construct(PawnComboProcessor comboProcessor)
        {
            _comboProcessor = comboProcessor;
        }

        private void Awake()
        {
            _runtime = PawnCombatRuntimeReferences.Capture(this);
            _comboProcessor ??= new PawnComboProcessor();
            _animationDriver = GetComponent<IActorAnimationController>();
            _health = GetComponent<HealthComponent>();

            if (equippedWeapons != null && equippedWeapons.Length > 0)
            {
                _activeWeaponIndex = Mathf.Clamp(startingWeaponIndex, 0, equippedWeapons.Length - 1);
            }

            ApplyActiveWeapon();

            SubscribeHitBoxes();
        }

        private void PublishCombatResult(in ActorCombatResult result)
        {
            IActorCombatResultReceiver[] receivers = _runtime?.CombatResultReceivers;
            if (receivers == null)
                return;

            for (int i = 0; i < receivers.Length; i++)
                receivers[i]?.HandleCombatResult(result);
        }

        private void OnDestroy()
        {
            UnsubscribeHitBoxes();
        }

        protected override void OnGameplayTick(in GameplayTickContext context)
        {
            TickCombatTimers(context.DeltaTime);
        }

        private void TickCombatTimers(float dt)
        {
            _attackTimer -= dt;
            _kickTimer -= dt;
            _aerialTimer -= dt;
            _combatTimer -= dt;

            _comboProcessor.Tick(dt, comboResetTime);
            HitBoxModule?.Tick(dt);
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.BlockLoop, _isBlocking);
            UpdateActionState();
        }

        public void SyncHitBoxSides(Transform root, bool facingRight)
        {
            HitBoxModule?.SyncHitBoxSides(facingRight);
        }

        public void ResetAerialAttackCount() => _aerialAttackCount = 0;

        public void HandleAttack()
        {
            if (Motor == null) return;

            if (Motor.IsAirborne)
            {
                PerformAerialAttack();
                return;
            }

            if (primarySequence != null && primarySequence.actions != null && primarySequence.actions.Length > 0)
            {
                ExecuteSequenceAction(_comboProcessor.PrimaryState, primarySequence, CombatInputType.Primary, attackWeapon, ref _attackTimer, attackCooldown);
            }
        }

        public void HandleKick()
        {
            if (Motor == null) return;

            if (Motor.IsAirborne)
            {
                PerformAerialAttack();
                return;
            }

            if (secondarySequence != null && secondarySequence.actions != null && secondarySequence.actions.Length > 0)
            {
                ExecuteSequenceAction(_comboProcessor.SecondaryState, secondarySequence, CombatInputType.Secondary, kickWeapon, ref _kickTimer, kickCooldown);
            }
        }

        public void HandleBlockStart()
        {
            if (Motor != null && Motor.IsActing)
                return;

            _isBlocking = true;
            _animationDriver?.TriggerSignal(ActorAnimationSignal.BlockStart);
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.BlockLoop, true);
        }

        public void HandleBlockEnd()
        {
            _isBlocking = false;
            _animationDriver?.TriggerSignal(ActorAnimationSignal.BlockEnd);
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.BlockLoop, false);
        }

        public void BeginGuard() => HandleBlockStart();
        public void EndGuard() => HandleBlockEnd();
        public void CycleWeapon(int direction)
        {
            if (equippedWeapons == null || equippedWeapons.Length <= 1)
                return;

            _activeWeaponIndex = (_activeWeaponIndex + direction + equippedWeapons.Length) % equippedWeapons.Length;
            ApplyActiveWeapon();
        }

        public bool TryHandleCombatCommand(in ActorCombatCommand command)
        {
            switch (command.Kind)
            {
                case ActorCombatCommandKind.PrimaryAttack:
                    HandleAttack();
                    return true;
                case ActorCombatCommandKind.SecondaryAttack:
                    HandleKick();
                    return true;
                case ActorCombatCommandKind.BlockStart:
                    HandleBlockStart();
                    return true;
                case ActorCombatCommandKind.BlockEnd:
                    HandleBlockEnd();
                    return true;
                case ActorCombatCommandKind.CycleWeapon:
                    CycleWeapon(command.Direction);
                    return true;
                default:
                    return false;
            }
        }

        public void ResetAttackCombo()
        {
            _comboProcessor.ResetPrimary();
        }

        public void ResetKickCombo()
        {
            _comboProcessor.ResetSecondary();
        }

        public bool TryModifyIncomingDamage(GameObject source, ref float incomingDamage)
        {
            return _damageHandler.TryModifyIncomingDamage(
                gameObject,
                source,
                ref incomingDamage,
                _isBlocking,
                blockDamageReduction,
                blockFrontalAngle,
                Motor?.FacingRight ?? true);
        }

        public void SetOutgoingDamageMultiplier(float multiplier) => _damageHandler.SetOutgoingDamageMultiplier(multiplier);
        public void SetOutgoingKnockbackMultiplier(float multiplier) => _damageHandler.SetOutgoingKnockbackMultiplier(multiplier);

        private WeaponData ActiveWeapon => (equippedWeapons != null && equippedWeapons.Length > _activeWeaponIndex) ? equippedWeapons[_activeWeaponIndex] : null;

        private void ApplyActiveWeapon()
        {
            WeaponData weapon = ActiveWeapon;
            _animationDriver?.SetRuntimeControllerOverride(weapon != null ? weapon.overrideController : null);
        }

        private void SetWeapons(WeaponData attack, WeaponData kick, WeaponData aerial)
        {
            attackWeapon = attack;
            kickWeapon = kick;
            aerialWeapon = aerial;
            ApplyActiveWeapon();
        }

        private void FireProjectile(WeaponData weapon, bool facingRight, float damageMultiplier, float knockbackMultiplier)
        {
            ProjectileLauncher3D launcher = ResolveProjectileLauncher();
            if (launcher == null)
            {
                Debug.LogWarning($"{nameof(PawnCombatBehaviour)} needs a {nameof(ProjectileLauncher3D)} to fire ranged weapon `{weapon.weaponName}`.", this);
                return;
            }

            if (weapon.projectileDefinition == null)
                return;

            Vector3 spawnPos = projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.position + Vector3.up * 1f + transform.forward * 0.5f;
            Vector3 forward = facingRight ? Vector3.right : Vector3.left;
            ProjectileFireRequest request = new ProjectileFireRequest(
                weapon.projectileDefinition,
                weapon.fireModeDefinition,
                spawnPos,
                forward,
                gameObject,
                _health != null ? _health.faction : Faction.Neutral,
                damageMultiplier: damageMultiplier,
                knockbackMultiplier: knockbackMultiplier);

            launcher.Fire(request);
        }

        private ProjectileLauncher3D ResolveProjectileLauncher()
        {
            if (projectileLauncher != null)
                return projectileLauncher;

            projectileLauncher = GetComponentInParent<ProjectileLauncher3D>();
            if (projectileLauncher == null)
                projectileLauncher = GetComponentInChildren<ProjectileLauncher3D>();

            return projectileLauncher;
        }

        private void UpdateActionState()
        {
            _actionStateMachine.ProjectFrom(
                Motor != null && Motor.IsActing,
                _combatTimer,
                Mathf.Max(_attackTimer, _kickTimer, _aerialTimer));
        }

    }
}
