using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Modules.Character;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class InputControllerConfigurationTests
    {
        [Test]
        public void PawnMovementProfile_AppliesDashPermissionAndMovementStyle()
        {
            GameObject actor = new GameObject("2D Dash Configuration Test");
            try
            {
                Pawn2DMovementComponent movement = actor.AddComponent<Pawn2DMovementComponent>();
                PawnMovementProfile profile = ScriptableObject.CreateInstance<PawnMovementProfile>();
                try
                {
                    profile.movementStyle = Pawn2DMovementStyle.TopDownNoGravity;
                    profile.allow2DJump = true;
                    Assert.That(profile.Effective2DMovementStyle, Is.EqualTo(Pawn2DMovementStyle.TopDownNoGravity));

                    profile.allow2DDash = false;
                    movement.ApplyMovementProfile(default, profile);

                    Assert.That(movement.TryDash(Vector2.right), Is.False);

                    profile.allow2DDash = true;
                    profile.dashSpeed = 16f;
                    profile.dashDuration = 0.2f;
                    profile.dashCooldown = 1.25f;
                    movement.ResetForRound(Vector3.zero);
                    movement.ApplyMovementProfile(default, profile);

                    Assert.That(movement.TryDash(Vector2.right), Is.True);
                    Assert.That(movement.IsDashing, Is.True);
                }
                finally
                {
                    Object.DestroyImmediate(profile);
                }
            }
            finally
            {
                Object.DestroyImmediate(actor);
            }
        }
    }
}
