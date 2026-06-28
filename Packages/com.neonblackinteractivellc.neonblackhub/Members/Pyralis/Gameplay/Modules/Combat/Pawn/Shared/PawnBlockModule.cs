using UnityEngine;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Combat
{
    [AuthoringContract(
        Category = "Combat, Tactics Defensive",
        CapabilityPath = "Combat/Actions/Pawn Block Module",
        Surface = AuthoringSurface.Goal,
        Summary = "Pawn module for blocking and damage reduction.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/combat",
        RequiredFields = new[] { nameof(blockDamageReduction), nameof(blockFrontalAngle) },
        SuccessChecks = new[] { "Hold the block button and verify damage from the front is reduced." },
        Tags = new[] { "capability:Combat", "capability:TacticsDefensive" }
    )]
    public class PawnBlockModule : MonoBehaviour
{
        [Header("Block Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float blockDamageReduction = 0.2f;
        [Range(10f, 180f)]
        [SerializeField] private float blockFrontalAngle = 90f;

        private ActorAnimationDriver _animationDriver;
        private IActorCombatMovementState _motor;
        private bool _isBlocking;

        public bool IsBlocking => _isBlocking;
        public float BlockDamageReduction => blockDamageReduction;
        public float BlockFrontalAngle => blockFrontalAngle;

        private void Awake()
        {
            _animationDriver = GetComponent<ActorAnimationDriver>();
            _motor = GetComponent<IActorCombatMovementState>();
        }

        public void HandleBlockStart()
        {
            if (_motor != null && _motor.IsActing)
                return;

            _isBlocking = true;
            _animationDriver?.TriggerSignal(ActorAnimationSignal.BlockStart);
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.BlockLoop, true);
        }

        public void HandleBlockEnd()
        {
            _isBlocking = false;
            _animationDriver?.TriggerSignal(ActorAnimationSignal.BlockEnd);
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.BlockLoop, false);
        }

        public void Tick()
        {
             _animationDriver?.SetBoolSignal(ActorAnimationSignal.BlockLoop, _isBlocking);
        }
    }
}
