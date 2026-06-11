using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Tuning for top-down or isometric hop actions where the actor remains on the map plane
    /// while its visual presentation lifts on an arc.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Movement | AuthoringCapability.Traversal,
        Relevance = "Tuning asset for top-down 'visual' hops (Z-axis simulation).",
        AssignmentFields = new[] { nameof(actionRole), nameof(duration), nameof(height), nameof(cooldown) },
        FirstProof = "Perform a hop in a top-down scene and verify the shadow stays on the ground while the sprite arcs up.",
        ExpertAdvice = "This is a purely visual/presentation hop. It does not change the physical collider height. Best used for 'Jump' actions in isometric RPGs.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/movement"
    )]
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
