using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies
{
    public interface IEnemyAIState
    {
        void OnEnter(EnemyAI ai);
        void OnUpdate(EnemyAI ai, float deltaTime);
        void OnExit(EnemyAI ai);
    }
}