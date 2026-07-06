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
            PawnHitBoxModule hitBoxModule)
        {
            Motor = motor;
            CombatResultReceivers = combatResultReceivers;
            FeedbackPublisher = feedbackPublisher;
            HitBoxModule = hitBoxModule;
        }

        public IActorCombatMovementState Motor { get; }
        public IActorCombatResultReceiver[] CombatResultReceivers { get; }
        public IActorFeedbackPublisher FeedbackPublisher { get; }
        public PawnHitBoxModule HitBoxModule { get; }

        public static PawnCombatRuntimeReferences Capture(Component owner)
        {
            return new PawnCombatRuntimeReferences(
                owner.GetComponent<IActorCombatMovementState>(),
                owner.GetComponents<IActorCombatResultReceiver>(),
                owner.GetComponent<IActorFeedbackPublisher>(),
                owner.GetComponent<PawnHitBoxModule>());
        }
    }
}
