using NeonBlack.Gameplay.Core.Contracts;
using System;
using NeonBlack.Gameplay.Core.Actions;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [AuthoringContract(
        Capability = AuthoringCapability.RangedFlow,
        Relevance = "Logic for planning projectile trajectories based on fire modes and spread rules.",
        AssignmentFields = new[] { nameof(ProjectileFireRequest.Projectile), nameof(ProjectileFireRequest.FireMode), nameof(ProjectileFireRequest.Origin), nameof(ProjectileFireRequest.Direction) },
        ExpertAdvice = "This class produces ProjectileSpawnCommands but does not execute them. Use it in conjunction with a Launcher to decouple firing logic from physical spawning.",
        FirstProof = "proof.npc-enemy-behavior",
        NativeSetup = new[] { "Ensure FireModeDefinition spread values are configured.", "Call BuildCommands from weapon or action logic." },
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat/projectiles"
    )]
    public static class ProjectileFirePlanner
    {
        public static ProjectileSpawnCommand[] BuildCommands(ProjectileFireRequest request)
        {
            if (request.Projectile == null)
                return Array.Empty<ProjectileSpawnCommand>();

            FireModeDefinition fireMode = request.FireMode;
            int burstCount = fireMode != null ? Mathf.Max(1, fireMode.burstCount) : 1;
            int projectilesPerShot = fireMode != null ? Mathf.Max(1, fireMode.projectilesPerShot) : 1;
            float burstInterval = fireMode != null ? Mathf.Max(0f, fireMode.burstInterval) : 0f;
            float spreadAngle = fireMode != null ? Mathf.Max(0f, fireMode.spreadAngle) : 0f;
            var commands = new ProjectileSpawnCommand[burstCount * projectilesPerShot];
            int index = 0;

            Vector3 baseDirection = ResolveDirection(request);
            float damage = request.Projectile.damage * request.DamageMultiplier;
            float knockback = request.Projectile.knockback * request.KnockbackMultiplier;

            for (int burst = 0; burst < burstCount; burst++)
            {
                for (int shot = 0; shot < projectilesPerShot; shot++)
                {
                    Vector3 direction = ApplyEvenSpread(baseDirection, shot, projectilesPerShot, spreadAngle);
                    commands[index++] = new ProjectileSpawnCommand(
                        request.Projectile.deliveryMode,
                        request.Projectile.projectilePrefab,
                        request.Origin,
                        direction,
                        damage,
                        knockback,
                        request.Projectile.speed,
                        request.Projectile.maxDistance,
                        request.Projectile.lifetime,
                        request.SourceFaction,
                        request.Owner,
                        burst * burstInterval,
                        request.Projectile.allowFriendlyFire,
                        request.Projectile.impactDefinition);
                }
            }

            return commands;
        }

        private static Vector3 ResolveDirection(ProjectileFireRequest request)
        {
            if (request.ActionContext != null && request.ActionContext.Targets != null)
            {
                for (int i = 0; i < request.ActionContext.Targets.Length; i++)
                {
                    if (request.ActionContext.Targets[i].targetKind == ActionTargetKind.Direction)
                        return request.ActionContext.Targets[i].direction.sqrMagnitude > 0f
                            ? request.ActionContext.Targets[i].direction.normalized
                            : request.Direction;

                    if (request.ActionContext.Targets[i].TryGetPosition(out Vector3 targetPosition))
                    {
                        Vector3 toTarget = targetPosition - request.Origin;
                        if (toTarget.sqrMagnitude > 0f)
                            return toTarget.normalized;
                    }
                }
            }

            return request.Direction.sqrMagnitude > 0f ? request.Direction.normalized : Vector3.forward;
        }

        private static Vector3 ApplyEvenSpread(Vector3 baseDirection, int shotIndex, int shotCount, float spreadAngle)
        {
            if (shotCount <= 1 || spreadAngle <= 0f)
                return baseDirection.sqrMagnitude > 0f ? baseDirection.normalized : Vector3.forward;

            float t = shotCount == 1 ? 0.5f : shotIndex / (float)(shotCount - 1);
            float yaw = Mathf.Lerp(-spreadAngle * 0.5f, spreadAngle * 0.5f, t);
            return Quaternion.AngleAxis(yaw, Vector3.up) * baseDirection.normalized;
        }
    }
}
