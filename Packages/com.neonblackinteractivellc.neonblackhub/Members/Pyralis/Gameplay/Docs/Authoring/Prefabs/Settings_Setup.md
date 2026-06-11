# Settings Setup - Step-by-Step

Covers `SettingsManager`, `SettingsProfile`, `SettingsScreen` (2D), and `SettingsMenu` (3D).

---

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Platform Core for shared settings
- Camera/Cursor Control when settings are menu, cursor, or UI driven
- Realtime Character if settings affect pawn input, gamepad deadzone, or touch controls

Resolve setup-profile validation before adding settings services, settings profile assets, or settings UI.

---

## Concepts

- **IGameplaySettingsApplier** - the runtime contract consumed by settings UI. It exposes current values, setters, `Apply()`, and `Save()`.
- **IInputSettingsRegistrar** - the runtime contract consumed by pawn input components that want joystick deadzone, gamepad deadzone, and swap-controls updates.
- **SettingsManager** - the default persistent implementation. It owns master volume, music volume, SFX volume, joystick deadzone, gamepad deadzone, and swap controls across scene loads.
- **SettingsProfile** - a ScriptableObject that provides default values and the AudioMixer reference. Assign one to `SettingsManager`.
- **SettingsScreen** - the reusable page-swap settings UI used in 2D arcade/menu scenes.
- **SettingsMenu** - the in-panel settings block used in 3D main menu scenes.

---

## Step 1 - Create a SettingsProfile asset

1. Right-click in the Project window -> **Create -> NeonBlack -> Profiles -> Settings Profile**.
2. Name it, for example `SettingsProfile_Default`.
3. In the Inspector, fill in:
   - **Mixer** - drag your project `AudioMixer` asset here. If you do not have one yet, create one via **Assets -> Create -> Audio Mixer**.
   - **Default Music Volume** - `1` = full.
   - **Default SFX Volume** - `1` = full.
   - **Default Joystick Deadzone** - `0.1` is a reasonable starting point.
   - **Default Gamepad Deadzone** - `0.2` is a reasonable starting point.
   - **Default Swap Controls** - leave off unless your game supports a swap-controls option.

---

## Step 2 - Add SettingsManager to the scene

`SettingsManager` must be in your first or menu scene. It calls `DontDestroyOnLoad` automatically and survives scene changes.

1. In the Hierarchy, right-click -> **Create Empty**. Rename it `SettingsManager`.
2. Add Component -> search `SettingsManager` -> click it.
3. In the Inspector on the component:
   - **Settings Profile** - drag your `SettingsProfile_Default` asset here.
   - **Mixer Override** - leave empty unless you want to override the mixer from the profile.

On `Awake`, the manager loads saved PlayerPrefs values, falls back to `SettingsProfile` defaults, applies them to the `AudioMixer`, and sets `AudioListener.volume`.

---

## Step 3 - Set up the AudioMixer

For music and SFX volume sliders to work, your `AudioMixer` needs two exposed parameters.

1. Open your `AudioMixer` asset.
2. In the **Mixer** window, right-click the **Music** group's volume knob -> **Expose 'Volume' to script**.
3. In the **Exposed Parameters** panel, rename that parameter exactly `MusicVolume`.
4. Repeat for the SFX group and rename that exposed parameter exactly `SFXVolume`.
5. Assign audio sources to the correct mixer groups.

`SettingsManager` uses exactly the names `MusicVolume` and `SFXVolume`; they must match.

---

## Step 4a - Wire SettingsScreen (2D arcade/menu scenes)

`SettingsScreen` is a full-page settings panel. It reads and writes through its assigned `Settings Source`, so it can use the default `SettingsManager` or a custom component that implements `IGameplaySettingsApplier`.

1. In your menu Canvas, create two root child GameObjects:
   - `MainMenuPage` for title, play button, settings button, and other menu controls.
   - `SettingsPage` for sliders, toggles, and the back button. Start this inactive.
2. Add Component -> `SettingsScreen` to the Canvas or another stable menu object.
3. Build the UI children you want:
   - A `Slider` for master volume, min `0`, max `1`.
   - A `Slider` for music volume, min `0`, max `1`.
   - A `Slider` for SFX volume, min `0`, max `1`.
   - A `Toggle` for swap controls.
   - A `Slider` for joystick deadzone, min `0`, max `0.5`.
   - A **Back** button to close the panel.
4. Wire the `SettingsScreen` Inspector:
   - **Main Menu Page** -> `MainMenuPage`.
   - **Settings Page** -> `SettingsPage`.
   - **Master Volume Slider**, **Music Volume Slider**, **SFX Volume Slider**, **Joystick Deadzone Slider**, and **Swap Controls Toggle** -> the matching UI controls.
   - **Back Button** -> your back `Button`.
   - **Settings Source** -> `SettingsManager` or another component that implements `IGameplaySettingsApplier`.
   - **Gameplay State Source** -> optional. Assign `GameManager` or another `IGameplayStateReader` only when opening this screen during active gameplay should pause time.
5. Call `Open()` from your settings button.

---

## Step 4b - Wire SettingsMenu (3D main menu scenes)

`SettingsMenu` is an in-panel settings block used alongside `MainMenuManager`. It also reads and writes through `Settings Source`.

1. Create a panel inside your main menu Canvas. Rename it `SettingsPanel`.
2. Add Component -> `SettingsMenu`.
3. Build the UI children:
   - A `Slider` for master volume.
   - A `Slider` for music volume.
   - A `Slider` for SFX volume.
   - A `Toggle` for fullscreen.
   - A `TMP_Dropdown` for resolution.
4. Wire the `SettingsMenu` Inspector:
   - **Master Slider**, **Music Slider**, **SFX Slider**, **Fullscreen Toggle**, and **Resolution Dropdown**.
   - **Settings Source** -> `SettingsManager` or another component that implements `IGameplaySettingsApplier`.
5. On your Settings button in the main menu, add an **On Click** event -> drag `SettingsPanel` -> choose `GameObject.SetActive(true)`.
6. Add a **Back** button inside the panel -> **On Click** -> `GameObject.SetActive(false)` on the panel.

---

## Step 5 - Apply after scene load

In scenes loaded after the menu, call `Apply()` on the explicit settings service once when the scene starts. This re-pushes current volume values to the AudioMixer and input receivers after the new scene's objects have registered.

```csharp
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using VContainer;

public sealed class GameplaySettingsBootstrap : MonoBehaviour
{
    private IGameplaySettingsApplier _settings;

    [Inject]
    private void Construct(IGameplaySettingsApplier settings)
    {
        _settings = settings;
    }

    private void Start()
    {
        _settings?.Apply();
    }
}
```

For simple scenes without a DI lifetime scope, assign a serialized `MonoBehaviour` field and resolve it as `IGameplaySettingsApplier`, matching the pattern used by `SettingsScreen` and `SettingsMenu`.

Pawn input components should also use explicit settings registration:

1. On `PlayerInputHandler`, assign **Settings Registrar Source** to `SettingsManager` or another component that implements `IInputSettingsRegistrar`.
2. If the field is empty, input still uses the serialized/default deadzone and swap-control values.
3. If a scene session configures input in code, call `ConfigureRuntime(gameplayStateReader, inputSettingsRegistrar)`.

---

## Runtime Flow

```text
Player moves a slider in SettingsScreen or SettingsMenu
  -> UI calls IGameplaySettingsApplier.SetMasterVolume(v), SetMusicVolume(v), or SetSFXVolume(v)
  -> default SettingsManager applies AudioListener or AudioMixer values
  -> SettingsManager pushes input values to registered IInputSettingsReceiver components
  -> UI calls Save() where the interaction should persist immediately or on close
  -> SettingsManager writes values to PlayerPrefs

On next launch:
  SettingsManager.Load() reads PlayerPrefs
  -> falls back to SettingsProfile defaults if no saved value exists
  -> Apply() pushes values to AudioMixer and input receivers
```

---

## Common Mistakes

| Problem | Likely cause |
|---|---|
| Settings UI logs an error and sliders do nothing | **Settings Source** is empty or does not implement `IGameplaySettingsApplier` |
| Volume sliders have no effect on audio | AudioMixer exposed parameters are not named exactly `MusicVolume` and `SFXVolume` |
| Settings reset every launch | The settings service is not saving values, or the persistent `SettingsManager` is missing between scenes |
| Two SettingsManagers in scene | Only add it to your first persistent/menu scene; it calls `DontDestroyOnLoad` automatically |
| Settings pause time from the main menu | **Gameplay State Source** is assigned in a pure menu setup where no active gameplay pause is needed |
