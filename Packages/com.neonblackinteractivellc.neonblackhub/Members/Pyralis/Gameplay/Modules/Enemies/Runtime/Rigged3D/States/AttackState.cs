using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    public class AttackState : IEnemyAIState
    {
        public void OnEnter(EnemyAI ai) { }

        public void OnUpdate(EnemyAI ai, float deltaTime)
        {
            float dist = ai.DetectionModule.HorizontalDistance(ai.MovementMode);
            ai.MovementModule.FaceTarget(ai.DetectionModule.PlayerPosition, ai.PresentationCamera, ai.VisualRoot, ai.SpriteDefaultFacesRight, ai.CombatTactics?.FacingMirrorTargets);
            ai.MovementModule.ApplyStationaryMotion(deltaTime);

            IActorCombatTacticalState tactics = ai.CombatTactics;
            if (tactics == null || ai.CombatRequests == null)
            {
                ai.ChangeState(EnemyAI.EnemyState.Chase);
                return;
            }

            if (dist > tactics.MinAttackRange * 1.4f)
            {
                ai.ChangeState(EnemyAI.EnemyState.Chase);
                return;
            }

            if (tactics.CanAttack(dist))
            {
                ai.CombatRequests.TryHandleCombatCommand(new ActorCombatCommand(
                    ActorCombatCommandKind.PrimaryAttack,
                    ai.gameObject,
                    ai.DetectionModule.PlayerTarget,
                    dist));
            }
        }

        public void OnExit(EnemyAI ai) { }
    }
}
