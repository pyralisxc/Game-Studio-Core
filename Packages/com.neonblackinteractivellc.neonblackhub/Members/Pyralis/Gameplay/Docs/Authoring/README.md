# Pyralis Authoring Docs

This folder is the living setup and authoring reference for Pyralis.

The Authoring Window, Inspector field guides, setup validation, and route facts are the primary setup surface. These docs support that workflow; they do not preserve old setup paths.

## Fast Reading Path

| Job | Read |
|---|---|
| I am creating a first scene | `START_HERE.md` |
| I need the asset/runtime mental model | `AUTHORING_MODEL.md` |
| I am improving the Authoring Window | `AUTHORING_BLUEPRINT.md`, then `AUTHORING_MODEL.md` |
| I am working on the resolved setup graph | `RESOLVED_SETUP_GRAPH_SCOPE.md`, then `AUTHORING_BLUEPRINT.md` |
| I need the technical contract | `CANONICAL_SETUP.md` |
| I need to choose route capabilities | `ROUTE_CAPABILITY_COOKBOOK.md` |
| I need scene-level route requirements | `SCENE_SETUP_GUIDE.md` |
| I need feature or prefab wiring | the matching guide in `Prefabs/` |
| I am changing architecture or folder rules | `Systems/Architecture_Overview.md`, `Systems/Migration_and_Readability_Standard.md` |

## Core Authoring Flow

Most game setups use this chain:

1. `SessionDefinition` and `GameModeDefinition` with participants, pawns, feature modules, scene evidence, contracts/reflection, and grammar vocabulary
2. `GameModeDefinition`
3. `SessionDefinition`
5. `ParticipantDefinition`
6. `PawnDefinition` only when the game needs pawn-backed actor bodies

Create assets through the native Project-window Create menu under `NeonBlack`. Use the Authoring Window and Inspector field guides for route state, field handoff, validation, and first proof guidance.

## Source-Of-Truth Map

The authoring system should have one operating model:

```text
Gameplay code / authored assets
  -> contracts + reflection + dependency tree + scene evidence + validators + grammar
      -> resolved setup graph
          -> Overview / Intent / Guide / Map / Validate / Facts / Inspector handoffs
```

Use `AUTHORING_BLUEPRINT.md` as the canonical map for where information comes from and how cleanup closes. In short:

- Contracts own feature meaning.
- Reflection owns code-proven facts.
- Dependency tree owns setup/reference structure.
- Scene evidence owns what exists in the open scene.
- Validators own readiness and blockers.
- Grammar/vocabulary owns generic wording and fallback templates.
- The resolved setup graph synthesizes those inputs.
- UI tabs project the graph; they should not invent separate setup truth.

## Setup Maintenance Contract

Authoring guidance is product code. When changing first-scene or route-guided setup behavior, keep these surfaces aligned in the same maintenance slice:

- feature-owned contracts and dependency-tree references
- `PyralisSetupDependencyTree` and `PyralisSetupRouteAnalysis`
- `PyralisSetupFlowValidator`
- `PyralisSceneReadinessValidator`
- `PyralisProofFamilyVocabulary` for generic fallback proof wording
- `PyralisAuthoringSetupGraph` and graph projection rows
- Inspector field guides
- these docs

Do not create new setup truth in docs alone. Put feature meaning in contracts, reflected setup/reference meaning in the dependency tree, readiness in validators/graph evidence, and generic wording in grammar/vocabulary first, then make docs explain it. The active UI should read the resolved setup graph; vocabulary is fallback wording, not a second setup model.
