using UnityEngine.SceneManagement;

namespace NeonBlack.Gameplay.Core.Runtime
{
    /// <summary>
    /// Lightweight static helper that routes scene load requests through the
    /// centralized SceneLoader when available, falling back to SceneManager.
    /// </summary>
    public static class SceneNavigator
    {
        public static void LoadScene(string sceneName)
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(sceneName);
                return;
            }

            SceneManager.LoadScene(sceneName);
        }

        public static void LoadScene(int buildIndex)
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(buildIndex);
                return;
            }

            SceneManager.LoadScene(buildIndex);
        }
    }
}
