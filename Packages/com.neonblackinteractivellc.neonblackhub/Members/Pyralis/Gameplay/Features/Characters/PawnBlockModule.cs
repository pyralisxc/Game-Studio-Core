using UnityEngine;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Features.Characters
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.TacticsDefensive,
        Relevance = "Pawn module for blocking and damage reduction.",
        AssignmentFields = new[] { nameof(blockDamageReduction), nameof(blockFrontalAngle) },
        FirstProof = "Hold the block button and verify damage from the front is reduced.",
        ExpertAdvice = "Block frontal angle defines the 'safe zone' for incoming damage. 90 degrees covers the entire forward hemisphere.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat"
    )]
    public class PawnBlockModule : MonoBehaviour
{
        [Header("Block Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float blockDamageReduction = 0.2f;
        [Range(10f, 180f)]
        [SerializeField] private float blockFrontalAngle = 90f;

        private ActorAnimationDriver _animationDriver;
        private ICharacterMotorState _motor;
        private bool _isBlocking;

        public bool IsBlocking => _isBlocking;
        public float BlockDamageReduction => blockDamageReduction;
        public float BlockFrontalAngle => blockFrontalAngle;

        private void Awake()
        {
            _animationDriver = GetComponent<ActorAnimationDriver>();
            _motor = GetComponent<ICharacterMotorState>();
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