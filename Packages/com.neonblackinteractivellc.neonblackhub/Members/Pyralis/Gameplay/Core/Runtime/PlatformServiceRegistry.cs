using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Runtime
{
    /// <summary>
    /// Small runtime service registry used by the platform composition layer.
    /// </summary>
    public sealed class PlatformServiceRegistry
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public IEnumerable<KeyValuePair<Type, object>> Entries => _services;

        public void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                return;
            }

            _services[typeof(T)] = service;
        }

        public bool TryResolve<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out object value) && value is T typed)
            {
                service = typed;
                return true;
            }

            service = null;
            return false;
        }

        public T Resolve<T>() where T : class
        {
            if (TryResolve(out T service))
            {
                return service;
            }

            throw new InvalidOperationException($"[PlatformServiceRegistry] Service not registered: {typeof(T).Name}");
        }

        public void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        public void Clear()
        {
            _services.Clear();
        }
    }
}
