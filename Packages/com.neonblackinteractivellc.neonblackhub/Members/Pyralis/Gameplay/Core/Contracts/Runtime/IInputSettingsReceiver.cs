namespace NeonBlack.Gameplay.Core.Contracts
{

/// <summary>
/// Implemented by any system that receives input-related settings pushed from SettingsManager.
/// Decouples SettingsManager from concrete input handlers.
/// </summary>
public interface IInputSettingsReceiver
{
    /// <summary>
    /// Apply the latest input settings.
    /// Called automatically by SettingsManager.Apply() and whenever an individual setting changes.
    /// </summary>
    void ApplySettings(float joystickDeadzone, bool swapControls, float gamepadDeadzone);
}

/// <summary>
/// Registers input modules that should receive current gameplay input settings.
/// </summary>
public interface IInputSettingsRegistrar
{
    void RegisterInputReceiver(IInputSettingsReceiver receiver);

    void UnregisterInputReceiver(IInputSettingsReceiver receiver);
}
}
