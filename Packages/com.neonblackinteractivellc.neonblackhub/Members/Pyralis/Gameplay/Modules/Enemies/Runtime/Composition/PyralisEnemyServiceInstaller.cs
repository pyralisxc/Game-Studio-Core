using NeonBlack.Gameplay.Core.Contracts;
using VContainer;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    public static class PyralisEnemyServiceInstaller
    {
        public static bool ContainsLoadedSceneEvidence()
        {
            return PyralisRuntimeSceneSearch.ContainsComponent<EnemyAI>();
        }

        public static void Register(IContainerBuilder builder)
        {
            builder.Register<EnemyDetectionService>(Lifetime.Singleton);
            builder.Register<EnemyCombatProcessor>(Lifetime.Singleton);
        }
    }
}
