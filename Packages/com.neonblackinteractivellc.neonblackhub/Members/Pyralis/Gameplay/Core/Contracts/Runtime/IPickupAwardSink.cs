using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public enum PickupAwardOutcome
    {
        Collected,
        DestroyedWithoutAward
    }

    public readonly struct PickupAwardPayload
    {
        public PickupAwardPayload(GameObject collector, Vector3 worldPosition, int scoreValue, PickupAwardOutcome outcome)
        {
            Collector = collector;
            WorldPosition = worldPosition;
            ScoreValue = Mathf.Max(0, scoreValue);
            Outcome = outcome;
        }

        public GameObject Collector { get; }

        public Vector3 WorldPosition { get; }

        public int ScoreValue { get; }

        public PickupAwardOutcome Outcome { get; }
    }

    public interface IPickupAwardSink
    {
        void ApplyAward(in PickupAwardPayload payload);
    }
}
