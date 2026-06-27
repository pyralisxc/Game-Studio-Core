using NeonBlack.Gameplay.Modules.Actor.Composition;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Combat
{
    internal sealed class PawnCombatRuntimeReferences
    {
        private PawnCombatRuntimeReferences(
            ICharacterMotorState motor,
            ActorAnimationDriver animationDriver,
            IActorFeedbackPublisher feedbackPublisher,
            PawnHitBoxModule hitBoxModule,
            PawnDamageModule damageModule,
            PawnProjectileModule projectileModule,
            PawnBlockModule blockModule,
            PawnWeaponModule weaponModule)
        {
            Motor = motor;
            AnimationDriver = animationDriver;
            FeedbackPublisher = feedbackPublisher;
            HitBoxModule = hitBoxModule;
            DamageModule = damageModule;
            ProjectileModule = projectileModule;
            BlockModule = blockModule;
            WeaponModule = weaponModule;
        }

        public ICharacterMotorState Motor { get; }
        public ActorAnimationDriver AnimationDriver { get; }
        public IActorFeedbackPublisher FeedbackPublisher { get; }
        public PawnHitBoxModule HitBoxModule { get; }
        public PawnDamageModule DamageModule { get; }
        public PawnProjectileModule ProjectileModule { get; }
        public PawnBlockModule BlockModule { get; }
        public PawnWeaponModule WeaponModule { get; }

        public static PawnCombatRuntimeReferences Capture(Component owner)
        {
            return new PawnCombatRuntimeReferences(
                owner.GetComponent<ICharacterMotorState>(),
                owner.GetComponent<ActorAnimationDriver>(),
                owner.GetComponent<IActorFeedbackPublisher>(),
                owner.GetComponent<PawnHitBoxModule>(),
                owner.GetComponent<PawnDamageModule>(),
                owner.GetComponent<PawnProjectileModule>(),
                owner.GetComponent<PawnBlockModule>(),
                owner.GetComponent<PawnWeaponModule>());
        }
    }
}
