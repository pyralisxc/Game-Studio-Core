namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Receives authored gameplay action requests from input adapters or AI.
    /// Implement this on feature runtimes when an action key such as Jump, Dash,
    /// Interact, or a custom action should be owned by the selected route.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Input | AuthoringCapability.Session, 
        Relevance = "Receives and handles gameplay actions on an actor.", 
        Axioms = AuthoringWorldAxiom.None,
        FirstProof = "Verify that TryHandleGameplayAction is called when an input action is performed.",
        NativeSetup = new[] { "Implement interface in a feature module" }
    )]
    public interface IActorGameplayActionReceiver
{
        bool TryHandleGameplayAction(string actionKey);
    }
}
