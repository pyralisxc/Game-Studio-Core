using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Participants;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Combat
{
    [AuthoringContract(
        Category = "Combat",
        CapabilityPath = "Combat/Actions/Pawn Combat Behaviour2D",
        Surface = AuthoringSurface.Goal,
        Summary = "2D pawn combat; receives attack input, resolves combos, activates HitBox2D, and fires projectiles.",
        RequiredFields = new[] { nameof(hitBoxZones), nameof(equippedWeapons), nameof(startingWeaponIndex), nameof(attackCooldown), nameof(kickCooldown), nameof(primarySequence), nameof(secondarySequence), nameof(projectileLauncher) },
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Modules.Character.Motor2D" },
        SetupSteps = new[] 
        { 
            "Attach to the same root as Motor2D.",
            "Assign HitBox2D zones for melee attacks.",
            "Assign CombatSequenceDefinition for authored combos.",
            "Assign Projectile Launcher for ranged attacks."
        },
        SuccessChecks = new[] { "Verify attacks trigger animations and hitboxes detect targets." },
        Tags = new[] { "capability:Combat", "axiom:Dimensions2D" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Modules/Combat/Pawn/Sprite2D/Pawn Combat Behaviour 2D")]
    public partial class PawnCombatBehaviour2D : GameplayTickBehaviour, IPawnCombatModule, IActorCombatRequestReceiver, ICombatActionStateReader, IActorCombatModifierReceiver, IRuntimeValidationProvider
    {
        [Header("Combo")]
        [SerializeField] private float comboResetTime = 1.5f;
        [SerializeField] private float combatWindow = 3f;

        [Header("Combat")]
        [SerializeField] private HitBoxSlot2D[] hitBoxZones;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private ProjectileLauncher2D projectileLauncher;

        [Header("Weapons")]
        [SerializeField] private WeaponData attackWeapon;
        [SerializeField] private WeaponData kickWeapon;
        [SerializeField] private WeaponData[] equippedWeapons;
        [SerializeField] private int startingWeaponIndex;
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float baseKnockback = 5f;
        [SerializeField] private float hitDelay = 0.1f;
        [SerializeField] private float hitDuration = 0.15f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private float kickCooldown = 0.8f;

        [Header("Combat Definitions")]
        [SerializeField] private CombatSequenceDefinition primarySequence;
        [SerializeField] private CombatSequenceDefinition secondarySequence;

        private PawnCombat2DRuntimeReferences _runtime;
        private PawnComboProcessor _comboProcessor;
        private readonly CombatActionStateMachine _actionStateMachine = new CombatActionStateMachine();

        private int _activeWeaponIndex;
        private float _attackTimer;
        private float _kickTimer;
        private float _combatTimer;
        private float _actingTimer;
        private float _outgoingDamageMultiplier = 1f;
        private float _outgoingKnockbackMultiplier = 1f;

        private PawnCombat2DRuntimeReferences Runtime =>
            _runtime ??= PawnCombat2DRuntimeReferences.Resolve(gameObject, projectileLauncher);

        public CombatActionState ActionState => _actionStateMachine.CurrentState;
        protected override GameplayTickDomain TickDomain => GameplayTickDomain.Combat;
        protected override bool UsesGameplayTick => true;

        private void Awake()
        {
            _runtime = PawnCombat2DRuntimeReferences.Resolve(gameObject, projectileLauncher);
            _comboProcessor = new PawnComboProcessor();

            CacheHitBoxOffsets();
            SubscribeHitBoxes();

            if (equippedWeapons != null && equippedWeapons.Length > 0)
            {
                _activeWeaponIndex = Mathf.Clamp(startingWeaponIndex, 0, equippedWeapons.Length - 1);
                attackWeapon = equippedWeapons[_activeWeaponIndex];
            }

            ApplyActiveWeapon();
        }

        private void PublishCombatResult(in ActorCombatResult result)
        {
            IActorCombatResultReceiver[] receivers = Runtime.CombatResultReceivers;
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
            _attackTimer -= context.DeltaTime;
            _kickTimer -= context.DeltaTime;
            _combatTimer -= context.DeltaTime;
            _actingTimer -= context.DeltaTime;

            _comboProcessor.Tick(context.DeltaTime, comboResetTime);

            if (_actingTimer <= 0f && Runtime.Motor != null)
                Runtime.Motor.IsActing = false;

            UpdateActionState();
        }

        private void HandlePrimaryAttackInput()
        {
            if (primarySequence != null && primarySequence.actions != null && primarySequence.actions.Length > 0)
            {
                ExecuteSequenceAction(_comboProcessor.PrimaryState, primarySequence, CombatInputType.Primary, attackWeapon, ref _attackTimer, attackCooldown);
            }
        }

        private void HandleSecondaryAttackInput()
        {
            if (secondarySequence != null && secondarySequence.actions != null && secondarySequence.actions.Length > 0)
            {
                ExecuteSequenceAction(_comboProcessor.SecondaryState, secondarySequence, CombatInputType.Secondary, kickWeapon, ref _kickTimer, kickCooldown);
            }
        }

        public bool TryHandleCombatCommand(in ActorCombatCommand command)
        {
            switch (command.Kind)
            {
                case ActorCombatCommandKind.PrimaryAttack:
                    HandlePrimaryAttackInput();
                    return true;
                case ActorCombatCommandKind.SecondaryAttack:
                    HandleSecondaryAttackInput();
                    return true;
                default:
                    return false;
            }
        }

        public void SetOutgoingDamageMultiplier(float multiplier)
        {
            _outgoingDamageMultiplier = Mathf.Max(multiplier, 0f);
        }

        public void SetOutgoingKnockbackMultiplier(float multiplier)
        {
            _outgoingKnockbackMultiplier = Mathf.Max(multiplier, 0f);
        }

        private WeaponData ActiveWeapon =>
            equippedWeapons != null && equippedWeapons.Length > _activeWeaponIndex
                ? equippedWeapons[_activeWeaponIndex]
                : null;

        private void ApplyActiveWeapon()
        {
            WeaponData weapon = ActiveWeapon;
            Runtime.AnimationDriver?.SetRuntimeControllerOverride(weapon != null ? weapon.overrideController : null);
        }

        private void UpdateActionState()
        {
            _actionStateMachine.ProjectFrom(
                Runtime.Motor != null && Runtime.Motor.IsActing,
                Mathf.Max(_combatTimer, _actingTimer),
                Mathf.Max(_attackTimer, _kickTimer));
        }
    }
}
