using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    public interface IActorStatusEffectReceiver
    {
        void ApplyStatusEffect(StatusEffectDefinition effectDefinition, GameObject source = null);
    }
}
