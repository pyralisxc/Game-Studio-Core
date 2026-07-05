using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;
namespace NeonBlack.Gameplay.Glue.SceneFlow.Navigation
{

/// <summary>
/// Lightweight static cross-scene contract for level selection.
/// Set by MainMenuController before loading a game scene.
/// </summary>
[AuthoringContract(
        Category = "Setup",
        Surface = AuthoringSurface.Goal,
        Summary = "Lightweight static cross-scene contract for level selection metadata.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/level-session",
        SetupSteps = new[] { "Set LevelSession.ChosenSceneName from the UI or mission selector.", "Clear the session using LevelSession.Clear() when returning to the menu." },
        SuccessChecks = new[] { "Verify ChosenSceneName is set correctly in the destination scene's Start method." },
        Tags = new[] { "capability:Setup" }
    )]
    public static class LevelSession
{
    public static string ChosenSceneName { get; set; }
    public static bool IsRandom { get; set; }

    public static void Clear()
    {
        ChosenSceneName = null;
        IsRandom = false;
    }
}
}
