using NeonBlack.Gameplay.Features.Settings;
using NeonBlack.Gameplay.Features.GameFlow;
using UnityEngine;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Features.Settings
{
/// <summary>
/// Controls the Settings screen as a full canvas page swap.
/// When opened, the main menu page hides and the settings page shows (and vice versa).
/// Reads current values from SettingsManager when opened and saves them on close.
///
/// SETUP:
///   1. In your Canvas create two root child GameObjects:
///        - MainMenuPage  â€” holds your title, play button, settings button, etc.
///        - SettingsPage  â€” holds all sliders and the Back button. Start it INACTIVE.
///   2. Add three Sliders inside SettingsPage:
///        - MasterVolume      : min=0, max=1
///        - SFXVolume         : min=0, max=1
///        - JoystickDeadzone  : min=0, max=0.5
///   3. Add a Back button inside SettingsPage.
///   4. Attach this script to any persistent GameObject (e.g. the Canvas).
///   5. Wire all references in the Inspector.
///   6. Call Open() from MainMenuController's settings button.
/// </summary>
public class SettingsScreen : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField, Tooltip("Root GameObject of the main menu content. Hidden while settings are open.")]
    private GameObject _mainMenuPage;
    [SerializeField, Tooltip("Root GameObject of the settings page. Starts inactive.")]
    private GameObject _settingsPage;

    [Header("Sliders")]
    [SerializeField, Tooltip("Slider controlling music volume. Range 0â€“1.")]
    private Slider _masterVolumeSlider;
    [SerializeField, Tooltip("Slider controlling SFX volume. Range 0â€“1.")]
    private Slider _sfxVolumeSlider;
    [SerializeField, Tooltip("Slider controlling the joystick deadzone. Range 0â€“0.5.")]
    private Slider _joystickDeadzoneSlider;

    [Header("Toggles")]
    [SerializeField, Tooltip("Toggle that swaps movement and dash zones: ON = joystick right, dash left.")]
    private Toggle _swapControlsToggle;

    [Header("Buttons")]
    [SerializeField, Tooltip("Button that returns to the main menu page and saves all values.")]
    private Button _backButton;

    private bool _isOpen;
    private bool _pausedGame;

    // Named delegates stored so OnDestroy can remove exactly these listeners
    // without nuking any other subscribers on the same UI controls.
    private UnityEngine.Events.UnityAction          _onBack;
    private UnityEngine.Events.UnityAction<float>   _onMusicVolume;
    private UnityEngine.Events.UnityAction<float>   _onSFXVolume;
    private UnityEngine.Events.UnityAction<float>   _onJoystickDeadzone;
    private UnityEngine.Events.UnityAction<bool>    _onSwapControls;

    private void Start()
    {
        _onBack               = Close;
        _onMusicVolume        = v => SettingsManager.Instance?.SetMasterVolume(v);
        _onSFXVolume          = v => SettingsManager.Instance?.SetSFXVolume(v);
        _onJoystickDeadzone   = v => SettingsManager.Instance?.SetJoystickDeadzone(v);
        _onSwapControls       = v => SettingsManager.Instance?.SetSwapControls(v);

        _backButton?.onClick.AddListener(_onBack);
        _masterVolumeSlider?.onValueChanged.AddListener(_onMusicVolume);
        _sfxVolumeSlider?.onValueChanged.AddListener(_onSFXVolume);
        _joystickDeadzoneSlider?.onValueChanged.AddListener(_onJoystickDeadzone);
        _swapControlsToggle?.onValueChanged.AddListener(_onSwapControls);
    }

    private void OnDestroy()
    {
        // Use RemoveListener on the stored delegate â€” never RemoveAllListeners,
        // which would also remove any listeners added by other scripts.
        // Guard against null: OnDestroy can fire before Start if the object is
        // destroyed while inactive or during editor stop, leaving delegates unassigned.
        if (_onBack != null)             _backButton?.onClick.RemoveListener(_onBack);
        if (_onMusicVolume != null)      _masterVolumeSlider?.onValueChanged.RemoveListener(_onMusicVolume);
        if (_onSFXVolume != null)        _sfxVolumeSlider?.onValueChanged.RemoveListener(_onSFXVolume);
        if (_onJoystickDeadzone != null) _joystickDeadzoneSlider?.onValueChanged.RemoveListener(_onJoystickDeadzone);
        if (_onSwapControls != null)     _swapControlsToggle?.onValueChanged.RemoveListener(_onSwapControls);
    }

    /// <summary>Switch to the settings page and populate sliders with current values.</summary>
    public void Open()
    {
        if (_isOpen) return;
        if (_settingsPage == null) { Debug.LogError("[SettingsScreen] _settingsPage is not assigned.", this); return; }
        _isOpen = true;

        var sm = SettingsManager.Instance;
        if (sm != null)
        {
            _masterVolumeSlider?.SetValueWithoutNotify(sm.MasterVolume);
            _sfxVolumeSlider?.SetValueWithoutNotify(sm.SFXVolume);
            _joystickDeadzoneSlider?.SetValueWithoutNotify(sm.JoystickDeadzone);
            _swapControlsToggle?.SetIsOnWithoutNotify(sm.SwapControls);
        }

        _mainMenuPage?.SetActive(false);
        _settingsPage.SetActive(true);

        // Freeze gameplay when opened in-game; main menu has no GameManager so this is a no-op there.
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            _pausedGame = true;
            Time.timeScale = 0f;
        }
    }

    /// <summary>Return to the main menu page and save all settings values.</summary>
    public void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;

        SettingsManager.Instance?.Save();
        _settingsPage?.SetActive(false);
        _mainMenuPage?.SetActive(true);

        // Restore time if we were the ones who paused it.
        if (_pausedGame)
        {
            _pausedGame = false;
            Time.timeScale = 1f;
        }
    }
}
}
