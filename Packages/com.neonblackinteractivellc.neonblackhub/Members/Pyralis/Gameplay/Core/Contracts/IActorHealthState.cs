using System;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public enum Faction
    {
        Neutral,
        Player,
        Enemy
    }

    public interface IActorHealthState
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsDead { get; }
        float HealthPercent { get; }
        Faction Faction { get; }

        event Action<float> Damaged;
        event Action<float> Healed;
        event Action Died;

        void TakeDamage(float amount, Vector3 hitPoint, GameObject source = null);
        void Heal(float amount);
    }
}
