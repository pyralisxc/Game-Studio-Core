# NeonBlack Gameplay Scene Setup Guide

This guide reflects the current `Members/Pyralis/Gameplay` codebase and is the main scene wiring reference for NeonBlack Gameplay.

**New to the package or need step-by-step Editor instructions?**
Start with `Docs/Authoring/START_HERE.md` and run the
`authoring-native-1p-proof-checklist.md` pass from the repository `docs/` root first.
Read `Docs/Authoring/AUTHORING_MODEL.md` if definitions and profiles are unclear,
then use `Docs/Authoring/Prefabs/Bootstrap_Example_Setup.md` for click-by-click Editor wiring.
Use `Docs/Authoring/README.md` when you need the authoring-doc index.

This file is the developer reference for what belongs in each scene type after the first playable proof makes sense.

## Core rule

Use `GameplaySessionBootstrap` for new scenes.

For the first proof of an authored route, follow native actions only:

- Hierarchy/Project to create the scene root and supporting transforms.
- Inspector Add Component to add `GameplaySessionBootstrap` (+ optional `PyralisGameplayLifetimeScope`).
- Inspector drag-and-drop wiring for:
  - `GameplaySessionBootstrap` -> `SessionDefinition`
  - `SessionDefinition` -> `GameModeDefinition`
  - `SessionDefinition` -> one `ParticipantDefinition`
  - Pawn route only: participant `Default Pawn` -> `PawnDefinition`, pawn prefab, and at least one spawn point.
- Play mode proof checks:
  - one pawn spawns and is controlled by input
  - intent-required blockers are clear in Overview/Validate
  - optional camera bounds are assigned only if tested.

Use native Unity creation and assignment while Authoring explains the route. Future scaffold tooling should be treated as route scaffolding only, not authoring-proof evidence.
It does not replace the manual authoring validation route or Inspector wiring of spawn points, participant links, pawn prefab assignment, input profile wiring, and bounds setup.

## Native 1P Route (Movement Proof)

Use this exact sequence when learning the platform from a clean scene:

1. Create `Gameplay Root` with `Hierarchy > Create Empty`.
2. Add `GameplaySessionBootstrap` (and optional `PyralisGameplayLifetimeScope`) with Add Component search.
3. Create in Project and wire: `SessionDefinition` -> `GameModeDefinition` -> `ParticipantDefinition` -> `PawnDefinition`.
4. Add one `InputProfile` to participant and assign one `Motor2DInputAdapter`-based prefab path to pawn.
5. Create one `SpawnPoint` transform and assign it to `GameplaySessionBootstrap.Spawn Points`.
6. In Authoring Window: clear `Do Now`, open `Map`, check intent-required blockers in `Validate`, then Play.
7. Confirm: one pawn spawns and moves from input before adding score/combat/HUD.

Only after these checks pass, add optional layers.

## Shared-core scene setup

Use this for new work.

### Required scene items

- one `GameplaySessionBootstrap`
- one assigned `SessionDefinition`

### Strongly recommended when relevant

- one `PlayerInputManager` if local join is needed
- one `CinemachineCameraRigController` if you want shared camera behavior
- explicit spawn point transforms if seat-based spawn positions matter

### Required authored assets

- `SessionDefinition`
- `GameModeDefinition`
- one or more `ParticipantDefinition` assets
- one `PawnDefinition` per pawn setup

### Create menu paths

- `Assets/Create/NeonBlack/Definitions/Session Definition`
- `Assets/Create/NeonBlack/Definitions/Game Mode Definition`
- `Assets/Create/NeonBlack/Definitions/Participant Definition`
- `Assets/Create/NeonBlack/Definitions/Pawn Definition`
- `Assets/Create/NeonBlack/Profiles/...`

### Shared-core pawn requirements

Each pawn prefab that will be spawned by `ParticipantSpawnService` should include:

- `PawnRoot`
- a valid `PawnDefinition`
- shared module components implementing pawn interfaces directly
- for 2D pawns, use the `Motor2D` stack with `Pawn2DMovementComponent`, `Pawn2DPresentationComponent`, `Motor2DInputAdapter`, and `PawnCombatBehaviour2D` as needed

### What `GameplaySessionBootstrap` configures

- `GameplayRuntimeContext.ActiveSessionDefinition`
- `GameplayRuntimeContext.DefaultInputActions`
- `SessionStateService`
- `ParticipantRosterService`
- `ParticipantSpawnService`
- `ParticipantInputRouter`
- `PlayerInputManager.maxPlayerCount`
- `PlayerInputManager.splitScreen`
- active game mode handoff to `CinemachineCameraRigController`

### Validation

Use the `GameplaySessionBootstrap` inspector setup flow before creating prefabs or hand-building scenes. The final scene/prefab readiness step checks:

- participant slot count and duplicate/out-of-range preferred seats
- missing scripts on the gameplay root and referenced prefab hierarchies
- pawn prefabs for `PawnRoot`, `IPawnMotor`, and `IPawnPresentationModule`
- enabled feature-module runtime prefab validation
- projectile prefab delivery for `Projectile`/`Projectile2D` runtime bodies and a single 2D or 3D physics lane
- networked sessions for a scene `NetworkManager`, `UnityTransport`, pawn `NetworkObject`, and Network Prefab registration guidance

Networking is MVP-ready at the scene setup layer when those checks pass. Treat rollback, prediction, reconciliation, replicated animation, remote input command streaming, and lobby/matchmaking as later networking enhancement lanes.

You can also use the component context menu:

- `Validate Gameplay Setup`

Watch for warnings about:

- missing `SessionDefinition`
- missing default game mode
- missing default participants
- missing default input profile for actor-driven scenes
- missing `PlayerInputManager`
- missing `CinemachineCameraRigController`
- route validation issues surfaced through `SessionDefinition.defaultGameMode`, mode fields, participants, pawns, feature modules, and contracts

## 2D score-loop scenes

All 2D systems live under `Features/[Name]/2D/`.

### Menu scene

Primary scripts:

- `MainMenuManager`
- `SettingsScreen`
- optional `LeaderboardScreen`

Current data assets:

- `LevelRegistry`
- `LevelData`

Current ownership:

- `Core/Navigation/LevelData` and `Core/Navigation/LevelRegistry` for level-selection assets
- `Core/Navigation/LevelSession` and `Core/Navigation/SceneNames` for cross-scene state
- `Core/Navigation/UI/MainMenuManager` for 2D menu UI
- `Features/Settings/UI/SettingsScreen` for reusable settings page flow
- `Core/Navigation/UI/SceneFader` for fade-based scene transitions
- `Core/Navigation/UI/SplashScreenController` for splash startup scenes

Current create menu names:

- `Assets/Create/NeonBlack/Scene Flow/Level Registry`
- `Assets/Create/NeonBlack/Scene Flow/Level Data`

Menu requirements:

- play button
- settings button
- `SettingsScreen`
- `LevelRegistry` if level selection is used
- optional prev/next buttons
- optional level preview image and name label
- optional random toggle
- optional high score label
- optional remove ads button
- optional leaderboard button and screen

`MainMenuManager` should be configured with its gameplay destination explicitly through its serialized scene fields.

### Gameplay scene

Primary scripts:

- `GameManager`
- `ParticipantScoreService`
- `DifficultyManager`
- `HazardSpawner`
- `CollectibleSpawner2D`

Current ownership:

- `Features/GameFlow/2D/GameManager` for 2D run/session orchestration
- `Features/Hazards/2D/DifficultyManager` for hazard difficulty pacing
- `Features/Hazards/2D/HazardSpawner` for 2D hazard spawning
- `Features/Scoring/Runtime/Shared/ParticipantScoreService` for run-level scoring
- `Features/Pickups/2D/CollectibleSpawner2D` for neutral new pickup-loop scenes

Typical player object:

- `Motor2D`
- `Motor2DInputAdapter`
- optional `StillnessBonus2D`

`GameManager` uses `CollectibleSpawner2D` and `Motor2D` as the canonical pickup and movement surfaces for the supported 2D score-loop flow.

Optional support components:

- `PlayerInputManager`
- `CinemachineCameraRigController`
- `CameraShake`
- `UIManager`
- `UIOrientationHandler`
- `SceneFader`

Optional support ownership:

- `Features/Characters/PlayerRegistry` only for older compatibility scenes that still need a primary-player bridge
- `Features/Settings/SettingsManager` for reusable settings flow shared by menu-driven scenes
- `Features/GameFlow/2D/UI/UIManager` for 2D HUD and game-over presentation
- `Presentation/Camera/CinemachineCameraRigController` for 2D camera framing and visible bounds
- `Presentation/Visuals/CameraShake` for impact feedback across combat, projectiles, hazards, and enemy reactions
- `Features/Input/2D/InputZoneSet` for 2D movement dead-zone authoring
- `Features/UI/UIOrientationHandler` for portrait/landscape-aware UI layout
- `Core/Navigation/UI/SceneFader` and `SceneGuard` for scene transition and duplicate-system safety

## 3D combat scenes

All 3D systems live under `Features/[Name]/3D/`.

### Menu scene

Primary script:

- `MainMenuManager`

Current ownership:

- `Core/Navigation/UI/MainMenuManager`
- `Features/Settings/UI/SettingsMenu` when the menu scene includes in-panel settings

Typical setup:

- `mainPanel`
- `settingsPanel`
- `coopPanel`
- new game, load game, settings, co-op, and exit buttons
- optional back, host, and join buttons

`MainMenuManager` should be configured with its gameplay destination explicitly through its serialized `gameSceneName` field.

### Gameplay scene

Typical player object:

- `CharacterController`
- `HealthComponent`
- `KnockbackReceiver`
- `Motor3D`
- animator
- hitbox children used by `Motor3D`

Typical encounter tools:

- `EnemyAI`
- `EnemySpawner`
- `ArenaZone`
- `CameraZone`
- `DamageZone`
- `PlayerSpawner`

Camera options:

- `Presentation/Camera/3D/CameraOcclusionFader` for occlusion fading
- `Presentation/Camera/CinemachineCameraRigController` for shared-core and new scenes

### Shared-core 3D combat path

To wire a shared-core pawn around the current 3D controller stack:

- `PawnRoot`
- `Motor3D`
- `Pawn3DInputModule`
- `Pawn3DMovementComponent`
- `Pawn3DTraversalComponent`
- `Pawn3DPresentationComponent`

Drive tuning from:

- `PawnMovementProfile`
- `PawnCombatProfile`
- `PawnTraversalProfile`
- `PawnPresentationProfile`

Each `Pawn3D*` component implements the corresponding profile interface directly. No bridge component is required.

## Recommended new-scene patterns

### New shared-core prototype

Prefer:

- `GameplaySessionBootstrap`
- `SessionDefinition`
- `ParticipantDefinition`
- `PawnDefinition`
- `PawnRoot`
- `CinemachineCameraRigController`

Avoid:

- tag-only player discovery as your main architecture
- adding new code under unsupported folder shapes

### New 2D score-loop prototype

Prefer:

- shared session and participant authoring
- the `Motor2D` stack as the supported 2D controller surface:
  `Motor2D`, `Pawn2DMovementComponent`, `Pawn2DPresentationComponent`, and `Motor2DInputAdapter`
- `Collectible2D` for pickups
- `StillnessBonus2D` when you want a stationary-score mechanic
- `PawnRoot` when you want shared-core profile-driven pawn composition

### New 3D combat prototype

Prefer:

- shared session and participant authoring
- encounter and world helpers in `Features/Encounters/3D`, `Features/Spawning/3D`, `Features/Zones/3D`, and `Features/Environment/3D`
- `Motor3D` as the supported 3D controller surface
- `PawnRoot` for profile-driven pawn composition
- authored profiles instead of hardcoded tuning in new scene-only scripts

### Hybrid score-and-combat prototype

Prefer:

- one `GameplaySessionBootstrap`
- one shared authored `SessionDefinition`
- one runtime surface choice:
  - feature `2D` lanes when your implementation depends on 2D physics and playfield assumptions
  - feature `3D` lanes when your implementation depends on 3D movement, depth, or CharacterController behavior
- feature combinations drawn from scoring, pickups, hazards, combat, respawn, and scene flow rather than genre-named top-level folders

## Final scene review checklist

- one startup model is active, with `GameplaySessionBootstrap` preferred for new scenes
- all required scenes are in Build Settings
- session assets are assigned
- player or participant spawning is explicit
- camera path matches the scene's actual setup
- menu and UI references are wired
- any shared-core pawn prefab includes `PawnRoot`


