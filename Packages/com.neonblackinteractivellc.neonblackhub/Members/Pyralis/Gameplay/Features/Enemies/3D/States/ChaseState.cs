using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies
{
    public class ChaseState : IEnemyAIState
    {
        public void OnEnter(EnemyAI ai) { }

        public void OnUpdate(EnemyAI ai, float deltaTime)
        {
            float dist = ai.DetectionModule.HorizontalDistance(ai.MovementMode);
            if (dist > ai.DetectionModule.LeashRange)
            {
                ai.ChangeState(EnemyAI.EnemyState.Patrol);
                return;
            }

            if (dist <= ai.CombatModule.MinAttackRange * 1.5f)
            {
                ai.ChangeState(EnemyAI.EnemyState.Attack);
                return;
            }

            ai.MovementModule.MoveToward(ai.DetectionModule.PlayerPosition, ai.MoveSpeed, ai.StatusMoveSpeedMultiplier, ai.PresentationCamera, ai.VisualRoot, ai.SpriteDefaultFacesRight, ai.CombatModule.HitBoxZones);
        }

        public void OnExit(EnemyAI ai) { }
    }
}