using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies
{
    public class AttackState : IEnemyAIState
    {
        public void OnEnter(EnemyAI ai) { }

        public void OnUpdate(EnemyAI ai, float deltaTime)
        {
            float dist = ai.DetectionModule.HorizontalDistance(ai.MovementMode);
            ai.MovementModule.FaceTarget(ai.DetectionModule.PlayerPosition, ai.PresentationCamera, ai.VisualRoot, ai.SpriteDefaultFacesRight, ai.CombatModule.HitBoxZones);
            ai.MovementModule.ApplyStationaryMotion(deltaTime);

            if (dist > ai.CombatModule.MinAttackRange * 1.4f)
            {
                ai.ChangeState(EnemyAI.EnemyState.Chase);
                return;
            }

            if (ai.CombatModule.CanAttack(dist))
            {
                ai.CombatModule.ExecuteAttack(dist, ai.AnimationModule);
            }
        }

        public void OnExit(EnemyAI ai) { }
    }
}