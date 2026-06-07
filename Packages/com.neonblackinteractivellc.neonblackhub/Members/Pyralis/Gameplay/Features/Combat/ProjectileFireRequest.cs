using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    public readonly struct ProjectileFireRequest
    {
        public ProjectileDefinition Projectile { get; }
        public FireModeDefinition FireMode { get; }
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public GameObject Owner { get; }
        public Faction SourceFaction { get; }
        public ActionExecutionContext ActionContext { get; }
        public float DamageMultiplier { get; }
        public float KnockbackMultiplier { get; }

        public ProjectileFireRequest(
            ProjectileDefinition projectile,
            FireModeDefinition fireMode,
            Vector3 origin,
            Vector3 direction,
            GameObject owner = null,
            Faction sourceFaction = Faction.Neutral,
            ActionExecutionContext actionContext = null,
            float damageMultiplier = 1f,
            float knockbackMultiplier = 1f)
        {
            Projectile = projectile;
            FireMode = fireMode;
            Origin = origin;
            Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
            Owner = owner;
            SourceFaction = sourceFaction;
            ActionContext = actionContext;
            DamageMultiplier = Mathf.Max(damageMultiplier, 0f);
            KnockbackMultiplier = Mathf.Max(knockbackMultiplier, 0f);
        }

        public ProjectileFireRequest WithActionContext(ActionExecutionContext actionContext)
        {
            return new ProjectileFireRequest(
                Projectile,
                FireMode,
                Origin,
                Direction,
                Owner,
                SourceFaction,
                actionContext,
                DamageMultiplier,
                KnockbackMultiplier);
        }
    }
}
