using System.Collections;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Characters;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    [AddComponentMenu("NeonBlack/Gameplay/Characters/Pawn Combat Behaviour")]
    public class PawnCombatBehaviour : MonoBehaviour, IPawnCombatModule, IPawnCombatMovementContext, IDamageModifier, IActorCombatModifierReceiver
    {
        private sealed class ComboRuntimeState
        {
            public CombatSequenceDefinition Sequence;
            public CombatActionDefinition ActiveAction;
            public int CurrentIndex = -1;
            public float Timer;
            public bool AllowNextBranch;
            public bool WaitingForHitConfirm;

            public void Reset()
            {
                ActiveAction = null;
                CurrentIndex = -1;
                Timer = 0f;
                AllowNextBranch = false;
                WaitingForHitConfirm = false;
            }
        }

        [Header("Combo")]
        [SerializeField] private float comboResetTime = 1.5f;

        [Header("Combat")]
        [SerializeField] private HitBoxSlot[] hitBoxZones;

        [Header("Projectile")]
        [SerializeField] private Transform projectileSpawnPoint;

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
        [SerializeField] private float combatWindow = 3f;
        [SerializeField] private int maxAerialAttacks = 2;
        [Range(0f, 1f)]
        [SerializeField] private float attackMoveMultiplier = 0.2f;
        [Range(0f, 1f)]
        [SerializeField] private float aerialAttackMoveMultiplier = 0.5f;
        [SerializeField] private string aerialHitBoxZone = "Aerial";
        [SerializeField] private WeaponData aerialWeapon;

        [Header("Combat Definitions")]
        [SerializeField] private CombatSequenceDefinition primarySequence;
        [SerializeField] private CombatSequenceDefinition secondarySequence;
        [SerializeField] private CombatSequenceDefinition aerialSequence;

        [Header("Block")]
        [Range(0f, 1f)]
        [SerializeField] private float blockDamageReduction = 0.2f;
        [Range(10f, 180f)]
        [SerializeField] private float blockFrontalAngle = 90f;

        private ICharacterMotorState _motor;
        private ActorAnimationDriver _animationDriver;
        private HealthComponent _health;
        private IActorFeedbackPublisher _feedbackPublisher;

        private readonly ComboRuntimeState _primaryState = new ComboRuntimeState();
        private readonly ComboRuntimeState _secondaryState = new ComboRuntimeState();
        private readonly ComboRuntimeState _aerialState = new ComboRuntimeState();
        private ComboRuntimeState _currentSequenceState;

        private bool _isBlocking;
        private int _attackCount;
        private int _kickCount;
        private int _activeWeaponIndex;
        private float _attackTimer;
        private float _kickTimer;
        private int _aerialAttackCount;
        private float _aerialTimer;
        private float _combatTimer;
        private float _outgoingDamageMultiplier = 1f;
        private float _outgoingKnockbackMultiplier = 1f;

        public bool IsBlocking => _isBlocking;
        public float BlockDamageReduction => blockDamageReduction;
        public float BlockFrontalAngle => blockFrontalAngle;
        public float AttackTimer => _attackTimer;
        public float KickTimer => _kickTimer;
        public float AttackMoveMultiplier => attackMoveMultiplier;
        public float AerialAttackMoveMultiplier => aerialAttackMoveMultiplier;
        public float CombatTimer => _combatTimer;

        private void Awake()
        {
            _motor = GetComponent<ICharacterMotorState>();
            _animationDriver = GetComponent<ActorAnimationDriver>();
            _health = GetComponent<HealthComponent>();
            _feedbackPublisher = GetComponent<IActorFeedbackPublisher>();

            CacheHitBoxOffsets();
            SubscribeHitBoxes();

            if (equippedWeapons != null && equippedWeapons.Length > 0)
            {
                _activeWeaponIndex = Mathf.Clamp(startingWeaponIndex, 0, equippedWeapons.Length - 1);
                attackWeapon = equippedWeapons[_activeWeaponIndex];
            }

            ApplyActiveWeapon();
        }

        private void OnDestroy()
        {
            UnsubscribeHitBoxes();
        }

        public void UpdateCombatTimers()
        {
            _attackTimer -= Time.deltaTime;
            _kickTimer -= Time.deltaTime;
            _aerialTimer -= Time.deltaTime;
            _combatTimer -= Time.deltaTime;

            TickComboState(_primaryState);
            TickComboState(_secondaryState);
            TickComboState(_aerialState);

            _animationDriver?.SetBoolSignal(ActorAnimationSignal.BlockLoop, _isBlocking);
        }

        public void SyncHitBoxSides(Transform root, bool facingRight)
        {
            if (hitBoxZones == null)
                return;

            foreach (HitBoxSlot slot in hitBoxZones)
                slot.MirrorToSide(root, facingRight);
        }

        public void ResetAerialAttackCount() => _aerialAttackCount = 0;

        public HitBox GetZoneByName(string zoneName)
        {
            if (hitBoxZones == null || string.IsNullOrEmpty(zoneName))
                return null;

            foreach (HitBoxSlot slot in hitBoxZones)
                if (slot.zoneName == zoneName)
                    return slot.hitBox;

            return null;
        }

        public void HandleAttack()
        {
            if (_motor == null)
                return;

            if (_motor.IsAirborne)
            {
                PerformAerialAttack();
                return;
            }

            if (primarySequence != null && primarySequence.actions != null && primarySequence.actions.Length > 0)
            {
                ExecuteSequenceAction(_primaryState, primarySequence, CombatInputType.Primary, attackWeapon, "Punch", ref _attackTimer, attackCooldown);
                return;
            }

            ExecuteLegacyAttack();
        }

        public void HandleKick()
        {
            if (_motor == null)
                return;

            if (_motor.IsAirborne)
            {
                PerformAerialAttack();
                return;
            }

            if (secondarySequence != null && secondarySequence.actions != null && secondarySequence.actions.Length > 0)
            {
                ExecuteSequenceAction(_secondaryState, secondarySequence, CombatInputType.Secondary, kickWeapon, "Kick", ref _kickTimer, kickCooldown);
                return;
            }

            ExecuteLegacyKick();
        }

        public void HandleBlockStart()
        {
            if (_motor != null && _motor.IsActing)
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

        public void CycleWeapon(int direction)
        {
            if (equippedWeapons == null || equippedWeapons.Length <= 1)
                return;

            _activeWeaponIndex = (_activeWeaponIndex + direction + equippedWeapons.Length) % equippedWeapons.Length;
            ApplyActiveWeapon();
        }

        public void ResetAttackCombo()
        {
            _attackCount = 0;
            _primaryState.Reset();
        }

        public void ResetKickCombo()
        {
            _kickCount = 0;
            _secondaryState.Reset();
        }

        public void ApplyCombatProfile(PawnProfileApplicationContext context, PawnCombatProfile profile)
        {
            if (profile == null)
                return;

            baseDamage = profile.baseDamage;
            baseKnockback = profile.baseKnockback;
            attackCooldown = profile.attackCooldown;
            kickCooldown = profile.kickCooldown;
            comboResetTime = profile.comboResetTime;
            combatWindow = profile.combatWindow;
            attackWeapon = profile.attackWeapon;
            kickWeapon = profile.kickWeapon;
            aerialWeapon = profile.aerialWeapon;
            primarySequence = profile.primarySequence;
            secondarySequence = profile.secondarySequence;
            aerialSequence = profile.aerialSequence;
            blockDamageReduction = profile.blockDamageReduction;
            maxAerialAttacks = profile.maxAerialAttacks;
            ApplyActiveWeapon();
        }

        public bool TryModifyIncomingDamage(GameObject source, ref float incomingDamage)
        {
            if (GetComponentInChildren<IActorGuardFeature>(true) != null)
                return false;

            if (!_isBlocking || source == null)
                return false;

            Vector3 toAttacker = source.transform.position - transform.position;
            toAttacker.y = 0f;
            if (toAttacker.sqrMagnitude <= 0.001f)
                return false;

            bool facingRight = _motor?.FacingRight ?? true;
            Vector3 facingDir = facingRight ? Vector3.right : Vector3.left;
            float dot = Vector3.Dot(facingDir, toAttacker.normalized);
            float threshold = Mathf.Cos(blockFrontalAngle * Mathf.Deg2Rad);
            if (dot < threshold)
                return false;

            incomingDamage *= blockDamageReduction;
            return true;
        }

        public void SetOutgoingDamageMultiplier(float multiplier)
        {
            _outgoingDamageMultiplier = Mathf.Max(multiplier, 0f);
        }

        public void SetOutgoingKnockbackMultiplier(float multiplier)
        {
            _outgoingKnockbackMultiplier = Mathf.Max(multiplier, 0f);
        }

        private void PerformAerialAttack()
        {
            if (maxAerialAttacks > 0 && _aerialAttackCount >= maxAerialAttacks)
                return;

            if (aerialSequence != null && aerialSequence.actions != null && aerialSequence.actions.Length > 0)
            {
                if (ExecuteSequenceAction(_aerialState, aerialSequence, CombatInputType.Aerial, aerialWeapon, aerialHitBoxZone, ref _aerialTimer, attackCooldown))
                    _aerialAttackCount++;
                return;
            }

            if (_aerialTimer > 0f)
                return;

            _aerialAttackCount++;
            _aerialTimer = attackCooldown;
            _animationDriver?.SetIntSignal(ActorAnimationSignal.AttackAerial, _aerialAttackCount);
            _animationDriver?.TriggerSignal(ActorAnimationSignal.AttackAerial, intValue: _aerialAttackCount);
            ActivateHitBoxForZone(aerialHitBoxZone, aerialWeapon);
        }

        private bool ExecuteSequenceAction(
            ComboRuntimeState state,
            CombatSequenceDefinition sequence,
            CombatInputType inputType,
            WeaponData fallbackWeapon,
            string fallbackZoneName,
            ref float cooldownTimer,
            float fallbackCooldown)
        {
            if (_motor == null || sequence == null || sequence.actions == null || sequence.actions.Length == 0)
                return false;

            bool canBranch = state.CurrentIndex >= 0 && state.Timer > 0f && state.AllowNextBranch;
            int nextIndex = canBranch ? state.CurrentIndex + 1 : 0;
            if (nextIndex >= sequence.actions.Length)
                nextIndex = sequence.restartFromFirstActionWhenBranchFails ? 0 : sequence.actions.Length - 1;

            CombatActionDefinition action = sequence.actions[nextIndex];
            if (action == null)
                return false;

            if (_motor.IsActing || cooldownTimer > 0f)
                return false;

            WeaponData resolvedWeapon = action.weapon != null ? action.weapon : fallbackWeapon;
            float resolvedCooldown = ResolveActionCooldown(action, resolvedWeapon, fallbackCooldown);
            cooldownTimer = resolvedCooldown;
            _motor.IsActing = true;
            _combatTimer = combatWindow;

            state.Sequence = sequence;
            state.ActiveAction = action;
            state.CurrentIndex = nextIndex;
            state.Timer = action.comboWindow > 0f ? action.comboWindow : comboResetTime;
            state.AllowNextBranch = !action.requiresHitConfirmForNextBranch;
            state.WaitingForHitConfirm = action.requiresHitConfirmForNextBranch;

            _currentSequenceState = state;
            _motor.ResetMoveToIdle();
            TriggerCombatAnimation(action, inputType);
            ActivateHitBoxForZone(action.fallbackHitBoxZone, resolvedWeapon ?? fallbackWeapon ?? ActiveWeapon ?? attackWeapon, action.fallbackHitBoxZone, state);

            if (action.finisherResetsCombo || (nextIndex >= sequence.actions.Length - 1 && sequence.resetAfterFinalAction))
            {
                state.AllowNextBranch = false;
                state.WaitingForHitConfirm = false;
                state.CurrentIndex = -1;
            }

            return true;
        }

        private void ExecuteLegacyAttack()
        {
            if (_motor == null || _motor.IsActing || _attackTimer > 0f)
                return;

            _attackTimer = attackCooldown;
            _motor.IsActing = true;
            _combatTimer = combatWindow;
            _attackCount = (_attackCount % 3) + 1;

            _motor.ResetMoveToIdle();
            _animationDriver?.SetIntSignal(ActorAnimationSignal.AttackPrimary, _attackCount);
            _animationDriver?.TriggerSignal(ActorAnimationSignal.AttackPrimary, intValue: _attackCount);

            ActivateHitBoxForZone("Punch", attackWeapon);
        }

        private void ExecuteLegacyKick()
        {
            if (_motor == null || _motor.IsActing || _kickTimer > 0f)
                return;

            _kickTimer = kickCooldown;
            _motor.IsActing = true;
            _combatTimer = combatWindow;
            _kickCount = (_kickCount % 3) + 1;

            _motor.ResetMoveToIdle();
            _animationDriver?.SetIntSignal(ActorAnimationSignal.AttackSecondary, _kickCount);
            _animationDriver?.TriggerSignal(ActorAnimationSignal.AttackSecondary, intValue: _kickCount);

            ActivateHitBoxForZone("Kick", kickWeapon);
        }

        private void TriggerCombatAnimation(CombatActionDefinition action, CombatInputType inputType)
        {
            ActorAnimationSignal signal = action != null ? action.animationSignal : ResolveDefaultSignal(inputType);
            int comboStep = action != null ? action.comboStep : 1;

            _animationDriver?.SetIntSignal(signal, comboStep);
            _animationDriver?.TriggerSignal(signal, intValue: comboStep);

            if (action != null && action.finisherResetsCombo)
                _animationDriver?.TriggerCustom("ComboFinisher", intValue: comboStep);
        }

        private static ActorAnimationSignal ResolveDefaultSignal(CombatInputType inputType)
        {
            return inputType switch
            {
                CombatInputType.Secondary => ActorAnimationSignal.AttackSecondary,
                CombatInputType.Aerial => ActorAnimationSignal.AttackAerial,
                _ => ActorAnimationSignal.AttackPrimary,
            };
        }

        private float ResolveActionCooldown(CombatActionDefinition action, WeaponData resolvedWeapon, float fallbackCooldown)
        {
            if (action != null && action.cooldownOverride >= 0f)
                return action.cooldownOverride;

            if (resolvedWeapon != null && resolvedWeapon.attackCooldown > 0f)
                return resolvedWeapon.attackCooldown;

            return fallbackCooldown;
        }

        private void ActivateHitBox(HitBox box, WeaponData weapon)
        {
            if (box == null)
                return;

            float damage = (weapon != null ? weapon.damage : baseDamage) * _outgoingDamageMultiplier;
            float knockback = (weapon != null ? weapon.knockbackForce : baseKnockback) * _outgoingKnockbackMultiplier;
            float delay = weapon != null ? weapon.hitDelay : hitDelay;
            float duration = weapon != null ? weapon.hitDuration : hitDuration;

            SyncHitBoxSides(transform, _motor?.FacingRight ?? true);
            StartCoroutine(HitBoxTimingRoutine(box, damage, knockback, delay, duration));
        }

        private IEnumerator HitBoxTimingRoutine(HitBox box, float damage, float knockback, float delay, float duration)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            box.Enable(damage, knockback);
            yield return new WaitForSeconds(Mathf.Max(duration, 0.01f));
            box.Disable();
        }

        private void ActivateHitBoxForZone(string defaultZoneName, WeaponData weapon, string explicitZoneName = null, ComboRuntimeState state = null)
        {
            if (weapon != null && weapon.weaponType == WeaponType.Ranged && weapon.projectilePrefab != null)
            {
                FireProjectile(weapon);
                return;
            }

            string zoneName = !string.IsNullOrEmpty(explicitZoneName)
                ? explicitZoneName
                : weapon != null && !string.IsNullOrEmpty(weapon.hitBoxZone)
                    ? weapon.hitBoxZone
                    : defaultZoneName;

            HitBox box = GetZoneByName(zoneName) ?? GetZoneByName(defaultZoneName);
            if (state != null)
                _currentSequenceState = state;

            ActivateHitBox(box, weapon);
        }

        private void FireProjectile(WeaponData weapon)
        {
            bool facingRight = _motor?.FacingRight ?? true;
            Vector3 spawnPos = projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.position + Vector3.up * 1f + transform.forward * 0.5f;

            Vector3 forward = facingRight ? Vector3.right : Vector3.left;
            GameObject instance = Instantiate(
                weapon.projectilePrefab,
                spawnPos,
                Quaternion.LookRotation(forward, Vector3.up));

            if (instance.TryGetComponent(out Projectile projectile))
            {
                projectile.Launch(
                    gameObject,
                    _health != null ? _health.faction : Faction.Neutral,
                    weapon.damage * _outgoingDamageMultiplier,
                    weapon.knockbackForce * _outgoingKnockbackMultiplier,
                    weapon.projectileSpeed);
            }
        }

        private WeaponData ActiveWeapon =>
            equippedWeapons != null && equippedWeapons.Length > _activeWeaponIndex
                ? equippedWeapons[_activeWeaponIndex]
                : null;

        private void ApplyActiveWeapon()
        {
            WeaponData weapon = ActiveWeapon;
            _animationDriver?.SetRuntimeControllerOverride(weapon != null ? weapon.overrideController : null);
        }

        private void TickComboState(ComboRuntimeState state)
        {
            if (state == null || state.Timer <= 0f)
                return;

            state.Timer -= Time.deltaTime;
            if (state.Timer > 0f)
                return;

            state.Reset();
            if (_currentSequenceState == state)
                _currentSequenceState = null;
        }

        private void HandleHitConfirmed(GameObject _)
        {
            if (_currentSequenceState == null || !_currentSequenceState.WaitingForHitConfirm)
                return;

            _currentSequenceState.WaitingForHitConfirm = false;
            _currentSequenceState.AllowNextBranch = true;
            _currentSequenceState.Timer = Mathf.Max(_currentSequenceState.Timer, comboResetTime);
            if (_currentSequenceState.ActiveAction != null)
            {
                _animationDriver?.TriggerCustom("ComboConfirm", intValue: _currentSequenceState.ActiveAction.comboStep);
                _feedbackPublisher?.PublishCombo(_currentSequenceState.ActiveAction.comboStep);
                if (_currentSequenceState.ActiveAction.finisherResetsCombo)
                    _feedbackPublisher?.PublishFinisher(_currentSequenceState.ActiveAction.comboStep);
            }
        }

        private void CacheHitBoxOffsets()
        {
            if (hitBoxZones == null)
                return;

            foreach (HitBoxSlot slot in hitBoxZones)
            {
                slot.absOffsetX = slot.hitBox != null
                    ? Mathf.Max(Mathf.Abs(slot.hitBox.transform.position.x - transform.position.x), 0.5f)
                    : 0.5f;
            }
        }

        private void SubscribeHitBoxes()
        {
            if (hitBoxZones == null)
                return;

            foreach (HitBoxSlot slot in hitBoxZones)
            {
                if (slot?.hitBox != null)
                    slot.hitBox.HitConfirmed += HandleHitConfirmed;
            }
        }

        private void UnsubscribeHitBoxes()
        {
            if (hitBoxZones == null)
                return;

            foreach (HitBoxSlot slot in hitBoxZones)
            {
                if (slot?.hitBox != null)
                    slot.hitBox.HitConfirmed -= HandleHitConfirmed;
            }
        }
    }
}
