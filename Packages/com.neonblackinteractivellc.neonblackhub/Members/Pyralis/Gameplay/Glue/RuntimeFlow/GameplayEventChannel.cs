using NeonBlack.Gameplay.Core.Contracts;
using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Glue.RuntimeFlow
{
    /// <summary>
    /// In-memory gameplay event stream scoped by the active VContainer lifetime.
    /// </summary>
    public sealed class GameplayEventChannel : IGameplayEventChannel
    {
        private readonly Dictionary<Type, List<Delegate>> _handlersByType = new Dictionary<Type, List<Delegate>>();

        public void Publish<TEvent>(TEvent gameplayEvent)
            where TEvent : IGameplayEvent
        {
            if ((object)gameplayEvent == null)
                throw new ArgumentNullException(nameof(gameplayEvent));

            Type eventType = typeof(TEvent);
            if (!_handlersByType.TryGetValue(eventType, out List<Delegate> handlers) || handlers.Count == 0)
                return;

            Delegate[] snapshot = handlers.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
                ((Action<TEvent>)snapshot[i]).Invoke(gameplayEvent);
        }

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
            where TEvent : IGameplayEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Type eventType = typeof(TEvent);
            if (!_handlersByType.TryGetValue(eventType, out List<Delegate> handlers))
            {
                handlers = new List<Delegate>();
                _handlersByType.Add(eventType, handlers);
            }

            handlers.Add(handler);
            return new Subscription<TEvent>(this, handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
            where TEvent : IGameplayEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Type eventType = typeof(TEvent);
            if (!_handlersByType.TryGetValue(eventType, out List<Delegate> handlers))
                return;

            handlers.Remove(handler);
            if (handlers.Count == 0)
                _handlersByType.Remove(eventType);
        }

        private sealed class Subscription<TEvent> : IDisposable
            where TEvent : IGameplayEvent
        {
            private GameplayEventChannel _channel;
            private Action<TEvent> _handler;

            public Subscription(GameplayEventChannel channel, Action<TEvent> handler)
            {
                _channel = channel;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_channel == null)
                    return;

                _channel.Unsubscribe(_handler);
                _channel = null;
                _handler = null;
            }
        }
    }
}
