namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Abstraction for locating the active player at runtime.
    /// </summary>
    public interface IPlayerProvider : IGameService
    {
        UnityEngine.Transform GetPlayerTransform();

        UnityEngine.GameObject GetPlayerGameObject();
    }
}
