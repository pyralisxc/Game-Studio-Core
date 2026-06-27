using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    public sealed partial class Pawn2DPresentationComponent
    {
        public void ApplyPresentationProfile(PawnProfileApplicationContext context, PawnPresentationProfile presentationProfile)
        {
            if (presentationProfile != null)
            {
                spriteDefaultFacesRight = presentationProfile.spriteDefaultFacesRight;
                Color participantTint = context.Participant?.Definition != null
                    ? context.Participant.Definition.tint
                    : Color.white;
                idleTint = MultiplyTint(presentationProfile.primaryTint, participantTint);
                movingTint = idleTint;
            }

            animationDriver?.ApplyProfiles(
                presentationProfile,
                context.PawnDefinition != null ? context.PawnDefinition.animationProfile : null);

            if (presentationProfile != null && spriteRenderer != null)
                spriteRenderer.color = idleTint;
        }

        private static Color MultiplyTint(Color baseTint, Color participantTint)
        {
            return new Color(
                baseTint.r * participantTint.r,
                baseTint.g * participantTint.g,
                baseTint.b * participantTint.b,
                baseTint.a * participantTint.a);
        }
    }
}
