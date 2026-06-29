using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public abstract class GameplayTickBehaviour : MonoBehaviour
    {
        [SerializeField, Tooltip("Optional time policy used for pause/hit-pause context. Leave empty when the component already gates itself through runtime services.")]
        private MonoBehaviour gameplayTimePolicySource;

        private IGameplayTimePolicy _timePolicy;

        protected virtual GameplayTickDomain TickDomain => GameplayTickDomain.Unspecified;
        protected virtual bool UsesGameplayTick => false;
        protected virtual bool UsesFixedGameplayTick => false;
        protected virtual bool UsesLateGameplayTick => false;
        protected virtual bool RequiresGameplayTickActive => false;

        protected IGameplayTimePolicy TimePolicy => ResolveTimePolicy();

        private void Update()
        {
            if (!UsesGameplayTick)
                return;

            if (!TryBuildContext(out GameplayTickContext context))
                return;

            OnGameplayTick(context);
        }

        private void FixedUpdate()
        {
            if (!UsesFixedGameplayTick)
                return;

            if (!TryBuildContext(out GameplayTickContext context))
                return;

            OnFixedGameplayTick(context);
        }

        private void LateUpdate()
        {
            if (!UsesLateGameplayTick)
                return;

            if (!TryBuildContext(out GameplayTickContext context))
                return;

            OnLateGameplayTick(context);
        }

        protected virtual void OnGameplayTick(in GameplayTickContext context)
        {
        }

        protected virtual void OnFixedGameplayTick(in GameplayTickContext context)
        {
        }

        protected virtual void OnLateGameplayTick(in GameplayTickContext context)
        {
        }

        private bool TryBuildContext(out GameplayTickContext context)
        {
            IGameplayTimePolicy policy = ResolveTimePolicy();
            context = GameplayTickContext.FromUnity(TickDomain, policy);
            return !RequiresGameplayTickActive || policy == null || policy.CanGameplayTick;
        }

        private IGameplayTimePolicy ResolveTimePolicy()
        {
            if (_timePolicy != null)
                return _timePolicy;

            if (gameplayTimePolicySource != null)
                _timePolicy = gameplayTimePolicySource as IGameplayTimePolicy;

            return _timePolicy;
        }
    }
}
