using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Characters;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class InputControllerAuthoringTests
    {
        [Test]
        public void InputProfile_DashAction_RemainsUnassignedUntilCreatorAddsBinding()
        {
            InputProfile profile = ScriptableObject.CreateInstance<InputProfile>();
            try
            {
                profile.actionBindings = new[]
                {
                    GameplayInputActionBinding.BuiltIn(GameplayInputActionRole.Move, "Move", GameplayInputValueType.Vector2, true)
                };
                profile.Sanitize();

                Assert.That(profile.FindBinding(GameplayInputActionRole.Dash), Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void PawnMovementProfile_Applies2DDashPermissionToMovementComponent()
        {
            GameObject actor = new GameObject("2D Dash Authoring Test");
            try
            {
                Pawn2DMovementComponent movement = actor.AddComponent<Pawn2DMovementComponent>();
                PawnMovementProfile profile = ScriptableObject.CreateInstance<PawnMovementProfile>();
                try
                {
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
