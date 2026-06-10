using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Feedback
{
    [AddComponentMenu("NeonBlack/Gameplay/Feedback/Actor Feedback Feature Runtime")]
    [AuthoringContract(
        ModuleId = "actor.feedback",
        Capability = AuthoringCapability.VFX,
        Relevance = "Runtime implementation for actor feedback events, bridging health state to visual receivers.",
        Lane = "Feedback",
        ProfileType = typeof(ActorFeedbackProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IActorFeedbackPublisher) },
        RequiredInterfaceNames = new[] { "NeonBlack.Gameplay.Features.Feedback.IActorFeedbackReceiver" },
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Features.Combat.HealthComponent" },
        NativeSetup = new[]
        {
            "create ActorFeedbackProfile",
            "create FeatureModuleDefinition",
            "assign runtime prefab with ActorFeedbackFeatureRuntime",
            "assign profile asset",
            "add module to PawnDefinition.featureModules"
        },
        FirstProof = "Trigger a damage event and verify visual feedback (flash, popup) occurs.",
        AssignmentFields = new[]
        {
            "FeatureModuleDefinition.moduleId",
            "FeatureModuleDefinition.runtimePrefab",
            "FeatureModuleDefinition.profileAsset"
        },
        CustomizationMoments = new[]
        {
            "ActorFeedbackProfile.publishDamageEvents",
            "ActorFeedbackProfile.publishScoreEvents"
        }
    )]
    public class ActorFeedbackFeatureRuntime : MonoBehaviour, IFeatureModuleRuntime, IActorFeedbackPublisher
{
        [SerializeField] private ActorFeedbackProfile feedbackProfile;
        private ActorFeatureContext _context;
        private IActorHealthState _health;

        public string ModuleId => "actor.feedback";

        public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext)
        {
            ActorFeatureContext context = initializationContext != null ? initializationContext.ActorContext : null;
            FeatureModuleDefinition definition = initializationContext != null ? initializationContext.Definition : null;
            _context = context;
            _health = context != null ? context.Health : null;
            feedbackProfile = initializationContext != null
                ? initializationContext.GetProfile<ActorFeedbackProfile>(definition != null ? definition.profileAsset : null)
                : null;

            if (_health != null)
            {
                _health.Damaged += HandleDamaged;
                _health.Healed += HandleHealed;
                _health.Died += HandleDeath;
            }
        }

        public void ShutdownFeature()
        {
            if (_health != null)
            {
                _health.Damaged -= HandleDamaged;
                _health.Healed -= HandleHealed;
                _health.Died -= HandleDeath;
            }

            _context = null;
            _health = null;
        }

        public void PublishDamage(float amount, GameObject source = null)
        {
            if (feedbackProfile != null && !feedbackProfile.publishDamageEvents)
                return;

            Dispatch(new ActorFeedbackEvent(ActorFeedbackEventType.Damage, floatValue: amount, source: source));
        }

        public void PublishHeal(float amount, GameObject source = null)
        {
            if (feedbackProfile != null && !feedbackProfile.publishHealingEvents)
                return;

            Dispatch(new ActorFeedbackEvent(ActorFeedbackEventType.Heal, floatValue: amount, source: source));
        }

        public void PublishDeath()
        {
            if (feedbackProfile != null && !feedbackProfile.publishDeathEvents)
                return;

            Dispatch(new ActorFeedbackEvent(ActorFeedbackEventType.Death));
        }

        public void PublishStatusApplied(StatusEffectDefinition effectDefinition, GameObject source = null)
        {
            if (feedbackProfile != null && !feedbackProfile.publishStatusEvents)
                return;

            Dispatch(new ActorFeedbackEvent(
                ActorFeedbackEventType.StatusApplied,
                floatValue: effectDefinition != null ? effectDefinition.magnitude : 0f,
                stringValue: effectDefinition != null ? effectDefinition.displayName : string.Empty,
                source: source,
                statusEffect: effectDefinition));
        }

        public void PublishScore(int amount)
        {
            if (feedbackProfile != null && !feedbackProfile.publishScoreEvents)
                return;

            Dispatch(new ActorFeedbackEvent(ActorFeedbackEventType.Score, intValue: amount));
        }

        public void PublishCombo(int comboStep)
        {
            if (feedbackProfile != null && !feedbackProfile.publishComboEvents)
                return;

            Dispatch(new ActorFeedbackEvent(ActorFeedbackEventType.Combo, intValue: comboStep));
        }

        public void PublishParry()
        {
            Dispatch(new ActorFeedbackEvent(ActorFeedbackEventType.Parry));
        }

        public void PublishStagger(float intensity = 0f)
        {
            Dispatch(new ActorFeedbackEvent(ActorFeedbackEventType.Stagger, floatValue: intensity));
        }

        public void PublishGuardBreak()
        {
            Dispatch(new ActorFeedbackEvent(ActorFeedbackEventType.GuardBreak));
        }

        public void PublishFinisher(int comboStep)
        {
            Dispatch(new ActorFeedbackEvent(ActorFeedbackEventType.Finisher, intValue: comboStep));
        }

        private void HandleDamaged(float amount)
        {
            PublishDamage(amount);
        }

        private void HandleHealed(float amount)
        {
            PublishHeal(amount);
        }

        private void HandleDeath()
        {
            PublishDeath();
        }

        private void Dispatch(ActorFeedbackEvent feedbackEvent)
        {
            if (_context == null || _context.ActorObject == null)
                return;

            IActorFeedbackReceiver[] receivers = _context.ActorObject.GetComponentsInChildren<IActorFeedbackReceiver>(true);
            for (int i = 0; i < receivers.Length; i++)
                receivers[i]?.HandleFeedbackEvent(feedbackEvent);
        }
    }
}
