# Settings Setup — Step-by-Step

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

- **SettingsManager** — the single persistent service that owns all settings at runtime: master volume, music volume, SFX volume, joystick deadzone, swap controls. Lives across scene loads.
- **SettingsProfile** — a ScriptableObject that provides default values and the AudioMixer reference. Assign one to `SettingsManager`.
- **SettingsScreen** — the reusable settings UI page used in 2D arcade scenes.
- **SettingsMenu** — the in-panel settings block used in 3D main menu scenes.

---

## Step 1 — Create a SettingsProfile asset

1. Right-click in the Project window → **Create → NeonBlack → Gameplay → Profiles → Settings Profile**.
2. Name it (e.g. `SettingsProfile_Default`).
3. In the Inspector, fill in:
   - **Mixer** — drag your project `AudioMixer` asset here. This is the same AudioMixer you have in your Audio folder. If you do not have one yet, create one via **Assets → Create → Audio Mixer**.
   - **Default Music Volume** — `1` = full (default).
   - **Default SFX Volume** — `1` = full (default).
   - **Default Joystick Deadzone** — `0.1` is a reasonable starting point.
   - **Default Gamepad Deadzone** — `0.2` is a reasonable starting point.
   - **Default Swap Controls** — leave off unless your game supports a swap-controls option.

---

## Step 2 — Add SettingsManager to the scene

`SettingsManager` must be in your first or menu scene. It calls `DontDestroyOnLoad` automatically and survives scene changes.

1. In the Hierarchy, right-click → **Create Empty**. Rename it `SettingsManager`.
2. Add Component → search `SettingsManager` → click it.
3. In the Inspector on the component:
   - **Settings Profile** — drag your `SettingsProfile_Default` asset here.
   - **Mixer Override** — leave empty unless you want to override the mixer from the profile.

That is all the required wiring. On `Awake`, it loads saved PlayerPrefs values (falling back to `SettingsProfile` defaults), applies them to the `AudioMixer`, and sets `AudioListener.volume`.

---

## Step 3 — Set up the AudioMixer (if you have not already)

For music and SFX volume sliders to work, your `AudioMixer` needs two exposed parameters.

1. Open your `AudioMixer` asset (double-click it in the Project window).
2. In the **Mixer** window, right-click the **Music** group's volume knob → **Expose 'Volume' to script**. A parameter appears in the **Exposed Parameters** panel.
3. In the **Exposed Parameters** panel (top-right of the Mixer window), double-click the parameter name and rename it exactly `MusicVolume`.
4. Repeat for the SFX group: expose its volume and rename it `SFXVolume`.
5. Assign all your audio sources to the correct groups (Music sources → Music group, SFX sources → SFX group).

`SettingsManager` uses exactly the names `MusicVolume` and `SFXVolume` — they must match.

---

## Step 4a — Wire SettingsScreen (2D arcade scenes)

`SettingsScreen` is the full-page settings panel used in 2D menus. It delegates all volume changes to `SettingsManager`.

1. In your menu Canvas, create a full-screen panel GameObject. Rename it `SettingsScreen`.
2. Add Component → `SettingsScreen`.
3. Build the UI children:
   - A `Slider` for master volume (min `0`, max `1`, value `1`).
   - A `Slider` for music volume.
   - A `Slider` for SFX volume.
   - A `Toggle` for swap controls.
   - A `Slider` for joystick deadzone (min `0`, max `0.5`).
   - A **Back** button to close the panel.
4. Wire them in the `SettingsScreen` Inspector:
   - **Master Slider** → your master volume `Slider`.
   - **Music Slider** → your music `Slider`.
   - **SFX Slider** → your SFX `Slider`.
   - **Swap Controls Toggle** → your `Toggle`.
   - **Deadzone Slider** → your deadzone `Slider`.
   - **Close Button** → your back `Button`.
5. Set the panel inactive by default (uncheck the checkbox next to its name in the Inspector). `SettingsScreen` shows and hides itself.

---

## Step 4b — Wire SettingsMenu (3D main menu scenes)

`SettingsMenu` is an in-panel settings block used alongside `MainMenuManager`. It also delegates to `SettingsManager`.

1. Create a panel inside your main menu Canvas. Rename it `SettingsPanel`.
2. Add Component → `SettingsMenu`.
3. Build the UI children:
   - A `Slider` for master volume.
   - A `Slider` for music volume.
   - A `Slider` for SFX volume.
   - A `Toggle` for fullscreen.
   - A `TMP_Dropdown` for resolution (optional).
4. Wire them in the `SettingsMenu` Inspector:
   - **Master Slider**, **Music Slider**, **SFX Slider**, **Fullscreen Toggle**, **Resolution Dropdown**.
5. On your Settings button in the main menu, add an **On Click** event → drag `SettingsPanel` → choose `GameObject.SetActive(true)`.
6. Add a **Back** button inside the panel → **On Click** → `GameObject.SetActive(false)` on the panel.

---

## Step 5 — Call Apply after scene load (optional but recommended)

In scenes that are loaded after the menu (gameplay scenes), call `SettingsManager.Instance.Apply()` once when the scene starts. This re-pushes the current volume values to the AudioMixer and input receivers after the new scene's audio sources have registered.

Add this to your `GameManager.Start()` or `GameplaySessionBootstrap`:

```csharp
private void Start()
{
    SettingsManager.Instance?.Apply();
}
```

---

## How settings flow at runtime

```
Player moves a slider in SettingsScreen or SettingsMenu
  → calls SettingsManager.Instance.SetMasterVolume(v)
  → SettingsManager sets AudioListener.volume
  → SettingsManager.Save() writes to PlayerPrefs

On next launch:
  SettingsManager.Load() reads PlayerPrefs
  → falls back to SettingsProfile defaults if no saved value
  SettingsManager.Apply() pushes values to AudioMixer + input receivers
```

---

## Common mistakes

| Problem | Likely cause |
|---|---|
| Volume sliders have no effect on audio | AudioMixer exposed parameters not renamed to `MusicVolume` / `SFXVolume` |
| Settings reset every launch | `SettingsManager` not calling `Save()`, or it is being destroyed between scenes |
| Two SettingsManagers in scene | Only add it to your first (persistent) scene — it calls `DontDestroyOnLoad` automatically |
| Master volume slider starts at wrong value | `SettingsProfile` not assigned — SettingsManager has no defaults to fall back to |
