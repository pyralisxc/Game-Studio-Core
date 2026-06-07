using System.Collections;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
/// <summary>
/// Receives knockback impulses and exposes the resulting velocity so the owning
/// character controller (Motor3D, EnemyAI) can fold it into its single
/// CharacterController.Move call each frame.
///
/// Setup:
///   Ã¢â‚¬Â¢ This component requires a CharacterController on the same GameObject.
///   Ã¢â‚¬Â¢ Call ApplyKnockback(direction * force) from HitBox or any damage source.
///   Ã¢â‚¬Â¢ The character controller script must call Tick(deltaTime) each frame and
///     add Velocity to its move vector before calling CharacterController.Move.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class KnockbackReceiver : MonoBehaviour, IActorKnockbackController
{
    [Header("Resistance")]
    [Tooltip("Multiplier on incoming knockback. 1 = full, 0 = immune.")]
    [SerializeField] private float knockbackResistance = 1f;

    [Tooltip("How quickly knockback velocity decays per second.")]
    [SerializeField] private float decayRate = 10f;

    /// <summary>Current knockback velocity in world space. Add to the character's move vector each frame.</summary>
    public Vector3 Velocity { get; private set; }

    /// <summary>
    /// Decay the knockback velocity by <paramref name="deltaTime"/>.
    /// Call once per frame from the character controller script BEFORE reading Velocity.
    /// </summary>
    public void Tick(float deltaTime)
    {
        if (Velocity.sqrMagnitude < 0.01f)
        {
            Velocity = Vector3.zero;
            return;
        }
        Velocity = Vector3.MoveTowards(Velocity, Vector3.zero, decayRate * deltaTime);
    }

    /// <summary>
    /// Apply a knockback impulse. Direction should already be normalized and
    /// pre-multiplied by force magnitude.
    /// </summary>
    public void ApplyKnockback(Vector3 forceVector)
    {
        Velocity += forceVector * knockbackResistance;
    }

    /// <summary>Instantly zeroes knockback velocity Ã¢â‚¬â€ call on death or teleport.</summary>
    public void ClearKnockback() => Velocity = Vector3.zero;
}
}
