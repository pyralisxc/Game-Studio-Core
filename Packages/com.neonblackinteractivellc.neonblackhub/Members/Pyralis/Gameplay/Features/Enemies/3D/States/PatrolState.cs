using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies
{
    public class PatrolState : IEnemyAIState
    {
        public void OnEnter(EnemyAI ai) { }

        public void OnUpdate(EnemyAI ai, float deltaTime)
        {
            if (ai.DetectionModule.CanSeePlayer(ai.MovementMode))
            {
                ai.ChangeState(EnemyAI.EnemyState.Chase);
                return;
            }

            Vector3 target = ai.GetPatrolTarget();
            ai.MovementModule.MoveToward(target, ai.MoveSpeed * 0.6f, ai.StatusMoveSpeedMultiplier, ai.PresentationCamera, ai.VisualRoot, ai.SpriteDefaultFacesRight, ai.CombatModule.HitBoxZones);

            if (Vector2.Distance(new Vector2(ai.transform.position.x, ai.transform.position.z), new Vector2(target.x, target.z)) < ai.WaypointTolerance)
            {
                ai.AdvancePatrol();
            }
        }

        public void OnExit(EnemyAI ai) { }
    }
}