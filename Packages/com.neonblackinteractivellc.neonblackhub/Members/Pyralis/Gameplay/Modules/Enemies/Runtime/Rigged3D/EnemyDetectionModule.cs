using System.Collections.Generic;
using UnityEngine;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    [AuthoringContract(
        Category = "Combat Sensors",
        CapabilityPath = "Combat/Sensors/Enemy Detection Module",
        Surface = AuthoringSurface.Goal,
        Summary = "Handles enemy line-of-sight and proximity detection.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/enemies",
        SuccessChecks = new[] { "Enemy should enter Aggro state when player enters aggroRange." },
        Tags = new[] { "capability:CombatSensors" }
    )]
    public class EnemyDetectionModule : MonoBehaviour, IRuntimeValidationProvider
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

        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (aggroRange < 0f)
                yield return RuntimeValidationIssue.Required("Aggro Range cannot be negative.");
            if (leashRange < 0f)
                yield return RuntimeValidationIssue.Required("Leash Range cannot be negative.");
            if (leashRange > 0f && leashRange < aggroRange)
                yield return RuntimeValidationIssue.Required("Leash Range should be greater than or equal to Aggro Range.");
            if (requireLineOfSight && obstacleMask == 0)
                yield return RuntimeValidationIssue.Required("Require Line Of Sight is enabled but Obstacle Mask is empty.");
        }

        private void Awake()
        {
            _detectionService = new EnemyDetectionService();
            _player = _detectionService.ResolvePlayerTarget(transform, targetOverride);
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
            _player = _detectionService.ResolvePlayerTarget(transform, targetOverride);
        }
    }
}
