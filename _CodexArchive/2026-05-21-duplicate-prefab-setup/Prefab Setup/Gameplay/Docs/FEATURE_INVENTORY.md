# NeonBlack Gameplay Feature Inventory

This is the current feature surface available in NeonBlack Gameplay after the shared-core refactor pass.

It describes what is actually present in the package today, not only what is planned.

## Shared-Core Foundations

### Session And Participants

Available:

- `SessionDefinition`
- `ParticipantDefinition`
- `ParticipantHandle`
- `ParticipantRosterService`
- `ParticipantSpawnService`
- `SessionStateService`
- `ParticipantInputRouter`
- `GameplaySessionBootstrap`

What this gives us:

- `N`-participant runtime terminology
- local-first participant registration
- participant-to-pawn assignment
- session-level mode/settings ownership
- room for future host-authoritative networking seams

### Pawn Composition

Available:

- `PawnRoot`
- `IPawnMotor`
- `IPawnCombatModule`
- `IPawnTraversalModule`
- `IPawnPresentationModule`
- `IFeatureModuleRuntime`

2D arcade pawn (direct module composition with a compatibility facade):

- `Motor2D` (`Features/Characters/2D/`) — thin compatibility facade over the 2D pawn stack
- `Pawn2DMovementComponent` — movement, dash, bounds, and reaction-lock ownership
- `Pawn2DPresentationComponent` — sprite, animator, tilt, squash, and death presentation
- `PawnCombatBehaviour2D` — combo and hitbox combat ownership
- `Motor2DInputAdapter` / `PlayerInputHandler` — input binding and action routing

3D brawler pawn (direct module composition — no bridge required):

- `Motor3D` (`Features/Characters/3D/`) — coordinator; sequences modules each frame, implements `ICharacterMotorState`
- `Pawn3DInputModule` — Input System binding; produces `FrameInput` each frame
- `Pawn3DMovementComponent` — implements `IPawnMotor`; owns `BrawlerMovementModel` and `CharacterController`
- `Pawn3DTraversalComponent` — implements `IPawnTraversalModule`; owns climb, hang, and ledge detection
- `Pawn3DPresentationComponent` — implements `IPawnPresentationModule`; owns Animator, billboard, land squash, and debug HUD

What this gives us:

- data-driven pawn tuning without rewriting the controllers
- a shared place to install feature modules
- clean separation between the profile/interface layer and the controller implementation

### Data Authoring

Available definitions:

- `SessionDefinition`
- `ParticipantDefinition`
- `PawnDefinition`
- `GameModeDefinition`
- `FeatureModuleDefinition`
- `RuntimePatternDefinition`
- `RuntimeControlSurface`
- `ParticipantEmbodimentRequirement`
- `RuntimeCapabilityFamily`

Available profiles:

- `InputProfile`
- `GameSetupProfile`
- `PawnMovementProfile`
- `PawnCombatProfile`
- `PawnTraversalProfile`
- `PawnPresentationProfile`
- `PlayfieldProfile`
- `CameraRigProfile`
- `SettingsProfile`

What this gives us:

- authored setup recipes for overlapping game-loop expectations
- a profile-level way to say a game is, for example, realtime character plus projectile combat rather than one exclusive genre
- validation for participant embodiment expectations, including non-pawn control surfaces such as camera, cursor, board seat, card hand, menu selection, faction, or system/AI
- optional `GameModeDefinition.setupProfile` linkage so game-mode validation can surface setup-profile issues before scene wiring

### Shared Services

Available:

- `SceneLoader` (`Core/`) — singleton scene transition service with fade
- `SceneNavigator` (`Core/`) — static helper routing through `SceneLoader` with `SceneManager` fallback
- `TimeManager` (`Core/`) — freeze frames and slow-motion
- `CameraShake` (`Presentation/Visuals/`) — singleton shake service
- `ParticipantScoreService` (`Features/Scoring/Runtime/Shared/`) — canonical multi-participant scoring

### Action And Targeting Core

Available:

- `ActionTargetKind`
- `ActionExecutionTiming`
- `ActionResolutionStatus`
- `ActionTargetRule`
- `ActionTargetDescriptor`
- `ActionExecutionContext`
- `ActionValidationResult`
- `ActionResolutionResult`
- `IActionResolver`
- `ActionDefinition`

What this gives us:

- a pawn-agnostic action vocabulary for future guns, projectiles, turn-based commands, card plays, board moves, menu actions, traps, and scripted abilities
- reusable target validation for no-target, self-target, actor-target, point-target, direction-target, board-space, card, zone, and custom payload flows
- an authored `ActionDefinition` starting point that can be referenced by future feature modules without hardwiring combat, pawn, or controller assumptions

## Extracted Feature Modules

These features have already started moving out of the older compatibility lanes and into feature-first folders:

### Scoring

Core runtime in Features/Scoring/Runtime/Shared/:

- ParticipantScoreService

Service and UI integration in Features/Scoring/:

- LeaderboardManager
- UI/LeaderboardScreen

2D game-specific runtime in Features/Scoring/2D/:

- StillnessBonus2D

What this gives us:

- a generic run-level scoring service for score and survival-time loops
- reusable leaderboard submission and retrieval hooks
- UI that no longer has to be treated as arcade-only just because it was authored there first

### Pickups

Collector runtime in Features/Pickups/Runtime/:

- Shared/IPickupCollectible
- 2D/ActorPickupCollectorFeature2D
- 3D/ActorPickupCollectorFeature3D

Concrete 2D pickup implementation in Features/Pickups/2D/:

- Collectible2D
- CollectibleSpawner2D
- CollectibleFeedback2D

What this gives us:

- generic point terminology for new authored content
- a clearer migration path away from crumb-themed scene language
- a neutral pickup spawner API for new content without alias-driven duplicate naming
- concrete 2D pickup implementations that can evolve independently of the runtime collector core

## Shared Combat And Health

Available:

- `HealthComponent`
- faction-aware damage filtering
- invulnerability frames
- optional health regeneration
- `KnockbackReceiver`
- `HitBox`
- `HitBox2D`
- `HitBoxSlot`
- `Projectile`
- `WeaponData`
- `DamageNumber`
- `DamageNumberSpawner`
- `HitFlash`
- `WorldHealthBar`

### Guns And Projectile Foundation

Available:

- `ProjectileDeliveryMode`
- `ProjectileDefinition`
- `FireModeDefinition`
- `ProjectileFireRequest`
- `ProjectileSpawnCommand`
- `ProjectileFirePlanner`
- `ProjectileSpawnStatus`
- `ProjectileSpawnResult`
- `ProjectileImpactDefinition`
- `ProjectileImpactEffectPlayer`
- `ProjectileMagazineState`
- `ProjectileLauncherBase`
- `ProjectileLauncher3D`
- `ProjectileLauncher2D`
- `ProjectilePoolHandle`

What this gives us:

- authored projectile-prefab and hitscan delivery data
- authored cooldown, ammo-per-shot, clip, reload, burst, projectile-count, and spread settings
- deterministic spawn command planning that can be used by pawns, enemies, traps, turrets, cards, menus, or scripted events
- action-context direction and target-position support without requiring a character controller
- reusable 3D and 2D launcher components that execute projectile-prefab or hitscan commands
- optional launcher-owned prefab pooling for pooled projectile instances
- plain runtime magazine state for clipped or unlimited fire modes
- authored hit/miss impact effects, hit pause, camera shake, and audio hooks through `ProjectileImpactDefinition`
- example authoring pack output for a sample hitscan projectile, fire mode, and impact definition

Gameplay implications:

- melee and projectile combat are already supported
- both player and enemy actors can share the same core health/damage primitives
- combat feedback tooling already exists for floating numbers, flashes, and health bars
- reusable gun/projectile planning, command execution, and impact effect routing have started, but charge/recoil policy, inventory-level ammo ownership, trail/material presets, and sample prefabs are still next-step work

## Arcade Feature Surface

### Player Loop

Available:

- `Features/Characters/2D/Motor2D`
- `Features/Input/2D/Motor2DInputAdapter`
- 2D motor-driven movement
- acceleration and deceleration
- dash
- screen clamp and optional wrap
- squash/stretch and tilt feedback
- touch, keyboard, and gamepad input
- virtual joystick
- swap-controls support
- deadzone settings

### Pickup And Score Loop

Available:

- canonical `Collectible2D`, `CollectibleSpawner2D`, and `CollectibleFeedback2D` files for new authored content
- pooled point-pickup spawning
- pickup idle bob animation
- pickup scoring
- pickup destroy-without-score path for hazards
- high score and best-time persistence
- stillness bonus hook via canonical `StillnessBonus2D`

### Hazard System

Available:

- `Features/Hazards/HazardData`
- `Features/Hazards/HazardPresetLibrary`
- custom inspector in `Features/Hazards/Editor/HazardDataEditor`
- `Features/Hazards/2D/DifficultyManager`
- `Features/Hazards/2D/Hazard`
- `Features/Hazards/2D/HazardSpawner`
- pooled hazard spawning
- weighted hazard selection
- difficulty-driven spawn pacing
- hazard telegraphing and warning phases
- multiple hazard behavior families
- runtime hazard pickup interactions now prefer neutral collectible APIs before falling back to crumb-era implementations

From current hazard data surface:

- slam hazards
- crossing hazards
- jump-style crossing variants
- wavy travel
- bouncy hazards
- targeting/steering behavior
- explosion modifier
- pickup destruction and pickup spawning interactions
- split-on-bounce behavior
- screen shake hooks
- per-hazard audio hooks

### Difficulty System

Available:

- `Features/Hazards/2D/DifficultyManager`
- linear difficulty curve
- exponential difficulty curve
- step-based difficulty curve
- named wave mode
- random, sequential, and weighted wave selection
- min/max hazards on screen
- min/max hazards per burst
- spawn margin and player-distance gating
- edge-biased spawn positioning

### Menu And Scene Flow

Available:

- `Core/Navigation/LevelData`
- `Core/Navigation/LevelRegistry`
- `Core/Navigation/LevelSession`
- `Core/Navigation/SceneNames`
- `Features/GameFlow/2D/GameManager`
- `Features/GameFlow/2D/UI/UIManager`
- `Core/Navigation/UI/MainMenuManager`
- `Core/Navigation/UI/SplashScreenController`
- `Core/Navigation/UI/LoadingScreenController`
- `Core/Navigation/UI/SceneFader`
- `Core/Navigation/UI/SceneGuard`
- splash screen video or timed logo flow
- loading and fade support
- scene guards and scene naming helpers
- level registry and level selection
- random level selection contract
- settings menu path
- leaderboard screen hooks
- a 2D loop manager that now prefers `CollectibleSpawner2D` and `Motor2D` when those supported surfaces are present

### Mobile And Monetization

Available:

- `Features/Settings/SettingsManager`
- `Features/Settings/UI/SettingsScreen`
- `Features/Settings/UI/SettingsMenu`
- `Features/UI/UIOrientationHandler`
- orientation-aware UI layout capture and replay
- ads manager
- IAP manager
- leaderboard manager

## Optional Networking Extension

NGO-dependent code lives in a separate assembly (`NeonBlack.Gameplay.Networking`) that references `Core`, `Data`, and `Characters` but not the broad gameplay aggregate. Local games compile and run without depending on NGO gameplay code.

Available in `Networking/Characters/`:

- `NetworkedSessionStateService` — extends `SessionStateService`; starts the NGO host when `SessionDefinition.autoStartHost` is true
- `NetworkedParticipantRosterService` — extends `ParticipantRosterService`; resolves `NetworkManager.LocalClientId` for participant ownership
- `NetworkedParticipantSpawnService` — extends `ParticipantSpawnService`; calls `NetworkObject.Spawn()` and `NetworkObject.Despawn()` around pawn lifecycle

For local games register the base classes at bootstrap. For online games register the `Networked*` variants — same interface, NGO behavior added transparently.

## Transitional Shared Compatibility

Available:

- `Features/Characters/PlayerRegistry` — a migration-safe `IPlayerProvider` bridge for older 2D scene wiring

## Brawler Feature Surface

### Player Controller

Available (`Features/Characters/3D/`):

- `Motor3D` — coordinator
- `Pawn3DInputModule`
- `Pawn3DMovementComponent`
- `Pawn3DTraversalComponent`
- `Pawn3DPresentationComponent`
- 2.5D movement
- side-scroller movement
- top-down movement
- sprint and crouch
- jump and gravity
- coyote time
- jump buffering
- early jump cut
- multi-jump
- landing squash and landing slow
- block
- dodge
- slope slide
- power slide
- climb
- wall slide
- combo windows
- aerial attacks
- weapon inventory cycling
- billboarding support
- debug HUD path

### Combat

Available:

- named hitbox zones
- weapon-driven attack tuning
- separate attack, kick, and aerial weapon slots
- damage and knockback fallback values
- block-based damage reduction
- projectile spawn point support

### Enemies

Available:

- patrol/chase/attack/death state machine
- line-of-sight option
- leash logic
- movement mode support
- patrol routes or random patrol
- weighted or priority-based attack choice
- attack-range evaluation from weapons/hitboxes

### Encounters And Space Control

Available:

- `Features/Encounters/3D/ArenaZone`
- `Features/Spawning/3D/EnemySpawner`
- `Features/Spawning/3D/Spawner`
- `Features/Zones/3D/CameraZone`
- `Features/Zones/3D/DamageZone`
- `Features/Environment/3D/TilemapGround`
- `Features/Environment/3D/DepthSorting`
- `Features/Respawn/3D/PlayerSpawner`
- `Features/Traversal/Runtime/3D/ClimbZone`
- `Features/Traversal/Runtime/3D/GrabDetector`
- `Features/Traversal/Runtime/3D/LedgeProbe3D`

### Camera

Available:

- `Presentation/Camera/2D/CameraAspectController`
- `Presentation/Camera/2D/CameraShaker`
- `Presentation/Camera/CinemachineCameraRigController`
- `Presentation/Camera/3D/CameraOcclusionFader`
- `CameraProfile` assets
- camera profile switching by trigger volume
- wall avoidance
- transparency for occluding objects
- zoom smoothing and profile transitions

## Editor Tooling

Available:

- custom inspector for `EnemyAI`
- custom inspector for `HitBox`
- custom inspector for `ClimbZone`
- custom inspector for `WorldHealthBar`
- custom inspector for `HazardData`
- custom inspector for `InputZoneSet`
- custom inspector for `UIOrientationHandler`
- custom inspector for `RuntimePatternDefinition`
- custom inspector for `GameSetupProfile`
- custom inspectors for the new shared-core definitions and profiles
- `Assets/Create/NeonBlack/Gameplay/Example Authoring Pack`, including sample projectile assets, canonical runtime pattern definitions, and a composed brawler-with-projectiles setup profile

## Current Gaps

These are the main areas where features exist but are not fully generalized yet:

- arcade pickup spacing, pickup collection, and hazard collision flow are now more participant-aware, but the arcade loop still needs deeper shared-core conversion before it should be treated as fully production-ready large-participant gameplay
- arcade UI and score presentation are still primarily run-level rather than per-participant or per-team
- some camera behavior is split between legacy and shared-core paths
- some menu/game scene flows still assume the older adapter-specific setup
- action and targeting have a first shared foundation, but they are not yet wired through realtime, turn-based, tactical, board, card, and menu-driven feature modules
- guns and projectile support now has authored delivery, fire-mode definitions, deterministic command planning, 2D/3D command launchers, optional prefab pooling, runtime magazine state, impact effect routing, and sample authoring assets, but not yet charge/recoil policies, inventory-level ammo ownership, trail/material presets, or sample weapon prefabs
- runtime pattern setup now describes composable game-loop intent, but does not yet generate scenes or inspect required scene services
- procedural generation is not yet a canonical feature family; side-scrolling 2D generation should start with authored chunks, sockets, budgets, seeds, and validation
- board, card, and tabletop setup is represented as a first-class runtime pattern, but board spaces, cards, decks, hands, zones, legal move validation, and turn rules are not implemented yet
- animation mapping is data-driven, but editor tooling should become more helpful for prebuilt Animator Controllers and partial mappings

## Practical Read

Today, NeonBlack Gameplay already supports:

- arcade survival or pickup games
- hazard-dense mobile score loops
- brawler combat prototypes
- top-down combat variants
- encounter-gated combat rooms
- level-selected menu-driven scene flows
- mobile UI and monetization hooks

The shared-core refactor means those features are now easier to reorganize around data and shared tools, but not every legacy mechanic has been fully converted yet.

## Intended Expansion Surface

The intended feature expansion scope is documented in `FEATURE_DEVELOPMENT_SCOPE.md`.

Near-term feature planning should prioritize:

1. action and targeting contracts that can serve realtime and rules-driven games
2. guns and projectiles as reusable delivery systems rather than controller-specific behavior
3. runtime setup profiles that keep overlapping game types explicit
4. animation mapping and Animator compatibility improvements
5. authored side-scrolling 2D procedural generation
6. turn, phase, board, card, and tabletop foundations

The guiding rule is that new systems should expand Pyralis as a gameplay composition platform, not only as a character-controller toolkit.

