# NeonBlack Gameplay

This folder contains the active Pyralis gameplay framework for Neon Black Hub.

The current source of truth is a shared gameplay stack built around:

- `GameplaySessionBootstrap`
- participant-owned pawns through `PawnRoot`
- authored `SessionDefinition`, `ParticipantDefinition`, `PawnDefinition`, and `GameModeDefinition`
- a clean-slate animation workflow using:
  - `PawnPresentationProfile`
  - `PawnAnimationProfile`
  - `ActorAnimationDefinition`
  - `ActorAnimationDriver`

## Supported pawn presentation targets

NeonBlack Gameplay now supports three official presentation modes:

- `Sprite2D`
- `Billboard2_5D`
- `Rigged3D`

Rigged 3D support is Animator-driven and intended for both `Generic` and `Humanoid` rigs.

## Layout

- `Core/`: runtime services, shared config, runtime contracts, and runtime-visible authoring metadata
- `Data/`: ScriptableObject definitions and profiles
- `Editor/`: authoring helpers and custom inspectors
- `Features/`: runtime systems and gameplay modules
- `Networking/`: ownership, authority, and backend-facing runtime contracts
- `Presentation/`: cross-feature visual and camera infrastructure
- `Integrations/`: adapters for external services and packages
- `Samples/`: package-level reference setups
- `Tests/`: package-level validation infrastructure
- `Docs/`: setup and architecture notes

## Current pawn animation architecture

Pawn animation is data-driven and Unity-authored:

1. `PawnDefinition` points to presentation and animation assets.
2. `PawnPresentationProfile` declares whether the pawn is 2D, 2.5D, or rigged 3D.
3. `PawnAnimationProfile` maps gameplay signals to Animator behavior.
4. `ActorAnimationDriver` applies those mappings at runtime.
5. movement, combat, and traversal systems emit shared animation signals instead of owning Animator logic directly.

## Recommended reading

- `Docs/Authoring/START_HERE.md`
- `Docs/Authoring/AUTHORING_BLUEPRINT.md`
- `Docs/Authoring/AUTHORING_MODEL.md`
- `Docs/Authoring/README.md`
- `Docs/Authoring/CANONICAL_SETUP.md`
- `Docs/Authoring/Prefabs/Bootstrap_Example_Setup.md`
- `Docs/Authoring/Prefabs/Pawn_Setup.md`
- `Docs/NewGameTypeGuide.md`
