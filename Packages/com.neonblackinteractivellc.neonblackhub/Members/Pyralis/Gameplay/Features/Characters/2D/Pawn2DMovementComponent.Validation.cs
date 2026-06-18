using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed partial class Pawn2DMovementComponent
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (moveSpeed <= 0f)
                yield return "Move Speed must be greater than zero.";

            if (dashEnabled)
            {
                if (dashSpeed <= 0f)
                    yield return "Dash Speed must be greater than zero when dash is enabled.";
                if (dashCooldown <= 0f)
                    yield return "Dash Cooldown must be greater than zero when dash is enabled.";
            }

            if (EffectiveMovementStyle == Pawn2DMovementStyle.SideViewGravity && jumpEnabled)
            {
                if (jumpVelocity <= 0f)
                    yield return "Jump Velocity must be greater than zero when side-view jump is enabled.";
                if (gravityScale <= 0f)
                    yield return "Gravity Scale must be greater than zero when side-view jump is enabled.";
            }

            if (useCameraVisibleBoundsForMovement && cameraBoundsProvider == null)
                yield return "Use Camera Visible Bounds For Movement is enabled, but no camera bounds provider has been supplied by the session. Assign GameplaySessionBootstrap.cameraRigController; prefer PlayfieldProfile for normal legal movement bounds.";
        }
    }
}
