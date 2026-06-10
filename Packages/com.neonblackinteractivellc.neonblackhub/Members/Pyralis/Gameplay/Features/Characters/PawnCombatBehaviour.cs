using System.Collections;
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
        Capability = AuthoringCapability.Combat,
        Relevance = "Primary pawn combat controller; handles sequences, combos, and delegates to modules.",
        Axioms = AuthoringWorldAxiom.Realtime,
        RequiredInterfaces = new[] { typeof(IPawnCombatModule) },
        ConsumedRoles = new[] { "AttackPrimary", "AttackSecondary", "Block" },
        FirstProof = "Perform an attack combo and verify the hits are detected and animations trigger."
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Characters/Pawn Combat Behaviour")]
    [RequireComponent(typeof(PawnHitBoxModule))]
    [RequireComponent(typeof(PawnDamageModule))]
    [RequireComponent(typeof(PawnProjectileModule))]
    [RequireComponent(typeof(PawnBlockModule))]
    [RequireComponent(typeof(PawnWeaponModule))]
    public class PawnCombatBehaviour : MonoBehaviour, IPawnCombatModule, IPawnCombatMovementContext, IDamageModifier, IActorCombatModifierReceiver
    {
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

        private ICharacterMotorState _motor;
        private ActorAnimationDriver _animationDriver;
        private HealthComponent _health;
        private IActorFeedbackPublisher _feedbackPublisher;

        private PawnComboProcessor _comboProcessor;
        private PawnHitBoxModule _hitBoxModule;
        private PawnDamageModule _damageModule;
        private PawnProjectileModule _projectileModule;
        private PawnBlockModule _blockModule;
        private PawnWeaponModule _weaponModule;

        private int _attackCount;
        private int _kickCount;
        private float _attackTimer;
        private float _kickTimer;
        private int _aerialAttackCount;
        private float _aerialTimer;
        private float _combatTimer;

        public bool IsBlocking => _blockModule != null && _blockModule.IsBlocking;
        public float BlockDamageReduction => _blockModule != null ? _blockModule.BlockDamageReduction : 0.2f;
        public float BlockFrontalAngle => _blockModule != null ? _blockModule.BlockFrontalAngle : 90f;
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
            _motor = GetComponent<ICharacterMotorState>();
            _animationDriver = GetComponent<ActorAnimationDriver>();
            _health = GetComponent<HealthComponent>();
            _feedbackPublisher = GetComponent<IActorFeedbackPublisher>();

            _hitBoxModule = GetComponent<PawnHitBoxModule>();
            _damageModule = GetComponent<PawnDamageModule>();
            _projectileModule = GetComponent<PawnProjectileModule>();
            _blockModule = GetComponent<PawnBlockModule>();
            _weaponModule = GetComponent<PawnWeaponModule>();

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
            _hitBoxModule?.Tick(dt);
            _blockModule?.Tick();
        }

        public void SyncHitBoxSides(Transform root, bool facingRight)
        {
            _hitBoxModule?.SyncHitBoxSides(facingRight);
        }

        public void ResetAerialAttackCount() => _aerialAttackCount = 0;

        public void HandleAttack()
        {
            if (_motor == null) return;

            if (_motor.IsAirborne)
            {
                PerformAerialAttack();
                return;
            }

            if (primarySequence != null && primarySequence.actions != null && primarySequence.actions.Length > 0)
            {
                ExecuteSequenceAction(_comboProcessor.PrimaryState, primarySequence, CombatInputType.Primary, _weaponModule.AttackWeapon, "Punch", ref _attackTimer, attackCooldown);
                return;
            }

            ExecuteFallbackAttack();
        }

        public void HandleKick()
        {
            if (_motor == null) return;

            if (_motor.IsAirborne)
            {
                PerformAerialAttack();
                return;
            }

            if (secondarySequence != null && secondarySequence.actions != null && secondarySequence.actions.Length > 0)
            {
                ExecuteSequenceAction(_comboProcessor.SecondaryState, secondarySequence, CombatInputType.Secondary, _weaponModule.KickWeapon, "Kick", ref _kickTimer, kickCooldown);
                return;
            }

            ExecuteFallbackKick();
        }

        public void HandleBlockStart() => _blockModule?.HandleBlockStart();
        public void HandleBlockEnd() => _blockModule?.HandleBlockEnd();
        public void CycleWeapon(int direction) => _weaponModule?.CycleWeapon(direction);

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

            _weaponModule?.SetWeapons(profile.attackWeapon, profile.kickWeapon, profile.aerialWeapon);
            // Damage scaling usually comes from the module
            _damageModule?.SetOutgoingDamageMultiplier(1.0f); // Default or from profile if added
        }

        public bool TryModifyIncomingDamage(GameObject source, ref float incomingDamage)
        {
            if (_blockModule == null || _damageModule == null) return false;
            return _damageModule.TryModifyIncomingDamage(
                source, 
                ref incomingDamage, 
                _blockModule.IsBlocking, 
                _blockModule.BlockDamageReduction, 
                _blockModule.BlockFrontalAngle, 
                _motor?.FacingRight ?? true);
        }

        public void SetOutgoingDamageMultiplier(float multiplier) => _damageModule?.SetOutgoingDamageMultiplier(multiplier);
        public void SetOutgoingKnockbackMultiplier(float multiplier) => _damageModule?.SetOutgoingKnockbackMultiplier(multiplier);

        private void PerformAerialAttack()
        {
            if (maxAerialAttacks > 0 && _aerialAttackCount >= maxAerialAttacks) return;

            if (aerialSequence != null && aerialSequence.actions != null && aerialSequence.actions.Length > 0)
            {
                if (ExecuteSequenceAction(_comboProcessor.AerialState, aerialSequence, CombatInputType.Aerial, _weaponModule.AerialWeapon, aerialHitBoxZone, ref _aerialTimer, attackCooldown))
                    _aerialAttackCount++;
                return;
            }

            if (_aerialTimer > 0f) return;

            _aerialAttackCount++;
            _aerialTimer = attackCooldown;
            _animationDriver?.SetIntSignal(ActorAnimationSignal.AttackAerial, _aerialAttackCount);
            _animationDriver?.TriggerSignal(ActorAnimationSignal.AttackAerial, intValue: _aerialAttackCount);
            ActivateHitBoxForZone(aerialHitBoxZone, _weaponModule.AerialWeapon);
        }

        private bool ExecuteSequenceAction(
            PawnComboProcessor.ComboRuntimeState state,
            CombatSequenceDefinition sequence,
            CombatInputType inputType,
            WeaponData fallbackWeapon,
            string fallbackZoneName,
            ref float cooldownTimer,
            float fallbackCooldown)
        {
            if (_comboProcessor.TryExecuteAction(
                state, 
                sequence, 
                comboResetTime, 
                combatWindow, 
                ref _combatTimer, 
                _motor.IsActing, 
                cooldownTimer, 
                out int _, 
                out CombatActionDefinition action))
            {
                WeaponData resolvedWeapon = action.weapon != null ? action.weapon : fallbackWeapon;
                cooldownTimer = ResolveActionCooldown(action, resolvedWeapon, fallbackCooldown);
                _motor.IsActing = true;
                _motor.ResetMoveToIdle();
                TriggerCombatAnimation(action, inputType);
                ActivateHitBoxForZone(action.fallbackHitBoxZone, resolvedWeapon ?? fallbackWeapon ?? _weaponModule.ActiveWeapon, action.fallbackHitBoxZone);
                return true;
            }

            return false;
        }

        private void ExecuteFallbackAttack()
        {
            if (_motor == null || _motor.IsActing || _attackTimer > 0f) return;

            _attackTimer = attackCooldown;
            _motor.IsActing = true;
            _combatTimer = combatWindow;
            _attackCount = (_attackCount % 3) + 1;

            _motor.ResetMoveToIdle();
            _animationDriver?.SetIntSignal(ActorAnimationSignal.AttackPrimary, _attackCount);
            _animationDriver?.TriggerSignal(ActorAnimationSignal.AttackPrimary, intValue: _attackCount);

            ActivateHitBoxForZone("Punch", _weaponModule.AttackWeapon);
        }

        private void ExecuteFallbackKick()
        {
            if (_motor == null || _motor.IsActing || _kickTimer > 0f) return;

            _kickTimer = kickCooldown;
            _motor.IsActing = true;
            _combatTimer = combatWindow;
            _kickCount = (_kickCount % 3) + 1;

            _motor.ResetMoveToIdle();
            _animationDriver?.SetIntSignal(ActorAnimationSignal.AttackSecondary, _kickCount);
            _animationDriver?.TriggerSignal(ActorAnimationSignal.AttackSecondary, intValue: _kickCount);

            ActivateHitBoxForZone("Kick", _weaponModule.KickWeapon);
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
            if (action != null && action.cooldownOverride >= 0f) return action.cooldownOverride;
            if (resolvedWeapon != null && resolvedWeapon.attackCooldown > 0f) return resolvedWeapon.attackCooldown;
            return fallbackCooldown;
        }

        private void ActivateHitBoxForZone(string defaultZoneName, WeaponData weapon, string explicitZoneName = null)
        {
            if (weapon != null && (weapon.weaponType == WeaponType.Ranged || weapon.weaponType == WeaponType.Thrown) && weapon.projectileDefinition != null)
            {
                _projectileModule?.FireProjectile(weapon, _motor?.FacingRight ?? true, _damageModule != null ? _damageModule.DamageHandler.OutgoingDamageMultiplier : 1.0f, _damageModule != null ? _damageModule.DamageHandler.OutgoingKnockbackMultiplier : 1.0f);
                return;
            }

            string zoneName = !string.IsNullOrEmpty(explicitZoneName) ? explicitZoneName : (weapon != null && !string.IsNullOrEmpty(weapon.hitBoxZone) ? weapon.hitBoxZone : defaultZoneName);

            float damage = _damageModule != null ? _damageModule.GetModifiedDamage(weapon != null ? weapon.damage : 10f) : 10f;
            float knockback = _damageModule != null ? _damageModule.GetModifiedKnockback(weapon != null ? weapon.knockbackForce : 5f) : 5f;
            float delay = weapon != null ? weapon.hitDelay : 0.1f;
            float duration = weapon != null ? weapon.hitDuration : 0.15f;

            _hitBoxModule?.SyncHitBoxSides(_motor?.FacingRight ?? true);
            _hitBoxModule?.ActivateHitBox(zoneName, damage, knockback, delay, duration);
        }

        private void HandleHitConfirmed(GameObject _)
        {
            _comboProcessor.HandleHitConfirmed(comboResetTime, (step, isFinisher) => 
            {
                _animationDriver?.TriggerCustom("ComboConfirm", intValue: step);
                _feedbackPublisher?.PublishCombo(step);
                if (isFinisher)
                    _feedbackPublisher?.PublishFinisher(step);
            });
        }

        private void SubscribeHitBoxes()
        {
            if (_hitBoxModule == null || _hitBoxModule.HitBoxZones == null) return;

            foreach (HitBoxSlot slot in _hitBoxModule.HitBoxZones)
            {
                if (slot?.hitBox != null)
                    slot.hitBox.HitConfirmed += HandleHitConfirmed;
            }
        }

        private void UnsubscribeHitBoxes()
        {
            if (_hitBoxModule == null || _hitBoxModule.HitBoxZones == null) return;

            foreach (HitBoxSlot slot in _hitBoxModule.HitBoxZones)
            {
                if (slot?.hitBox != null)
                    slot.hitBox.HitConfirmed -= HandleHitConfirmed;
            }
        }
    }
}
