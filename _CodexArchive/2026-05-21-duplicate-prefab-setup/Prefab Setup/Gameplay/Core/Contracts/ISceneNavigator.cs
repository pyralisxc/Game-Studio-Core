namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Scene transition abstraction so adapters do not hardcode scene loading details.
    /// </summary>
    public interface ISceneNavigator
    {
        void LoadScene(string sceneName);

        void LoadScene(int buildIndex);

        void ReloadCurrentScene();
    }
}
