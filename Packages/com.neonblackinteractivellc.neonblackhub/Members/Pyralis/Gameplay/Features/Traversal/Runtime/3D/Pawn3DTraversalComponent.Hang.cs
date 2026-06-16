using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal
{
    public sealed partial class Pawn3DTraversalComponent
    {
        public bool HandleHangFrame(FrameInput frameInput)
        {
            if (!EnsureDependencies())
                return false;

            if (!_movement.State.IsHanging)
                return false;

            if (_hangZone == null)
            {
                ExitHang();
                return false;
            }

            bool climbPressed = _movement.State.JumpBufferCounter > 0f || frameInput.Move.y > 0.5f;
            if (climbPressed)
            {
                IClimbZone zone = _hangZone;
                ExitHang();
                PerformClimb(zone);
                return true;
            }

            bool dropPressed = _movement.State.IsCrouching || frameInput.Move.y < -0.5f;
            if (dropPressed)
            {
                DropFromHang();
                return true;
            }

            _shimmyVelocityX = 0f;
            if (_hangZone.ShimmyWidth > 0f && Mathf.Abs(frameInput.Move.x) > 0.1f)
            {
                float shimmy = frameInput.Move.x * _hangZone.ShimmySpeed;
                float halfWidth = _hangZone.ShimmyWidth * 0.5f;
                float minX = _hangZone.WorldPosition.x - halfWidth;
                float maxX = _hangZone.WorldPosition.x + halfWidth;
                float nextX = transform.position.x + shimmy * Time.deltaTime;
                if (nextX < minX || nextX > maxX)
                    shimmy = 0f;

                _shimmyVelocityX = shimmy;
            }

            _controller.Move(new Vector3(_shimmyVelocityX, 0f, 0f) * Time.deltaTime);
            _animationDriver?.SetFloatSignal(ActorAnimationSignal.Shimmy, _shimmyVelocityX);
            return true;
        }

        private void StartHang(IClimbZone zone)
        {
            if (!EnsureDependencies())
                return;

            if (!allowHang || zone == null || _movement.State.IsHanging || _movement.State.IsClimbing)
                return;

            _hangZone = zone;
            _shimmyVelocityX = 0f;
            _movement.NotifyHangStart();
            zone.DisableTemporarily();
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.Hang, true);
        }

        private void DropFromHang()
        {
            _animationDriver?.TriggerSignal(ActorAnimationSignal.LedgeDrop);
            IClimbZone zone = _hangZone;
            ExitHang();
            _movement?.SetVelocityY(-1f);
            zone?.EnableAfterClimb();
        }

        private void ExitHang()
        {
            _movement?.NotifyHangEnd();
            _shimmyVelocityX = 0f;
            _hangZone = null;
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.Hang, false);
        }
    }
}
