using UnityEngine;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Features.Characters
{
    public class PawnProjectileModule : MonoBehaviour
    {
        [Header("Projectile")]
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private ProjectileLauncher3D projectileLauncher;

        private HealthComponent _health;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
        }

        public void FireProjectile(WeaponData weapon, bool facingRight, float damageMultiplier, float knockbackMultiplier)
        {
            ProjectileLauncher3D launcher = ResolveProjectileLauncher();
            if (launcher == null)
            {
                Debug.LogWarning($"{nameof(PawnProjectileModule)} needs a {nameof(ProjectileLauncher3D)} to fire ranged weapon `{weapon.weaponName}`.", this);
                return;
            }

            Vector3 spawnPos = projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.position + Vector3.up * 1f + transform.forward * 0.5f;

            Vector3 forward = facingRight ? Vector3.right : Vector3.left;
            
            if (weapon.projectileDefinition == null)
                return;

            ProjectileFireRequest request = new ProjectileFireRequest(
                weapon.projectileDefinition,
                weapon.fireModeDefinition,
                spawnPos,
                forward,
                gameObject,
                _health != null ? _health.faction : Faction.Neutral,
                damageMultiplier: damageMultiplier,
                knockbackMultiplier: knockbackMultiplier);

            launcher.Fire(request);
        }

        private ProjectileLauncher3D ResolveProjectileLauncher()
        {
            if (projectileLauncher != null)
                return projectileLauncher;

            projectileLauncher = GetComponentInParent<ProjectileLauncher3D>();
            if (projectileLauncher == null)
                projectileLauncher = GetComponentInChildren<ProjectileLauncher3D>();

            return projectileLauncher;
        }

        public void SetProjectileLauncher(ProjectileLauncher3D launcher)
        {
            projectileLauncher = launcher;
        }
    }
}