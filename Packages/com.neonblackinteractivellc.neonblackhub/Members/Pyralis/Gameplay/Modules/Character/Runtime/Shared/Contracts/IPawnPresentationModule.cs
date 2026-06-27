using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Participants;

namespace NeonBlack.Gameplay.Modules.Character
{
    public interface IPawnPresentationModule
    {
        void ApplyPresentationProfile(PawnProfileApplicationContext context, PawnPresentationProfile presentationProfile);
    }
}
