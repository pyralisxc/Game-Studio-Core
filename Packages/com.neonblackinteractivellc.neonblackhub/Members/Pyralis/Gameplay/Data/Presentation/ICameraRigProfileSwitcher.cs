using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Data.Presentation
{
    public interface ICameraRigProfileSwitcher
    {
        void SwitchProfile(CameraRigProfile profile, float transitionDuration = 0.5f);
    }
}
