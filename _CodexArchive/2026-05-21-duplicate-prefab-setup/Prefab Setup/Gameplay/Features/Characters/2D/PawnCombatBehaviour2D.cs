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
    [AddComponentMenu("NeonBlack/Gameplay/Characters/2D/Pawn Combat Behaviour 2D")]
    [RequireComponent(typeof(Motor2D))]
    public class PawnCombatBehaviour2D : MonoBehaviour, IPawnCombatModule, IDamageModifier, IPawnCombatInputReceiver2D, IActorCombatModifierReceiver
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
                Sequence = null;
                ActiveAction = null;
                CurrentIndex = -1;
                Timer = 0f;
                AllowNextBranch = false;
                WaitingForHitConfirm = false;
            }
        }

        [Header("Combo")]
        [SerializeField] private float comboResetTime = 1.5f;
        [SerializeField] private float combatWindow = 3f;

        [Header("Combat")]
        [SerializeField] private HitBoxSlot2D[] hitBoxZones;
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

        [Header("Combat Definitions")]
        [SerializeField] private CombatSequenceDefinition primarySequence;
        [SerializeField] private CombatSequenceDefinition secondarySequence;

        [Header("Block")]
        [Range(0f, 1f)]
        [SerializeField] private float blockDamageReduction = 0.2f;
        [Range(10f, 180f)]
        [SerializeField] private float blockFrontalAngle = 90f;

        private Motor2D _motor;
        private ActorAnimationDriver _animationDriver;
        private HealthComponent _health;
        private IActorFeedbackPublisher _feedbackPublisher;

        private readonly ComboRuntimeState _primaryState = new ComboRuntimeState();
        private readonly ComboRuntimeState _secondaryState = new ComboRuntimeState();
        private ComboRuntimeState _currentSequenceState;

        private bool _isBlocking;
        private int _attackCount;
        private int _kickCount;
        private int _activeWeaponIndex;
        private float _attackTimer;
        private float _kickTimer;
        private float _combatTimer;
        private float _actingTimer;
        private float _outgoingDamageMultiplier = 1f;
        private float _outgoingKnockbackMultiplier = 1f;

        public bool IsBlocking => _isBlocking;

        private void Awake()
        {
            _motor = GetComponent<Motor2D>();
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

        private void Update()
        {
            _attackTimer -= Time.deltaTime;
            _kickTimer -= Time.deltaTime;
            _combatTimer -= Time.deltaTime;
            _actingTimer -= Time.deltaTime;

            TickComboState(_primaryState);
            TickComboState(_secondaryState);

            if (_actingTimer <= 0f && _motor != null)
                _motor.SetActionLock(false);
        }

        public void HandlePrimaryAttackInput()
        {
            if (primarySequence != null && primarySequence.actions != null && primarySequence.actions.Length > 0)
            {
                ExecuteSequenceAction(_primaryState, primarySequence, CombatInputType.Primary, attackWeapon, "Punch", ref _attackTimer, attackCooldown);
                return;
            }

            ExecuteLegacyAttack();
        }

        public void HandleSecondaryAttackInput()
        {
            if (secondarySequence != null && secondarySequence.actions != null && secondarySequence.actions.Length > 0)
            {
                ExecuteSequenceAction(_secondaryState, secondarySequence, CombatInputType.Secondary, kickWeapon, "Kick", ref _kickTimer, kickCooldown);
                return;
            }

            ExecuteLegacyKick();
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
            primarySequence = profile.primarySequence;
            secondarySequence = profile.secondarySequence;
            blockDamageReduction = profile.blockDamageReduction;
            ApplyActiveWeapon();
        }

        public bool TryModifyIncomingDamage(GameObject source, ref float incomingDamage)
        {
            if (GetComponentInChildren<IActorGuardFeature>(true) != null)
                return false;

            if (!_isBlocking || source == null || _motor == null)
                return false;

            Vector3 toAttacker = source.transform.position - transform.position;
            float facingSign = _motor.FacingRight ? 1f : -1f;
            Vector3 facingDir = new Vector3(facingSign, 0f, 0f);
            float dot = Vector3.Dot(facingDir.normalized, toAttacker.normalized);
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
            if (action == null || cooldownTimer > 0f || _motor.IsActionLocked)
                return false;

            WeaponData resolvedWeapon = action.weapon != null ? action.weapon : fallbackWeapon;
            float resolvedCooldown = ResolveActionCooldown(action, resolvedWeapon, fallbackCooldown);
            cooldownTimer = resolvedCooldown;
            _combatTimer = combatWindow;

            state.Sequence = sequence;
            state.ActiveAction = action;
            state.CurrentIndex = nextIndex;
            state.Timer = action.comboWindow > 0f ? action.comboWindow : comboResetTime;
            state.AllowNextBranch = !action.requiresHitConfirmForNextBranch;
            state.WaitingForHitConfirm = action.requiresHitConfirmForNextBranch;
            _currentSequenceState = state;

            _motor.ResetMoveToIdle();
            _motor.SetActionLock(true);
            _actingTimer = Mathf.Max(resolvedWeapon != null ? resolvedWeapon.hitDelay + resolvedWeapon.hitDuration : hitDelay + hitDuration, 0.05f);

            TriggerCombatAnimation(action, inputType);
            ActivateHitBoxForZone(action.fallbackHitBoxZone, resolvedWeapon ?? fallbackWeapon, action.fallbackHitBoxZone, state);

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
            if (_motor == null || _attackTimer > 0f || _motor.IsActionLocked)
                return;

            _attackTimer = attackCooldown;
            _combatTimer = combatWindow;
            _attackCount = (_attackCount % 3) + 1;
            _motor.ResetMoveToIdle();
            _motor.SetActionLock(true);
            _actingTimer = Mathf.Max(attackWeapon != null ? attackWeapon.hitDelay + attackWeapon.hitDuration : hitDelay + hitDuration, 0.05f);
            _animationDriver?.SetIntSignal(ActorAnimationSignal.AttackPrimary, _attackCount);
            _animationDriver?.TriggerSignal(ActorAnimationSignal.AttackPrimary, intValue: _attackCount);
            ActivateHitBoxForZone("Punch", attackWeapon);
        }

        private void ExecuteLegacyKick()
        {
            if (_motor == null || _kickTimer > 0f || _motor.IsActionLocked)
                return;

            _kickTimer = kickCooldown;
            _combatTimer = combatWindow;
            _kickCount = (_kickCount % 3) + 1;
            _motor.ResetMoveToIdle();
            _motor.SetActionLock(true);
            _actingTimer = Mathf.Max(kickWeapon != null ? kickWeapon.hitDelay + kickWeapon.hitDuration : hitDelay + hitDuration, 0.05f);
            _animationDriver?.SetIntSignal(ActorAnimationSignal.AttackSecondary, _kickCount);
            _animationDriver?.TriggerSignal(ActorAnimationSignal.AttackSecondary, intValue: _kickCount);
            ActivateHitBoxForZone("Kick", kickWeapon);
        }

        private void TriggerCombatAnimation(CombatActionDefinition action, CombatInputType inputType)
        {
            ActorAnimationSignal signal = action != null ? action.animationSignal : inputType == CombatInputType.Secondary
                ? ActorAnimationSignal.AttackSecondary
                : ActorAnimationSignal.AttackPrimary;

            int comboStep = action != null ? action.comboStep : 1;
            _animationDriver?.SetIntSignal(signal, comboStep);
            _animationDriver?.TriggerSignal(signal, intValue: comboStep);

            if (action != null && action.finisherResetsCombo)
                _animationDriver?.TriggerCustom("ComboFinisher", intValue: comboStep);
        }

        private float ResolveActionCooldown(CombatActionDefinition action, WeaponData resolvedWeapon, float fallbackCooldown)
        {
            if (action != null && action.cooldownOverride >= 0f)
                return action.cooldownOverride;

            if (resolvedWeapon != null && resolvedWeapon.attackCooldown > 0f)
                return resolvedWeapon.attackCooldown;

            return fallbackCooldown;
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

            HitBox2D box = GetZoneByName(zoneName) ?? GetZoneByName(defaultZoneName);
            if (box == null)
                return;

            float damage = (weapon != null ? weapon.damage : baseDamage) * _outgoingDamageMultiplier;
            float knockback = (weapon != null ? weapon.knockbackForce : baseKnockback) * _outgoingKnockbackMultiplier;
            float delay = weapon != null ? weapon.hitDelay : hitDelay;
            float duration = weapon != null ? weapon.hitDuration : hitDuration;

            SyncHitBoxSides();
            if (state != null)
                _currentSequenceState = state;

            StartCoroutine(HitBoxTimingRoutine(box, damage, knockback, delay, duration));
        }

        private IEnumerator HitBoxTimingRoutine(HitBox2D box, float damage, float knockback, float delay, float duration)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            box.ConfigureDamage(damage, knockback);
            box.Enable();
            yield return new WaitForSeconds(Mathf.Max(duration, 0.01f));
            box.Disable();
        }

        private void FireProjectile(WeaponData weapon)
        {
            if (weapon == null || weapon.projectilePrefab == null)
                return;

            bool facingRight = _motor == null || _motor.FacingRight;
            Vector3 spawnPos = projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.position + Vector3.up * 0.25f + (facingRight ? Vector3.right : Vector3.left) * 0.5f;

            Vector3 forward = facingRight ? Vector3.right : Vector3.left;
            GameObject instance = Instantiate(weapon.projectilePrefab, spawnPos, Quaternion.LookRotation(forward, Vector3.up));
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

        private HitBox2D GetZoneByName(string zoneName)
        {
            if (hitBoxZones == null || string.IsNullOrEmpty(zoneName))
                return null;

            foreach (HitBoxSlot2D slot in hitBoxZones)
            {
                if (slot != null && slot.zoneName == zoneName)
                    return slot.hitBox;
            }

            return null;
        }

        private void SyncHitBoxSides()
        {
            if (_motor == null || hitBoxZones == null)
                return;

            foreach (HitBoxSlot2D slot in hitBoxZones)
                slot?.MirrorToSide(transform, _motor.FacingRight);
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

        private void CacheHitBoxOffsets()
        {
            if (hitBoxZones == null)
                return;

            foreach (HitBoxSlot2D slot in hitBoxZones)
            {
                slot.absOffsetX = slot.hitBox != null
                    ? Mathf.Max(Mathf.Abs(slot.hitBox.transform.position.x - transform.position.x), 0.25f)
                    : 0.25f;
            }
        }

        private void SubscribeHitBoxes()
        {
            if (hitBoxZones == null)
                return;

            foreach (HitBoxSlot2D slot in hitBoxZones)
            {
                if (slot?.hitBox != null)
                    slot.hitBox.HitConfirmed += HandleHitConfirmed;
            }
        }

        private void UnsubscribeHitBoxes()
        {
            if (hitBoxZones == null)
                return;

            foreach (HitBoxSlot2D slot in hitBoxZones)
            {
                if (slot?.hitBox != null)
                    slot.hitBox.HitConfirmed -= HandleHitConfirmed;
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
    }
}
