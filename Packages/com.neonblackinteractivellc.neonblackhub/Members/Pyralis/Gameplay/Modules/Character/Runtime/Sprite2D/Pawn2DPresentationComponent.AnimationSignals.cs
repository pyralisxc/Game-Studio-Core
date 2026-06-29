using NeonBlack.Gameplay.Core.Types.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    public sealed partial class Pawn2DPresentationComponent
    {
        private void TickAnimationSignalLane()
        {
            if (animationDriver == null || movement == null)
                return;

            bool moving = IsMovingForPresentation();
            animationDriver.SetBoolSignal(ActorAnimationSignal.Move, moving);
            animationDriver.SetBoolSignal(ActorAnimationSignal.Idle, !moving);
            animationDriver.SetBoolSignal(ActorAnimationSignal.Dash, movement.IsDashing);
            ApplyBlendTreeChannels();
        }

        private void ApplyBlendTreeChannels()
        {
            Vector2 velocity = movement.CurrentVelocity;
            float speed = velocity.magnitude;
            float normalizedSpeed = Mathf.Clamp01(speed / Mathf.Max(0.01f, movement.MoveSpeed));

            animationDriver.SetFloatCustom("Speed", speed);
            animationDriver.SetFloatCustom("NormalizedSpeed", normalizedSpeed);
            animationDriver.SetFloatCustom("MoveX", movement.MoveDirection.x);
            animationDriver.SetFloatCustom("MoveY", movement.MoveDirection.y);
            animationDriver.SetFloatCustom("VelocityX", velocity.x);
            animationDriver.SetFloatCustom("VelocityY", velocity.y);
        }
    }
}
