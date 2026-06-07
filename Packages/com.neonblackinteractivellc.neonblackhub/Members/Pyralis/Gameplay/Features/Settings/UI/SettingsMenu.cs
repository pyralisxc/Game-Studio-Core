using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;

namespace NeonBlack.Gameplay.Features.Settings
{
/// <summary>
/// Settings panel controller for the 3D main menu.
/// Delegates all volume changes to an explicit settings service; fullscreen and resolution are handled locally.
///
/// Setup:
///   1. Attach to the Settings Panel GameObject alongside MainMenuManager.
///   2. Assign sliders, toggle, and dropdown in the Inspector.
///   3. Assign Settings Source to SettingsManager or another IGameplaySettingsApplier.
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    private const string KEY_FULLSCREEN = "Fullscreen";

    [Header("Volume Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Fullscreen")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Resolution Dropdown")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Runtime Services")]
    [SerializeField, Tooltip("Settings service that stores and applies volume values. SettingsManager implements IGameplaySettingsApplier.")]
    private MonoBehaviour settingsSource;

    private Resolution[] _resolutions;
    private IGameplaySettingsApplier _settings;
    private bool _loggedMissingSettings;
    private UnityEngine.Events.UnityAction<float> _onMasterVolume;
    private UnityEngine.Events.UnityAction<float> _onMusicVolume;
    private UnityEngine.Events.UnityAction<float> _onSFXVolume;
    private UnityEngine.Events.UnityAction<bool> _onFullscreen;
    private UnityEngine.Events.UnityAction<int> _onResolutionChanged;

    [Inject]
    private void Construct(IGameplaySettingsApplier settings = null)
    {
        if (settings != null)
            _settings = settings;
    }

    private void OnEnable()
    {
        LoadSettings();
        PopulateResolutions();
        // Wire listeners here; OnDisable removes them so they don't stack on repeated show/hide.
        if (_onMasterVolume == null) _onMasterVolume = _ => OnMasterVolume();
        if (_onMusicVolume == null) _onMusicVolume = _ => OnMusicVolume();
        if (_onSFXVolume == null) _onSFXVolume = _ => OnSFXVolume();
        if (_onFullscreen == null) _onFullscreen = _ => OnFullscreen();
        if (_onResolutionChanged == null) _onResolutionChanged = OnResolutionChanged;

        masterSlider?.onValueChanged.AddListener(_onMasterVolume);
        musicSlider?.onValueChanged.AddListener(_onMusicVolume);
        sfxSlider?.onValueChanged.AddListener(_onSFXVolume);
        fullscreenToggle?.onValueChanged.AddListener(_onFullscreen);
        resolutionDropdown?.onValueChanged.AddListener(_onResolutionChanged);
    }

    private void LoadSettings()
    {
        IGameplaySettingsApplier sm = ResolveSettings();
        // SetValueWithoutNotify so populating the sliders doesn't fire change callbacks.
        masterSlider?.SetValueWithoutNotify(sm != null ? sm.MasterVolume : 1f);
        musicSlider?.SetValueWithoutNotify(sm  != null ? sm.MusicVolume  : 1f);
        sfxSlider?.SetValueWithoutNotify(sm    != null ? sm.SFXVolume    : 1f);
        fullscreenToggle?.SetIsOnWithoutNotify(PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1);
        ApplyFullscreen();
    }

    private void OnMasterVolume()
    {
        IGameplaySettingsApplier settings = ResolveSettings();
        if (settings == null) return;
        settings.SetMasterVolume(masterSlider != null ? masterSlider.value : 1f);
        settings.Save();
    }

    private void OnMusicVolume()
    {
        IGameplaySettingsApplier settings = ResolveSettings();
        if (settings == null) return;
        settings.SetMusicVolume(musicSlider != null ? musicSlider.value : 1f);
        settings.Save();
    }

    private void OnSFXVolume()
    {
        IGameplaySettingsApplier settings = ResolveSettings();
        if (settings == null) return;
        settings.SetSFXVolume(sfxSlider != null ? sfxSlider.value : 1f);
        settings.Save();
    }

    private void OnFullscreen()
    {
        ApplyFullscreen();
        PlayerPrefs.SetInt(KEY_FULLSCREEN, fullscreenToggle != null && fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnResolutionChanged(int index)
    {
        if (_resolutions == null || index < 0 || index >= _resolutions.Length) return;
        Resolution r = _resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    }

    private void ApplyFullscreen()
    {
        Screen.fullScreen = fullscreenToggle != null && fullscreenToggle.isOn;
    }

    private void PopulateResolutions()
    {
        if (resolutionDropdown == null) return;

        _resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentIndex = 0;
        var options = new System.Collections.Generic.List<string>();

        for (int i = 0; i < _resolutions.Length; i++)
        {
            options.Add($"{_resolutions[i].width} x {_resolutions[i].height}");
            if (_resolutions[i].width  == Screen.currentResolution.width &&
                _resolutions[i].height == Screen.currentResolution.height)
                currentIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(currentIndex);
        resolutionDropdown.RefreshShownValue();
    }

    private void OnDisable()
    {
        // Remove only the listeners owned by this component.
        if (_onMasterVolume != null) masterSlider?.onValueChanged.RemoveListener(_onMasterVolume);
        if (_onMusicVolume != null) musicSlider?.onValueChanged.RemoveListener(_onMusicVolume);
        if (_onSFXVolume != null) sfxSlider?.onValueChanged.RemoveListener(_onSFXVolume);
        if (_onFullscreen != null) fullscreenToggle?.onValueChanged.RemoveListener(_onFullscreen);
        if (_onResolutionChanged != null) resolutionDropdown?.onValueChanged.RemoveListener(_onResolutionChanged);
    }

    private IGameplaySettingsApplier ResolveSettings()
    {
        if (_settings != null)
            return _settings;

        if (settingsSource != null)
        {
            _settings = settingsSource as IGameplaySettingsApplier;
            if (_settings == null)
                _settings = settingsSource.GetComponent<IGameplaySettingsApplier>();
        }

        if (_settings == null && !_loggedMissingSettings)
        {
            _loggedMissingSettings = true;
            Debug.LogError("[SettingsMenu] Settings Source is not configured. Assign SettingsManager or another IGameplaySettingsApplier.", this);
        }

        return _settings;
    }
}
}
