using UnityEngine;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    [AuthoringContract(
        Category = "Animation",
        CapabilityPath = "Presentation/Feedback/Enemy Animation Module",
        Surface = AuthoringSurface.Goal,
        Summary = "Binds enemy gameplay states to visual signals and animator triggers.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/visuals",
        SuccessChecks = new[] { "Verify enemy plays walk, hurt, and attack animations." },
        Tags = new[] { "capability:Animation" }
    )]
    public class EnemyAnimationModule : MonoBehaviour
{
        private Animator _animator;
        private IActorAnimationController _animationDriver;

        private static readonly int H_IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int H_Grounded = Animator.StringToHash("IsGrounded");
        private static readonly int H_Death = Animator.StringToHash("Death");
        private static readonly int H_Hit = Animator.StringToHash("Hit");

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _animationDriver = GetComponent<IActorAnimationController>();
        }

        public void UpdateMovement(bool isMoving, bool isGrounded)
        {
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.Move, isMoving);
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.Idle, !isMoving);
            
            if (_animator != null)
            {
                _animator.SetBool(H_IsMoving, isMoving);
                _animator.SetBool(H_Grounded, isGrounded);
            }
        }

        public void TriggerDeath()
        {
            _animationDriver?.TriggerSignal(ActorAnimationSignal.Death);
            _animator?.SetTrigger(H_Death);
        }

        public void TriggerHurt()
        {
            _animationDriver?.TriggerSignal(ActorAnimationSignal.Hurt);
            _animator?.SetTrigger(H_Hit);
        }

    }
}
