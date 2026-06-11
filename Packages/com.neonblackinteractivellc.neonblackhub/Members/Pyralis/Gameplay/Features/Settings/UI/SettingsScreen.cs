using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace NeonBlack.Gameplay.Features.Settings
{
/// <summary>
/// Controls the Settings screen as a full canvas page swap.
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.UI | AuthoringCapability.Setup,
    Relevance = "Swaps between a main menu page and a settings page, forwards slider/toggle values to a settings service, and can pause gameplay.",
    NativeSetup = new[] 
    { 
        "Assign Main Menu Page and Settings Page roots from the same Canvas.",
        "Assign Settings Source to SettingsManager.",
        "Assign the Back Button so Close can save values.",
        "Start the Settings Page inactive."
    },
    AssignmentFields = new[] { nameof(_mainMenuPage), nameof(_settingsPage), nameof(_settingsSource), nameof(_backButton) },
    FirstProof = "Open settings from the menu and verify it pauses gameplay and populates sliders correctly.",
    ExpertAdvice = "Do not assign child controls as page roots; page swapping should hide whole panels. Sliders will not save unless Settings Source is assigned."
)]
public class SettingsScreen : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField, Tooltip("Root GameObject of the main menu content. Hidden while settings are open.")]
    private GameObject _mainMenuPage;
    [SerializeField, Tooltip("Root GameObject of the settings page. Starts inactive.")]
    private GameObject _settingsPage;

    [Header("Sliders")]
    [SerializeField, Tooltip("Slider controlling master volume. Range 0-1.")]
    private Slider _masterVolumeSlider;
    [SerializeField, Tooltip("Slider controlling music volume. Range 0-1.")]
    private Slider _musicVolumeSlider;
    [SerializeField, Tooltip("Slider controlling SFX volume. Range 0-1.")]
    private Slider _sfxVolumeSlider;
    [SerializeField, Tooltip("Slider controlling the joystick deadzone. Range 0-0.5.")]
    private Slider _joystickDeadzoneSlider;

    [Header("Toggles")]
    [SerializeField, Tooltip("Toggle that swaps movement and dash zones: ON = joystick right, dash left.")]
    private Toggle _swapControlsToggle;

    [Header("Buttons")]
    [SerializeField, Tooltip("Button that returns to the main menu page and saves all values.")]
    private Button _backButton;

    [Header("Runtime Services")]
    [SerializeField, Tooltip("Settings service that stores and applies slider/toggle values. SettingsManager implements IGameplaySettingsApplier.")]
    private MonoBehaviour _settingsSource;

    [SerializeField, Tooltip("Optional gameplay state reader used to pause time only when settings are opened during active gameplay. GameManager implements IGameplayStateReader.")]
    private MonoBehaviour _gameplayStateSource;

    private bool _isOpen;
    private bool _pausedGame;
    private IGameplaySettingsApplier _settings;
    private IGameplayStateReader _gameplayStateReader;
    private bool _loggedMissingSettings;

    // Named delegates stored so OnDestroy can remove exactly these listeners
    // without nuking any other subscribers on the same UI controls.
    private UnityEngine.Events.UnityAction          _onBack;
    private UnityEngine.Events.UnityAction<float>   _onMasterVolume;
    private UnityEngine.Events.UnityAction<float>   _onMusicVolume;
    private UnityEngine.Events.UnityAction<float>   _onSFXVolume;
    private UnityEngine.Events.UnityAction<float>   _onJoystickDeadzone;
    private UnityEngine.Events.UnityAction<bool>    _onSwapControls;

    private void Start()
    {
        _onBack               = Close;
        _onMasterVolume       = v => ResolveSettings()?.SetMasterVolume(v);
        _onMusicVolume        = v => ResolveSettings()?.SetMusicVolume(v);
        _onSFXVolume          = v => ResolveSettings()?.SetSFXVolume(v);
        _onJoystickDeadzone   = v => ResolveSettings()?.SetJoystickDeadzone(v);
        _onSwapControls       = v => ResolveSettings()?.SetSwapControls(v);

        _backButton?.onClick.AddListener(_onBack);
        _masterVolumeSlider?.onValueChanged.AddListener(_onMasterVolume);
        _musicVolumeSlider?.onValueChanged.AddListener(_onMusicVolume);
        _sfxVolumeSlider?.onValueChanged.AddListener(_onSFXVolume);
        _joystickDeadzoneSlider?.onValueChanged.AddListener(_onJoystickDeadzone);
        _swapControlsToggle?.onValueChanged.AddListener(_onSwapControls);
    }

    [Inject]
    private void Construct(IGameplaySettingsApplier settings = null, IGameplayStateReader gameplayStateReader = null)
    {
        if (settings != null)
            _settings = settings;
        if (gameplayStateReader != null)
            _gameplayStateReader = gameplayStateReader;
    }

    private void OnDestroy()
    {
        // Use RemoveListener on the stored delegate - never RemoveAllListeners,
        // which would also remove any listeners added by other scripts.
        // Guard against null: OnDestroy can fire before Start if the object is
        // destroyed while inactive or during editor stop, leaving delegates unassigned.
        if (_onBack != null)             _backButton?.onClick.RemoveListener(_onBack);
        if (_onMasterVolume != null)     _masterVolumeSlider?.onValueChanged.RemoveListener(_onMasterVolume);
        if (_onMusicVolume != null)      _musicVolumeSlider?.onValueChanged.RemoveListener(_onMusicVolume);
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

        IGameplaySettingsApplier sm = ResolveSettings();
        if (sm != null)
        {
            _masterVolumeSlider?.SetValueWithoutNotify(sm.MasterVolume);
            _musicVolumeSlider?.SetValueWithoutNotify(sm.MusicVolume);
            _sfxVolumeSlider?.SetValueWithoutNotify(sm.SFXVolume);
            _joystickDeadzoneSlider?.SetValueWithoutNotify(sm.JoystickDeadzone);
            _swapControlsToggle?.SetIsOnWithoutNotify(sm.SwapControls);
        }

        _mainMenuPage?.SetActive(false);
        _settingsPage.SetActive(true);

        // Freeze gameplay when opened in-game; main menu setups can leave gameplay state empty.
        if (ResolveGameplayStateReader()?.IsGameplayActive == true)
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

        ResolveSettings()?.Save();
        _settingsPage?.SetActive(false);
        _mainMenuPage?.SetActive(true);

        // Restore time if we were the ones who paused it.
        if (_pausedGame)
        {
            _pausedGame = false;
            Time.timeScale = 1f;
        }
    }

    private IGameplaySettingsApplier ResolveSettings()
    {
        if (_settings != null)
            return _settings;

        if (_settingsSource != null)
        {
            _settings = _settingsSource as IGameplaySettingsApplier;
            if (_settings == null)
                _settings = _settingsSource.GetComponent<IGameplaySettingsApplier>();
        }

        if (_settings == null && !_loggedMissingSettings)
        {
            _loggedMissingSettings = true;
            Debug.LogError("[SettingsScreen] Settings Source is not configured. Assign SettingsManager or another IGameplaySettingsApplier.", this);
        }

        return _settings;
    }

    private IGameplayStateReader ResolveGameplayStateReader()
    {
        if (_gameplayStateReader != null)
            return _gameplayStateReader;

        if (_gameplayStateSource != null)
        {
            _gameplayStateReader = _gameplayStateSource as IGameplayStateReader;
            if (_gameplayStateReader == null)
                _gameplayStateReader = _gameplayStateSource.GetComponent<IGameplayStateReader>();
        }

        return _gameplayStateReader;
    }
}
}
