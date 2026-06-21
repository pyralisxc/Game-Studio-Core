using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed partial class Pawn2DMovementComponent
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (moveSpeed <= 0f)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Move Speed must be greater than zero.",
                    nameof(moveSpeed),
                    nameof(Pawn2DMovementComponent),
                    "Set Pawn2DMovementComponent.moveSpeed above zero, or apply a PawnMovementProfile with a positive walk speed.",
                    "Pawn2DMovementComponent can produce horizontal/planar movement.",
                    "Pawn2DMovement.MoveSpeed.Minimum");
            }

            if (dashEnabled)
            {
                if (dashSpeed <= 0f)
                {
                    yield return PyralisRuntimeValidationIssue.Required(
                        "Dash Speed must be greater than zero when dash is enabled.",
                        nameof(dashSpeed),
                        nameof(Pawn2DMovementComponent),
                        "Set Pawn2DMovementComponent.dashSpeed above zero, or disable dash for this route.",
                        "Dash either has usable speed or is disabled.",
                        "Pawn2DMovement.DashSpeed.Minimum");
                }

                if (dashCooldown <= 0f)
                {
                    yield return PyralisRuntimeValidationIssue.Required(
                        "Dash Cooldown must be greater than zero when dash is enabled.",
                        nameof(dashCooldown),
                        nameof(Pawn2DMovementComponent),
                        "Set Pawn2DMovementComponent.dashCooldown above zero, or disable dash for this route.",
                        "Dash either has a usable cooldown or is disabled.",
                        "Pawn2DMovement.DashCooldown.Minimum");
                }
            }

            if (EffectiveMovementStyle == Pawn2DMovementStyle.SideViewGravity && jumpEnabled)
            {
                if (jumpVelocity <= 0f)
                {
                    yield return PyralisRuntimeValidationIssue.Required(
                        "Jump Velocity must be greater than zero when side-view jump is enabled.",
                        nameof(jumpVelocity),
                        nameof(Pawn2DMovementComponent),
                        "Set Pawn2DMovementComponent.jumpVelocity above zero, or disable jump for this route.",
                        "Side-view jump has a positive launch velocity.",
                        "Pawn2DMovement.JumpVelocity.Minimum");
                }

                if (gravityScale <= 0f)
                {
                    yield return PyralisRuntimeValidationIssue.Required(
                        "Gravity Scale must be greater than zero when side-view jump is enabled.",
                        nameof(gravityScale),
                        nameof(Pawn2DMovementComponent),
                        "Set Pawn2DMovementComponent.gravityScale above zero for side-view gravity routes.",
                        "Side-view jump has gravity to bring the pawn back down.",
                        "Pawn2DMovement.GravityScale.Minimum");
                }
            }

            if (useCameraVisibleBoundsForMovement && cameraBoundsProvider == null)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Use Camera Visible Bounds For Movement is enabled, but no camera bounds provider has been supplied by the session. Assign GameplaySessionBootstrap.cameraRigController; prefer PlayfieldProfile for normal legal movement bounds.",
                    nameof(useCameraVisibleBoundsForMovement),
                    nameof(Pawn2DMovementComponent),
                    "Assign GameplaySessionBootstrap.cameraRigController, or disable camera-visible movement bounds and use PlayfieldProfile for normal legal movement bounds.",
                    "Movement bounds come from a camera bounds provider or the pawn does not require camera-visible bounds.",
                    "Pawn2DMovement.CameraBoundsProvider.Missing");
            }

            Rigidbody2D body = GetComponent<Rigidbody2D>();
            if (body == null)
                yield break;

            if (EffectiveMovementStyle == Pawn2DMovementStyle.TopDownNoGravity)
            {
                if (Mathf.Abs(body.gravityScale) > 0.001f)
                {
                    yield return PyralisRuntimeValidationIssue.Required(
                        "Rigidbody2D gravity is non-zero while Movement Style is TopDownNoGravity.",
                        "Rigidbody2D.gravityScale",
                        nameof(Pawn2DMovementComponent),
                        "Set Rigidbody2D > Gravity Scale to 0, or switch Pawn2DMovementComponent.movementStyle to SideViewGravity for platformer-style gravity.",
                        "Top-down/no-gravity pawns stay on the map plane and are moved by script.",
                        "Pawn2DMovement.Rigidbody2D.GravityScale.TopDown");
                }

                if (body.bodyType != RigidbodyType2D.Kinematic)
                {
                    yield return PyralisRuntimeValidationIssue.Required(
                        "Rigidbody2D Body Type should be Kinematic while Movement Style is TopDownNoGravity.",
                        "Rigidbody2D.bodyType",
                        nameof(Pawn2DMovementComponent),
                        "Set Rigidbody2D > Body Type to Kinematic, or switch Pawn2DMovementComponent.movementStyle to SideViewGravity.",
                        "Top-down/no-gravity movement uses script-driven map-plane motion.",
                        "Pawn2DMovement.Rigidbody2D.BodyType.TopDown");
                }
            }

            if ((body.constraints & RigidbodyConstraints2D.FreezeRotation) == 0)
            {
                yield return PyralisRuntimeValidationIssue.Recommended(
                    "Rigidbody2D rotation is not frozen; collision nudges can spin the pawn during movement proofs.",
                    "Rigidbody2D.constraints",
                    nameof(Pawn2DMovementComponent),
                    "Set Rigidbody2D > Constraints > Freeze Rotation.",
                    "2D pawn collisions do not rotate the authored sprite unexpectedly.",
                    "Pawn2DMovement.Rigidbody2D.FreezeRotation");
            }
        }
    }
}
