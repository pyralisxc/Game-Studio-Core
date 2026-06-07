using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Characters;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace NeonBlack.Gameplay.Features.Feedback
{
    [DefaultExecutionOrder(-25)]
    [AddComponentMenu("NeonBlack/Gameplay/Feedback/Participant Feedback Service")]
    public class ParticipantFeedbackService : MonoBehaviour, IGameService, IParticipantFeedbackStream, IParticipantFeedbackPublisher
    {
        [System.Serializable]
        public sealed class ParticipantIntEvent : UnityEvent<ParticipantHandle, int> { }

        [System.Serializable]
        public sealed class ParticipantFloatEvent : UnityEvent<ParticipantHandle, float> { }

        [System.Serializable]
        public sealed class ParticipantStatusEvent : UnityEvent<ParticipantHandle, string> { }

        [System.Serializable]
        public sealed class ParticipantAlertEvent : UnityEvent<ParticipantHandle, string, int> { }

        public ParticipantIntEvent OnParticipantScorePopup = new ParticipantIntEvent();
        public ParticipantIntEvent OnParticipantComboPopup = new ParticipantIntEvent();
        public ParticipantFloatEvent OnParticipantDamageFeedback = new ParticipantFloatEvent();
        public ParticipantFloatEvent OnParticipantHealFeedback = new ParticipantFloatEvent();
        public ParticipantStatusEvent OnParticipantStatusFeedback = new ParticipantStatusEvent();
        public ParticipantAlertEvent OnParticipantCombatAlert = new ParticipantAlertEvent();
        public event Action<ParticipantFeedbackMessage> FeedbackPublished;

        public void Initialize() { }
        public void Shutdown() { }

        public void Publish(ParticipantFeedbackMessage message)
        {
            if (message.Participant == null)
                return;

            FeedbackPublished?.Invoke(message);

            switch (message.Kind)
            {
                case ParticipantFeedbackKind.Score:
                    OnParticipantScorePopup?.Invoke(message.Participant, message.IntValue);
                    break;
                case ParticipantFeedbackKind.Combo:
                    OnParticipantComboPopup?.Invoke(message.Participant, message.IntValue);
                    break;
                case ParticipantFeedbackKind.Damage:
                    OnParticipantDamageFeedback?.Invoke(message.Participant, message.FloatValue);
                    break;
                case ParticipantFeedbackKind.Heal:
                    OnParticipantHealFeedback?.Invoke(message.Participant, message.FloatValue);
                    break;
                case ParticipantFeedbackKind.Status:
                    OnParticipantStatusFeedback?.Invoke(message.Participant, message.TextValue);
                    break;
                case ParticipantFeedbackKind.CombatAlert:
                    OnParticipantCombatAlert?.Invoke(message.Participant, message.TextValue, message.IntValue);
                    break;
            }
        }

        public void PublishScore(ParticipantHandle participant, int amount)
        {
            Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Score, intValue: amount));
        }

        public void PublishCombo(ParticipantHandle participant, int comboStep)
        {
            Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Combo, intValue: comboStep));
        }

        public void PublishDamage(ParticipantHandle participant, float amount)
        {
            Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Damage, floatValue: amount));
        }

        public void PublishHeal(ParticipantHandle participant, float amount)
        {
            Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Heal, floatValue: amount));
        }

        public void PublishStatus(ParticipantHandle participant, string effectId)
        {
            Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.Status, textValue: effectId));
        }

        public void PublishCombatAlert(ParticipantHandle participant, string alertKey, int value = 0)
        {
            Publish(new ParticipantFeedbackMessage(participant, ParticipantFeedbackKind.CombatAlert, intValue: value, textValue: alertKey));
        }
    }
}
