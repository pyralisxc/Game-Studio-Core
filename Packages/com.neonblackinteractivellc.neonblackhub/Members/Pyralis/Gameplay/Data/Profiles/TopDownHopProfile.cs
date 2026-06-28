using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Tuning for top-down or isometric hop actions where the actor remains on the map plane
    /// while its visual presentation lifts on an arc.
    /// </summary>
    [AuthoringContract(
        Category = "Movement, Traversal",
        CapabilityPath = "Movement/Traversal/FakeGravityJump",
        Surface = AuthoringSurface.Profile,
        Summary = "Tuning asset for fake-gravity visual jumps where the pawn sprite or visual child arcs without changing collider position.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/movement",
        RequiredFields = new[] { nameof(actionRole), nameof(duration), nameof(height), nameof(cooldown) },
        SuccessChecks = new[] { "Perform a hop in a top-down scene and verify the shadow stays on the ground while the sprite arcs up." },
        RoleTags = new[] { "VisualHop", "FakeGravityJump", "JumpProfile" },
        Tags = new[] { "capability:Movement", "capability:Traversal", "runtime:CharacterPawnGameplay", "axiom:Dimensions2D", "axiom:GravityNone", "axiom:Realtime" },
        Selectable = false
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
