using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Characters;
using UnityEngine;

namespace NeonBlack.Gameplay.Characters
{
    [AuthoringContract(
        Capability = AuthoringCapability.Animation,
        Relevance = "3D presentation module; maps movement state to Animator signals and handles billboarding.",
        Axioms = AuthoringWorldAxiom.Dimensions3D,
        NativeSetup = new[] { "Attach to a Pawn with ActorAnimationDriver.", "Ensure Animator parameters match signal names." },
        FirstProof = "Verify the pawn's animations respond correctly to movement and action states.",
        ExpertAdvice = "Presentation logic should be visual-only. It reads from movement/combat state and writes to the Animator. Use Billboarding settings if your 3D pawn uses 2D sprites."
    )]
    [AddComponentMenu("NeonBlack/Gameplay/3D/Pawn 3D Presentation Component")]
    [RequireComponent(typeof(ActorAnimationDriver))]
    public sealed class Pawn3DPresentationComponent : MonoBehaviour, IPawnPresentationModule
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugHUD;
        private Pawn3DMovementComponent _movement;
        private PawnCombatBehaviour _combat;
        private ActorAnimationDriver _animationDriver;
        private HealthComponent _health;
        private bool _lookAroundActive;

        private void Awake()
        {
            _movement = GetComponent<Pawn3DMovementComponent>();
            _combat = GetComponent<PawnCombatBehaviour>();
            _animationDriver = GetComponent<ActorAnimationDriver>();
            _health = GetComponent<HealthComponent>();

            if (_health != null)
            {
                _health.OnDeath.RemoveListener(HandleDeath);
                _health.OnDeath.AddListener(HandleDeath);
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
                _health.OnDeath.RemoveListener(HandleDeath);
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
            _animationDriver.SetBoolSignal(ActorAnimationSignal.BlockLoop, _combat != null && _combat.IsBlocking);
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
            _animationDriver?.ApplyProfiles(
                presentationProfile,
                context.PawnDefinition != null ? context.PawnDefinition.animationProfile : null);
        }

        private void HandleModelTriggers(NeonBlack.Gameplay.Features.Characters.MovementState state)
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

        private void ApplyBlendTreeChannels(NeonBlack.Gameplay.Features.Characters.MovementState state)
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
