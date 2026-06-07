using NeonBlack.Gameplay.Features.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Feedback
{
    public readonly struct ActorFeedbackEvent
    {
        public ActorFeedbackEventType EventType { get; }
        public float FloatValue { get; }
        public int IntValue { get; }
        public string StringValue { get; }
        public GameObject Source { get; }
        public StatusEffectDefinition StatusEffect { get; }

        public ActorFeedbackEvent(
            ActorFeedbackEventType eventType,
            float floatValue = 0f,
            int intValue = 0,
            string stringValue = null,
            GameObject source = null,
            StatusEffectDefinition statusEffect = null)
        {
            EventType = eventType;
            FloatValue = floatValue;
            IntValue = intValue;
            StringValue = stringValue;
            Source = source;
            StatusEffect = statusEffect;
        }
    }
}
