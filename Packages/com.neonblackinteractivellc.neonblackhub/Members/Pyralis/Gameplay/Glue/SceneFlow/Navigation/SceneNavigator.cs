using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine.SceneManagement;

namespace NeonBlack.Gameplay.Glue.SceneFlow.Navigation
{
    /// <summary>
    /// Lightweight static helper for simple scene loads when no authored
    /// ISceneNavigator service is available. User-facing components should
    /// prefer explicit ISceneNavigator references.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Setup,
        Relevance = "Static fallback for direct SceneManager loads when no authored ISceneNavigator route exists.",
        Proof = "Calling LoadScene correctly changes the active Unity scene.",
        ExpertAdvice = "SceneNavigator is a static bypass for utility scripts. User-facing runtime components should depend on ISceneNavigator, with SceneFader as the current menu/game-shell route and SceneLoader as a lightweight fallback.",
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
