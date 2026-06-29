using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    [AddComponentMenu("NeonBlack/Gameplay/Enemies/Enemy Ambient Component")]
    [AuthoringContract(
        StableId = "enemy.ambient.component",
        Category = "Combat, Animation",
        CapabilityPath = "Combat/Actions/Enemy Ambient Component",
        Surface = AuthoringSurface.Profile,
        RequiredFields = new[] { nameof(ambientProfile) },
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Modules.Enemies.EnemyAI" },
        RequiredInterfaces = new[] { typeof(EnemyAmbientComponent) },
        SetupSteps = new[]
        {
            "add EnemyAmbientComponent to the enemy root",
            "assign EnemyAmbientProfile"
        },
        SuccessChecks = new[] { "Verify that the enemy performs ambient look-around animations while patrolling." },
        Tags = new[] { "capability:Combat", "capability:Animation", "lane:Enemy" },
        Selectable = false
    )]
    public class EnemyAmbientComponent : GameplayTickBehaviour
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

        protected override GameplayTickDomain TickDomain => GameplayTickDomain.Enemies;
        protected override bool UsesGameplayTick => true;

        protected override void OnGameplayTick(in GameplayTickContext context)
        {
            if (ambientProfile == null || !ambientProfile.enableAmbientLookAround || _enemyAI == null)
                return;

            if (ambientProfile.requirePatrolState && !_enemyAI.IsPatrolling)
                return;

            if (ambientProfile.suppressDuringReactionLock && _reactionState != null && _reactionState.IsReactionLocked)
                return;

            _lookAroundTimer -= context.DeltaTime;
            if (_lookAroundTimer > 0f)
                return;

            _lookAroundTimer = ambientProfile.lookAroundInterval;
            _animation?.TriggerSignal(ActorAnimationSignal.LookAround);
        }
    }
}
