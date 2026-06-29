using System;

namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Typed runtime event stream for cross-subsystem notifications.
    /// </summary>
    public interface IGameplayEventChannel
    {
        void Publish<TEvent>(TEvent gameplayEvent)
            where TEvent : IGameplayEvent;

        IDisposable Subscribe<TEvent>(Action<TEvent> handler)
            where TEvent : IGameplayEvent;

        void Unsubscribe<TEvent>(Action<TEvent> handler)
            where TEvent : IGameplayEvent;
    }
}
