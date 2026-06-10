using NeonBlack.Gameplay.Core.Contracts;
using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Rules.TurnPhase
{
    /// <summary>
    /// Runtime cursor for seat-based turn order.
    /// </summary>
    [Serializable]
    [AuthoringContract(
        Capability = AuthoringCapability.TurnBased,
        Relevance = "Runtime cursor for seat-based turn order tracking.",
        FirstProof = "Advancing the turn correctly moves the active seat index through the defined order."
    )]
    public sealed class TurnRuntimeState
    {
        private readonly int[] _seatOrder;
        private int _activeSeatIndex;

        public TurnRuntimeState(IReadOnlyList<int> seatOrder, int startingSeat)
        {
            if (seatOrder == null || seatOrder.Count == 0)
            {
                _seatOrder = Array.Empty<int>();
                RoundIndex = 0;
                TurnIndex = 0;
                _activeSeatIndex = -1;
                return;
            }

            _seatOrder = new int[seatOrder.Count];
            for (int i = 0; i < seatOrder.Count; i++)
                _seatOrder[i] = seatOrder[i];

            _activeSeatIndex = 0;
            for (int i = 0; i < _seatOrder.Length; i++)
            {
                if (_seatOrder[i] == startingSeat)
                {
                    _activeSeatIndex = i;
                    break;
                }
            }

            RoundIndex = 1;
            TurnIndex = 0;
        }

        public int ActiveSeat => _activeSeatIndex >= 0 && _activeSeatIndex < _seatOrder.Length ? _seatOrder[_activeSeatIndex] : -1;
        public int RoundIndex { get; private set; }
        public int TurnIndex { get; private set; }
        public IReadOnlyList<int> SeatOrder => _seatOrder;

        public bool TryAdvance(out string issue)
        {
            if (_seatOrder.Length == 0)
            {
                issue = "Turn order has no seats.";
                return false;
            }

            _activeSeatIndex++;
            if (_activeSeatIndex >= _seatOrder.Length)
            {
                _activeSeatIndex = 0;
                RoundIndex++;
            }

            TurnIndex++;
            issue = string.Empty;
            return true;
        }
    }
}
