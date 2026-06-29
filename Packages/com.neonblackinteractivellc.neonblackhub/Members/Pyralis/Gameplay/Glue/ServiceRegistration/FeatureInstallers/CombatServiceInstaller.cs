using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Combat;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace NeonBlack.Gameplay.Glue.ServiceRegistration
{
    internal static class CombatServiceInstaller
    {
        public static bool ContainsLoadedSceneEvidence()
        {
            return RuntimeSceneSearch.ContainsComponent<PawnCombatBehaviour>()
                || RuntimeSceneSearch.ContainsComponent<PawnCombatBehaviour2D>()
                || RuntimeSceneSearch.ContainsComponent<CombatFlowController>();
        }

        public static void Register(IContainerBuilder builder, Component scopeRoot)
        {
            builder.Register<PawnComboProcessor>(VContainer.Lifetime.Transient);
            builder.Register<PawnDamageHandler>(VContainer.Lifetime.Transient);
            RegisterComponent(builder, RuntimeSceneSearch.Find<CombatFlowController>() ?? FindServiceInHierarchy<CombatFlowController>(scopeRoot));
        }

        private static void RegisterComponent<T>(IContainerBuilder builder, T component)
            where T : Component
        {
            if (component == null)
                return;

            builder.RegisterComponent(component).AsSelf().AsImplementedInterfaces();
        }

        private static T FindServiceInHierarchy<T>(Component scopeRoot) where T : class
        {
            return scopeRoot != null ? scopeRoot.GetComponentInChildren<T>(true) : null;
        }
    }
}
