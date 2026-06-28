using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    [AddComponentMenu("NeonBlack/Gameplay/Enemies/Enemy Ambient Component")]
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Animation,
        ModuleId = "enemy.ambient",
        Lane = "Enemy",
        ProfileType = typeof(EnemyAmbientProfile),
        RequiredInterfaces = new[] { typeof(EnemyAmbientComponent) },
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Modules.Enemies.EnemyAI" },
        AssignmentFields = new[] { nameof(ambientProfile) },
        Proof = "Verify that the enemy performs ambient look-around animations while patrolling.",
        ProofTargetId = "proof.npc-enemy-behavior",
        NativeSetup = new[]
        {
            "add EnemyAmbientComponent to the enemy root",
            "assign EnemyAmbientProfile"
        },
        CustomizationMoments = new[]
        {
            "EnemyAmbientProfile.enableAmbientLookAround",
            "EnemyAmbientProfile.lookAroundInterval"
        },
        CapabilityPath = "Combat/Actions/Enemy Ambient Component"
    )]
    public class EnemyAmbientComponent : MonoBehaviour
{
        [SerializeField] private EnemyAmbientProfile ambientProfile;
        private EnemyAI _enemyAI;
        private IEnemyReactionState _reactionState;
        private IActorAnimationController _animation;
        private float _lookAroundTimer;

        private void Awake()
        {
            _enemyAI = GetComponent<EnemyAI>();
            _reactionState = GetComponent<IEnemyReactionState>();
            _animation = GetComponent<IActorAnimationController>();
            ambientProfile?.Sanitize();
            _lookAroundTimer = ambientProfile != null ? ambientProfile.lookAroundInterval : 0f;
        }

        private void Update()
        {
            if (ambientProfile == null || !ambientProfile.enableAmbientLookAround || _enemyAI == null)
                return;

            if (ambientProfile.requirePatrolState && !_enemyAI.IsPatrolling)
                return;

            if (ambientProfile.suppressDuringReactionLock && _reactionState != null && _reactionState.IsReactionLocked)
                return;

            _lookAroundTimer -= Time.deltaTime;
            if (_lookAroundTimer > 0f)
                return;

            _lookAroundTimer = ambientProfile.lookAroundInterval;
            _animation?.TriggerSignal(ActorAnimationSignal.LookAround);
        }
    }
}
