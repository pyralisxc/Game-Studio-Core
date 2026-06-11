using System.Collections.Generic;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat, 
        Relevance = "Defines how an actor reacts to combat events (guard, parry, block, shield break).",
        NativeSetup = new[] { "Create Asset.", "Configure parry and guard windows.", "Set shield break durations." },
        AssignmentFields = new[] { nameof(enableGuard), nameof(enableParry), nameof(blockDamageReduction) },
        FirstProof = "Trigger a parry in-game and verify the reaction lock is applied.",
        ExpertAdvice = "Use parryReactionLockDuration to stun the attacker when a parry is successful.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat"
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Actor Combat Reaction Profile", fileName = "ActorCombatReactionProfile")]
    public class ActorCombatReactionProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (parryWindowDuration < 0f) yield return "Parry Window Duration cannot be negative.";
            if (blockDamageReduction < 0f || blockDamageReduction > 1f) yield return "Block Damage Reduction must be between 0 and 1.";
        }

        public bool enableGuard = true;
        public bool enableParry = true;
        public float parryWindowDuration = 0.12f;
        public float parryReactionLockDuration = 0.2f;
        public float shieldBreakLockDuration = 0.35f;
        [Range(0f, 1f)] public float blockDamageReduction = 0.2f;
        [Range(10f, 180f)] public float blockFrontalAngle = 90f;
        public bool enableReactionLocks = true;
        public float hurtLockDuration = 0.08f;
        public float staggerDamageThreshold = 20f;
        public float staggerLockDuration = 0.18f;
        public bool clearKnockbackOnStagger = false;
        public bool clearKnockbackOnDeath = false;

        public void Sanitize()
        {
            blockDamageReduction = Mathf.Clamp01(blockDamageReduction);
            blockFrontalAngle = Mathf.Clamp(blockFrontalAngle, 10f, 180f);
            parryWindowDuration = Mathf.Max(0f, parryWindowDuration);
            parryReactionLockDuration = Mathf.Max(0f, parryReactionLockDuration);
            shieldBreakLockDuration = Mathf.Max(0f, shieldBreakLockDuration);
            hurtLockDuration = Mathf.Max(0f, hurtLockDuration);
            staggerDamageThreshold = Mathf.Max(0f, staggerDamageThreshold);
            staggerLockDuration = Mathf.Max(0f, staggerLockDuration);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
