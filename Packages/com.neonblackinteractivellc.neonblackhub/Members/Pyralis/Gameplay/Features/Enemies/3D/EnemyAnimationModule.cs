using System.Collections.Generic;
using UnityEngine;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Features.Enemies
{
    [AuthoringContract(
        Capability = AuthoringCapability.Animation,
        Relevance = "Binds enemy gameplay states to visual signals and animator triggers.",
        FirstProof = "Verify enemy plays walk, hurt, and attack animations.",
        ExpertAdvice = "Ensure the child Animator has 'IsMoving' and 'IsGrounded' parameters.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/visuals"
    )]
    public class EnemyAnimationModule : MonoBehaviour
{
        private Animator _animator;
        private ActorAnimationDriver _animationDriver;

        private static readonly int H_IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int H_Grounded = Animator.StringToHash("IsGrounded");
        private static readonly int H_Death = Animator.StringToHash("Death");
        private static readonly int H_Hit = Animator.StringToHash("Hit");

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _animationDriver = GetComponent<ActorAnimationDriver>();
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

        public void TriggerAttack(EnemyAttack atk, Dictionary<EnemyAttack, int> attackTriggerHashes)
        {
             if (atk == null) return;

            if (_animationDriver != null)
            {
                int step = Mathf.Max(atk.animationStep, 1);
                if (atk.useCustomAnimationKey && !string.IsNullOrWhiteSpace(atk.customAnimationKey))
                    _animationDriver.TriggerCustom(atk.customAnimationKey, intValue: step);
                else
                {
                    _animationDriver.SetIntSignal(atk.animationSignal, step);
                    _animationDriver.TriggerSignal(atk.animationSignal, intValue: step);
                }
            }

            if (!string.IsNullOrEmpty(atk.animatorTrigger) && attackTriggerHashes.TryGetValue(atk, out int hash))
                _animator?.SetTrigger(hash);
        }
    }
}