using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Feedback
{
    [AddComponentMenu("NeonBlack/Gameplay/Feedback/Actor Feedback Component")]
    [AuthoringContract(
        StableId = "feature.actor.feedback",
        Category = "V F X",
        Surface = AuthoringSurface.Profile,
        Summary = "Listens to actor health and publishes damage, heal, death, status, score, combo, parry, stagger, guard-break, and finisher events to feedback receivers.",
        RequiredFields = new[] { nameof(feedbackProfile) },
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Modules.Combat.HealthComponent" },
        RequiredInterfaces = new[] { typeof(IActorFeedbackPublisher) },
        RequiredInterfaceNames = new[] { "NeonBlack.Gameplay.Modules.Feedback.IActorFeedbackReceiver" },
        SetupSteps = new[]
        {
            "Create ActorFeedbackProfile.",
            "Add ActorFeedbackComponent to the actor root.",
            "Assign ActorFeedbackProfile.",
            "Add at least one IActorFeedbackReceiver in the actor hierarchy."
        },
        SuccessChecks = new[] { "Trigger a damage event and verify visual feedback (flash, popup) occurs." },
        Tags = new[] { "capability:VFX", "lane:Feedback" },
        Selectable = false
    )]
    public class ActorFeedbackComponent : MonoBehaviour, IActorFeedbackPublisher
    {
        [SerializeField] private ActorFeedbackProfile feedbackProfile;
        private IActorHealthState _health;

        private void Awake()
        {
            _health = GetComponent<IActorHealthState>();

            if (_health != null)
            {
                _health.Damaged += HandleDamaged;
                _health.Healed += HandleHealed;
                _health.Died += HandleDeath;
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.Damaged -= HandleDamaged;
                _health.Healed -= HandleHealed;
                _health.Died -= HandleDeath;
            }

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

        public void PublishStatusApplied(string statusName, float magnitude = 0f, GameObject source = null)
        {
            if (feedbackProfile != null && !feedbackProfile.publishStatusEvents)
                return;

            Dispatch(new ActorFeedbackEvent(
                ActorFeedbackEventType.StatusApplied,
                floatValue: magnitude,
                stringValue: statusName ?? string.Empty,
                source: source));
        }

        public void PublishStatusApplied(StatusEffectDefinition effectDefinition, GameObject source = null)
        {
            PublishStatusApplied(
                effectDefinition != null ? effectDefinition.displayName : string.Empty,
                effectDefinition != null ? effectDefinition.magnitude : 0f,
                source);
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
            IActorFeedbackReceiver[] receivers = GetComponentsInChildren<IActorFeedbackReceiver>(true);
            for (int i = 0; i < receivers.Length; i++)
                receivers[i]?.HandleFeedbackEvent(feedbackEvent);
        }
    }
}
