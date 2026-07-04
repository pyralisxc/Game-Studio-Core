using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Definitions.Combat
{
    [AuthoringContract(
        Category = "Combat",
        CapabilityPath = "Combat/Actions/Weapon Data",
        Surface = AuthoringSurface.Goal,
        Summary = "The primary definition for an actor's weapon; defines damage, timing, range, and presentation.",
        RequiredFields = new[] { nameof(weaponName) },
        SetupSteps = new[] { "Set Weapon Type.", "Assign Projectile or Hitbox Zone." },
        SuccessChecks = new[] { "Assign to a Pawn or Enemy and verify attacks trigger animations and deal damage." },
        Tags = new[] { "capability:Combat", "runtime:Combat" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Combat/Weapon Data", fileName = "NewWeapon")]
    public class WeaponData : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (string.IsNullOrWhiteSpace(weaponName))
                yield return RuntimeValidationIssue.Required("Weapon Name is required.", nameof(weaponName), nameof(WeaponData), issueCode: "WeaponData.Name.Missing");
            if (damage < 0f)
                yield return RuntimeValidationIssue.Required("Damage cannot be negative.", nameof(damage), nameof(WeaponData), issueCode: "WeaponData.Damage.Invalid");
            if (attackCooldown <= 0f)
                yield return RuntimeValidationIssue.Required("Attack Cooldown must be greater than zero.", nameof(attackCooldown), nameof(WeaponData), issueCode: "WeaponData.AttackCooldown.Invalid");

            if ((weaponType == WeaponType.Ranged || weaponType == WeaponType.Thrown) && projectileDefinition == null)
                yield return RuntimeValidationIssue.Required("Ranged/thrown weapons require a Projectile Definition.", nameof(projectileDefinition), nameof(WeaponData), issueCode: "WeaponData.ProjectileDefinition.Missing");

            if (weaponType == WeaponType.Melee && string.IsNullOrWhiteSpace(hitBoxZone))
                yield return RuntimeValidationIssue.Required("Melee weapons should name the actor Hit Box Zone they use.", nameof(hitBoxZone), nameof(WeaponData), issueCode: "WeaponData.HitBoxZone.Missing");

            if (projectileDefinition != null)
            {
                foreach (RuntimeValidationIssue issue in projectileDefinition.GetRuntimeValidationIssues())
                {
                    if (issue != null && !string.IsNullOrWhiteSpace(issue.Message))
                    {
                        yield return new RuntimeValidationIssue(
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
