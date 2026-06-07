using NeonBlack.Gameplay.Features.Settings;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NeonBlack.Gameplay.Features.Settings
{
/// <summary>
/// Settings panel controller for the 3D main menu.
/// Delegates all volume changes to SettingsManager; fullscreen and resolution are handled locally.
///
/// Setup:
///   1. Attach to the Settings Panel GameObject alongside MainMenuManager.
///   2. Assign sliders, toggle, and dropdown in the Inspector.
///   3. Ensure SettingsManager is present in the scene (persistent root).
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

    private Resolution[] _resolutions;

    private void OnEnable()
    {
        LoadSettings();
        PopulateResolutions();
        // Wire listeners here; OnDisable removes them so they don't stack on repeated show/hide.
        masterSlider?.onValueChanged.AddListener(_     => OnMasterVolume());
        musicSlider?.onValueChanged.AddListener(_      => OnMusicVolume());
        sfxSlider?.onValueChanged.AddListener(_        => OnSFXVolume());
        fullscreenToggle?.onValueChanged.AddListener(_ => OnFullscreen());
    }

    // â”€â”€ Load â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //

    private void LoadSettings()
    {
        SettingsManager sm = SettingsManager.Instance;
        // SetValueWithoutNotify so populating the sliders doesn't fire change callbacks.
        masterSlider?.SetValueWithoutNotify(sm != null ? sm.MasterVolume : 1f);
        musicSlider?.SetValueWithoutNotify(sm  != null ? sm.MusicVolume  : 1f);
        sfxSlider?.SetValueWithoutNotify(sm    != null ? sm.SFXVolume    : 1f);
        fullscreenToggle?.SetIsOnWithoutNotify(PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1);
        ApplyFullscreen();
    }

    // â”€â”€ Listeners â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //

    private void OnMasterVolume()
    {
        if (SettingsManager.Instance == null) return;
        SettingsManager.Instance.SetMasterVolume(masterSlider != null ? masterSlider.value : 1f);
        SettingsManager.Instance.Save();
    }

    private void OnMusicVolume()
    {
        if (SettingsManager.Instance == null) return;
        SettingsManager.Instance.SetMusicVolume(musicSlider != null ? musicSlider.value : 1f);
        SettingsManager.Instance.Save();
    }

    private void OnSFXVolume()
    {
        if (SettingsManager.Instance == null) return;
        SettingsManager.Instance.SetSFXVolume(sfxSlider != null ? sfxSlider.value : 1f);
        SettingsManager.Instance.Save();
    }

    private void OnFullscreen()
    {
        ApplyFullscreen();
        PlayerPrefs.SetInt(KEY_FULLSCREEN, fullscreenToggle != null && fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnResolutionChanged(int index)
    {
        if (_resolutions == null || index >= _resolutions.Length) return;
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
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void OnDisable()
    {
        // Remove listeners to avoid stacking them if the panel is toggled multiple times
        masterSlider?.onValueChanged.RemoveAllListeners();
        musicSlider?.onValueChanged.RemoveAllListeners();
        sfxSlider?.onValueChanged.RemoveAllListeners();
        fullscreenToggle?.onValueChanged.RemoveAllListeners();
        resolutionDropdown?.onValueChanged.RemoveAllListeners();
    }
}
}
