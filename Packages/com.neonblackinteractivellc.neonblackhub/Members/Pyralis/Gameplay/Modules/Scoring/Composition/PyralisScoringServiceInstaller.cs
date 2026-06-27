using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace NeonBlack.Gameplay.Modules.Scoring
{
    public static class PyralisScoringServiceInstaller
    {
        public static bool ContainsLoadedSceneEvidence()
        {
            return PyralisRuntimeSceneSearch.ContainsComponent<ParticipantScoreService>()
                || PyralisRuntimeSceneSearch.ContainsComponent<LeaderboardManager>()
                || PyralisRuntimeSceneSearch.ContainsComponent<StillnessBonus2D>();
        }

        public static void Register(IContainerBuilder builder, Component scopeRoot)
        {
            RegisterComponent(
                builder,
                PyralisRuntimeSceneSearch.Find<ParticipantScoreService>()
                ?? FindServiceInHierarchy<ParticipantScoreService>(scopeRoot));

            RegisterComponent(
                builder,
                PyralisRuntimeSceneSearch.Find<LeaderboardManager>()
                ?? FindServiceInHierarchy<LeaderboardManager>(scopeRoot));
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
