using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [AddComponentMenu("NeonBlack/Gameplay/Combat/Actor Status Effect Feature Runtime")]
    [AuthoringContract(
        ModuleId = "actor.status",
        Capability = AuthoringCapability.Combat,
        Relevance = "Applies timed status effects, damage/heal ticks, action locks, movement modifiers, combat multipliers, and shield-style damage modifiers.",
        Lane = "Combat",
        ProfileType = typeof(ActorStatusEffectProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IActorStatusEffectReceiver), typeof(IDamageModifier) },
        RequiredInterfaceNames = new[] 
        { 
            "NeonBlack.Gameplay.Features.Combat.IActorMovementModifierReceiver",
            "NeonBlack.Gameplay.Features.Combat.IActorCombatModifierReceiver",
            "NeonBlack.Gameplay.Features.Combat.IActorHealthModifierReceiver"
        },
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Features.Combat.HealthComponent" },
        NativeSetup = new[]
        {
            "Create ActorStatusEffectProfile.",
            "Create FeatureModuleDefinition.",
            "Assign runtime prefab with ActorStatusEffectFeatureRuntime.",
            "Assign profile asset.",
            "Add module to PawnDefinition.featureModules."
        },
        FirstProof = "Apply a status effect at runtime and verify it modifies actor stats as expected.",
        AssignmentFields = new[] { nameof(statusProfile) },
        ExpertAdvice = "Core implementation for status effects. Requires receivers for movement and combat multipliers to take effect.",
        CustomizationMoments = new[]
        {
            "ActorStatusEffectProfile.startingEffects",
            "ActorStatusEffectProfile.defaultShieldDamageReduction"
        }
    )]
    public class ActorStatusEffectFeatureRuntime : MonoBehaviour, IFeatureModuleRuntime, IActorStatusEffectReceiver, IDamageModifier
{
        private sealed class ActiveStatusEffect
        {
            public StatusEffectDefinition Definition;
            public GameObject Source;
            public float RemainingDuration;
            public float TickTimer;
            public int StackCount = 1;
        }

        [SerializeField] private ActorStatusEffectProfile statusProfile;

        private readonly List<ActiveStatusEffect> _activeEffects = new List<ActiveStatusEffect>(8);

        private ActorFeatureContext _context;
        private IActorHealthState _health;
        private IActorMovementModifierReceiver _movementReceiver;
        private IActorCombatModifierReceiver _combatReceiver;
        private IActorHealthModifierReceiver _healthModifierReceiver;
        private IActorFeedbackPublisher _feedbackPublisher;

        public string ModuleId => "actor.status";

        private void Update()
        {
            if (_context == null || _activeEffects.Count == 0)
                return;

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveStatusEffect effect = _activeEffects[i];
                if (effect.Definition == null)
                {
                    _activeEffects.RemoveAt(i);
                    continue;
                }

                effect.RemainingDuration -= Time.deltaTime;
                if (effect.Definition.effectKind == StatusEffectKind.DamageOverTime
                    || effect.Definition.effectKind == StatusEffectKind.Poison
                    || effect.Definition.effectKind == StatusEffectKind.Burn)
                {
                    effect.TickTimer -= Time.deltaTime;
                    if (effect.TickTimer <= 0f)
                    {
                        effect.TickTimer = effect.Definition.tickInterval;
                        _health?.TakeDamage(GetEffectMagnitude(effect), _context.ActorTransform.position, effect.Source);
                    }
                }
                else if (effect.Definition.effectKind == StatusEffectKind.HealOverTime)
                {
                    effect.TickTimer -= Time.deltaTime;
                    if (effect.TickTimer <= 0f)
                    {
                        effect.TickTimer = effect.Definition.tickInterval;
                        _health?.Heal(GetEffectMagnitude(effect));
                    }
                }

                if (effect.RemainingDuration > 0f)
                    continue;

                _activeEffects.RemoveAt(i);
            }

            RecomputeAggregates();
        }

        public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext)
        {
            ActorFeatureContext context = initializationContext != null ? initializationContext.ActorContext : null;
            var definition = initializationContext != null ? initializationContext.Definition : null;
            _context = context;
            _health = context != null ? context.Health : null;
            _movementReceiver = context != null && context.ActorObject != null
                ? context.ActorObject.GetComponent<IActorMovementModifierReceiver>()
                : null;
            _combatReceiver = context != null && context.ActorObject != null
                ? context.ActorObject.GetComponent<IActorCombatModifierReceiver>()
                : null;
            _healthModifierReceiver = context != null && context.ActorObject != null
                ? context.ActorObject.GetComponent<IActorHealthModifierReceiver>()
                : null;
            _feedbackPublisher = context != null && context.ActorObject != null
                ? context.ActorObject.GetComponent<IActorFeedbackPublisher>()
                : null;

            statusProfile = initializationContext.GetProfile<ActorStatusEffectProfile>(definition != null ? definition.profileAsset : null);
            statusProfile?.Sanitize();

            if (statusProfile != null && statusProfile.startingEffects != null)
            {
                for (int i = 0; i < statusProfile.startingEffects.Length; i++)
                    ApplyStatusEffect(statusProfile.startingEffects[i], context != null ? context.ActorObject : null);
            }
        }

        public void ShutdownFeature()
        {
            _activeEffects.Clear();
            _movementReceiver?.SetStatusMoveSpeedMultiplier(1f);
            _movementReceiver?.SetStatusActionLock(false);
            _combatReceiver?.SetOutgoingDamageMultiplier(1f);
            _combatReceiver?.SetOutgoingKnockbackMultiplier(1f);
            _healthModifierReceiver?.SetIncomingDamageMultiplier(1f);
            _healthModifierReceiver?.SetRegenRateMultiplier(1f);
            _context = null;
            _health = null;
            _movementReceiver = null;
            _combatReceiver = null;
            _healthModifierReceiver = null;
            _feedbackPublisher = null;
        }

        public void ApplyStatusEffect(StatusEffectDefinition effectDefinition, GameObject source = null)
        {
            if (effectDefinition == null)
                return;

            effectDefinition.Sanitize();
            ActiveStatusEffect existing = FindEffect(effectDefinition.effectId);
            if (existing != null)
            {
                ApplyExistingEffectStrategy(existing, effectDefinition, source);
            }
            else if (existing == null)
            {
                _activeEffects.Add(new ActiveStatusEffect
                {
                    Definition = effectDefinition,
                    Source = source,
                    RemainingDuration = effectDefinition.duration,
                    TickTimer = effectDefinition.tickInterval,
                    StackCount = 1
                });
            }

            TriggerApplySignal(effectDefinition);
            _feedbackPublisher?.PublishStatusApplied(effectDefinition, source);
            RecomputeAggregates();
        }

        public bool TryModifyIncomingDamage(GameObject source, ref float incomingDamage)
        {
            float shieldReduction = statusProfile != null ? statusProfile.defaultShieldDamageReduction : 0f;
            bool hasShield = false;
            float armorBreakMultiplier = 1f;

            for (int i = 0; i < _activeEffects.Count; i++)
            {
                ActiveStatusEffect effect = _activeEffects[i];
                if (effect.Definition == null)
                    continue;

                if (effect.Definition.effectKind == StatusEffectKind.Shield)
                {
                    hasShield = true;
                    shieldReduction = Mathf.Max(shieldReduction, GetEffectMagnitude(effect));
                }
                else if (effect.Definition.effectKind == StatusEffectKind.ArmorBreak)
                {
                    armorBreakMultiplier = Mathf.Max(armorBreakMultiplier, GetEffectMagnitude(effect));
                }
            }

            if (!hasShield && armorBreakMultiplier <= 1f)
                return false;

            if (hasShield)
                incomingDamage *= 1f - Mathf.Clamp01(shieldReduction);

            incomingDamage *= Mathf.Max(armorBreakMultiplier, 0f);
            return true;
        }

        private ActiveStatusEffect FindEffect(string effectId)
        {
            if (string.IsNullOrWhiteSpace(effectId))
                return null;

            for (int i = 0; i < _activeEffects.Count; i++)
            {
                if (_activeEffects[i].Definition != null && _activeEffects[i].Definition.effectId == effectId)
                    return _activeEffects[i];
            }

            return null;
        }

        private void RecomputeAggregates()
        {
            float moveMultiplier = 1f;
            float damageMultiplier = 1f;
            float knockbackMultiplier = 1f;
            float incomingDamageMultiplier = 1f;
            float regenRateMultiplier = 1f;
            bool actionLocked = false;

            for (int i = 0; i < _activeEffects.Count; i++)
            {
                StatusEffectDefinition definition = _activeEffects[i].Definition;
                if (definition == null)
                    continue;

                float magnitude = GetEffectMagnitude(_activeEffects[i]);

                switch (definition.effectKind)
                {
                    case StatusEffectKind.Stun:
                        actionLocked = true;
                        break;
                    case StatusEffectKind.Slow:
                        moveMultiplier = Mathf.Min(moveMultiplier, Mathf.Clamp01(magnitude));
                        break;
                    case StatusEffectKind.SpeedBoost:
                        moveMultiplier = Mathf.Max(moveMultiplier, Mathf.Max(magnitude, 0f));
                        break;
                    case StatusEffectKind.DamageBoost:
                        damageMultiplier *= Mathf.Max(magnitude, 0f);
                        break;
                    case StatusEffectKind.KnockbackBoost:
                        knockbackMultiplier *= Mathf.Max(magnitude, 0f);
                        break;
                    case StatusEffectKind.Armor:
                        incomingDamageMultiplier *= 1f - Mathf.Clamp01(magnitude);
                        break;
                    case StatusEffectKind.RegenBoost:
                        regenRateMultiplier *= Mathf.Max(magnitude, 0f);
                        break;
                }
            }

            _movementReceiver?.SetStatusMoveSpeedMultiplier(moveMultiplier);
            _movementReceiver?.SetStatusActionLock(actionLocked);
            _combatReceiver?.SetOutgoingDamageMultiplier(damageMultiplier);
            _combatReceiver?.SetOutgoingKnockbackMultiplier(knockbackMultiplier);
            _healthModifierReceiver?.SetIncomingDamageMultiplier(incomingDamageMultiplier);
            _healthModifierReceiver?.SetRegenRateMultiplier(regenRateMultiplier);
        }

        private void TriggerApplySignal(StatusEffectDefinition definition)
        {
            if (_context == null || definition == null)
                return;

            if (definition.applySignal == ActorAnimationSignal.Custom && !string.IsNullOrWhiteSpace(definition.customAnimationKey))
                _context.Animation?.TriggerCustom(definition.customAnimationKey);
            else
                _context.Animation?.TriggerSignal(definition.applySignal);
        }

        private void ApplyExistingEffectStrategy(ActiveStatusEffect existing, StatusEffectDefinition definition, GameObject source)
        {
            existing.Definition = definition;
            existing.Source = source;

            switch (definition.stackMode)
            {
                case StatusEffectStackMode.Ignore:
                    if (statusProfile != null && statusProfile.allowRefreshExistingEffects)
                    {
                        existing.RemainingDuration = definition.duration;
                        existing.TickTimer = definition.tickInterval;
                    }
                    break;

                case StatusEffectStackMode.RefreshDuration:
                    existing.RemainingDuration = definition.duration;
                    existing.TickTimer = definition.tickInterval;
                    break;

                case StatusEffectStackMode.StackDuration:
                    existing.StackCount = Mathf.Min(existing.StackCount + 1, definition.maxStacks);
                    existing.RemainingDuration += definition.duration;
                    existing.TickTimer = definition.tickInterval;
                    break;

                case StatusEffectStackMode.StackMagnitude:
                    existing.StackCount = Mathf.Min(existing.StackCount + 1, definition.maxStacks);
                    existing.RemainingDuration = definition.duration;
                    existing.TickTimer = definition.tickInterval;
                    break;
            }
        }

        private static float GetEffectMagnitude(ActiveStatusEffect effect)
        {
            if (effect?.Definition == null)
                return 0f;

            float magnitude = effect.Definition.magnitude;
            if (effect.Definition.stackMode == StatusEffectStackMode.StackMagnitude)
                magnitude *= Mathf.Max(1, effect.StackCount);

            return magnitude;
        }
    }
}
