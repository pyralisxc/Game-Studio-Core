using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine.SceneManagement;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Glue.SceneFlow.Navigation
{
    /// <summary>
    /// Lightweight static helper for simple scene loads when no authored
    /// ISceneNavigator service is available. User-facing components should
    /// prefer explicit ISceneNavigator references.
    /// </summary>
    [AuthoringContract(
        Category = "Setup",
        Surface = AuthoringSurface.Goal,
        Summary = "Static fallback for direct SceneManager loads when no authored ISceneNavigator route exists.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/navigation",
        SuccessChecks = new[] { "Calling LoadScene correctly changes the active Unity scene." },
        Tags = new[] { "capability:Setup" }
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
