using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    public readonly struct ProjectileSpawnCommand
    {
        public ProjectileDeliveryMode DeliveryMode { get; }
        public GameObject ProjectilePrefab { get; }
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public float Damage { get; }
        public float Knockback { get; }
        public float Speed { get; }
        public float MaxDistance { get; }
        public float Lifetime { get; }
        public Faction SourceFaction { get; }
        public GameObject Owner { get; }
        public float Delay { get; }
        public bool AllowFriendlyFire { get; }
        public ProjectileImpactDefinition ImpactDefinition { get; }

        public ProjectileSpawnCommand(
            ProjectileDeliveryMode deliveryMode,
            GameObject projectilePrefab,
            Vector3 origin,
            Vector3 direction,
            float damage,
            float knockback,
            float speed,
            float maxDistance,
            float lifetime,
            Faction sourceFaction,
            GameObject owner,
            float delay,
            bool allowFriendlyFire,
            ProjectileImpactDefinition impactDefinition = null)
        {
            DeliveryMode = deliveryMode;
            ProjectilePrefab = projectilePrefab;
            Origin = origin;
            Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
            Damage = damage;
            Knockback = knockback;
            Speed = speed;
            MaxDistance = maxDistance;
            Lifetime = lifetime;
            SourceFaction = sourceFaction;
            Owner = owner;
            Delay = delay;
            AllowFriendlyFire = allowFriendlyFire;
            ImpactDefinition = impactDefinition;
        }
    }
}
