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

    [AuthoringContract(
        Capability = AuthoringCapability.CombatState,
        Relevance = "Provides the current health and life state of an actor.",
        ExpertAdvice = "Use Damaged and Died events to trigger UI updates and death sequences. Ensure Faction is correctly set for friendly-fire filtering.",
        Axioms = AuthoringWorldAxiom.None,
        NativeSetup = new[] { "Implement on HealthComponent or Actor core.", "Initialize MaxHealth to a value greater than zero." },
        AssignmentFields = new[] { nameof(IActorHealthState.CurrentHealth), nameof(IActorHealthState.MaxHealth) },
        FirstProof = "Actor takes damage and its health percent decreases.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat/health"
    )]
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
