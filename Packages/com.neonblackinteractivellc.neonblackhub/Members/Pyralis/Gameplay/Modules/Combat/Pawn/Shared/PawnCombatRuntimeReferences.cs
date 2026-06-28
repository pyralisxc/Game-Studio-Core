using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Combat
{
    internal sealed class PawnCombatRuntimeReferences
    {
        private PawnCombatRuntimeReferences(
            IActorCombatMovementState motor,
            IActorCombatResultReceiver[] combatResultReceivers,
            IActorFeedbackPublisher feedbackPublisher,
            PawnHitBoxModule hitBoxModule,
            PawnDamageModule damageModule,
            PawnProjectileModule projectileModule,
            PawnBlockModule blockModule,
            PawnWeaponModule weaponModule)
        {
            Motor = motor;
            CombatResultReceivers = combatResultReceivers;
            FeedbackPublisher = feedbackPublisher;
            HitBoxModule = hitBoxModule;
            DamageModule = damageModule;
            ProjectileModule = projectileModule;
            BlockModule = blockModule;
            WeaponModule = weaponModule;
        }

        public IActorCombatMovementState Motor { get; }
        public IActorCombatResultReceiver[] CombatResultReceivers { get; }
        public IActorFeedbackPublisher FeedbackPublisher { get; }
        public PawnHitBoxModule HitBoxModule { get; }
        public PawnDamageModule DamageModule { get; }
        public PawnProjectileModule ProjectileModule { get; }
        public PawnBlockModule BlockModule { get; }
        public PawnWeaponModule WeaponModule { get; }

        public static PawnCombatRuntimeReferences Capture(Component owner)
        {
            return new PawnCombatRuntimeReferences(
                owner.GetComponent<IActorCombatMovementState>(),
                owner.GetComponents<IActorCombatResultReceiver>(),
                owner.GetComponent<IActorFeedbackPublisher>(),
                owner.GetComponent<PawnHitBoxModule>(),
                owner.GetComponent<PawnDamageModule>(),
                owner.GetComponent<PawnProjectileModule>(),
                owner.GetComponent<PawnBlockModule>(),
                owner.GetComponent<PawnWeaponModule>());
        }
    }
}
