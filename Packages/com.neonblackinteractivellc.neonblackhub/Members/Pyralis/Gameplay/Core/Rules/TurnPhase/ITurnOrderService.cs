using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Core.Rules.TurnPhase
{
    /// <summary>
    /// Service boundary for turn-based features and UI.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Tabletop | AuthoringCapability.TurnBased, 
        Relevance = "Manages turn sequence and active participant in turn-based games.", 
        Axioms = AuthoringWorldAxiom.TurnBased,
        FirstProof = "Verify that TryAdvanceTurn cycles the turn order correctly.",
        NativeSetup = new[] { "Implement interface in a service component" },
        ExpertAdvice = "Central authority for whose turn it is. Use this to gate player input and trigger AI decision phases in Tabletop modes.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/tabletop"
    )]
public interface ITurnOrderService
{
        TurnRuntimeState TurnState { get; }
        int ActiveSeat { get; }
        bool TryAdvanceTurn(out string issue);
    }
}
