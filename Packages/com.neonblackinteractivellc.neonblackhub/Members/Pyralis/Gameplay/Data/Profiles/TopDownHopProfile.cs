using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Tuning for top-down or isometric hop actions where the actor remains on the map plane
    /// while its visual presentation lifts on an arc.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Top Down Hop Profile", fileName = "TopDownHopProfile")]
    public class TopDownHopProfile : ScriptableObject
    {
        public GameplayInputActionRole actionRole = GameplayInputActionRole.Jump;
        [Min(0.01f)] public float duration = 0.35f;
        [Min(0f)] public float height = 0.75f;
        [Min(0f)] public float cooldown = 0.15f;
        [Tooltip("When enabled, another hop request before landing restarts the hop arc.")]
        public bool allowRestartWhileHopping = false;
        [Tooltip("Trigger ActorAnimationSignal.Jump when the hop starts if the actor has an ActorAnimationDriver.")]
        public bool triggerJumpAnimation = true;

        public void Sanitize()
        {
            duration = Mathf.Max(0.01f, duration);
            height = Mathf.Max(0f, height);
            cooldown = Mathf.Max(0f, cooldown);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
