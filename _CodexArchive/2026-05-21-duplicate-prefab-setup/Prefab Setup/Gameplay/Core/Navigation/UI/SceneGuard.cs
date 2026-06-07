using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace NeonBlack.Gameplay.Core.Navigation
{
    /// <summary>
    /// Destroys duplicate EventSystems and AudioListeners during scene transitions.
    /// </summary>
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
