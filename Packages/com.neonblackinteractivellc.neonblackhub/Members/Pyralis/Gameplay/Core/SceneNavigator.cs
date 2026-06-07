using UnityEngine.SceneManagement;

namespace NeonBlack.Gameplay.Core.Runtime
{
    /// <summary>
    /// Lightweight static helper for simple scene loads when no authored
    /// ISceneNavigator service is available. User-facing components should
    /// prefer explicit ISceneNavigator references.
    /// </summary>
    public static class SceneNavigator
    {
        public static void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public static void LoadScene(int buildIndex)
        {
            SceneManager.LoadScene(buildIndex);
        }
    }
}
