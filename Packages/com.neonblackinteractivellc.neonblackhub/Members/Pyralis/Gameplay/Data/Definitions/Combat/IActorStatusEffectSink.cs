using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Combat
{
    public interface IActorStatusEffectSink
    {
        void ApplyStatusEffect(StatusEffectDefinition effectDefinition, GameObject source = null);
    }
}
