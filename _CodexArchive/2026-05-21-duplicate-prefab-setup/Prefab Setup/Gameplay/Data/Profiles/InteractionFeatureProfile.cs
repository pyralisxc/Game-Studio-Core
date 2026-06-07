using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Profiles/Interaction Feature Profile", fileName = "InteractionFeatureProfile")]
    public class InteractionFeatureProfile : ScriptableObject
    {
        public bool enableInteraction = true;
        public float interactionCooldown = 0.1f;
        public bool triggerInteractAnimationWhenUnhandled = true;

        public void Sanitize()
        {
            interactionCooldown = Mathf.Max(0f, interactionCooldown);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
