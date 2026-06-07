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
    [RequireComponent(typeof(Pawn3DMovementComponent))]
    [RequireComponent(typeof(CharacterController))]
    public sealed class Pawn3DTraversalComponent : MonoBehaviour, IPawnTraversalModule
    {
        [Header("Climb")]
        [SerializeField] private bool allowClimb;
        [SerializeField] private bool allowHang;
        [SerializeField] private float climbCooldown = 1.2f;
        [SerializeField] private LedgeProbe3D ledgeProbe = new LedgeProbe3D();

        private Pawn3DMovementComponent _movement;
        private CharacterController _controller;
        private ActorAnimationDriver _animationDriver;
        private IClimbZone _currentClimbZone;
        private IClimbZone _hangZone;
        private Coroutine _activeClimb;
        private IClimbZone _activeClimbZone;
        private float _shimmyVelocityX;

        public float ShimmyVelocityX => _shimmyVelocityX;

        private void Awake()
        {
            _movement = GetComponent<Pawn3DMovementComponent>();
            _controller = GetComponent<CharacterController>();
            _animationDriver = GetComponent<ActorAnimationDriver>();
        }

        private void OnDisable()
        {
            if (_activeClimb != null)
            {
                StopCoroutine(_activeClimb);
                CleanupClimb(_activeClimbZone, triggerAnimation: false);
            }

            if (_hangZone != null)
            {
                IClimbZone zone = _hangZone;
                ExitHang();
                zone.EnableAfterClimb();
            }
        }

        public void ProbeLedge()
        {
            if (!EnsureDependencies())
                return;

            if (!allowClimb && !allowHang)
                return;

            var state = _movement.State;
            if (state.IsClimbing || state.IsHanging || state.ClimbTimer > 0f)
                return;

            IClimbZone found = ledgeProbe?.FindClimbZone(transform, state.VelocityY);
            if (found != null)
            {
                _currentClimbZone = found;
                if (found.AutoGrab && !state.IsGrounded && state.VelocityY <= found.MaxGrabVelocityY)
                {
                    if (found.HangOnGrab && allowHang)
                        StartHang(found);
                    else if (allowClimb)
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

        public void HandleInteract()
        {
            if (!TryHandleTraversalInteraction())
                _animationDriver?.TriggerSignal(ActorAnimationSignal.Interact);
        }

        public bool TryHandleTraversalInteraction()
        {
            if (_currentClimbZone != null)
            {
                if (_currentClimbZone.HangOnGrab && allowHang)
                {
                    StartHang(_currentClimbZone);
                    return true;
                }

                if (allowClimb)
                {
                    PerformClimb(_currentClimbZone);
                    return true;
                }
            }

            return false;
        }

        public void TryLedgeGrab(IClimbZone zone, float maxVelocityY = 0f)
        {
            if (zone == null || !EnsureDependencies())
                return;

            var state = _movement.State;
            if (state.IsGrounded || state.IsClimbing || state.IsHanging || state.ClimbTimer > 0f || state.VelocityY > maxVelocityY)
                return;

            if (zone.HangOnGrab && allowHang)
                StartHang(zone);
            else if (allowClimb)
                PerformClimb(zone);
        }

        public void SetClimbZone(IClimbZone zone) => _currentClimbZone = zone;

        public void ClearClimbZone() => _currentClimbZone = null;

        public void TriggerClimbUp() => _animationDriver?.TriggerSignal(ActorAnimationSignal.ClimbEnd);

        public void ApplyTraversalProfile(PawnProfileApplicationContext context, PawnTraversalProfile profile)
        {
            if (profile == null)
                return;

            allowClimb = profile.allowClimb;
            allowHang = profile.allowHang;
            climbCooldown = profile.climbCooldown;
            if (EnsureDependencies())
                _movement.ApplyTraversalProfile(profile);
        }

        private void PerformClimb(IClimbZone zone)
        {
            if (!EnsureDependencies())
                return;

            if (!allowClimb || zone == null || _movement.State.ClimbTimer > 0f || _movement.State.IsActing)
                return;

            _movement.NotifyClimbStart(climbCooldown);
            _animationDriver?.TriggerSignal(
                zone.TraversalType == ClimbTraversalType.Side
                    ? ActorAnimationSignal.SideClimb
                    : ActorAnimationSignal.ForwardClimb);

            _activeClimb = StartCoroutine(ExecuteClimb(zone));
        }

        private IEnumerator ExecuteClimb(IClimbZone zone)
        {
            _activeClimbZone = zone;
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
            CleanupClimb(zone, triggerAnimation: true);
        }

        private void CleanupClimb(IClimbZone zone, bool triggerAnimation)
        {
            if (_controller != null)
                _controller.enabled = true;

            zone?.EnableAfterClimb();
            _movement?.NotifyClimbEnd();
            if (triggerAnimation)
                _animationDriver?.TriggerSignal(ActorAnimationSignal.ClimbEnd);

            _activeClimb = null;
            _activeClimbZone = null;
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

        private bool EnsureDependencies()
        {
            _movement ??= GetComponent<Pawn3DMovementComponent>();
            _controller ??= GetComponent<CharacterController>();
            _animationDriver ??= GetComponent<ActorAnimationDriver>();
            return _movement != null && _controller != null;
        }
    }
}
