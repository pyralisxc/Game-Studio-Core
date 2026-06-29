using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public enum GameplayTickDomain
    {
        Unspecified = 0,
        Character = 10,
        Combat = 20,
        Enemies = 30,
        Hazards = 40,
        Input = 50,
        Interaction = 60,
        Scoring = 70,
        Spawning = 80,
        Traversal = 90,
        Tabletop = 100
    }

    public readonly struct GameplayTickContext
    {
        public GameplayTickContext(
            GameplayTickDomain domain,
            float deltaTime,
            float fixedDeltaTime,
            float unscaledDeltaTime,
            bool isGameplayActive,
            bool isPaused,
            bool isHitPaused,
            int frameCount)
        {
            Domain = domain;
            DeltaTime = deltaTime;
            FixedDeltaTime = fixedDeltaTime;
            UnscaledDeltaTime = unscaledDeltaTime;
            IsGameplayActive = isGameplayActive;
            IsPaused = isPaused;
            IsHitPaused = isHitPaused;
            FrameCount = frameCount;
        }

        public GameplayTickDomain Domain { get; }
        public float DeltaTime { get; }
        public float FixedDeltaTime { get; }
        public float UnscaledDeltaTime { get; }
        public bool IsGameplayActive { get; }
        public bool IsPaused { get; }
        public bool IsHitPaused { get; }
        public int FrameCount { get; }

        public static GameplayTickContext FromUnity(GameplayTickDomain domain, IGameplayTimePolicy timePolicy = null)
        {
            bool active = timePolicy == null || timePolicy.IsGameplayActive;
            bool paused = timePolicy != null && timePolicy.IsPaused;
            bool hitPaused = timePolicy != null && timePolicy.IsHitPaused;
            return new GameplayTickContext(
                domain,
                Time.deltaTime,
                Time.fixedDeltaTime,
                Time.unscaledDeltaTime,
                active,
                paused,
                hitPaused,
                Time.frameCount);
        }
    }

    public interface IGameplayTimePolicy : IGameplayStateReader
    {
        bool IsPaused { get; }
        bool IsHitPaused { get; }
        bool CanGameplayTick { get; }
        float TimeScale { get; }
    }

    public interface IGameplayTickReceiver
    {
        GameplayTickDomain TickDomain { get; }
        void TickGameplay(in GameplayTickContext context);
    }

    public interface IFixedGameplayTickReceiver
    {
        GameplayTickDomain TickDomain { get; }
        void TickFixedGameplay(in GameplayTickContext context);
    }

    public interface ILateGameplayTickReceiver
    {
        GameplayTickDomain TickDomain { get; }
        void TickLateGameplay(in GameplayTickContext context);
    }
}
