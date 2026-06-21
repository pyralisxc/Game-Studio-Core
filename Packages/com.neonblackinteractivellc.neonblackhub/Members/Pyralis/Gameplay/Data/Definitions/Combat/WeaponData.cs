using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        Relevance = "The primary definition for an actor's weapon; defines damage, timing, range, and presentation.",
        NativeSetup = new[] { "Set Weapon Type.", "Assign Projectile or Hitbox Zone." },
        AssignmentFields = new[] { nameof(weaponName), nameof(damage), nameof(attackCooldown) },
        FirstProof = "Assign to a Pawn or Enemy and verify attacks trigger animations and deal damage.",
        ExpertAdvice = "Use overrideController to change actor animations when this weapon is equipped."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Combat/Weapon Data", fileName = "NewWeapon")]
    public class WeaponData : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (string.IsNullOrWhiteSpace(weaponName))
                yield return PyralisRuntimeValidationIssue.Required("Weapon Name is required.", nameof(weaponName), nameof(WeaponData), issueCode: "WeaponData.Name.Missing");
            if (damage < 0f)
                yield return PyralisRuntimeValidationIssue.Required("Damage cannot be negative.", nameof(damage), nameof(WeaponData), issueCode: "WeaponData.Damage.Invalid");
            if (attackCooldown <= 0f)
                yield return PyralisRuntimeValidationIssue.Required("Attack Cooldown must be greater than zero.", nameof(attackCooldown), nameof(WeaponData), issueCode: "WeaponData.AttackCooldown.Invalid");

            if ((weaponType == WeaponType.Ranged || weaponType == WeaponType.Thrown) && projectileDefinition == null)
                yield return PyralisRuntimeValidationIssue.Required("Ranged/thrown weapons require a Projectile Definition.", nameof(projectileDefinition), nameof(WeaponData), issueCode: "WeaponData.ProjectileDefinition.Missing");

            if (weaponType == WeaponType.Melee && string.IsNullOrWhiteSpace(hitBoxZone))
                yield return PyralisRuntimeValidationIssue.Required("Melee weapons should name the actor Hit Box Zone they use.", nameof(hitBoxZone), nameof(WeaponData), issueCode: "WeaponData.HitBoxZone.Missing");

            if (projectileDefinition != null)
            {
                foreach (PyralisRuntimeValidationIssue issue in projectileDefinition.GetRuntimeValidationIssues())
                {
                    if (issue != null && !string.IsNullOrWhiteSpace(issue.Message))
                    {
                        yield return new PyralisRuntimeValidationIssue(
                            $"Projectile Definition: {issue.Message}",
                            nameof(projectileDefinition),
                            nameof(WeaponData),
                            "Open the assigned ProjectileDefinition and resolve the named issue.",
                            "Assigned ProjectileDefinition reports no validation issues.",
                            issue.Severity,
                            "WeaponData.ProjectileDefinition." + issue.IssueCode);
                    }
                }
            }
        }

        [Header("Identity")]
        public string weaponName = "Unnamed Weapon";
        [TextArea(2, 4)] public string description = "";
        public Sprite icon;

        [Header("Damage")]
        public float damage = 20f;
        public float knockbackForce = 6f;

        [Header("Timing")]
        public float attackCooldown = 0.45f;
        public float hitDelay = 0f;
        public float hitDuration = 0.15f;

        [Header("Range")]
        public float attackRange = 0f;

        [Header("Type")]
        public WeaponType weaponType = WeaponType.Melee;

        [Header("Animation")]
        public RuntimeAnimatorController overrideController;

        [Header("Hit Zone")]
        public string hitBoxZone = "Punch";

        [Header("Projectile (ranged only)")]
        [Tooltip("Authored projectile payload. Required for ranged and thrown weapons; owns hitscan/prefab delivery, damage, speed, range, lifetime, and impact behavior.")]
        public ProjectileDefinition projectileDefinition;
        [Tooltip("Optional authored firing pattern used by ProjectileFirePlanner for burst, spread, clip, and reload data.")]
        public FireModeDefinition fireModeDefinition;
    }

    public enum WeaponType
    {
        Melee,
        Ranged,
        Thrown
    }
}
