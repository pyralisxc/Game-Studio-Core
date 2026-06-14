using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using UnityEngine;
using UnityEngine.Audio;

namespace NeonBlack.Gameplay.Features.Settings
{

/// <summary>
/// Runtime settings service that loads, applies, and saves player settings via PlayerPrefs.
///
/// Settings managed:
///   MusicVolume         - pushed to the "MusicVolume" exposed parameter on the AudioMixer (dB)
///   SFXVolume           - pushed to the "SFXVolume" exposed parameter on the AudioMixer (dB)
///   MasterVolume        - pushed to AudioListener.volume
///   JoystickDeadzone    - pushed to registered IInputSettingsReceivers
///   SwapControls        - pushed to registered IInputSettingsReceivers
///
/// SETUP:
///   1. Attach to a persistent GameObject (GameManager or DontDestroyOnLoad root).
///   2. Assign a SettingsProfile asset - it provides the AudioMixer and all default values.
///   3. Optionally override the Mixer field directly if the profile mixer is unavailable.
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.Audio | AuthoringCapability.UI,
    Relevance = "Manages global audio volume levels and mixer integration. Connects settings profiles to the active Unity AudioMixer.",
    NativeSetup = new[] { "Attach to a persistent root GameObject.", "Assign a SettingsProfile asset." },
    AssignmentFields = new[] { nameof(settingsProfile), nameof(_mixerOverride) },
    FirstProof = "Verify AudioMixer parameters 'MusicVolume' and 'SFXVolume' change when sliders are moved in the Settings UI.",
    ExpertAdvice = "Ensure your AudioMixer has exposed parameters named 'MusicVolume' and 'SFXVolume' (case sensitive) for the manager to drive. This component persists across scenes if placed on a DontDestroyOnLoad root.",
    DocumentationURL = "https://docs.neonblack.com/pyralis/settings"
)]
[DefaultExecutionOrder(-40)]
public class SettingsManager : MonoBehaviour, IGameplaySettingsApplier, IInputSettingsRegistrar, IRuntimeValidationProvider
{
    public IEnumerable<string> GetRuntimeValidationIssues()
    {
        if (settingsProfile == null)
            yield return "Settings Profile is required for default volume and deadzone values.";
        
        if (Mixer == null)
            yield return "No AudioMixer found. Assign one in the Settings Profile or use the Mixer Override field.";
    }
    [Header("Profile")]
    [SerializeField, Tooltip("Provides AudioMixer, default volumes, and deadzone values. Required for clean defaults.")]
    private SettingsProfile settingsProfile;

    [Header("Mixer Override")]
    [SerializeField, Tooltip("Direct AudioMixer override. If assigned, takes priority over the profile mixer.")]
    private AudioMixer _mixerOverride;

    // PlayerPrefs keys
    private const string KeyMusicVolume      = "Settings_MusicVolume";
    private const string KeySFXVolume        = "Settings_SFXVolume";
    private const string KeyMasterVolume     = "Settings_MasterVolume";
    private const string KeyJoystickDeadzone = "Settings_JoystickDeadzone";
    private const string KeySwapControls     = "Settings_SwapControls";
    private const string KeyGamepadDeadzone  = "Settings_GamepadDeadzone";

    // AudioMixer exposed parameter names
    private const string MixerMusicParam = "MusicVolume";
    private const string MixerSFXParam   = "SFXVolume";

    // Live values
    public float MusicVolume      { get; private set; }
    public float SFXVolume        { get; private set; }
    public float MasterVolume     { get; private set; }
    public float JoystickDeadzone { get; private set; }
    public bool  SwapControls     { get; private set; }
    public float GamepadDeadzone  { get; private set; }

    // Resolved mixer
    private AudioMixer Mixer => _mixerOverride != null ? _mixerOverride
        : (settingsProfile != null ? settingsProfile.mixer : null);

    private void Awake()
    {
        Load();
        ApplyMixer();
    }

    private void OnDestroy()
    {
    }

    private void Start()
    {
        Apply();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) Save();
    }

    // Public setters
    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        SetMixerVolume(MixerMusicParam, MusicVolume);
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        SetMixerVolume(MixerSFXParam, SFXVolume);
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        AudioListener.volume = MasterVolume;
    }

    public void SetJoystickDeadzone(float value)  { JoystickDeadzone = Mathf.Clamp(value, 0f, 0.5f); PushToInputReceivers(); }
    public void SetSwapControls(bool value)        { SwapControls = value; PushToInputReceivers(); }
    public void SetGamepadDeadzone(float value)    { GamepadDeadzone = Mathf.Clamp(value, 0f, 0.5f); PushToInputReceivers(); }

    /// <summary>Push all live values to the AudioMixer and all registered input receivers. Call after loading a scene.</summary>
    public void Apply()
    {
        ApplyMixer();
        AudioListener.volume = MasterVolume;
        PushToInputReceivers();
    }

    /// <summary>Write current values to PlayerPrefs.</summary>
    public void Save()
    {
        PlayerPrefs.SetFloat(KeyMusicVolume,      MusicVolume);
        PlayerPrefs.SetFloat(KeySFXVolume,        SFXVolume);
        PlayerPrefs.SetFloat(KeyMasterVolume,     MasterVolume);
        PlayerPrefs.SetFloat(KeyJoystickDeadzone, JoystickDeadzone);
        PlayerPrefs.SetInt(KeySwapControls,       SwapControls ? 1 : 0);
        PlayerPrefs.SetFloat(KeyGamepadDeadzone,  GamepadDeadzone);
        PlayerPrefs.Save();
    }

    /// <summary>Read values from PlayerPrefs, falling back to SettingsProfile defaults when no saved value exists.</summary>
    public void Load()
    {
        float defMusic    = settingsProfile != null ? settingsProfile.defaultMusicVolume      : 1f;
        float defSfx      = settingsProfile != null ? settingsProfile.defaultSfxVolume        : 1f;
        float defJoy      = settingsProfile != null ? settingsProfile.defaultJoystickDeadzone : 0.1f;
        float defGamepad  = settingsProfile != null ? settingsProfile.defaultGamepadDeadzone  : 0.2f;
        bool  defSwap     = settingsProfile != null && settingsProfile.defaultSwapControls;

        MusicVolume      = PlayerPrefs.GetFloat(KeyMusicVolume,      defMusic);
        SFXVolume        = PlayerPrefs.GetFloat(KeySFXVolume,        defSfx);
        MasterVolume     = PlayerPrefs.GetFloat(KeyMasterVolume,     1f);
        JoystickDeadzone = PlayerPrefs.GetFloat(KeyJoystickDeadzone, defJoy);
        SwapControls     = PlayerPrefs.GetInt(KeySwapControls,       defSwap ? 1 : 0) == 1;
        GamepadDeadzone  = PlayerPrefs.GetFloat(KeyGamepadDeadzone,  defGamepad);
    }

    // Internal
    private void ApplyMixer()
    {
        SetMixerVolume(MixerMusicParam, MusicVolume);
        SetMixerVolume(MixerSFXParam,   SFXVolume);
    }

    private void SetMixerVolume(string parameter, float linear)
    {
        AudioMixer mixer = Mixer;
        if (mixer == null) return;
        float dB = linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
        mixer.SetFloat(parameter, dB);
    }

    // Input settings receivers
    private readonly List<IInputSettingsReceiver> _inputReceivers = new List<IInputSettingsReceiver>();

    public void RegisterInputReceiver(IInputSettingsReceiver receiver)
    {
        if (receiver == null) return;
        if (!_inputReceivers.Contains(receiver))
            _inputReceivers.Add(receiver);
        PushToInputReceivers();
    }

    public void UnregisterInputReceiver(IInputSettingsReceiver receiver)
    {
        if (receiver != null)
            _inputReceivers.Remove(receiver);
    }

    private void PushToInputReceivers()
    {
        for (int i = _inputReceivers.Count - 1; i >= 0; i--)
        {
            IInputSettingsReceiver receiver = _inputReceivers[i];
            if (receiver == null) { _inputReceivers.RemoveAt(i); continue; }
            receiver.ApplySettings(JoystickDeadzone, SwapControls, GamepadDeadzone);
        }
    }
}

} // namespace NeonBlack.Gameplay.Features.Settings
