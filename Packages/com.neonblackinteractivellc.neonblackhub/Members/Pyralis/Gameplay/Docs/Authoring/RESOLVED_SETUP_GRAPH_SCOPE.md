# Resolved Authoring Setup Graph Scope

This document defines the full migration scope for the Pyralis resolved authoring setup graph. Use it before implementing graph work, moving Authoring Window tabs, or adding new contract fields.

## North Star

Gameplay systems should declare their authoring meaning once, and the Authoring Window should project that meaning consistently.

```text
Gameplay systems
  -> feature-owned authoring contracts and facts
      -> read-only resolved setup graph
          -> Intent, Overview, Guide, Map, Validate, Facts, and selected-context projections
```

The graph is the authoring memory. Tabs are projections. Contracts and existing Unity setup evidence are the source material.

## Why This Exists

The current authoring system is organized, but setup meaning is still interpreted by several cooperating models:

- `ResolvedAuthoringContractRegistry` resolves feature-owned `[AuthoringContract]` metadata.
- `PyralisAuthoringFactRegistry` aggregates cookbook facts.
- `PyralisSetupRouteAnalysis` interprets `GameSetupProfile.runtimeCapabilities` and optional runtime patterns.
- `PyralisSetupFlowValidator` reports setup-chain readiness.
- `PyralisSceneReadinessValidator` reports concrete scene and prefab evidence.
- `PyralisAuthoringRouteProof`, `PyralisAuthoringOverviewModel`, `PyralisAuthoringValidationModel`, `PyralisAuthoringRouteReport`, graph projections, and tab renderers each project pieces of that truth.
- `PyralisContractProofFactProjector` fills missing proof facts from resolved feature contracts when a contract owns a proof target that is not already covered by the broad central proof grammar.

That is workable, but it still means feature work can require too many authoring touch points. The graph migration should reduce that pressure so normal feature development is:

```text
write gameplay system
  -> declare/update authoring contract
      -> add validation evidence only when real Unity setup needs proof
          -> graph projections inherit the meaning
```

## Non-Negotiable Boundary

The graph is read-only setup intelligence.

It may describe:

- setup nodes
- dependencies
- satisfied requirements
- missing links
- proof readiness
- native Unity actions
- evidence state
- customization checkpoints
- source contracts
- source Unity objects

It must not:

- create assets
- generate scenes
- add components
- assign fields
- choose art
- choose camera composition
- choose combat feel
- choose board layout
- apply presets
- become a route generator

Intent remains the route declaration surface. It writes explicit creator choices such as world/playfield, control shape, presentation lane, capability toggles, and proof goals. The graph explains the consequences of those choices.

## Full Migration Scope

### Phase 1: Read-Only Graph Facade

Add the graph beside the current spine.

Inputs:

- selected `GameplaySessionBootstrap`, `SessionDefinition`, `GameModeDefinition`, or `GameSetupProfile`
- `PyralisSetupRouteAnalysis`
- `ResolvedAuthoringContractRegistry`
- `PyralisAuthoringFactRegistry`
- `PyralisRuntimeCapabilityCatalog`
- `PyralisAuthoringRouteProof`
- `PyralisSetupFlowValidator`
- `PyralisSceneReadinessValidator`

Outputs:

- stable graph nodes
- graph edges
- evidence states
- lookup helpers
- tests proving route/capability/contract/proof/evidence nodes exist

Visible Authoring Window behavior should not change in this phase.

### Phase 2: Map And Overview Projection

Move the easiest duplicate projections onto the graph.

Map should render graph topology:

- bootstrap
- session
- mode
- setup profile
- selected capabilities
- participants
- pawn or no-pawn surface
- scene surfaces
- feature modules
- proof target
- evidence nodes

Overview should render graph priority:

- best next unresolved node
- required setup
- proof enhancers
- later feature cards
- play-mode readiness checklist
- strongest inspect target

Keep existing visible layout unless the graph exposes a clearer ordering.

### Phase 3: Validate Projection

Attach validation issues to graph node ids.

`PyralisSceneReadinessValidator` keeps its concrete Unity knowledge. The graph normalizes scene-readiness evidence into validation rows so Validate can group issues by resolved graph node id instead of by repeated string heuristics.

Good direction:

```text
scene.camera
  -> evidence: missing active camera or audio listener issue
  -> native action: inspect camera root / add camera / assign listener
```

Avoid flattening useful validator detail into generic graph warnings.

Current implementation: scene-readiness issues are reflected as `ValidationEvidence` graph nodes, and Validate renders required, recommended, and proof-enhancer buckets from `PyralisAuthoringSetupGraphProjection.BuildValidationRows`.

### Phase 4: Guide And Selected Context Projection

Move Guide cards and selected-object explanation onto graph projections.

Guide should rank:

- active intent nodes
- missing setup nodes
- relevant contract nodes
- nearby recommended capability nodes
- proof-supporting nodes

Selected context should explain the selected graph node or source Unity object instead of hand-writing meaning per object type wherever possible.

Current implementation: selected setup assets, bootstrap roots, pawn roots, and reflected contracts resolve through `PyralisAuthoringSetupGraphProjection.BuildSelectedContextRow`, and Guide uses that row for role, graph id, evidence, native setup, and first check. Keep specialized selection renderers only where they expose object-specific details such as runtime pattern lane fields or GameObject component lists.

### Phase 5: Contract Enrichment

Only after the graph proves which metadata it repeatedly needs, enrich `[AuthoringContract]`.

Current implementation: `SetupNodeId` is available on `[AuthoringContract]` and normalized onto `ResolvedAuthoringContract`. Use it when a contract enriches a stable graph setup concept such as `bootstrap.root`, `session.definition`, `mode.definition`, `setup.profile`, `participant.default`, or `pawn.definition`. The graph links contract nodes to those setup nodes, selected-context projection prefers the setup node when a selected contract declares one, and cookbook facts include the setup node as a related stable id. This replaces repeated type-to-setup guessing without adding a separate mapping registry.

Current implementation: `FirstProofTargetId` remains the machine-readable proof route and `FirstProof` remains human developer guidance. `PyralisAuthoringRouteProof` still owns broad route proof grammar such as pawn movement, tabletop action, UI/HUD, camera/cursor, generated content, and networking. `PyralisContractProofFactProjector` only fills proof facts for contract-owned proof ids that do not already exist in that broad grammar, so new feature contracts can become visible without adding another central proof switch.

Current implementation: `PyralisRuntimeCapabilityFamilyMap` is the shared translation layer from reflected `AuthoringCapability` flags, route lane, and world axioms into `RuntimeCapabilityFamily` rows. Intent projection and reflective contract validation consume this map instead of maintaining separate capability-family switches. Keep this as spine grammar: it describes how contract vocabulary maps to setup-family vocabulary, but it must not create assets, imply presets, or choose game content.

Remaining candidates:

- `DependsOnNodeIds`
- `SatisfiesNodeIds`
- `EvidenceKind`
- structured native action data
- setup guidance separate from developer proof guidance
- customization guidance separate from assignment fields

Do not add contract fields because they sound complete. Add them when the graph has a real repeated derivation gap.

## Minimum Graph Vocabulary

Node kinds:

- setup chain
- capability
- contract
- proof
- scene surface
- prefab requirement
- assignment field
- validation evidence

Edge kinds:

- depends on
- satisfies
- recommends
- blocks proof
- relates to

Evidence states:

- unknown
- optional
- missing
- candidate detected
- ready
- blocked

## Example Nodes

Common stable node ids should look like:

```text
bootstrap.root
session.definition
mode.definition
setup.profile
capability.character-pawn-gameplay
capability.combat
participant.default
pawn.definition
pawn.prefab.root
scene.camera
scene.event-system
feature.contract.actor.traversal.topdown-hop
proof.1p-pawn-movement
```

Use readable, stable ids. Do not derive ids from UI labels when a durable contract id or setup concept id exists.

## Current Duplication To Retire Gradually

These areas should shrink as graph projections take over:

- Map-specific topology construction.
- Overview-specific proof and checklist reconstruction.
- Validate keyword/category guessing.
- Guide card duplication.
- Facts semantic tag fallback guessing.
- Selected-context hard-coded explanations.
- Route proof and capability rows that duplicate contract-backed graph data.

Do not delete these immediately. Retire them only after an equivalent graph projection is tested and visible behavior remains useful.

## Test Strategy

Phase 1 tests should prove graph shape, not UI polish:

- a blank or null source still builds a graph with reflected contract nodes
- a `GameSetupProfile` with pawn/combat capabilities builds setup, capability, and proof nodes
- a reflected feature module contract becomes a contract node with proof target, native setup, assignment fields, and customization moments
- a `GameplaySessionBootstrap` source can attach setup-flow and scene-readiness evidence nodes

Later phases should add tab-specific projection tests before deleting old logic.

## Documentation Strategy

Active docs should describe the graph as the target authoring spine once Phase 1 exists. Until tabs migrate, docs should say the graph is being introduced beside existing models.

Avoid saying the graph already powers every tab until the code does. Stale optimism is just another form of duplicated truth.

## Implementation Artifacts

The Phase 1 design and implementation plan live at:

- `docs/superpowers/specs/2026-06-12-read-only-authoring-setup-graph-design.md`
- `docs/superpowers/plans/2026-06-12-read-only-authoring-setup-graph.md`

Future agents should use this scope document for the full migration boundary and the plan document for the first implementation slice.
