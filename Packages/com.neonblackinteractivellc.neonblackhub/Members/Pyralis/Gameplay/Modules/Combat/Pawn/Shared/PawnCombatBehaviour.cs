using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Core.Contracts;
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
        RequiredFields = new[] { nameof(primarySequence), nameof(secondarySequence), nameof(aerialSequence), nameof(attackCooldown), nameof(kickCooldown), nameof(maxAerialAttacks) },
        RequiredInterfaces = new[] { typeof(IPawnCombatModule) },
        SuccessChecks = new[] { "Perform an attack combo in Play Mode and verify that 'HitBox.Fire()' is called via animation events and damage is applied." },
        Tags = new[] { "capability:MeleeFlow", "axiom:Realtime", "lane:Combat", "priority:Primary" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Modules/Combat/Pawn/Pawn Combat Behaviour")]
    [RequireComponent(typeof(PawnHitBoxModule))]
    [RequireComponent(typeof(PawnDamageModule))]
    [RequireComponent(typeof(PawnProjectileModule))]
    [RequireComponent(typeof(PawnBlockModule))]
    [RequireComponent(typeof(PawnWeaponModule))]
    public partial class PawnCombatBehaviour : GameplayTickBehaviour, IPawnCombatModule, IActorCombatMovementInfluence, IActorCombatRequestReceiver, IActorGuardController, ICombatActionStateReader, IDamageModifier, IActorCombatModifierReceiver, IRuntimeValidationProvider
    {
        private PawnCombatRuntimeReferences _runtime;
        private PawnComboProcessor _comboProcessor;
        private readonly CombatActionStateMachine _actionStateMachine = new CombatActionStateMachine();

        private float _attackTimer;
        private float _kickTimer;
        private int _aerialAttackCount;
        private float _aerialTimer;
        private float _combatTimer;

        private IActorCombatMovementState Motor => _runtime?.Motor;
        private IActorFeedbackPublisher FeedbackPublisher => _runtime?.FeedbackPublisher;
        private PawnHitBoxModule HitBoxModule => _runtime?.HitBoxModule;
        private PawnDamageModule DamageModule => _runtime?.DamageModule;
        private PawnProjectileModule ProjectileModule => _runtime?.ProjectileModule;
        private PawnBlockModule BlockModule => _runtime?.BlockModule;
        private PawnWeaponModule WeaponModule => _runtime?.WeaponModule;

        public bool IsBlocking => BlockModule != null && BlockModule.IsBlocking;
        public bool IsGuarding => IsBlocking;
        public float BlockDamageReduction => BlockModule != null ? BlockModule.BlockDamageReduction : 0.2f;
        public float BlockFrontalAngle => BlockModule != null ? BlockModule.BlockFrontalAngle : 90f;
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
            BlockModule?.Tick();
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
                ExecuteSequenceAction(_comboProcessor.PrimaryState, primarySequence, CombatInputType.Primary, WeaponModule.AttackWeapon, ref _attackTimer, attackCooldown);
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
                ExecuteSequenceAction(_comboProcessor.SecondaryState, secondarySequence, CombatInputType.Secondary, WeaponModule.KickWeapon, ref _kickTimer, kickCooldown);
            }
        }

        public void HandleBlockStart() => BlockModule?.HandleBlockStart();
        public void HandleBlockEnd() => BlockModule?.HandleBlockEnd();
        public void BeginGuard() => HandleBlockStart();
        public void EndGuard() => HandleBlockEnd();
        public void CycleWeapon(int direction) => WeaponModule?.CycleWeapon(direction);

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
            if (BlockModule == null || DamageModule == null) return false;
            return DamageModule.TryModifyIncomingDamage(
                source, 
                ref incomingDamage, 
                BlockModule.IsBlocking,
                BlockModule.BlockDamageReduction,
                BlockModule.BlockFrontalAngle,
                Motor?.FacingRight ?? true);
        }

        public void SetOutgoingDamageMultiplier(float multiplier) => DamageModule?.SetOutgoingDamageMultiplier(multiplier);
        public void SetOutgoingKnockbackMultiplier(float multiplier) => DamageModule?.SetOutgoingKnockbackMultiplier(multiplier);

        private void UpdateActionState()
        {
            _actionStateMachine.ProjectFrom(
                Motor != null && Motor.IsActing,
                _combatTimer,
                Mathf.Max(_attackTimer, _kickTimer, _aerialTimer));
        }

    }
}
