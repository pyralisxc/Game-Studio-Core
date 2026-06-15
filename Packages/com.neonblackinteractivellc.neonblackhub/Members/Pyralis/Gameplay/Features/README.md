# Pyralis Gameplay Features

Feature folders are organized by gameplay responsibility first, then by runtime lane only when the implementation truly differs by lane.

## Folder Rules

- `Runtime/Shared` owns reusable contracts, services, contexts, and feature logic.
- `Runtime/2D`, `Runtime/2_5D`, and `Runtime/3D` own lane-specific adapters.
- Feature-local `Editor/Authoring` may explain or validate a feature, but setup truth should come from contracts, dependency structure, and graph evidence.
- Avoid one-file forwarding folders. Add a folder only when it makes ownership easier to see.
- Do not put authored scene state in feature code. Definitions, profiles, participants, pawns, and Unity components remain the setup surfaces.

## Current Ownership Notes

- `Platform/Session` owns the visible session entry point; `Characters` owns pawn initialization and runtime pawn service injection.
- `Input` owns input routing and participant input profile resolution.
- `Spawning` owns spawn/respawn timing, lives, countdown, and revive feedback; pawn identity and instantiation stay with `ParticipantSpawnService`.
- `Platform` owns cross-feature runtime composition state, not gameplay-specific defaults. Its lifetime scope registers the core spine unconditionally, then registers combat, enemy, RPG, game-flow, scoring, and feedback service groups only when route metadata, feature modules, reflected contracts, or loaded scene components provide evidence that the route uses them.

When touching a feature branch, prefer making its source of truth obvious over adding compatibility glue.
