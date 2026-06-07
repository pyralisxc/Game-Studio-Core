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

- `Core/`: runtime services, contracts, and shared config
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

- `Docs/Setup/MANUAL.md`
- `Docs/Setup/START_HERE.md`
- `Docs/Setup/AUTHORING_BLUEPRINT.md`
- `Docs/Setup/AUTHORING_MODEL.md`
- `Docs/Setup/README.md`
- `Docs/Setup/CANONICAL_SETUP.md`
- `Docs/Setup/Prefabs/Bootstrap_Example_Setup.md`
- `Docs/Setup/Prefabs/Pawn_Setup.md`
- `Docs/NewGameTypeGuide.md`
