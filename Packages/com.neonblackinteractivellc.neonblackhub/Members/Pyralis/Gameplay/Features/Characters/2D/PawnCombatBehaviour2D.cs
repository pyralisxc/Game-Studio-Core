using System.Collections;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Characters;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        Relevance = "2D pawn combat; receives attack input, resolves combos, activates HitBox2D, and fires projectiles.",
        Axioms = AuthoringWorldAxiom.Dimensions2D,
        NativeSetup = new[] 
        { 
            "Attach to the same root as Motor2D.",
            "Assign HitBox2D zones for melee attacks.",
            "Assign CombatSequenceDefinition for authored combos.",
            "Assign Projectile Launcher for ranged attacks."
        },
        AssignmentFields = new[] { nameof(hitBoxZones), nameof(equippedWeapons), nameof(startingWeaponIndex), nameof(attackCooldown), nameof(kickCooldown), nameof(projectileLauncher) },
        FirstProof = "Verify attacks trigger animations and hitboxes detect targets.",
        ExpertAdvice = "For 2D-only combat, prefer PawnCombatBehaviour2D. Do not leave hitbox zone names mismatched with WeaponData fallback zones."
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Characters/2D/Pawn Combat Behaviour 2D")]
    [RequireComponent(typeof(Motor2D))]
    public class PawnCombatBehaviour2D : MonoBehaviour, IPawnCombatModule, IPawnCombatInputReceiver2D, IActorCombatModifierReceiver, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (hitBoxZones == null || hitBoxZones.Length == 0)
                yield return "Hit Box Zones is empty. Melee attacks need HitBox2D slots.";
            if (attackCooldown < 0f) yield return "Attack Cooldown cannot be negative.";
        }
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

        private Motor2D _motor;
        private ActorAnimationDriver _animationDriver;
        private HealthComponent _health;
        private IActorFeedbackPublisher _feedbackPublisher;

        private readonly ComboRuntimeState _primaryState = new ComboRuntimeState();
        private readonly ComboRuntimeState _secondaryState = new ComboRuntimeState();
        private ComboRuntimeState _currentSequenceState;

        private int _attackCount;
        private int _kickCount;
        private int _activeWeaponIndex;
        private float _attackTimer;
        private float _kickTimer;
        private float _combatTimer;
        private float _actingTimer;
        private float _outgoingDamageMultiplier = 1f;
        private float _outgoingKnockbackMultiplier = 1f;

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

            ExecuteFallbackAttack();
        }

        public void HandleSecondaryAttackInput()
        {
            if (secondarySequence != null && secondarySequence.actions != null && secondarySequence.actions.Length > 0)
            {
                ExecuteSequenceAction(_secondaryState, secondarySequence, CombatInputType.Secondary, kickWeapon, "Kick", ref _kickTimer, kickCooldown);
                return;
            }

            ExecuteFallbackKick();
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
            ApplyActiveWeapon();
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

        private void ExecuteFallbackAttack()
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

        private void ExecuteFallbackKick()
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
            if (weapon != null
                && (weapon.weaponType == WeaponType.Ranged || weapon.weaponType == WeaponType.Thrown)
                && weapon.projectileDefinition != null)
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
            box.Fire(duration);
        }

        private void FireProjectile(WeaponData weapon)
        {
            if (weapon == null || weapon.projectileDefinition == null)
                return;

            ProjectileLauncher2D launcher = ResolveProjectileLauncher();
            if (launcher == null)
            {
                Debug.LogWarning($"{nameof(PawnCombatBehaviour2D)} needs a {nameof(ProjectileLauncher2D)} to fire ranged weapon `{weapon.weaponName}` through the authored projectile path.", this);
                return;
            }

            bool facingRight = _motor == null || _motor.FacingRight;
            Vector3 spawnPos = projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.position + Vector3.up * 0.25f + (facingRight ? Vector3.right : Vector3.left) * 0.5f;

            Vector3 forward = facingRight ? Vector3.right : Vector3.left;
            ProjectileFireRequest request = new ProjectileFireRequest(
                weapon.projectileDefinition,
                weapon.fireModeDefinition,
                spawnPos,
                forward,
                gameObject,
                _health != null ? _health.faction : Faction.Neutral,
                damageMultiplier: _outgoingDamageMultiplier,
                knockbackMultiplier: _outgoingKnockbackMultiplier);

            launcher.Fire(request);
        }

        private ProjectileLauncher2D ResolveProjectileLauncher()
        {
            if (projectileLauncher != null)
                return projectileLauncher;

            projectileLauncher = GetComponentInParent<ProjectileLauncher2D>();
            if (projectileLauncher == null)
                projectileLauncher = GetComponentInChildren<ProjectileLauncher2D>();

            return projectileLauncher;
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
