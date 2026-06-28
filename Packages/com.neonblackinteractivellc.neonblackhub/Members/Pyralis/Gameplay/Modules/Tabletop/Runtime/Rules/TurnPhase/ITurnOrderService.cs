using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Tabletop;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Tabletop.Runtime
{
    /// <summary>
    /// Service boundary for turn-based features and UI.
    /// </summary>
    [AuthoringContract(
        Category = "Tabletop, Turn Based",
        Surface = AuthoringSurface.Goal,
        Summary = "Manages turn sequence and active participant in turn-based games.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/tabletop",
        SetupSteps = new[] { "Implement interface in a service component" },
        SuccessChecks = new[] { "Verify that TryAdvanceTurn cycles the turn order correctly." },
        Tags = new[] { "capability:Tabletop", "capability:TurnBased", "axiom:TurnBased" }
    )]
public interface ITurnOrderService
{
        TurnRuntimeState TurnState { get; }
        int ActiveSeat { get; }
        bool TryAdvanceTurn(out string issue);
    }
}
