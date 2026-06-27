using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace NeonBlack.Gameplay.Modules.Combat
{
    public static class PyralisCombatServiceInstaller
    {
        public static bool ContainsLoadedSceneEvidence()
        {
            return PyralisRuntimeSceneSearch.ContainsComponent<PawnCombatBehaviour>()
                || PyralisRuntimeSceneSearch.ContainsComponent<PawnCombatBehaviour2D>()
                || PyralisRuntimeSceneSearch.ContainsComponent<BattleManager>();
        }

        public static void Register(IContainerBuilder builder, Component scopeRoot)
        {
            builder.Register<PawnComboProcessor>(Lifetime.Transient);
            builder.Register<PawnDamageHandler>(Lifetime.Transient);
            RegisterComponent(builder, PyralisRuntimeSceneSearch.Find<BattleManager>() ?? FindServiceInHierarchy<BattleManager>(scopeRoot));
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
