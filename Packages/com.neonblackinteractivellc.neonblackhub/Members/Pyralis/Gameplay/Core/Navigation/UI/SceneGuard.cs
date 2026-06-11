using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace NeonBlack.Gameplay.Core.Navigation
{
    /// <summary>
    /// Destroys duplicate EventSystems and AudioListeners during scene transitions.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Setup,
        Relevance = "Lightweight scene-transition cleanup helper that destroys duplicate active EventSystems and AudioListeners at Awake.",
        NativeSetup = new[] 
        { 
            "Place this in scenes that may be loaded after a persistent UI or camera bootstrap."
        },
        FirstProof = "Load a scene with a duplicate EventSystem and verify SceneGuard destroys it in the console.",
        ExpertAdvice = "Keep one active EventSystem and one active AudioListener as the expected final state. Use it as cleanup support, not as a substitute for clean scene ownership."
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
