using NeonBlack.Gameplay.Features.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Composition
{
    public interface IActorFeedbackPublisher
    {
        void PublishDamage(float amount, GameObject source = null);
        void PublishHeal(float amount, GameObject source = null);
        void PublishDeath();
        void PublishStatusApplied(StatusEffectDefinition effectDefinition, GameObject source = null);
        void PublishScore(int amount);
        void PublishCombo(int comboStep);
        void PublishParry();
        void PublishStagger(float intensity = 0f);
        void PublishGuardBreak();
        void PublishFinisher(int comboStep);
    }
}
