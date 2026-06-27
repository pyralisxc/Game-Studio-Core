using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace NeonBlack.Gameplay.Modules.Feedback
{
    public static class PyralisFeedbackServiceInstaller
    {
        private const string FeedbackNamespace = "NeonBlack.Gameplay.Modules.Feedback";

        public static bool ContainsLoadedSceneEvidence()
        {
            return PyralisRuntimeSceneSearch.ContainsComponent<ParticipantFeedbackService>()
                || PyralisRuntimeSceneSearch.ContainsComponentInNamespace(FeedbackNamespace);
        }

        public static void Register(IContainerBuilder builder, Component scopeRoot)
        {
            RegisterComponent(
                builder,
                PyralisRuntimeSceneSearch.Find<ParticipantFeedbackService>()
                ?? FindServiceInHierarchy<ParticipantFeedbackService>(scopeRoot));
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
