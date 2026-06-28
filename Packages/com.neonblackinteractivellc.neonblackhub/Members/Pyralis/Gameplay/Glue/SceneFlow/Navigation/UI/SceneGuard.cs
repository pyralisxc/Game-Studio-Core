using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Glue.SceneFlow.Navigation
{
    /// <summary>
    /// Destroys duplicate EventSystems and AudioListeners during scene transitions.
    /// </summary>
    [AuthoringContract(
        Category = "Setup",
        CapabilityPath = "Core Setup/Navigation/Scene Guard",
        Surface = AuthoringSurface.RequiredSetup,
        Summary = "Lightweight scene-transition cleanup helper that destroys duplicate active EventSystems and AudioListeners at Awake.",
        SetupSteps = new[] 
        { 
            "Place this in scenes that may be loaded after a persistent UI or camera bootstrap."
        },
        SuccessChecks = new[] { "Load a scene with a duplicate EventSystem and verify SceneGuard destroys it in the console." },
        Tags = new[] { "capability:Setup" },
        Selectable = false
    )]
    public class SceneGuard : MonoBehaviour
    {
        private void Awake()
        {
            EnforceSingleEventSystem();
            EnforceSingleAudioListener();
        }

        private void EnforceSingleEventSystem()
        {
            EventSystem[] systems = FindObjectsByType<EventSystem>(FindObjectsInactive.Exclude);
            if (systems.Length <= 1)
            {
                return;
            }

            EventSystem toKeep = systems[0];
            Scene activeScene = SceneManager.GetActiveScene();
            foreach (EventSystem system in systems)
            {
                if (system.gameObject.scene == activeScene)
                {
                    toKeep = system;
                    break;
                }
            }

            foreach (EventSystem system in systems)
            {
                if (system == toKeep)
                {
                    continue;
                }

                Debug.Log($"[SceneGuard] Destroying duplicate EventSystem on '{system.gameObject.name}'.");
                Destroy(system.gameObject);
            }
        }

        private void EnforceSingleAudioListener()
        {
            AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude);
            if (listeners.Length <= 1)
            {
                return;
            }

            AudioListener toKeep = listeners[0];
            Scene activeScene = SceneManager.GetActiveScene();
            foreach (AudioListener listener in listeners)
            {
                if (listener.gameObject.scene == activeScene)
                {
                    toKeep = listener;
                    break;
                }
            }

            foreach (AudioListener listener in listeners)
            {
                if (listener == toKeep)
                {
                    continue;
                }

                Debug.Log($"[SceneGuard] Destroying duplicate AudioListener on '{listener.gameObject.name}'.");
                Destroy(listener);
            }
        }
    }
}
