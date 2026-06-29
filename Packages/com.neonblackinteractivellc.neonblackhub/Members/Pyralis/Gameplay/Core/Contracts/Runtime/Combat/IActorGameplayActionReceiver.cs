namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Receives authored gameplay action requests from input adapters or AI.
    /// Implement this on feature runtimes when an action key such as Jump, Dash,
    /// Interact, or a custom action should be owned by the selected route.
    /// </summary>
    public interface IActorGameplayActionReceiver
{
        bool TryHandleGameplayAction(string actionKey);
    }
}
