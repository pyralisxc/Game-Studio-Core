# NeonBlack Gameplay Setup Guide

This folder contains the setup docs for the current Pyralis gameplay stack.

If you are trying to understand or wire Pyralis in Unity, start here:

- `START_HERE.md`

The hierarchy is: Inspector guides and the Authoring Window are the live setup surfaces, `START_HERE.md` is the first written path, `AUTHORING_BLUEPRINT.md` is the Authoring Window product blueprint, `CANONICAL_SETUP.md` is the technical contract, and `MANUAL.md` is the book-style index.

## Fast Reading Path

Read only what matches your current job:

| Job | Read |
|---|---|
| I am setting up my first scene | `START_HERE.md` |
| I want the setup manual/index | `MANUAL.md` |
| I do not understand definitions vs profiles | `AUTHORING_MODEL.md` |
| I am improving the Authoring Window | `AUTHORING_BLUEPRINT.md`, then `AUTHORING_MODEL.md` |
| I need the technical contract | `CANONICAL_SETUP.md` |
| I need to choose runtime patterns | `RUNTIME_PATTERN_COOKBOOK.md` |
| I need step-by-step bootstrap wiring | `Prefabs/Bootstrap_Example_Setup.md` |
| I need a pawn or character prefab | `Prefabs/Pawn_Setup.md` |
| I need a camera setup | `Prefabs/Camera_Setup.md` |
| I need combat, projectiles, or hitboxes | `Prefabs/Combat_Definitions_Setup.md`, `Prefabs/Health_Combat_Setup.md` |
| I need pickups, scoring, hazards, UI, settings, or scene flow | the matching guide in `Prefabs/` |
| I am building board, card, tabletop, seat, hand, faction, or camera-only gameplay | `Prefabs/Board_Card_Tabletop_Setup.md`, then camera/UI/scoring guides as needed |
| I need RPG save/load ownership | `RPG_Persistence_Setup.md` |
| I need RPG zones, travel, or open-world state | `RPG_Open_Zone_Setup.md` |
| I need the code-backed Golden RPG Sample route | `RPG_Golden_Sample_Setup.md` |
| I am changing architecture or folder rules | `Systems/Architecture_Overview.md`, `Systems/Migration_and_Readability_Standard.md` |

## Core Authoring Flow

Most game setups use this chain:

1. existing `RuntimePatternDefinition` assets
2. `GameSetupProfile`
3. `GameModeDefinition`
4. `SessionDefinition`
5. `ParticipantDefinition`
6. `PawnDefinition` only when the game needs pawn-backed actor bodies

Create these assets through the native Project-window Create menu under `NeonBlack`. The beginner path should prove each reference manually before any future scaffold or template tooling is considered.

## Beginner Rule

Do not create every possible root, prefab, or profile up front.

Create:

- one `Gameplay Root`
- one setup/profile/session authoring chain
- only the optional scene roots that match the selected runtime patterns

Then add features one at a time.

## Setup Maintenance Contract

Setup guidance is product code. When changing first-scene or route-guided setup behavior, keep these surfaces aligned in the same maintenance slice:

- `PyralisSetupRouteAnalysis` is the shared route fact source for editor setup tools.
- `GameplaySessionBootstrap` Setup Flow is the live scene checklist.
- `Pyralis Authoring Window` is the asset helper and should mirror the same route facts.
- scaffold or template tooling, when it returns later, must validate against the same setup route expectations and must not be used as evidence that the manual authoring path is followable.
- docs should teach `START_HERE.md` and Setup Flow before the book-style manual or raw asset creation.

Do not duplicate pawn-route, runtime-pattern, participant, camera/input, playfield, or scoring detection in new editor windows. Add route facts to `PyralisSetupRouteAnalysis`, then have inspectors and windows consume that shared analysis.

## Pawn Rule

Pawns are optional.

Use `PawnRoot` when the participant owns an actor body with movement, combat, animation, traversal, pickups, or feature modules.

Avoid `PawnRoot` for board games, card games, camera-only scenes, menu-only interaction, or turn state that does not need an actor body.

## Doc Roles

- `MANUAL.md` is the maintained setup manual and reading order.
- `START_HERE.md` is the human-first setup guide and first written path.
- `AUTHORING_BLUEPRINT.md` is the canonical product and implementation blueprint for the Pyralis Authoring Window.
- `AUTHORING_MODEL.md` explains every major definition/profile relationship.
- `AUTHORING_WINDOW_VISION.md` is retained only as a compatibility pointer to `AUTHORING_BLUEPRINT.md`.
- `CANONICAL_SETUP.md` is the technical contract.
- `SCENE_SETUP_GUIDE.md` maps common scene types to required systems.
- `RUNTIME_PATTERN_COOKBOOK.md` helps choose overlapping runtime patterns.
- `Prefabs/` contains task-specific wiring guides.
- `Systems/` contains architecture and migration rules.
