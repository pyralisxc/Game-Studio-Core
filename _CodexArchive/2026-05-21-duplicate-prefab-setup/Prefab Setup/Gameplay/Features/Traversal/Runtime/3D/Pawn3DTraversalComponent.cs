using System.Collections;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal
{
    [AddComponentMenu("NeonBlack/Gameplay/3D/Pawn 3D Traversal Component")]
    public sealed class Pawn3DTraversalComponent : MonoBehaviour, IPawnTraversalModule
    {
        [Header("Climb")]
        [SerializeField] private float climbCooldown = 1.2f;
        [SerializeField] private LedgeProbe3D ledgeProbe = new LedgeProbe3D();

        private Pawn3DMovementComponent _movement;
        private CharacterController _controller;
        private ActorAnimationDriver _animationDriver;
        private IClimbZone _currentClimbZone;
        private IClimbZone _hangZone;
        private float _shimmyVelocityX;

        public float ShimmyVelocityX => _shimmyVelocityX;

        private void Awake()
        {
            _movement = GetComponent<Pawn3DMovementComponent>();
            _controller = GetComponent<CharacterController>();
            _animationDriver = GetComponent<ActorAnimationDriver>();
        }

        public void ProbeLedge()
        {
            var state = _movement.State;
            if (state.IsClimbing || state.IsHanging || state.ClimbTimer > 0f)
                return;

            IClimbZone found = ledgeProbe?.FindClimbZone(transform, state.VelocityY);
            if (found != null)
            {
                _currentClimbZone = found;
                if (found.AutoGrab && !state.IsGrounded && state.VelocityY <= found.MaxGrabVelocityY)
                {
                    if (found.HangOnGrab)
                        StartHang(found);
                    else
                        PerformClimb(found);
                }
            }
            else
            {
                _currentClimbZone = null;
            }
        }

        public bool HandleHangFrame(FrameInput frameInput)
        {
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
                ExitHang();
                PerformClimb(_hangZone);
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

        public void HandleInteract()
        {
            if (!TryHandleTraversalInteraction())
                _animationDriver?.TriggerSignal(ActorAnimationSignal.Interact);
        }

        public bool TryHandleTraversalInteraction()
        {
            if (_currentClimbZone != null)
            {
                if (_currentClimbZone.HangOnGrab)
                    StartHang(_currentClimbZone);
                else
                    PerformClimb(_currentClimbZone);
                return true;
            }

            return false;
        }

        public void TryLedgeGrab(IClimbZone zone, float maxVelocityY = 0f)
        {
            if (zone == null)
                return;

            var state = _movement.State;
            if (state.IsGrounded || state.IsClimbing || state.IsHanging || state.ClimbTimer > 0f || state.VelocityY > maxVelocityY)
                return;

            if (zone.HangOnGrab)
                StartHang(zone);
            else
                PerformClimb(zone);
        }

        public void SetClimbZone(IClimbZone zone) => _currentClimbZone = zone;

        public void ClearClimbZone() => _currentClimbZone = null;

        public void TriggerClimbUp() => _animationDriver?.TriggerSignal(ActorAnimationSignal.ClimbEnd);

        public void ApplyTraversalProfile(PawnProfileApplicationContext context, PawnTraversalProfile profile)
        {
            if (profile == null)
                return;

            climbCooldown = profile.climbCooldown;
            _movement.ApplyTraversalProfile(profile);
        }

        private void PerformClimb(IClimbZone zone)
        {
            if (zone == null || _movement.State.ClimbTimer > 0f || _movement.State.IsActing)
                return;

            _movement.NotifyClimbStart(climbCooldown);
            _animationDriver?.TriggerSignal(
                zone.TraversalType == ClimbTraversalType.Side
                    ? ActorAnimationSignal.SideClimb
                    : ActorAnimationSignal.ForwardClimb);

            StartCoroutine(ExecuteClimb(zone));
        }

        private IEnumerator ExecuteClimb(IClimbZone zone)
        {
            zone.DisableTemporarily();
            Vector3 startPos = transform.position;
            _controller.enabled = false;

            float elapsed = 0f;
            float duration = Mathf.Max(zone.ClimbDuration, 0.05f);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                transform.position = zone.SamplePath(t, startPos);
                yield return null;
            }

            transform.position = zone.ClimbTargetPosition;
            _controller.enabled = true;
            zone.EnableAfterClimb();
            _movement.NotifyClimbEnd();
            _animationDriver?.TriggerSignal(ActorAnimationSignal.ClimbEnd);
        }

        private void StartHang(IClimbZone zone)
        {
            if (zone == null || _movement.State.IsHanging || _movement.State.IsClimbing)
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
            _movement.SetVelocityY(-1f);
            zone?.EnableAfterClimb();
        }

        private void ExitHang()
        {
            _movement.NotifyHangEnd();
            _shimmyVelocityX = 0f;
            _hangZone = null;
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.Hang, false);
        }
    }
}
