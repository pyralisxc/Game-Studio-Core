using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Enemies;
using VContainer;

namespace NeonBlack.Gameplay.Glue.ServiceRegistration
{
    internal static class PyralisEnemyServiceInstaller
    {
        public static bool ContainsLoadedSceneEvidence()
        {
            return PyralisRuntimeSceneSearch.ContainsComponent<EnemyAI>();
        }

        public static void Register(IContainerBuilder builder)
        {
            builder.Register<EnemyDetectionService>(VContainer.Lifetime.Singleton);
        }
    }
}
