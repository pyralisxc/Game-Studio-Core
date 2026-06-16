using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Characters;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Features.Characters
{
    [AuthoringContract(
        Capability = AuthoringCapability.MeleeFlow,
        Priority = AuthoringPriority.Primary,
        Lane = "Combat",
        Relevance = "Primary pawn combat controller; handles sequences, combos, and delegates to modules.",
        Axioms = AuthoringWorldAxiom.Realtime,
        RequiredInterfaces = new[] { typeof(IPawnCombatModule) },
        ConsumedRoles = new[] { "AttackPrimary", "AttackSecondary", "Block" },
        AssignmentFields = new[] { nameof(primarySequence), nameof(secondarySequence), nameof(aerialSequence), nameof(attackCooldown), nameof(kickCooldown), nameof(maxAerialAttacks) },
        FirstProof = "Perform an attack combo in Play Mode and verify that 'HitBox.Fire()' is called via animation events and damage is applied.",
        ExpertAdvice = "PawnCombatBehaviour is sequence-driven. If attacks feel floaty or don't land, check that your Animation Sequence assets have the 'FireHitBox' event timed precisely with the swing frame.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat"
    )]
[AddComponentMenu("NeonBlack/Gameplay/Characters/Pawn Combat Behaviour")]
    [RequireComponent(typeof(PawnHitBoxModule))]
    [RequireComponent(typeof(PawnDamageModule))]
    [RequireComponent(typeof(PawnProjectileModule))]
    [RequireComponent(typeof(PawnBlockModule))]
    [RequireComponent(typeof(PawnWeaponModule))]
    public partial class PawnCombatBehaviour : MonoBehaviour, IPawnCombatModule, IPawnCombatMovementContext, IDamageModifier, IActorCombatModifierReceiver, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (attackCooldown < 0f) yield return "Attack Cooldown cannot be negative.";
            if (maxAerialAttacks < 0) yield return "Max Aerial Attacks cannot be negative.";
        }
        [Header("Combo Settings")]
        [SerializeField] private float comboResetTime = 1.5f;
        [SerializeField] private float combatWindow = 3f;
        [SerializeField] private int maxAerialAttacks = 2;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private float kickCooldown = 0.8f;

        [Header("Movement Modifiers")]
        [Range(0f, 1f)]
        [SerializeField] private float attackMoveMultiplier = 0.2f;
        [Range(0f, 1f)]
        [SerializeField] private float aerialAttackMoveMultiplier = 0.5f;

        [Header("Combat Definitions")]
        [SerializeField] private CombatSequenceDefinition primarySequence;
        [SerializeField] private CombatSequenceDefinition secondarySequence;
        [SerializeField] private CombatSequenceDefinition aerialSequence;
        [SerializeField] private string aerialHitBoxZone = "Aerial";

        private PawnCombatRuntimeReferences _runtime;
        private PawnComboProcessor _comboProcessor;

        private int _attackCount;
        private int _kickCount;
        private float _attackTimer;
        private float _kickTimer;
        private int _aerialAttackCount;
        private float _aerialTimer;
        private float _combatTimer;

        private ICharacterMotorState Motor => _runtime?.Motor;
        private ActorAnimationDriver AnimationDriver => _runtime?.AnimationDriver;
        private IActorFeedbackPublisher FeedbackPublisher => _runtime?.FeedbackPublisher;
        private PawnHitBoxModule HitBoxModule => _runtime?.HitBoxModule;
        private PawnDamageModule DamageModule => _runtime?.DamageModule;
        private PawnProjectileModule ProjectileModule => _runtime?.ProjectileModule;
        private PawnBlockModule BlockModule => _runtime?.BlockModule;
        private PawnWeaponModule WeaponModule => _runtime?.WeaponModule;

        public bool IsBlocking => BlockModule != null && BlockModule.IsBlocking;
        public float BlockDamageReduction => BlockModule != null ? BlockModule.BlockDamageReduction : 0.2f;
        public float BlockFrontalAngle => BlockModule != null ? BlockModule.BlockFrontalAngle : 90f;
        public float AttackTimer => _attackTimer;
        public float KickTimer => _kickTimer;
        public float AttackMoveMultiplier => attackMoveMultiplier;
        public float AerialAttackMoveMultiplier => aerialAttackMoveMultiplier;
        public float CombatTimer => _combatTimer;

        [Inject]
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

        private void OnDestroy()
        {
            UnsubscribeHitBoxes();
        }

        public void UpdateCombatTimers()
        {
            float dt = Time.deltaTime;
            _attackTimer -= dt;
            _kickTimer -= dt;
            _aerialTimer -= dt;
            _combatTimer -= dt;

            _comboProcessor.Tick(dt, comboResetTime);
            HitBoxModule?.Tick(dt);
            BlockModule?.Tick();
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
                ExecuteSequenceAction(_comboProcessor.PrimaryState, primarySequence, CombatInputType.Primary, WeaponModule.AttackWeapon, "Punch", ref _attackTimer, attackCooldown);
                return;
            }

            ExecuteFallbackAttack();
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
                ExecuteSequenceAction(_comboProcessor.SecondaryState, secondarySequence, CombatInputType.Secondary, WeaponModule.KickWeapon, "Kick", ref _kickTimer, kickCooldown);
                return;
            }

            ExecuteFallbackKick();
        }

        public void HandleBlockStart() => BlockModule?.HandleBlockStart();
        public void HandleBlockEnd() => BlockModule?.HandleBlockEnd();
        public void CycleWeapon(int direction) => WeaponModule?.CycleWeapon(direction);

        public void ResetAttackCombo()
        {
            _attackCount = 0;
            _comboProcessor.ResetPrimary();
        }

        public void ResetKickCombo()
        {
            _kickCount = 0;
            _comboProcessor.ResetSecondary();
        }

        public void ApplyCombatProfile(PawnProfileApplicationContext context, PawnCombatProfile profile)
        {
            if (profile == null) return;

            attackCooldown = profile.attackCooldown;
            kickCooldown = profile.kickCooldown;
            comboResetTime = profile.comboResetTime;
            combatWindow = profile.combatWindow;
            primarySequence = profile.primarySequence;
            secondarySequence = profile.secondarySequence;
            aerialSequence = profile.aerialSequence;
            maxAerialAttacks = profile.maxAerialAttacks;

            WeaponModule?.SetWeapons(profile.attackWeapon, profile.kickWeapon, profile.aerialWeapon);
            // Damage scaling usually comes from the module
            DamageModule?.SetOutgoingDamageMultiplier(1.0f); // Default or from profile if added
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

    }
}
