using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [CreateAssetMenu(menuName = "NeonBlack/Combat/Weapon Data", fileName = "NewWeapon")]
    public class WeaponData : ScriptableObject
    {
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
