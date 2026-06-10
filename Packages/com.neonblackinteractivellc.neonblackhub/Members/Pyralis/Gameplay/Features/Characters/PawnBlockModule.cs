using UnityEngine;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Characters;

namespace NeonBlack.Gameplay.Features.Characters
{
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