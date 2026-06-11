using NeonBlack.Gameplay.Core.Contracts;
namespace NeonBlack.Gameplay.Core.Navigation
{

/// <summary>
/// Lightweight static cross-scene contract for level selection.
/// Set by MainMenuController before loading a game scene.
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.Setup,
    Relevance = "Lightweight static cross-scene contract for level selection metadata.",
    ExpertAdvice = "Set ChosenSceneName before triggering a SceneManager.LoadScene call.",
    FirstProof = "Verify ChosenSceneName is set correctly in the destination scene's Start method.",
    AssignmentFields = new[] { nameof(ChosenSceneName), nameof(IsRandom) },
    NativeSetup = new[] { "Set LevelSession.ChosenSceneName from the UI or mission selector.", "Clear the session using LevelSession.Clear() when returning to the menu." },
    DocumentationURL = "https://docs.neonblack.com/pyralis/level-session"
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
