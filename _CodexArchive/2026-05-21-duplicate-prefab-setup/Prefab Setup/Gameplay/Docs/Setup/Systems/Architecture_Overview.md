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

`GameplaySessionBootstrap` and `PyralisGameplayLifetimeScope` form the supported composition root.
Cross-cutting services are installed through `VContainer`, scene objects are injected at startup,
and feature runtimes receive dependencies through `FeatureRuntimeInitializationContext`.

### SceneLoader and SceneNavigator

Use `SceneNavigator.LoadScene(...)` from gameplay code when possible. It routes through the centralized `SceneLoader` when one is available and falls back to `SceneManager` otherwise.

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

- `Motor3D` — coordinator; implements `ICharacterMotorState`
- `Pawn3DInputModule` — Input System binding
- `Pawn3DMovementComponent` — physics and locomotion; implements `IPawnMotor`
- `Pawn3DTraversalComponent` — Traversal-owned climb, hang, and ledge runtime; implements `IPawnTraversalModule`
- `Pawn3DPresentationComponent` — animation, billboard, HUD; implements `IPawnPresentationModule`

The 2D arcade pawn now participates through direct component composition:

- `Motor2D` as compatibility facade
- `Pawn2DMovementComponent`
- `Pawn2DPresentationComponent`
- `PlayerInputHandler`
- `PawnCombatBehaviour2D`

### Current extension points

The post-refactor cleanup introduced a few important extension seams:

- participant HUD feedback now flows through `ParticipantFeedbackService`, `IParticipantFeedbackStream`, and `IParticipantFeedbackPublisher`
- leaderboard reads now flow through `ILeaderboardService`
- gameplay-state adapter reads now flow through `IGameplayStateReader`
- pickups now distinguish `CollectBy(...)` from `RemoveFromPlay()`

## Folder placement guidance

### Core

Put foundational runtime infrastructure here: VContainer lifetime scopes/installers, contracts (`IGameService`, `IInputProvider`, etc.), config (`GameConfig`, `InputConfig`), and cross-cutting utilities (`SceneLoader`, `TimeManager`).

### Data

Put ScriptableObject definitions and profiles here: `GameModeDefinition`, `ParticipantDefinition`, `PawnDefinition`, `CameraRigProfile`, `PawnMovementProfile`, and similar authored assets.

### Editor

Put all editor-only tools here. No `Inspectors/` subfolder — keep it flat.

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
- keep direct `SceneManager.LoadScene(...)` usage to a minimum
- keep new work inside supported source folders and documented feature/runtime domains
- treat `Docs/Setup/CANONICAL_SETUP.md` as the setup source of truth
