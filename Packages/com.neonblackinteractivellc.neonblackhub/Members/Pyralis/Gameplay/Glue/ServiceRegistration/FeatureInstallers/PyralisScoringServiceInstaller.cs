using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Scoring;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace NeonBlack.Gameplay.Glue.ServiceRegistration
{
    internal static class PyralisScoringServiceInstaller
    {
        public static bool ContainsLoadedSceneEvidence()
        {
            return PyralisRuntimeSceneSearch.ContainsComponent<ParticipantScoreService>()
                || PyralisRuntimeSceneSearch.ContainsComponent<StillnessBonus2D>();
        }

        public static void Register(IContainerBuilder builder, Component scopeRoot)
        {
            ParticipantScoreService participantScoreService =
                PyralisRuntimeSceneSearch.Find<ParticipantScoreService>()
                ?? FindServiceInHierarchy<ParticipantScoreService>(scopeRoot);

            RegisterComponent(
                builder,
                participantScoreService);
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
