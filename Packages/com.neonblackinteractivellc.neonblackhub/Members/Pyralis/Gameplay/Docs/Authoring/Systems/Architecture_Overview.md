# NeonBlack Gameplay Architecture Overview

This document summarizes the current runtime wiring conventions used by NeonBlack Gameplay.

## Current source layout

The active NeonBlack Gameplay codebase lives under:

- `Members/Pyralis/Gameplay/Core`
- `Members/Pyralis/Gameplay/Data`
- `Members/Pyralis/Gameplay/Editor`
- `Members/Pyralis/Gameplay/Features`
- `Members/Pyralis/Gameplay/Presentation`
- `Members/Pyralis/Gameplay/Networking`
- `Members/Pyralis/Gameplay/Integrations`

## Startup model

### Supported new-scene startup

Use `GameplaySessionBootstrap` for new scenes.

This path is built around:

- `SessionDefinition`
- `GameModeDefinition`
- `ParticipantDefinition`
- `PawnDefinition`
- `ParticipantRosterService`
- `ParticipantSpawnService`
- `ParticipantInputRouter`
- `SessionStateService`
- `PyralisGameplayLifetimeScope`

## Core runtime concepts

### Composition Root

`GameplaySessionBootstrap` is the Unity-facing entry point. It reads `SessionDefinition`,
creates or connects the core persistent scene services, registers platform defaults, and configures
`PyralisGameplayLifetimeScope`.

`PyralisGameplayLifetimeScope` is the runtime DI graph. It registers scope-owned components and
explicit contracts first, then bridges remaining `PlatformServiceRegistry` entries into VContainer
so injected scene objects and transition-era registry consumers resolve the same owned services.

`PlatformServiceRegistry` remains migration support. New runtime dependencies should prefer
constructor or method injection, explicit service contracts, or `GameplayPlatformContext.TryResolve`
only when a non-DI bridge is still required.

Static `Instance` properties on persistence helpers are compatibility and duplicate-control surfaces.
They are not the beginner dependency path and should clear themselves on teardown or subsystem
registration.

### Scene Navigation

User-facing scene flow should depend on `ISceneNavigator`, not on `SceneLoader.Instance`, `SceneFader.Instance`, or `SceneNavigator.LoadScene(...)`.

Default implementations:

- `SceneFader` - fades through an overlay and implements `ISceneNavigator`.
- `SceneLoader` - builds a runtime fade canvas and implements `ISceneNavigator`.

`SceneNavigator.LoadScene(...)` remains only as a tiny direct `SceneManager` fallback for simple non-authored utility paths.

### Participant model

The shared-core path treats gameplay seats as participants rather than assuming one global player.

Key types:

- `ParticipantHandle`
- `ParticipantDefinition`
- `ParticipantRosterService`
- `ParticipantSpawnService`

### Pawn model

`PawnRoot` is the composition root for shared-core pawns.

It applies authored profiles from `PawnDefinition` and can install feature prefabs from `FeatureModuleDefinition`.

The 3D brawler uses direct module composition through five sibling components on the pawn root:

- `Motor3D` - coordinator; implements `ICharacterMotorState`
- `Pawn3DInputModule` - Input System binding
- `Pawn3DMovementComponent` - physics and locomotion; implements `IPawnMotor`
- `Pawn3DTraversalComponent` - Traversal-owned climb, hang, and ledge runtime; implements `IPawnTraversalModule`
- `Pawn3DPresentationComponent` - animation, billboard, HUD; implements `IPawnPresentationModule`

The 2D arcade pawn now participates through direct component composition:

- `Motor2D` - shared 2D pawn motor surface
- `Motor2DInputAdapter` - preferred input-profile bridge for player-controlled 2D pawns
- `Pawn2DMovementComponent`
- `Pawn2DPresentationComponent`
- `PlayerInputHandler` - lower-level keyboard, gamepad, and touch input reader used by the adapter route
- `PawnCombatBehaviour2D` (when combat is enabled)

### Current extension points

The post-refactor cleanup introduced a few important extension seams:

- participant HUD feedback now flows through `ParticipantFeedbackService`, `IParticipantFeedbackStream`, and `IParticipantFeedbackPublisher`
- leaderboard reads now flow through `ILeaderboardService`
- gameplay-state adapter reads now flow through `IGameplayStateReader`
- scene/menu navigation now flows through `ISceneNavigator`
- pawn input settings registration now flows through `IInputSettingsRegistrar`
- combat impact feedback now flows through `IHitPauseSink` and `ICameraShakeSink`
- pickups now distinguish `CollectBy(...)` from `RemoveFromPlay()`

## Folder placement guidance

### Core

Put foundational runtime infrastructure here: contracts (`IGameService`, `IInputProvider`, etc.),
runtime context, config (`GameConfig`, `InputConfig`), and cross-cutting utilities (`SceneLoader`,
`TimeManager`). Platform composition helpers that build the VContainer graph belong under
`Features/Platform/Composition` unless they are pure contracts or runtime context.

### Data

Put ScriptableObject definitions and profiles here: `GameModeDefinition`, `ParticipantDefinition`, `PawnDefinition`, `CameraRigProfile`, `PawnMovementProfile`, and similar authored assets.

### Editor

Put all editor-only tools here. No `Inspectors/` subfolder - keep it flat.

### Features

Put all gameplay capability here. Each feature lives in `Features/[Name]/` and increasingly follows the governed runtime shape:

- `Runtime/Shared`
- `Runtime/2D`
- `Runtime/3D`
- feature-local `Data`, `Editor`, `Tests`, and `Docs` where applicable

The supported 2D controller surface for new work is the `Motor2D` stack: `Motor2D`, `Pawn2DMovementComponent`, `Pawn2DPresentationComponent`, and `Motor2DInputAdapter` (`Features/Characters/2D/`).
The supported 3D controller surface for new work is `Motor3D` (`Features/Characters/3D/`).

## Practical rules

- prefer `GameplaySessionBootstrap` for new scenes
- prefer neutral runtime surfaces and feature-first naming over themed controller identities
- prefer shared definitions, profiles, and feature composition over genre-coded folder ownership
- keep direct `SceneManager.LoadScene(...)` and scene-transition singleton usage out of user-visible runtime components
- keep new work inside supported source folders and documented feature/runtime domains
- treat the Authoring Window and Inspector guides as the live checklist, `Docs/Authoring/START_HERE.md` as the first-scene beginner path, and `Docs/Authoring/CANONICAL_SETUP.md` as the technical contract
