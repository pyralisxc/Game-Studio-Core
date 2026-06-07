namespace NeonBlack.Gameplay.Core.Navigation
{

/// <summary>
/// Lightweight static cross-scene contract for level selection.
/// Set by MainMenuController before loading a game scene.
/// </summary>
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
