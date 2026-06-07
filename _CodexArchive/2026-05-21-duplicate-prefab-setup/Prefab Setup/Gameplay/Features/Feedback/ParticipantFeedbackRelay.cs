using NeonBlack.Gameplay.Characters;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Features.Feedback
{
    [AddComponentMenu("NeonBlack/Gameplay/Feedback/Participant Feedback Relay")]
    public class ParticipantFeedbackRelay : MonoBehaviour, IActorFeedbackReceiver
    {
        private IParticipantFeedbackPublisher _publisher;
        [Inject]
        private void Construct(IParticipantFeedbackPublisher publisher = null)
        {
            _publisher = publisher;
        }

        public void HandleFeedbackEvent(ActorFeedbackEvent feedbackEvent)
        {
            if (!ParticipantQueryUtility.TryResolveParticipant(gameObject, out ParticipantHandle participant))
                return;

            if (_publisher == null)
                return;

            switch (feedbackEvent.EventType)
            {
                case ActorFeedbackEventType.Damage:
                    _publisher.Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Damage, floatValue: feedbackEvent.FloatValue));
                    break;
                case ActorFeedbackEventType.Heal:
                    _publisher.Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Heal, floatValue: feedbackEvent.FloatValue));
                    break;
                case ActorFeedbackEventType.Score:
                    _publisher.Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Score, intValue: feedbackEvent.IntValue));
                    break;
                case ActorFeedbackEventType.Combo:
                    _publisher.Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Combo, intValue: feedbackEvent.IntValue));
                    break;
                case ActorFeedbackEventType.Parry:
                    PublishCombatAlert(participant, "Parry");
                    break;
                case ActorFeedbackEventType.StatusApplied:
                    _publisher.Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Status, textValue: feedbackEvent.StringValue));
                    break;
                case ActorFeedbackEventType.Stagger:
                    PublishCombatAlert(participant, "Stagger");
                    break;
                case ActorFeedbackEventType.GuardBreak:
                    PublishCombatAlert(participant, "GuardBreak");
                    break;
                case ActorFeedbackEventType.Finisher:
                    PublishCombatAlert(participant, "Finisher", feedbackEvent.IntValue);
                    break;
            }
        }

        private void PublishCombatAlert(ParticipantHandle participant, string alertKey, int value = 0)
        {
            _publisher?.Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.CombatAlert, intValue: value, textValue: alertKey));
        }
    }
}
