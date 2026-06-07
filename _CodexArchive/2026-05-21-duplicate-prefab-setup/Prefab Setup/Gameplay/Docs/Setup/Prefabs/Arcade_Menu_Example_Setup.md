# Arcade Setup

Use this guide for supported 2D score-loop scene setup. All 2D systems now live under `Members/Pyralis/Gameplay/Features/[Name]/2D/`.

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Realtime Character
- Scoring/Objectives
- Camera/Cursor Control if the scene uses menu or camera-owned selection
- Projectile Combat if the arcade loop includes guns, spells, traps, or turrets

Resolve setup-profile validation before wiring menu, scene flow, pawn, scoring, pickup, or hazard objects.

## What this guide covers

- splash scene
- main menu scene
- gameplay scene
- required data assets

## Required 2D data assets

Create these from the current menu paths:

- `Assets/Create/NeonBlack/Gameplay/Scene Flow/Level Registry`
- `Assets/Create/NeonBlack/Gameplay/Scene Flow/Level Data`

`LevelRegistry` is used by random restart flow in `GameManager`.

## Splash scene

Primary script:

- `SplashScreenController`

### Required setup

1. Create a scene that will be your splash scene.
2. Add a Canvas.
3. Add:
   - a `RawImage` if you want video playback
   - a full-screen black `Image` for fade-out
4. Add `SplashScreenController`.
5. Assign:
   - optional `VideoPlayer`
   - optional logo clip
   - the display `RawImage`
   - the fade `Image`
   - the next scene name

### Current ownership

- `SplashScreenController` now lives in `Core/Navigation/UI`

## Main menu scene

Primary scripts:

- `MainMenuManager`
- `SettingsScreen`
- optional `LeaderboardScreen`

### Required setup

1. Create your menu scene and add it to Build Settings.
2. Add a Canvas and the menu UI.
3. Create an empty object such as `ArcadeMenuRoot`.
4. Add `MainMenuManager`.
5. Wire the fields you actually use:
   - `Play Button`
   - `Settings Button`
   - `Settings Screen`
   - `Level Registry`
   - optional prev/next level buttons
   - optional preview image and level name label
   - optional random toggle
   - optional high score label
   - optional remove ads button
   - optional leaderboard button and `LeaderboardScreen`
6. If you want level selection, create one `LevelRegistry` and populate it with `LevelData` assets.
7. If you want fade-based transitions, add `SceneFader` to the flow.
8. Set the gameplay destination explicitly on `MainMenuManager` through its serialized scene-name field.

### Current ownership

- `MainMenuManager` lives in `Core/Navigation/UI`
- `SettingsScreen` now lives in `Features/Settings/UI`
- `SceneFader` now lives in `Core/Navigation/UI`

### Notes

- `GameManager` falls back to `SceneNavigator.LoadScene(...)` when `SceneFader` is absent.
- The gameplay scene used on Play is driven by the `MainMenuManager.gameSceneName` value you assign in the Inspector.
- the selected level scene names must match Build Settings exactly.
- the high score label reads from `ParticipantScoreService` through the active scene runtime.

## Gameplay scene

Primary scripts:

- `GameManager`
- `ParticipantScoreService`
- `DifficultyManager`
- `HazardSpawner`
- one pickup spawner path:
  - `CollectibleSpawner2D` for neutral new scenes
  - `CollectibleSpawner2D` for score-loop pickups
- player controller path:
  - `Motor2D`
  - `Motor2DInputAdapter`

### Minimum scene wiring

1. Add one `GameManager`.
2. Add `ParticipantScoreService`.
3. Add `DifficultyManager`.
4. Add `HazardSpawner`.
5. Add `CollectibleSpawner2D` or a migration-equivalent pickup spawner expected by your scene.
6. Add at least one player object with:
   - `Motor2D`
   - `Motor2DInputAdapter`
   - collider and rigidbody setup expected by that controller
7. In `GameManager`, assign:
   - `ParticipantScoreService`
   - `HazardSpawner`
   - pickup spawner
   - `DifficultyManager`
   - optional `LevelRegistry` for random restart
   - optional explicit player list for multiplayer scenes

### Current ownership

- `GameManager` now lives in `Features/GameFlow/2D`
- `DifficultyManager` now lives in `Features/Hazards/2D`
- `PlayerRegistry` now lives in `Features/Characters`
- `CameraAspectController` and `CameraShaker` now live in `Presentation/Camera/2D`
- `InputZoneSet` now lives in `Features/Input/2D`
- `UIManager` now lives in `Features/GameFlow/2D/UI`
- `UIOrientationHandler` now lives in `Features/UI`
- `SceneFader` and `SceneGuard` now live in `Core/Navigation/UI`

### Optional scene pieces

- `PlayerInputManager` for local join flows
- `PlayerRegistry`
- `CameraAspectController`
- `CameraShaker`
- `StillnessBonus2D`
- `UIManager`
- `UIOrientationHandler`
- `SceneFader`

## Recommended startup choice

- for new 2D scenes: `GameplaySessionBootstrap`
- for shared-core-compatible arcade-style pawns: `GameplaySessionBootstrap` plus `PawnRoot` and the direct 2D pawn stack (`Motor2D`, `Pawn2DMovementComponent`, `Pawn2DPresentationComponent`, `Motor2DInputAdapter`)

## Final checklist

- all splash, menu, and gameplay scenes are in Build Settings
- every `LevelData.sceneName` exactly matches a scene in Build Settings
- `GameManager` and `UIManager` references are wired for the UI you actually show
- `GameManager` has its required scene references assigned
- your startup path matches the scene's real architecture
