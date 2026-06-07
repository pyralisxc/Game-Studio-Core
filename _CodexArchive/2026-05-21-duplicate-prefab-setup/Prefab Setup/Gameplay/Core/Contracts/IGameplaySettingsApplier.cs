namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IGameplaySettingsApplier
    {
        void SetMusicVolume(float value);
        void SetSFXVolume(float value);
        void SetJoystickDeadzone(float value);
        void SetGamepadDeadzone(float value);
        void SetSwapControls(bool value);
    }
}
