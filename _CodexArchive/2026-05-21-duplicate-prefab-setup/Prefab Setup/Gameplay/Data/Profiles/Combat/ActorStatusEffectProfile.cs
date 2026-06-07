using NeonBlack.Gameplay.Features.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Profiles/Actor Status Effect Profile", fileName = "ActorStatusEffectProfile")]
    public class ActorStatusEffectProfile : ScriptableObject
    {
        public StatusEffectDefinition[] startingEffects;
        public bool allowRefreshExistingEffects = true;
        [Range(0f, 1f)] public float defaultShieldDamageReduction = 0.5f;

        public void Sanitize()
        {
            defaultShieldDamageReduction = Mathf.Clamp01(defaultShieldDamageReduction);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
