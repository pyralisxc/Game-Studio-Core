using UnityEngine;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Features.Enemies
{
    [AuthoringContract(
        Capability = AuthoringCapability.CombatSensors,
        Relevance = "Handles enemy line-of-sight and proximity detection.",
        AssignmentFields = new[] { nameof(aggroRange), nameof(leashRange), nameof(requireLineOfSight), nameof(obstacleMask) },
        FirstProof = "Enemy should enter Aggro state when player enters aggroRange.",
        ExpertAdvice = "Increase leashRange to prevent enemies from resetting too early in large arenas.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/enemies"
    )]
    public class EnemyDetectionModule : MonoBehaviour
{
        [Header("Detection")]
        [SerializeField] private float aggroRange = 8f;
        [SerializeField] private float leashRange = 16f;
        [SerializeField] private bool requireLineOfSight = false;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private Transform targetOverride;

        private EnemyDetectionService _detectionService;
        private Transform _player;

        public Transform PlayerTarget => _player;
        public float AggroRange => aggroRange;
        public float LeashRange => leashRange;

        private void Awake()
        {
            _detectionService = new EnemyDetectionService();
            _player = _detectionService.ResolvePlayerTarget(targetOverride);
        }

        public bool CanSeePlayer(MovementMode movementMode)
        {
            return _detectionService.CanSeePlayer(transform, _player, aggroRange, requireLineOfSight, obstacleMask, movementMode);
        }

        public float HorizontalDistance(MovementMode movementMode)
        {
            if (_player == null) return float.MaxValue;
            return _detectionService.HorizontalDistance(transform, _player.position, movementMode);
        }

        public Vector3 PlayerPosition => _player != null ? _player.position : transform.position;

        public void SetTargetOverride(Transform target)
        {
            targetOverride = target;
            _player = _detectionService.ResolvePlayerTarget(targetOverride);
        }
    }
}