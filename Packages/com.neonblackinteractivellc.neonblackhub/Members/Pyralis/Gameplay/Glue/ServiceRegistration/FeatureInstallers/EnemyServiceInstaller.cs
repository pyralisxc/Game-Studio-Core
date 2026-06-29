using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Enemies;
using VContainer;

namespace NeonBlack.Gameplay.Glue.ServiceRegistration
{
    internal static class EnemyServiceInstaller
    {
        public static bool ContainsLoadedSceneEvidence()
        {
            return RuntimeSceneSearch.ContainsComponent<EnemyAI>();
        }

        public static void Register(IContainerBuilder builder)
        {
            builder.Register<EnemyDetectionService>(VContainer.Lifetime.Singleton);
        }
    }
}
