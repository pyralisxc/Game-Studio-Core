using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Features.Enemies
{
    public class EnemyCombatModule : MonoBehaviour
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
        private ActorAnimationDriver _animationDriver;
        private Animator _animator;
        private float _attackTimer;
        private int _sequenceIndex;
        private float _computedAttackRange;
        private float _minAttackRangeFromAttacks;
        private float _outgoingDamageMultiplier = 1f;
        private float _outgoingKnockbackMultiplier = 1f;
        private readonly Dictionary<EnemyAttack, int> _attackTriggerHashes = new Dictionary<EnemyAttack, int>();

        public float MinAttackRange => _minAttackRangeFromAttacks;
        public HitBoxSlot[] HitBoxZones => hitBoxZones;
        public Dictionary<EnemyAttack, int> AttackTriggerHashes => _attackTriggerHashes;

        private void Awake()
        {
            _combatProcessor = new EnemyCombatProcessor();
            _animationDriver = GetComponent<ActorAnimationDriver>();
            _animator = GetComponentInChildren<Animator>();

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

        public bool CanAttack(float distanceToPlayer)
        {
            return _attackTimer <= 0f && distanceToPlayer <= _minAttackRangeFromAttacks * 1.4f;
        }

        public void ExecuteAttack(float distanceToPlayer, EnemyAnimationModule animationModule)
        {
            EnemyAttack atk = _combatProcessor.PickNextAttack(
                attackSequence,
                attackMode,
                usePrioritySelection,
                attackPriorityProfile,
                preferAttacksCurrentlyInRange,
                distanceToPlayer,
                ref _sequenceIndex,
                rangeWeight,
                damageWeight,
                knockbackWeight,
                assetPriorityWeight,
                GetAttackEffectiveRange);

            if (atk == null) return;

            _attackTimer = atk.attackCooldown > 0f ? atk.attackCooldown : attackCooldown;

            animationModule.TriggerAttack(atk, _attackTriggerHashes);

            HitBox box = GetZoneHitBox(atk.hitBoxZone);
            if (box == null && hitBoxZones != null && hitBoxZones.Length > 0)
                box = hitBoxZones[0].hitBox;
                
            if (box != null)
                StartCoroutine(_combatProcessor.EnemyHitBoxRoutine(
                box,
                atk.damage * _outgoingDamageMultiplier,
                atk.knockbackForce * _outgoingKnockbackMultiplier,
                atk.hitDelay,
                atk.hitDuration,
                atk.attackRadius));
        }

        public void ApplyCombatProfile(EnemyCombatProfile profile)
        {
            if (profile == null) return;
            profile.Sanitize();
            attackSequence = profile.attackSequence;
            attackMode = profile.attackMode;
            usePrioritySelection = profile.usePrioritySelection;
            preferAttacksCurrentlyInRange = profile.preferAttacksCurrentlyInRange;
            attackCooldown = profile.attackCooldown;
            attackRangeOverride = profile.attackRangeOverride;
            rangeWeight = profile.rangeWeight;
            damageWeight = profile.damageWeight;
            knockbackWeight = profile.knockbackWeight;
            assetPriorityWeight = profile.assetPriorityWeight;
        }

        private float GetAttackEffectiveRange(EnemyAttack atk)
        {
            if (atk == null) return _computedAttackRange;
            HitBox zone = GetZoneHitBox(atk.hitBoxZone);
            if (zone != null && zone.TryGetEnemyAttackRangeOverride(out float hitBoxRangeOverride))
                return hitBoxRangeOverride;
            if (atk.attackRange > 0f) return atk.attackRange;
            return _computedAttackRange + Mathf.Max(0f, atk.attackRadius);
        }

        private float GetMinAttackRange()
        {
            if (attackSequence == null || attackSequence.Length == 0) return Mathf.Max(0.5f, _computedAttackRange);
            float minRange = float.MaxValue;
            bool found = false;
            foreach (var atk in attackSequence)
            {
                if (atk == null) continue;
                found = true;
                minRange = Mathf.Min(minRange, Mathf.Max(0.1f, GetAttackEffectiveRange(atk)));
            }
            return found ? minRange : Mathf.Max(0.5f, _computedAttackRange);
        }

        private HitBox GetZoneHitBox(string zoneName)
        {
            if (hitBoxZones == null || string.IsNullOrEmpty(zoneName)) return null;
            foreach (var slot in hitBoxZones)
                if (slot.zoneName == zoneName) return slot.hitBox;
            return null;
        }

        private float MeasureHitBoxRange(HitBox box, float absOffsetX)
        {
            if (box == null) return 1.0f;
            var col = box.GetComponent<Collider>();
            if (col == null) return 1.0f;
            float halfExtent = col is BoxCollider bc ? bc.size.x * 0.5f * Mathf.Abs(box.transform.lossyScale.x) : col.bounds.extents.x;
            return absOffsetX + halfExtent;
        }

        public void SetOutgoingDamageMultiplier(float multiplier) => _outgoingDamageMultiplier = Mathf.Max(multiplier, 0f);
        public void SetOutgoingKnockbackMultiplier(float multiplier) => _outgoingKnockbackMultiplier = Mathf.Max(multiplier, 0f);
        
        public void DisableAllHitBoxes()
        {
            foreach (var hb in GetComponentsInChildren<HitBox>())
                hb.Disable();
        }
    }
}