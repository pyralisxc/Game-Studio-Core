using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Modules.Feedback;
using NeonBlack.Gameplay.Modules.Feedback.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace NeonBlack.Gameplay.Glue.ServiceRegistration
{
    internal static class FeedbackServiceInstaller
    {
        private const string FeedbackNamespace = "NeonBlack.Gameplay.Modules.Feedback";

        public static bool ContainsLoadedSceneEvidence()
        {
            return RuntimeSceneSearch.ContainsComponent<ParticipantFeedbackService>()
                || RuntimeSceneSearch.ContainsComponentInNamespace(FeedbackNamespace);
        }

        public static void Register(IContainerBuilder builder, Component scopeRoot)
        {
            ParticipantFeedbackService feedbackService =
                RuntimeSceneSearch.Find<ParticipantFeedbackService>()
                ?? FindServiceInHierarchy<ParticipantFeedbackService>(scopeRoot);

            RegisterComponent(builder, feedbackService);
            ConfigureFeedbackComponents(feedbackService, FindServiceInHierarchy<IParticipantRoster>(scopeRoot));
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

        private static void ConfigureFeedbackComponents(
            ParticipantFeedbackService feedbackService,
            IParticipantRoster participantRoster)
        {
            if (feedbackService == null && participantRoster == null)
                return;

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                    continue;

                GameObject[] roots = scene.GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    GameObject root = roots[rootIndex];
                    if (root == null)
                        continue;

                    ParticipantFeedbackRelay[] relays = root.GetComponentsInChildren<ParticipantFeedbackRelay>(true);
                    for (int i = 0; i < relays.Length; i++)
                        relays[i]?.ConfigureRuntime(feedbackService);

                    ParticipantHealthHudBinder[] healthBinders = root.GetComponentsInChildren<ParticipantHealthHudBinder>(true);
                    for (int i = 0; i < healthBinders.Length; i++)
                        healthBinders[i]?.ConfigureRuntime(participantRoster);

                    ParticipantFeedbackHudPresenter[] presenters = root.GetComponentsInChildren<ParticipantFeedbackHudPresenter>(true);
                    for (int i = 0; i < presenters.Length; i++)
                        presenters[i]?.ConfigureRuntime(feedbackService);
                }
            }
        }
    }
}
