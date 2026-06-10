using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat, 
        Relevance = "Project-window creation path for combat reaction behavior.",
        AssignmentFields = new[] { nameof(enableGuard), nameof(enableParry), nameof(blockDamageReduction) },
        FirstProof = "Trigger a parry in-game and verify the reaction lock is applied.",
        NativeSetup = new[] { "Create Asset" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Actor Combat Reaction Profile", fileName = "ActorCombatReactionProfile")]
    public class ActorCombatReactionProfile : ScriptableObject
    {
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
