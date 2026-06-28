using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Combat
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        Relevance = "Manages enemy combat AI, attack sequencing, and hitboxes.",
        AssignmentFields = new[] { nameof(combatProfile), nameof(hitBoxZones), nameof(attackSequence), nameof(attackMode) },
        Proof = "Verify enemy attacks when player is in range.",
        ExpertAdvice = "Use attackRangeOverride if the calculated hitbox range is inaccurate. Sequential mode is best for simple bosses.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/enemies",
        CapabilityPath = "Combat/Actions/Enemy Combat Module"
    )]
    public partial class EnemyCombatModule : MonoBehaviour, IActorCombatRequestReceiver, IActorCombatTacticalState, IActorCombatModifierReceiver, IEnemyCombatProfileReceiver
{
        [Header("Combat Settings")]
        [SerializeField] private EnemyCombatProfile combatProfile;
        [SerializeField] private HitBoxSlot[] hitBoxZones;
        [SerializeField] private EnemyAttack[] attackSequence;
        [SerializeField] private AttackMode attackMode = AttackMode.Sequential;
        [SerializeField] private bool usePrioritySelection = true;
        [SerializeField] private EnemyCombatProcessor.AttackPriorityProfile attackPriorityProfile = EnemyCombatProcessor.AttackPriorityProfile.WeightedScore;
        [SerializeField] private bool preferAttacksCurrentlyInRange = true;
        [SerializeField] private float rangeWeight = 1.0f;
        [SerializeField] private float damageWeight = 1.0f;
        [SerializeField] private float knockbackWeight = 0.75f;
        [SerializeField] private float assetPriorityWeight = 1.0f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private float attackRangeOverride = 0f;

        private EnemyCombatProcessor _combatProcessor;
        private IActorCombatResultReceiver[] _combatResultReceivers;
        private float _attackTimer;
        private int _sequenceIndex;
        private float _computedAttackRange;
        private float _minAttackRangeFromAttacks;
        private float _outgoingDamageMultiplier = 1f;
        private float _outgoingKnockbackMultiplier = 1f;
        private readonly Dictionary<EnemyAttack, int> _attackTriggerHashes = new Dictionary<EnemyAttack, int>();
        private readonly Dictionary<HitBox, Vector3> _hitBoxOriginalScales = new Dictionary<HitBox, Vector3>();

        public float MinAttackRange => _minAttackRangeFromAttacks;
        public HitBoxSlot[] HitBoxZones => hitBoxZones;
        public IActorFacingMirrorTarget[] FacingMirrorTargets => hitBoxZones;
        public Dictionary<EnemyAttack, int> AttackTriggerHashes => _attackTriggerHashes;

        private void Awake()
        {
            _combatProcessor = new EnemyCombatProcessor();
            _combatResultReceivers = GetComponents<IActorCombatResultReceiver>();

            InitializeCombat();
        }

        public void InitializeCombat()
        {
            ApplyCombatProfile(combatProfile);
            
            if (hitBoxZones != null)
                foreach (var slot in hitBoxZones)
                    slot.absOffsetX = slot.hitBox != null
                    ? Mathf.Max(Mathf.Abs(slot.hitBox.transform.position.x - transform.position.x), 0.5f)
                    : 0.5f;

            if (attackRangeOverride > 0f) _computedAttackRange = attackRangeOverride;
            else if (hitBoxZones != null && hitBoxZones.Length > 0)
            {
                _computedAttackRange = 0f;
                foreach (var slot in hitBoxZones)
                    if (slot.hitBox != null)
                        _computedAttackRange = Mathf.Max(_computedAttackRange, MeasureHitBoxRange(slot.hitBox, slot.absOffsetX));
                if (_computedAttackRange < 0.01f) _computedAttackRange = 1.0f;
            }
            else _computedAttackRange = 1.0f;

            _minAttackRangeFromAttacks = GetMinAttackRange();

            if (attackSequence != null)
                foreach (var atk in attackSequence)
                    if (atk != null && !string.IsNullOrEmpty(atk.animatorTrigger) && !_attackTriggerHashes.ContainsKey(atk))
                        _attackTriggerHashes[atk] = Animator.StringToHash(atk.animatorTrigger);
        }

        public void Tick(float deltaTime)
        {
            if (_attackTimer > 0f) _attackTimer -= deltaTime;
        }

        public void UpdateCombatTimers()
        {
            Tick(Time.deltaTime);
        }

        public bool CanAttack(float distanceToPlayer)
        {
            return _attackTimer <= 0f && distanceToPlayer <= _minAttackRangeFromAttacks * 1.4f;
        }

        public bool TryHandleCombatCommand(in ActorCombatCommand command)
        {
            if (command.Kind != ActorCombatCommandKind.PrimaryAttack)
                return false;

            if (!CanAttack(command.Distance))
                return false;

            ExecuteAttack(command.Distance);
            return true;
        }

        private void PublishCombatResult(in ActorCombatResult result)
        {
            if (_combatResultReceivers == null)
                return;

            for (int i = 0; i < _combatResultReceivers.Length; i++)
                _combatResultReceivers[i]?.HandleCombatResult(result);
        }

    }
}
