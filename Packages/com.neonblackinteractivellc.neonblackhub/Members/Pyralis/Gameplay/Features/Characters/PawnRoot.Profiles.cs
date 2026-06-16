using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Input;
using UnityEngine;

namespace NeonBlack.Gameplay.Characters
{
    public partial class PawnRoot
    {
        private void ApplyProfiles()
        {
            if (pawnDefinition == null)
                return;

            PawnProfileApplicationContext profileContext = new PawnProfileApplicationContext(gameObject, pawnDefinition, Participant);
            InputProfile inputProfile = ParticipantInputProfileUtility.ResolveEffectiveInputProfile(Participant != null ? Participant.Definition : null);

            _runtime ??= PawnRootRuntimeReferences.Capture(gameObject);
            MonoBehaviour[] behaviours = _runtime.GetProfileReceivers();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IPawnInputModule inputModule)
                    inputModule.ApplyInputProfile(profileContext, inputProfile);
                if (behaviour is IPawnMotor motor)
                    motor.ApplyMovementProfile(profileContext, pawnDefinition.movementProfile);
                if (behaviour is IPawnCombatModule combatModule)
                    combatModule.ApplyCombatProfile(profileContext, pawnDefinition.combatProfile);
                if (behaviour is IPawnTraversalModule traversalModule)
                    traversalModule.ApplyTraversalProfile(profileContext, pawnDefinition.traversalProfile);
                if (behaviour is IPawnPresentationModule presentationModule)
                    presentationModule.ApplyPresentationProfile(profileContext, pawnDefinition.presentationProfile);
            }
        }
    }
}
