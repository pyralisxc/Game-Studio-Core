using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine.SceneManagement;

namespace NeonBlack.Gameplay.Core.Runtime
{
    /// <summary>
    /// Lightweight static helper for simple scene loads when no authored
    /// ISceneNavigator service is available. User-facing components should
    /// prefer explicit ISceneNavigator references.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Setup,
        Relevance = "Lightweight static helper for simple scene loads. Prefers ISceneNavigator when available.",
        FirstProof = "Calling LoadScene correctly changes the active Unity scene.",
        ExpertAdvice = "SceneNavigator is a static bypass for logic scripts. For production flows with UI faders, prefer injecting ISceneNavigator (SceneLoader) into your systems.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/navigation"
    )]
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
