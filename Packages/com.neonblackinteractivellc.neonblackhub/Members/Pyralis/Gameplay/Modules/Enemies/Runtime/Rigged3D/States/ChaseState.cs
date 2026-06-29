using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Enemies
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

            if (ai.CombatTactics != null && dist <= ai.CombatTactics.MinAttackRange * 1.5f)
            {
                ai.ChangeState(EnemyAI.EnemyState.Attack);
                return;
            }

            ai.MovementModule.MoveToward(ai.DetectionModule.PlayerPosition, ai.MoveSpeed, ai.StatusMoveSpeedMultiplier, deltaTime, ai.PresentationCamera, ai.VisualRoot, ai.SpriteDefaultFacesRight, ai.CombatTactics?.FacingMirrorTargets);
        }

        public void OnExit(EnemyAI ai) { }
    }
}
