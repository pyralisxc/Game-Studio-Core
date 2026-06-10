using NeonBlack.Gameplay.Core.Contracts;
using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Runtime
{
    /// <summary>
    /// Small runtime service registry used by the platform composition layer.
    /// </summary>
    [Obsolete("PlatformServiceRegistry is a legacy service locator. New features should prefer direct VContainer injection via IObjectResolver.")]
    [AuthoringContract(
        Capability = AuthoringCapability.Setup,
        Relevance = "Small runtime service registry used by the platform composition layer during the VContainer transition.",
        ExpertAdvice = "Legacy service locator. New features should prefer direct VContainer injection."
    )]
    public sealed class PlatformServiceRegistry
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private Func<Type, object> _fallbackResolver;

        public IEnumerable<KeyValuePair<Type, object>> Entries => _services;

        public void SetFallbackResolver(Func<Type, object> resolver)
        {
            _fallbackResolver = resolver;
        }

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

            if (_fallbackResolver != null)
            {
                object fallback = _fallbackResolver(typeof(T));
                if (fallback is T fallbackTyped)
                {
                    service = fallbackTyped;
                    return true;
                }
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
