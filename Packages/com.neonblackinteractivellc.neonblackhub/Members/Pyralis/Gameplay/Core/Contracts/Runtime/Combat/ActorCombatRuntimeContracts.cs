using UnityEngine;
using NeonBlack.Gameplay.Core.Types.Animation;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IActorCombatRequestReceiver
    {
        bool TryHandleCombatCommand(in ActorCombatCommand command);
    }

    public interface IActorCombatResultReceiver
    {
        void HandleCombatResult(in ActorCombatResult result);
    }

    public interface IActorCombatTacticalState
    {
        float MinAttackRange { get; }
        IActorFacingMirrorTarget[] FacingMirrorTargets { get; }
        bool CanAttack(float distanceToTarget);
        void DisableAllHitBoxes();
    }

    public readonly struct ActorCombatCommand
    {
        public ActorCombatCommand(
            ActorCombatCommandKind kind,
            GameObject source = null,
            Transform target = null,
            float distance = 0f,
            int direction = 0)
        {
            Kind = kind;
            Source = source;
            Target = target;
            Distance = distance;
            Direction = direction;
        }

        public ActorCombatCommandKind Kind { get; }
        public GameObject Source { get; }
        public Transform Target { get; }
        public float Distance { get; }
        public int Direction { get; }
    }

    public enum ActorCombatCommandKind
    {
        PrimaryAttack,
        SecondaryAttack,
        BlockStart,
        BlockEnd,
        CycleWeapon,
        Cancel
    }

    public readonly struct ActorCombatResult
    {
        public ActorCombatResult(
            ActorCombatResultKind kind,
            GameObject source = null,
            Transform target = null,
            ActorAnimationSignal animationSignal = ActorAnimationSignal.Custom,
            int step = 1,
            string customAnimationKey = null,
            bool isFinisher = false,
            int animatorTriggerHash = 0)
        {
            Kind = kind;
            Source = source;
            Target = target;
            AnimationSignal = animationSignal;
            Step = step;
            CustomAnimationKey = customAnimationKey;
            IsFinisher = isFinisher;
            AnimatorTriggerHash = animatorTriggerHash;
        }

        public ActorCombatResultKind Kind { get; }
        public GameObject Source { get; }
        public Transform Target { get; }
        public ActorAnimationSignal AnimationSignal { get; }
        public int Step { get; }
        public string CustomAnimationKey { get; }
        public bool IsFinisher { get; }
        public int AnimatorTriggerHash { get; }
    }

    public enum ActorCombatResultKind
    {
        AttackStarted,
        HitConfirmed,
        Blocked,
        Whiffed,
        ComboConfirmed,
        Finisher
    }
}
