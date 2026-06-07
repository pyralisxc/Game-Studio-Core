namespace NeonBlack.Gameplay.Core.Rules.TurnPhase
{
    /// <summary>
    /// Service boundary for turn-based features and UI.
    /// </summary>
    public interface ITurnOrderService
    {
        TurnRuntimeState TurnState { get; }
        int ActiveSeat { get; }
        bool TryAdvanceTurn(out string issue);
    }
}
