using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared reaction and presentation feedback tuning for enemies.
    /// </summary>
    [AuthoringContract(
        Category = "Combat",
        CapabilityPath = "Combat/Actions/Enemy Reaction Profile",
        Surface = AuthoringSurface.Profile,
        Summary = "Configures stagger, hit-stun, and death reaction timing for enemies.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/enemies",
        RequiredFields = new[] { nameof(hurtLockDuration), nameof(staggerDamageThreshold) },
        SetupSteps = new[] { "Set hit stun and death delays.", "Assign to an EnemyProfile." },
        SuccessChecks = new[] { "Hit the enemy and verify it enters a hit-stun state for the specified duration." },
        Tags = new[] { "capability:Combat", "runtime:Combat" },
        Selectable = false
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Enemy Reaction Profile", fileName = "EnemyReactionProfile")]
    public class EnemyReactionProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (hurtLockDuration < 0f) yield return PyralisRuntimeValidationIssue.Required("Hurt Lock Duration cannot be negative.");
            if (staggerLockDuration < 0f) yield return PyralisRuntimeValidationIssue.Required("Stagger Lock Duration cannot be negative.");
            if (hitPauseDuration < 0f) yield return PyralisRuntimeValidationIssue.Required("Hit Pause Duration cannot be negative.");
        }

        public bool enableReactions = true;
        public float hurtLockDuration = 0.08f;
        public float staggerDamageThreshold = 20f;
        public float staggerLockDuration = 0.18f;
        public float hitPauseDuration = 0.03f;
        public float cameraShakeIntensity = 0.08f;
        public float cameraShakeDuration = 0.08f;
        public bool clearKnockbackOnDeath = true;

        public void Sanitize()
        {
            hurtLockDuration = Mathf.Max(0f, hurtLockDuration);
            staggerDamageThreshold = Mathf.Max(0f, staggerDamageThreshold);
            staggerLockDuration = Mathf.Max(0f, staggerLockDuration);
            hitPauseDuration = Mathf.Max(0f, hitPauseDuration);
            cameraShakeIntensity = Mathf.Max(0f, cameraShakeIntensity);
            cameraShakeDuration = Mathf.Max(0f, cameraShakeDuration);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
