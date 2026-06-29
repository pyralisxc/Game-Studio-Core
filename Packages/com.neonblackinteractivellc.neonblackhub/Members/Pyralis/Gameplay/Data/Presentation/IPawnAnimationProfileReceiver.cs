using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Data.Presentation
{
    public interface IPawnAnimationProfileReceiver
    {
        void ApplyProfiles(PawnPresentationProfile presentation, PawnAnimationProfile animation);
    }
}
