# Brawler Setup

Use this guide for supported 3D combat scene setup. All 3D systems now live under `Members/Pyralis/Gameplay/Features/[Name]/3D/`.

## Before You Wire This

Start with a `SessionDefinition` assigned to `GameplaySessionBootstrap.sessionDefinition` and a `GameModeDefinition` assigned to `SessionDefinition.defaultGameMode`.

Recommended route capabilities:

- Realtime Character
- Combat
- Animation/Presentation
- Camera/Cursor Control for menu and camera ownership
- Projectile Combat if the brawler supports guns, spells, thrown objects, turrets, or ranged enemy attacks

Resolve route validation before wiring menu panels, pawn prefabs, cameras, encounters, enemies, or combat objects.

## Main menu scene

Primary script:

- `MainMenuManager`

### Required setup

1. Create your menu scene and add it to Build Settings.
2. Add a Canvas and create the menu panels you want to use.
3. Add an empty object such as `BrawlerMenuRoot`.
4. Add `MainMenuManager`.
5. Wire:
   - `mainPanel`
   - `settingsPanel`
   - `coopPanel`
   - `newGameButton`
   - `loadGameButton`
   - `settingsButton`
   - `coopButton`
   - `exitButton`
   - optional back, host, and join buttons if those panels exist
6. Set `gameSceneName` directly in the Inspector.

### Current ownership

- `MainMenuManager` now lives in `Core/Navigation/UI`
- `SettingsMenu` now lives in `Features/Settings/UI` when you use an in-panel settings screen

### Behavior notes

- `MainMenuManager` resolves `ISceneNavigator` when one is available, then falls back to the assigned `SceneFader` or `SceneLoader` compatibility surface for `New Game` and `Host Co-op`
- `MainMenuManager` uses the serialized `gameSceneName` value you assign in the Inspector
- the cursor is unlocked and made visible on start
- the co-op buttons are placeholders for your own multiplayer flow

## Gameplay scene

Typical player object:

- `CharacterController`
- `HealthComponent`
- `KnockbackReceiver`
- `Motor3D`
- animator setup
- hitbox children used by `Motor3D`

Typical scene systems:

- `EnemyAI` on enemies
- `EnemySpawner` for encounter spawning
- `ArenaZone` for combat locks
- `CameraZone` for profile changes
- `DamageZone` for environmental hazards
- `CinemachineCameraRigController` for camera framing and zone transitions
- optional `PlayerSpawner` from `Features/Respawn/3D` for respawn-based scenes

## Recommended startup choice

### New 3D scenes

Use `GameplaySessionBootstrap` when you want:

- authored participants and pawns
- `PawnRoot`
- five 3D pawn components (`Motor3D`, `Pawn3DInputModule`, `Pawn3DMovementComponent`, `Pawn3DTraversalComponent`, `Pawn3DPresentationComponent`) applying movement, traversal, and presentation profiles directly

For a shared-core brawler pawn prefab, include:

- `PawnRoot`
- `Motor3D`
- `Pawn3DInputModule`, `Pawn3DMovementComponent`, `Pawn3DTraversalComponent`, `Pawn3DPresentationComponent`

### Existing controller-heavy scenes

Keep controller-heavy scenes on `GameplaySessionBootstrap` and use the supported pawn/runtime-pattern path rather than adding a second bootstrap path.

## Camera choices

Use one of these camera paths:

- `CinemachineCameraRigController` for new shared-core scenes

## Final checklist

- the menu scene is in Build Settings
- `MainMenuManager` references are fully wired
- the gameplay scene name points to a valid gameplay scene
- player objects include the controller and combat components they actually depend on
- your startup path matches the scene's real architecture
