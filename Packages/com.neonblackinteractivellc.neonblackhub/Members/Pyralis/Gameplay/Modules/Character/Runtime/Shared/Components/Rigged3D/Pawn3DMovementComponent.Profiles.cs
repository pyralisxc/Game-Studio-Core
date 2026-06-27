using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Participants;

namespace NeonBlack.Gameplay.Modules.Character
{
    public sealed partial class Pawn3DMovementComponent
    {
        public void ApplyMovementProfile(PawnProfileApplicationContext context, PawnMovementProfile profile)
        {
            if (profile == null) return;

            movementMode = profile.movementMode;
            walkSpeed = profile.walkSpeed;
            sprintSpeed = profile.sprintSpeed;
            crouchSpeed = profile.crouchSpeed;
            depthSpeedMultiplier = profile.depthSpeedMultiplier;
            _config = BuildConfig();
            _model.Configure(_config);
        }

        /// <summary>Apply traversal tuning (jump, dodge, gravity) from a profile.</summary>
        public void ApplyTraversalProfile(PawnTraversalProfile profile)
        {
            if (profile == null) return;

            allowJump = profile.allowJump;
            allowDodge = profile.allowDodge;
            allowCrouch = profile.allowCrouch;
            allowPowerSlide = profile.allowDodge && profile.allowCrouch;
            jumpHeight = profile.jumpHeight;
            gravity = profile.gravity;
            dodgeDistance = profile.dodgeDistance;
            dodgeDuration = profile.dodgeDuration;
            dodgeCooldown = profile.dodgeCooldown;
            _config = BuildConfig();
            _model.Configure(_config);
        }
    }
}
