using UnityEngine;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Enums;

namespace NeonBlack.Gameplay.Features.Enemies
{
    public class EnemyDetectionService
{
        public Transform ResolvePlayerTarget(Transform targetOverride)
        {
            if (targetOverride != null)
                return targetOverride;

            if (ParticipantQueryUtility.TryResolvePlayerProvider(out var provider) && provider != null)
                return provider.GetPlayerTransform();

            return null;
        }

        public bool CanSeePlayer(
            Transform owner,
            Transform player, 
            float aggroRange, 
            bool requireLineOfSight, 
            LayerMask obstacleMask,
            MovementMode movementMode)
        {
            if (player == null) return false;

            float dist = HorizontalDistance(owner, player.position, movementMode);
            if (dist > aggroRange) return false;

            if (!requireLineOfSight) return true;

            // Line-of-sight raycast
            Vector3 origin = owner.position + Vector3.up * 0.5f;
            Vector3 to = player.position + Vector3.up * 0.5f;
            return !Physics.Linecast(origin, to, obstacleMask);
        }

        public float HorizontalDistance(Transform owner, Vector3 other, MovementMode movementMode)
        {
            Vector3 diff = other - owner.position;
            diff.y = 0f;
            if (movementMode == MovementMode.TwoD) diff.z = 0f;
            return diff.magnitude;
        }
    }
}
