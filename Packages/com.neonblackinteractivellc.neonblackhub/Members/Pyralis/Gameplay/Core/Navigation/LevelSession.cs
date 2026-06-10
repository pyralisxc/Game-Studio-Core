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
        ExpertAdvice = "Used to pass data between the Main Menu and the Gameplay scenes without persistent assets."
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
