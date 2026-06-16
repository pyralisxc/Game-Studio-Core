using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal
{
    [AuthoringContract(
        Capability = AuthoringCapability.Traversal,
        Relevance = "3D traversal module; handles ledge climbing, hanging, and shimmying.",
Axioms = AuthoringWorldAxiom.Dimensions3D,
        NativeSetup = new[] { "Attach to a Pawn with Motor3D and Pawn3DMovementComponent.", "Configure Ledge Probe settings." },
        AssignmentFields = new[] { nameof(allowClimb), nameof(allowHang), nameof(climbCooldown), nameof(ledgeProbe) },
        FirstProof = "Verify the pawn can grab and climb ledges in Play Mode.",
        ExpertAdvice = "Traversal logic is separated from base movement. Ensure your Animator has 'Climb' and 'Hang' signals wired to valid animations.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/traversal"
    )]
[AddComponentMenu("NeonBlack/Gameplay/3D/Pawn 3D Traversal Component")]
    [RequireComponent(typeof(Pawn3DMovementComponent))]
    [RequireComponent(typeof(CharacterController))]
    public sealed partial class Pawn3DTraversalComponent : MonoBehaviour, IPawnTraversalModule
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

        private bool EnsureDependencies()
        {
            _movement ??= GetComponent<Pawn3DMovementComponent>();
            _controller ??= GetComponent<CharacterController>();
            _animationDriver ??= GetComponent<ActorAnimationDriver>();
            return _movement != null && _controller != null;
        }
    }
}
