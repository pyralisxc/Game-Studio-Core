# Pyralis Authoring Docs

This folder is the living setup and authoring reference for Pyralis.

The Authoring Window, Inspector field guides, setup validation, and route facts are the primary setup surface. These docs support that workflow; they do not preserve old setup paths.

## Fast Reading Path

| Job | Read |
|---|---|
| I am creating a first scene | `START_HERE.md` |
| I need the asset/runtime mental model | `AUTHORING_MODEL.md` |
| I am improving the Authoring Window | `AUTHORING_BLUEPRINT.md`, then `AUTHORING_MODEL.md` |
| I need the technical contract | `CANONICAL_SETUP.md` |
| I need to choose runtime patterns | `RUNTIME_PATTERN_COOKBOOK.md` |
| I need scene-level route requirements | `SCENE_SETUP_GUIDE.md` |
| I need feature or prefab wiring | the matching guide in `Prefabs/` |
| I am changing architecture or folder rules | `Systems/Architecture_Overview.md`, `Systems/Migration_and_Readability_Standard.md` |

## Core Authoring Flow

Most game setups use this chain:

1. existing `RuntimePatternDefinition` assets
2. `GameSetupProfile`
3. `GameModeDefinition`
4. `SessionDefinition`
5. `ParticipantDefinition`
6. `PawnDefinition` only when the game needs pawn-backed actor bodies

Create assets through the native Project-window Create menu under `NeonBlack`. Use the Authoring Window and Inspector field guides for route state, field handoff, validation, and first proof guidance.

## Setup Maintenance Contract

Authoring guidance is product code. When changing first-scene or route-guided setup behavior, keep these surfaces aligned in the same maintenance slice:

- feature-owned contracts and convention facts
- `PyralisSetupRouteAnalysis`
- `PyralisSetupFlowValidator`
- `PyralisSceneReadinessValidator`
- `PyralisAuthoringRouteProof`
- Inspector field guides
- these docs

Do not create new setup truth in docs alone. Put reusable meaning in the authoring spine first, then make docs explain it.
