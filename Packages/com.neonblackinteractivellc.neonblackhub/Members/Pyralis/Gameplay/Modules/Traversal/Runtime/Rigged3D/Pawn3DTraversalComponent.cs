using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Core.Types.Input;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Interactions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Participants;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Traversal
{
    [AuthoringContract(
        Category = "Traversal",
        CapabilityPath = "Movement/Traversal/Pawn3D Traversal Component",
        Surface = AuthoringSurface.Profile,
        Summary = "3D traversal component; handles ledge climbing, hanging, shimmying, profile tuning, and traversal interaction.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/traversal",
        RequiredFields = new[] { nameof(traversalProfile), nameof(allowClimb), nameof(allowHang), nameof(climbCooldown), nameof(ledgeProbe) },
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Modules.Character.Motor3D", "NeonBlack.Gameplay.Modules.Character.Pawn3DMovementComponent" },
        RequiredInterfaces = new[] { typeof(IActorTraversalFeature), typeof(IActorInteractionHandler) },
        SetupSteps = new[]
        {
            "Attach Pawn3DTraversalComponent to a pawn with Motor3D and Pawn3DMovementComponent.",
            "Assign a PawnTraversalProfile when reusable traversal tuning is needed.",
            "Configure Ledge Probe settings.",
            "Bind Jump or Interact in InputProfile."
        },
        SuccessChecks = new[] { "Verify the pawn can grab and climb ledges in Play Mode." },
        Tags = new[] { "capability:Traversal", "axiom:Dimensions3D" },
        Selectable = false
    )]
[AddComponentMenu("NeonBlack/Gameplay/Modules/Traversal/Rigged3D/Pawn 3D Traversal Component")]
    [RequireComponent(typeof(CharacterController))]
    public sealed partial class Pawn3DTraversalComponent : GameplayTickBehaviour, IPawnTraversalModule, IActorTraversalFeature, IActorInteractionHandler
    {
        [Header("Profile")]
        [SerializeField] private PawnTraversalProfile traversalProfile;

        [Header("Climb")]
        [SerializeField] private bool allowClimb;
        [SerializeField] private bool allowHang;
        [SerializeField] private float climbCooldown = 1.2f;
        [SerializeField] private LedgeProbe3D ledgeProbe = new LedgeProbe3D();

        private IClimbZone _currentClimbZone;
        private IClimbZone _hangZone;
        private Coroutine _activeClimb;
        private IClimbZone _activeClimbZone;
        private float _shimmyVelocityX;
        private float _gameplayDeltaTime;

        public float ShimmyVelocityX => _shimmyVelocityX;

        protected override GameplayTickDomain TickDomain => GameplayTickDomain.Traversal;
        protected override bool UsesGameplayTick => true;

        private float GameplayDeltaTime => _gameplayDeltaTime;

        protected override void OnGameplayTick(in GameplayTickContext context)
        {
            _gameplayDeltaTime = context.DeltaTime;
        }

        public void ProbeTraversal() => ProbeLedge();

        public bool TryHandleInteraction(ActorInteractionContext context)
        {
            return TryHandleTraversalInteraction();
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

            if (_movement.IsClimbing || _movement.IsHanging || _movement.ClimbTimer > 0f)
                return;

            IClimbZone found = ledgeProbe?.FindClimbZone(transform, _movement.VelocityY);
            if (found != null)
            {
                _currentClimbZone = found;
                if (found.AutoGrab && !_movement.IsGrounded && _movement.VelocityY <= found.MaxGrabVelocityY)
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

            if (_movement.IsGrounded || _movement.IsClimbing || _movement.IsHanging || _movement.ClimbTimer > 0f || _movement.VelocityY > maxVelocityY)
                return;

            if (zone.HangOnGrab && allowHang)
                StartHang(zone);
            else if (allowClimb)
                PerformClimb(zone);
        }

        public void SetClimbZone(IClimbZone zone) => _currentClimbZone = zone;

        public void ClearClimbZone() => _currentClimbZone = null;

        public void TriggerClimbUp() => _animationDriver?.TriggerSignal(ActorAnimationSignal.ClimbEnd);
    }
}
