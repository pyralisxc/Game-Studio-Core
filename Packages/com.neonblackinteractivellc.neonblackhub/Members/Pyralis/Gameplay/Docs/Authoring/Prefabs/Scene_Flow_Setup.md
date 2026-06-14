# Scene Flow Setup - Step-by-Step

Covers the Game Shell MVP route: boot scene, loading scene, main menu, settings, credits, and gameplay scene transition through `SceneFader`, `LoadingScreenController`, `MainMenuManager`, and `ISceneNavigator`.

---

## Before You Wire This

Start with a `SessionDefinition` assigned to `GameplaySessionBootstrap.sessionDefinition` and a `GameModeDefinition` assigned to `SessionDefinition.defaultGameMode`.

Recommended route capabilities:

- Platform Core for shared scene bootstrapping, navigation, and setup validation
- Camera/Cursor Control when the player is the camera, cursor, menu focus, or tabletop selection surface
- Realtime Character, Board/Card/Tabletop, or Turn/Menu Action based on the destination scene's actual control model

Resolve route validation before assigning menu destinations, faders, loading screens, or restart flow.

---

## Concepts

- **Game Shell MVP route** - the beginner setup path that proves a project can boot, show loading progress, open a main menu, open settings, open credits, and transition into gameplay.
- **LevelData** - a ScriptableObject defining one playable world: its scene name, display name, and preview image.
- **LevelRegistry** - an ordered list of all `LevelData` assets. Use it when a menu or game mode needs world selection or random level picking.
- **ISceneNavigator** - the scene-transition contract consumed by menu and game-flow components.
- **SceneFader** - the default scene-transition coordinator that fades to black before loading a new scene and fades back in after it loads. It implements `ISceneNavigator`.
- **SceneLoader** - an alternate `ISceneNavigator` that builds its own fade canvas at runtime.
- **LoadingScreenController** - the optional loading-scene component that reads `SceneFader.PendingScene`, updates progress UI, and activates the target scene.
- **MainMenuManager** - the panel-driven menu component for New Game, Load Game, Settings, Credits, Co-op, and Exit buttons.

---

## Step 1 - Add all scenes to Build Settings

Every scene you want to load at runtime must be in **File -> Build Settings**.

1. Open **File -> Build Settings**.
2. Drag each scene file from the Project window into the **Scenes In Build** list.
3. For the shell route, include your boot or splash scene, loading scene, main menu scene, and gameplay scene.
4. Note the exact scene names shown in the list. Scene names are case-sensitive and should not include `.unity`.

---

## Step 2 - Create optional LevelData assets

Create `LevelData` assets when the menu should offer a selectable or random world list. Skip this step for a single-scene prototype that only needs one **Game Scene Name** on `MainMenuManager`.

1. Right-click in the Project window -> **Create -> NeonBlack -> Scene Flow -> Level Data**.
2. Name it, for example `LevelData_Kitchen`.
3. Fill in the Inspector:
   - **Scene Name** - type the scene name exactly as shown in Build Settings, for example `KitchenScene`.
   - **Display Name** - the friendly world name shown in a selector, for example `The Kitchen`.
   - **Preview Image** - drag a `Sprite` asset to use as a menu preview.
4. Repeat for every selectable world.

---

## Step 3 - Create an optional LevelRegistry asset

Create a `LevelRegistry` only when a component in your project needs a list of playable worlds.

1. Right-click in the Project window -> **Create -> NeonBlack -> Scene Flow -> Level Registry**.
2. Name it `LevelRegistry`.
3. In the Inspector, expand the **Levels** array and add one slot per world.
4. Drag each `LevelData` asset into its slot, in the order it should appear.

`MainMenuManager` loads the scene named in **Game Scene Name**. Type the gameplay scene name exactly as it appears in Build Settings.

---

## Step 4 - Add SceneFader to your first scene

`SceneFader` is a persistent scene-flow service. Add it once to your boot, splash, or main menu scene.

1. In the Hierarchy, right-click -> **Create Empty**. Rename it `SceneFader`.
2. Add Component -> `SceneFader`.
3. Wire the Inspector fields:
   - **Fade Out Duration** - seconds for the screen to go to black, for example `0.35`.
   - **Fade In Duration** - seconds for the screen to come back in after the new scene loads, for example `0.35`.

`SceneFader` calls `DontDestroyOnLoad` automatically and creates its own black overlay image at runtime. No Canvas setup is required.

---

## Step 5 - Add the loading scene

Use a loading scene when the game should show progress before activating the destination scene.

1. Create a scene named exactly like `SceneNames.LoadingScreen`.
2. Add a Canvas with an optional Slider and TextMeshPro label.
3. Add Component -> `LoadingScreenController` to a stable object in the loading scene.
4. Assign **Progress Bar** and **Label** if the loading scene displays them.
5. Add the loading scene to Build Settings.

When you want a loading scene, call `SceneFader.FadeToSceneViaLoader(gameSceneName)` from custom code or a small menu adapter. `SceneFader` stores the target scene in `PendingScene`, loads `SceneNames.LoadingScreen`, and `LoadingScreenController` activates the final gameplay scene.

For direct fades without the loading scene, `MainMenuManager` can use `SceneFader` or `SceneLoader` through **Scene Navigator Source**.

---

## Step 6 - Build the main menu panels

Create a Canvas in your main menu scene.

1. Create panels named `MainPanel`, `SettingsPanel`, `CreditsPanel`, and optionally `CoopPanel`.
2. Put your New Game, Load Game, Settings, Credits, Co-op, and Exit buttons on `MainPanel`.
3. Put settings controls on `SettingsPanel`; see `Settings_Setup.md`.
4. Put your credits text, studio name, tools, asset acknowledgements, and helper names inside `CreditsPanel`.
5. Put one Back button on `SettingsPanel`, `CreditsPanel`, and `CoopPanel` if that panel exists.
6. Start `MainPanel` active and the other panels inactive.

Add Component -> `MainMenuManager` to a stable menu object and assign:

- **Main Panel** -> `MainPanel`
- **Settings Panel** -> `SettingsPanel`
- **Credits Panel** -> `CreditsPanel`
- **Co-op Panel** -> `CoopPanel` when used
- **New Game Button**, **Load Game Button**, **Settings Button**, **Credits Button**, **Co-op Button**, and **Exit Button** from `MainPanel`
- **Settings Back Button**, **Credits Back Button**, and **Co-op Back Button** from their panels
- **Game Scene Name** -> the gameplay scene name exactly as shown in Build Settings
- **Scene Navigator Source** -> the scene `SceneFader`, `SceneLoader`, or another component that implements `ISceneNavigator`

---

## Step 7 - Assign Scene Navigator Source in gameplay UI

For user-facing components, assign a scene navigator instead of calling a singleton directly.

1. On `MainMenuManager`, assign **Scene Navigator Source** to the scene `SceneFader` or `SceneLoader`.
2. On `GameManager`, assign **Scene Navigator Source** to the same service when Restart or Main Menu buttons should navigate.
3. Keep one owner per menu flow. Use `SceneFader` or `SceneLoader`, not both for the same buttons.

For custom scripts, resolve an assigned `MonoBehaviour` as `ISceneNavigator`:

```csharp
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

public sealed class DoorSceneExit : MonoBehaviour
{
    [SerializeField] private MonoBehaviour sceneNavigatorSource;
    [SerializeField] private string destinationScene;

    private ISceneNavigator _sceneNavigator;

    public void Exit()
    {
        if (_sceneNavigator == null && sceneNavigatorSource != null)
        {
            _sceneNavigator = sceneNavigatorSource as ISceneNavigator;
            if (_sceneNavigator == null)
                _sceneNavigator = sceneNavigatorSource.GetComponent<ISceneNavigator>();
        }

        if (_sceneNavigator != null)
            _sceneNavigator.LoadScene(destinationScene);
    }
}
```

---

## Step 8 - Verify scene transition in Play Mode

1. Press **Play** in the boot, splash, or menu scene.
2. Open Settings, change a value, and press Back.
3. Open Credits and press Back.
4. Press New Game or Load Game.
5. Confirm the screen fades, the gameplay scene loads, and the screen fades back in.
6. In the gameplay scene, pressing Restart or Return to Menu should fade out and navigate correctly.

---

## Common mistakes

| Problem | Likely cause |
|---|---|
| `Scene 'X' not found` exception at runtime | Scene name in `Game Scene Name` or `LevelData.sceneName` does not match Build Settings exactly |
| Loading scene falls back to MainMenu | `SceneFader.PendingScene` was not set before opening the loading scene |
| No fade - loads instantly | Scene Navigator Source is empty or points to a navigator that performs instant loads |
| Fader persists into a second instance | Second `SceneFader` in a later scene - keep one active scene-flow instance in the first scene |
| Credits button hides the main panel but shows nothing | Credits Panel is empty or inactive under a disabled parent |
| Back button does nothing | The panel back button is not assigned to `MainMenuManager` |
| Restart or menu buttons log navigation errors | Scene Navigator Source is empty or does not implement `ISceneNavigator` |
| Restart loads wrong scene | Destination scene name typo or a random/registry-backed restart flow is pointing at the wrong `LevelData` |
