using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IEncounterSpawnSource
    {
        event Action<IActorHealthState> ActorSpawned;
        bool IsFinished { get; }
        IReadOnlyList<IActorHealthState> TrackedActors { get; }
    }
}
