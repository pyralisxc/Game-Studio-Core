namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IGameplaySettingsApplier
    {
        float MusicVolume { get; }
        float SFXVolume { get; }
        float MasterVolume { get; }
        float JoystickDeadzone { get; }
        bool SwapControls { get; }
        float GamepadDeadzone { get; }

        void SetMasterVolume(float value);
        void SetMusicVolume(float value);
        void SetSFXVolume(float value);
        void SetJoystickDeadzone(float value);
        void SetGamepadDeadzone(float value);
        void SetSwapControls(bool value);
        void Apply();
        void Save();
    }

    /// <summary>
    /// Safe settings sink for sessions that have not installed a SettingsManager yet.
    /// </summary>
    public sealed class NullGameplaySettingsApplier : IGameplaySettingsApplier
    {
        public float MusicVolume { get; private set; } = 1f;
        public float SFXVolume { get; private set; } = 1f;
        public float MasterVolume { get; private set; } = 1f;
        public float JoystickDeadzone { get; private set; } = 0.15f;
        public bool SwapControls { get; private set; }
        public float GamepadDeadzone { get; private set; } = 0.15f;

        public void SetMasterVolume(float value) => MasterVolume = value;
        public void SetMusicVolume(float value) => MusicVolume = value;
        public void SetSFXVolume(float value) => SFXVolume = value;
        public void SetJoystickDeadzone(float value) => JoystickDeadzone = value;
        public void SetGamepadDeadzone(float value) => GamepadDeadzone = value;
        public void SetSwapControls(bool value) => SwapControls = value;
        public void Apply() { }
        public void Save() { }
    }
}
