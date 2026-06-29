using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Data.Presentation;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Core.Types.Input;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Character
{
    [AuthoringContract(
        StableId = "presentation.pawn.3d",
        Category = "Animation",
        CapabilityPath = "Presentation/Feedback/Pawn3D Presentation Component",
        Surface = AuthoringSurface.Goal,
        Summary = "3D presentation module; maps movement state to Animator signals and handles billboarding.",
        RequiredFields = new[] { nameof(showDebugHUD) },
        SetupSteps = new[] { "Attach to a Pawn with ActorAnimationDriver.", "Ensure Animator parameters match signal names." },
        SuccessChecks = new[] { "Move the 3D pawn and verify Animator signals follow movement and combat state." },
        Tags = new[] { "capability:Animation", "axiom:Dimensions3D" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Modules/Character/Rigged3D/Pawn 3D Presentation Component")]
    public sealed class Pawn3DPresentationComponent : MonoBehaviour, IPawnPresentationModule, IRuntimeValidationProvider
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugHUD;
        private Pawn3DMovementComponent _movement;
        private IActorGuardController _guardState;
        private IActorAnimationController _animationDriver;
        private IActorHealthState _health;
        private bool _lookAroundActive;

        private void Awake()
        {
            _movement = GetComponent<Pawn3DMovementComponent>();
            _guardState = GetComponent<IActorGuardController>();
            _animationDriver = GetComponent<IActorAnimationController>();
            _health = GetComponent<IActorHealthState>();

            if (_health != null)
            {
                _health.Died -= HandleDeath;
                _health.Died += HandleDeath;
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
                _health.Died -= HandleDeath;
        }

        public void Apply(float shimmyVelocityX)
        {
            if (_movement == null || _animationDriver == null)
                return;

            var state = _movement.State;

            HandleModelTriggers(state);

            _animationDriver.SetBoolSignal(ActorAnimationSignal.Move, state.IsMoving);
            _animationDriver.SetBoolSignal(ActorAnimationSignal.Idle, !state.IsMoving && state.IsGrounded && !state.IsHanging);
            _animationDriver.SetBoolSignal(ActorAnimationSignal.Sprint, state.IsSprinting && state.IsMoving);
            _animationDriver.SetBoolSignal(ActorAnimationSignal.Crouch, state.IsCrouching);
            _animationDriver.SetBoolSignal(ActorAnimationSignal.Hang, state.IsHanging);
            _animationDriver.SetBoolSignal(ActorAnimationSignal.BlockLoop, _guardState != null && _guardState.IsGuarding);
            _animationDriver.SetBoolSignal(ActorAnimationSignal.Fall, !state.IsGrounded && state.VelocityY < -0.01f);
            _animationDriver.SetBoolSignal(ActorAnimationSignal.LookAround, _lookAroundActive);
            _animationDriver.SetFloatSignal(ActorAnimationSignal.Shimmy, state.IsHanging ? shimmyVelocityX : 0f);
            ApplyBlendTreeChannels(state);
            _animationDriver.SetFacing(state.FacingRight);
            _animationDriver.ApplyBillboard();
        }

        public void UpdateLookAround(FrameInput frameInput)
        {
            if (frameInput.LookAroundPressed)
            {
                _lookAroundActive = true;
                _animationDriver?.TriggerSignal(ActorAnimationSignal.LookAround);
            }

            if (frameInput.LookAroundReleased)
                _lookAroundActive = false;
        }

        public void ResetMoveToIdle()
        {
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.Idle, true);
        }

        public void ApplyPresentationProfile(PawnProfileApplicationContext context, PawnPresentationProfile presentationProfile)
        {
            GetComponent<IPawnAnimationProfileReceiver>()?.ApplyProfiles(
                presentationProfile,
                context.PawnDefinition != null ? context.PawnDefinition.animationProfile : null);
        }

        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (GetComponent<IActorAnimationController>() == null)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Pawn3DPresentationComponent needs a component that implements IActorAnimationController.",
                    "IActorAnimationController",
                    nameof(Pawn3DPresentationComponent),
                    "Add ActorAnimationDriver or another presentation-owned animation controller to the pawn root.",
                    "The 3D pawn can receive animation signals from movement, traversal, and feedback.",
                    "Pawn3DPresentation.AnimationController.Missing");
            }
        }

        private void HandleModelTriggers(NeonBlack.Gameplay.Modules.Character.MovementState state)
        {
            if (state.TriggerJump)
                _animationDriver.TriggerSignal(ActorAnimationSignal.Jump);

            if (state.TriggerDiveRoll)
                _animationDriver.TriggerSignal(ActorAnimationSignal.Dash);

            if (state.TriggerPowerSlide)
                _animationDriver.TriggerSignal(ActorAnimationSignal.Slide);

            if (state.TriggerJustLanded)
                _animationDriver.TriggerSignal(ActorAnimationSignal.Land);

            if (state.TriggerKnockedBack)
                _animationDriver.TriggerSignal(ActorAnimationSignal.Hurt);
        }

        private void ApplyBlendTreeChannels(NeonBlack.Gameplay.Modules.Character.MovementState state)
        {
            float planarSpeed = new Vector2(state.VelocityX, state.VelocityZ).magnitude;
            float normalizedSpeed = Mathf.Clamp01(planarSpeed / Mathf.Max(0.01f, _movement.MoveSpeed));

            _animationDriver.SetFloatCustom("Speed", planarSpeed);
            _animationDriver.SetFloatCustom("NormalizedSpeed", normalizedSpeed);
            _animationDriver.SetFloatCustom("MoveX", state.VelocityX);
            _animationDriver.SetFloatCustom("MoveY", state.VelocityZ);
            _animationDriver.SetFloatCustom("MoveZ", state.VelocityZ);
            _animationDriver.SetFloatCustom("VelocityX", state.VelocityX);
            _animationDriver.SetFloatCustom("VelocityY", state.VelocityY);
            _animationDriver.SetFloatCustom("VelocityZ", state.VelocityZ);
        }

        private void HandleDeath()
        {
            _animationDriver?.TriggerSignal(ActorAnimationSignal.Death);
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!showDebugHUD || !Application.isPlaying || _movement == null)
                return;

            var s = _movement.State;
            int line = 0;
            const float x = 10f;
            const float h = 22f;
            const float w = 340f;

            GUI.color = Color.black;
            GUI.Box(new Rect(x - 4, 6, w + 8, h * 14 + 4), GUIContent.none);
            GUI.color = Color.white;

            void Row(string label, object value, Color? color = null)
            {
                GUI.color = color ?? Color.white;
                GUI.Label(new Rect(x, 10 + line * h, w, h), $"{label}: {value}");
                line++;
            }

            Row("Grounded", s.IsGrounded, s.IsGrounded ? Color.green : Color.red);
            Row("VelocityY", $"{s.VelocityY:F2}");
            Row("VelocityX", $"{s.VelocityX:F2}");
            Row("Crouching", s.IsCrouching);
            Row("Sprinting", s.IsSprinting);
            Row("Hanging", s.IsHanging);
            Row("WallSliding", s.IsWallSliding);
            Row("Sliding", s.IsSliding);
            Row("Acting", s.IsActing);
        }
#endif
    }
}
