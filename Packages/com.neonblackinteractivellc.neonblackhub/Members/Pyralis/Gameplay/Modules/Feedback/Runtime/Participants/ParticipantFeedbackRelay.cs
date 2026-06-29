using NeonBlack.Gameplay.Data.Participants;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Feedback
{
    [AddComponentMenu("NeonBlack/Gameplay/Feedback/Participant Feedback Relay")]
    public class ParticipantFeedbackRelay : MonoBehaviour, IActorFeedbackReceiver
    {
        private IParticipantFeedbackPublisher _publisher;

        public void ConfigureRuntime(IParticipantFeedbackPublisher publisher)
        {
            if (publisher != null)
                _publisher = publisher;
        }

        public void HandleFeedbackEvent(ActorFeedbackEvent feedbackEvent)
        {
            if (!ParticipantQueryUtility.TryResolveParticipant(gameObject, out ParticipantHandle participant))
                return;

            IParticipantFeedbackPublisher publisher = ResolvePublisher();
            if (publisher == null)
                return;

            switch (feedbackEvent.EventType)
            {
                case ActorFeedbackEventType.Damage:
                    publisher.Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Damage, floatValue: feedbackEvent.FloatValue));
                    break;
                case ActorFeedbackEventType.Heal:
                    publisher.Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Heal, floatValue: feedbackEvent.FloatValue));
                    break;
                case ActorFeedbackEventType.Score:
                    publisher.Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Score, intValue: feedbackEvent.IntValue));
                    break;
                case ActorFeedbackEventType.Combo:
                    publisher.Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Combo, intValue: feedbackEvent.IntValue));
                    break;
                case ActorFeedbackEventType.Parry:
                    PublishCombatAlert(participant, "Parry");
                    break;
                case ActorFeedbackEventType.StatusApplied:
                    publisher.Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Status, textValue: feedbackEvent.StringValue));
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
            ResolvePublisher()?.Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.CombatAlert, intValue: value, textValue: alertKey));
        }

        private IParticipantFeedbackPublisher ResolvePublisher()
        {
            return _publisher;
        }
    }
}
