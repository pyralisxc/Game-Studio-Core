using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Characters
{
    public interface IPawnPresentationModule
    {
        void ApplyPresentationProfile(PawnProfileApplicationContext context, PawnPresentationProfile presentationProfile);
    }
}
